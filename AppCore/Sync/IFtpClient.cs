namespace DedicatedFTPServerValheim.AppCore.Sync;

internal interface IFtpClient
{
    Task<IReadOnlyCollection<FileSystemSnapshotEntry>> GetSnapshotAsync(CancellationToken cancellationToken);
    Task DownloadFileAsync(string relativePath, string localRootPath, CancellationToken cancellationToken);
    Task UploadFileAsync(string relativePath, string localRootPath, CancellationToken cancellationToken);
    Task CreateDirectoryAsync(string relativePath, CancellationToken cancellationToken);
    Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken);
    Task DeleteDirectoryAsync(string relativePath, CancellationToken cancellationToken);
    Task<string> TryConnectAsync(CancellationToken cancellationToken);
}
