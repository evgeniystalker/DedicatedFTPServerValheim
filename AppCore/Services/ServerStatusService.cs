using System.Net;

namespace DedicatedFTPServerValheim.AppCore.Services;

/// <summary>
/// Сервис работы со статусами серверов в StatusServer.json на FTP.
/// Содержит retry-логику для более надежного check-in.
/// </summary>
internal sealed class ServerStatusService(string ftpUrl)
{
    private readonly string _ftpUrl = ftpUrl;
    private const int MaxRetries = 4;

    public async Task<string> CheckRunningAsync(string world, CancellationToken cancellationToken)
    {
        var current = new ServerStatus(
            DateTime.UtcNow,
            world,
            ServerStatus.StatusCodeServer.Creating,
            Dns.GetHostAddresses(Dns.GetHostName())[0].MapToIPv4().ToString(),
            Environment.MachineName,
            $"{Environment.MachineName}\\{Environment.UserName}",
            _ftpUrl);

        return await current.ServerCheckRunningAsync();
    }

    public async Task CheckInAsync(ServerStatus status, CancellationToken cancellationToken)
    {
        ServerStatus.PathFtp = _ftpUrl;
        var attempt = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;
            try
            {
                await status.CheckInAsync();
                return;
            }
            catch when (attempt < MaxRetries)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken);
            }
        }
    }

    public Task<List<ServerStatus>> GetStatusesAsync(CancellationToken cancellationToken)
    {
        ServerStatus.PathFtp = _ftpUrl;
        return ServerStatus.GetListServersAsync();
    }

    public Task UploadStatusesAsync(List<ServerStatus> statuses, CancellationToken cancellationToken)
    {
        ServerStatus.PathFtp = _ftpUrl;
        return ServerStatus.UploadJsonListAsync(statuses);
    }

    public Task ResetAsync(CancellationToken cancellationToken)
    {
        ServerStatus.PathFtp = _ftpUrl;
        return ServerStatus.DeleteJson();
    }
}
