using System.Net;

namespace DedicatedFTPServerValheim.AppCore.Sync;

internal sealed class FtpClientAdapter(string ftpRootUrl) : IFtpClient
{
    private readonly Uri _rootUri = EnsureTrailingSlash(new Uri(ftpRootUrl));

    public async Task<string> TryConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            _ = await ListDirectoryAsync(_rootUri, cancellationToken);
            return "Проверка успешна!";
        }
        catch (WebException ex) when (TryGetFtpStatus(ex) == FtpStatusCode.NotLoggedIn)
        {
            return "Неверный логин или пароль!";
        }
        catch (Exception ex)
        {
            return "Ошибка подключения к FTP... " + ex.Message;
        }
    }

    public async Task<IReadOnlyCollection<FileSystemSnapshotEntry>> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        var result = new List<FileSystemSnapshotEntry>();
        await TraverseDirectoryAsync(_rootUri, result, cancellationToken);
        return result;
    }

    public async Task DownloadFileAsync(string relativePath, string localRootPath, CancellationToken cancellationToken)
    {
        var remoteUri = GetRemoteUri(relativePath);
        var localPath = Path.Combine(localRootPath, ToPlatformPath(relativePath));
        Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);

        var request = CreateRequest(remoteUri, WebRequestMethods.Ftp.DownloadFile);
        using var response = await GetResponseCancellableAsync(request, cancellationToken);
        await using var remoteStream = response.GetResponseStream();
        await using var localFile = File.Create(localPath);
        await remoteStream.CopyToAsync(localFile, 81920, cancellationToken);
    }

    public async Task UploadFileAsync(string relativePath, string localRootPath, CancellationToken cancellationToken)
    {
        var remoteUri = GetRemoteUri(relativePath);
        var localPath = Path.Combine(localRootPath, ToPlatformPath(relativePath));
        if (!File.Exists(localPath))
        {
            throw new FileNotFoundException("Local file not found for upload.", localPath);
        }

        var request = CreateRequest(remoteUri, WebRequestMethods.Ftp.UploadFile);
        await using (var requestStream = await GetRequestStreamCancellableAsync(request, cancellationToken))
        await using (var localFile = File.OpenRead(localPath))
        {
            await localFile.CopyToAsync(requestStream, 81920, cancellationToken);
        }

        using var response = await GetResponseCancellableAsync(request, cancellationToken);
        if (response.StatusCode != FtpStatusCode.ClosingData && response.StatusCode != FtpStatusCode.CommandOK)
        {
            throw new WebException($"Upload failed for {relativePath}: {response.StatusDescription}");
        }
    }

    public async Task CreateDirectoryAsync(string relativePath, CancellationToken cancellationToken)
    {
        var request = CreateRequest(GetRemoteUri(relativePath), WebRequestMethods.Ftp.MakeDirectory);
        try
        {
            using var _ = await GetResponseCancellableAsync(request, cancellationToken);
        }
        catch (WebException ex) when (TryGetFtpStatus(ex) == FtpStatusCode.ActionNotTakenFileUnavailable)
        {
            // Directory already exists.
        }
    }

    public async Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken)
    {
        var request = CreateRequest(GetRemoteUri(relativePath), WebRequestMethods.Ftp.DeleteFile);
        using var _ = await GetResponseCancellableAsync(request, cancellationToken);
    }

    public async Task DeleteDirectoryAsync(string relativePath, CancellationToken cancellationToken)
    {
        var request = CreateRequest(GetRemoteUri(relativePath), WebRequestMethods.Ftp.RemoveDirectory);
        using var _ = await GetResponseCancellableAsync(request, cancellationToken);
    }

    private async Task TraverseDirectoryAsync(Uri directoryUri, List<FileSystemSnapshotEntry> entries, CancellationToken cancellationToken)
    {
        var lines = await ListDirectoryDetailsAsync(directoryUri, cancellationToken);
        foreach (var line in lines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var (isDirectory, name) = ParseListDirectoryDetails(line);
            if (string.IsNullOrWhiteSpace(name) || name is "." or "..")
            {
                continue;
            }

            var childUri = new Uri(directoryUri, Uri.EscapeDataString(name));
            var relativePath = Uri.UnescapeDataString(_rootUri.MakeRelativeUri(childUri).ToString()).Replace('\\', '/').Trim('/');

            if (isDirectory)
            {
                entries.Add(new FileSystemSnapshotEntry(relativePath, SyncEntryType.Directory, null, null));
                await TraverseDirectoryAsync(EnsureTrailingSlash(childUri), entries, cancellationToken);
            }
            else
            {
                var (size, lastWriteUtc) = await GetFileMetadataAsync(childUri, cancellationToken);
                entries.Add(new FileSystemSnapshotEntry(relativePath, SyncEntryType.File, size, lastWriteUtc));
            }
        }
    }

    private async Task<(long Size, DateTime LastWriteUtc)> GetFileMetadataAsync(Uri fileUri, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sizeRequest = CreateRequest(fileUri, WebRequestMethods.Ftp.GetFileSize);
        long size;
        using (var sizeResponse = await GetResponseCancellableAsync(sizeRequest, cancellationToken))
        {
            size = sizeResponse.ContentLength;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var dateRequest = CreateRequest(fileUri, WebRequestMethods.Ftp.GetDateTimestamp);
        DateTime lastWriteUtc;
        using (var dateResponse = await GetResponseCancellableAsync(dateRequest, cancellationToken))
        {
            lastWriteUtc = dateResponse.LastModified.ToUniversalTime();
        }

        return (size, lastWriteUtc);
    }

    private async Task<List<string>> ListDirectoryAsync(Uri uri, CancellationToken cancellationToken)
    {
        var request = CreateRequest(uri, WebRequestMethods.Ftp.ListDirectory);
        using var response = await GetResponseCancellableAsync(request, cancellationToken);
        using var stream = response.GetResponseStream();
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync(cancellationToken);
        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    private async Task<List<string>> ListDirectoryDetailsAsync(Uri uri, CancellationToken cancellationToken)
    {
        var request = CreateRequest(uri, WebRequestMethods.Ftp.ListDirectoryDetails);
        using var response = await GetResponseCancellableAsync(request, cancellationToken);
        using var stream = response.GetResponseStream();
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync(cancellationToken);
        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    private static (bool IsDirectory, string Name) ParseListDirectoryDetails(string line)
    {
        // Supports typical Unix-style output: drwxr-xr-x 1 owner group 0 Jan 01 00:00 dirname
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return (false, string.Empty);
        }

        var isDirectory = parts[0].StartsWith('d');
        var name = parts.Length >= 9 ? string.Join(' ', parts.Skip(8)) : parts[^1];
        return (isDirectory, name);
    }

    private FtpWebRequest CreateRequest(Uri uri, string method)
    {
        var request = (FtpWebRequest)WebRequest.Create(uri);
        request.Method = method;
        request.UseBinary = true;
        request.KeepAlive = false;
        request.Timeout = 15000;
        request.ReadWriteTimeout = 15000;
        return request;
    }

    private static async Task<FtpWebResponse> GetResponseCancellableAsync(FtpWebRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var registration = cancellationToken.Register(static state =>
        {
            try
            {
                ((FtpWebRequest)state!).Abort();
            }
            catch
            {
                // Ignored: request may already be completed/disposed.
            }
        }, request);

        try
        {
            return (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("FTP-запрос отменён.", ex, cancellationToken);
        }
        catch (WebException ex) when (ex.Status == WebExceptionStatus.RequestCanceled)
        {
            throw new OperationCanceledException("FTP-запрос отменён.", ex, cancellationToken);
        }
    }

    private static async Task<Stream> GetRequestStreamCancellableAsync(FtpWebRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var registration = cancellationToken.Register(static state =>
        {
            try
            {
                ((FtpWebRequest)state!).Abort();
            }
            catch
            {
                // Ignored: request may already be completed/disposed.
            }
        }, request);

        try
        {
            return await request.GetRequestStreamAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("FTP-запрос отменён.", ex, cancellationToken);
        }
        catch (WebException ex) when (ex.Status == WebExceptionStatus.RequestCanceled)
        {
            throw new OperationCanceledException("FTP-запрос отменён.", ex, cancellationToken);
        }
    }

    private Uri GetRemoteUri(string relativePath)
    {
        var normalized = NormalizePath(relativePath);
        return new Uri(_rootUri, normalized);
    }

    private static Uri EnsureTrailingSlash(Uri uri) => uri.AbsoluteUri.EndsWith('/') ? uri : new Uri(uri.AbsoluteUri + "/");
    private static string NormalizePath(string path) => path.Replace('\\', '/').TrimStart('/');
    private static string ToPlatformPath(string path) => path.Replace('/', Path.DirectorySeparatorChar);

    private static FtpStatusCode? TryGetFtpStatus(WebException ex)
        => ex.Response is FtpWebResponse ftp ? ftp.StatusCode : null;
}
