namespace DedicatedFTPServerValheim.AppCore.Sync;

/// <summary>
/// Сервис применения двухфазной синхронизации.
/// 1) строит план изменений;
/// 2) применяет подтвержденный план с учетом направления.
/// </summary>
internal sealed class SyncService(IFtpClient ftpClient)
{
    private readonly IFtpClient _ftpClient = ftpClient;

    /// <summary>
    /// Строит dry-run план синхронизации FTP -> локальная папка.
    /// </summary>
    public async Task<SyncPlan> BuildDownloadPlanAsync(string localRootPath, CancellationToken cancellationToken)
    {
        var remoteSnapshot = await _ftpClient.GetSnapshotAsync(cancellationToken);
        var localSnapshot = BuildLocalSnapshot(localRootPath);
        return SyncPlanBuilder.BuildPlan(SyncDirection.DownloadFromRemote, remoteSnapshot, localSnapshot);
    }

    /// <summary>
    /// Строит dry-run план синхронизации локальная папка -> FTP.
    /// </summary>
    public async Task<SyncPlan> BuildUploadPlanAsync(string localRootPath, CancellationToken cancellationToken)
    {
        var remoteSnapshot = await _ftpClient.GetSnapshotAsync(cancellationToken);
        var localSnapshot = BuildLocalSnapshot(localRootPath);
        return SyncPlanBuilder.BuildPlan(SyncDirection.UploadToRemote, localSnapshot, remoteSnapshot);
    }

    /// <summary>
    /// Применяет ранее рассчитанный план синхронизации.
    /// Порядок операций подобран для безопасной обработки файлов/папок:
    /// сначала создание, затем копирование/обновление, после этого удаления.
    /// </summary>
    public async Task ApplyPlanAsync(
        SyncPlan plan,
        string localRootPath,
        IProgress<(float Overall, string Item, float ItemProgress)> progress,
        CancellationToken cancellationToken)
    {
        var total = plan.Items.Count == 0 ? 1 : plan.Items.Count;
        var completed = 0;

        foreach (var dir in plan.Items.Where(i => i.EntryType == SyncEntryType.Directory && i.Operation == SyncOperationType.Create))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (plan.Direction == SyncDirection.DownloadFromRemote)
            {
                Directory.CreateDirectory(Path.Combine(localRootPath, ToPlatformPath(dir.RelativePath)));
            }
            else
            {
                await _ftpClient.CreateDirectoryAsync(dir.RelativePath, cancellationToken);
            }

            completed++;
            progress?.Report((completed / (float)total, dir.RelativePath, 1));
        }

        foreach (var file in plan.Items.Where(i => i.EntryType == SyncEntryType.File && i.Operation != SyncOperationType.Delete))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (plan.Direction == SyncDirection.DownloadFromRemote)
            {
                await _ftpClient.DownloadFileAsync(file.RelativePath, localRootPath, cancellationToken);
            }
            else
            {
                var parent = Path.GetDirectoryName(file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                if (!string.IsNullOrWhiteSpace(parent))
                {
                    await _ftpClient.CreateDirectoryAsync(parent.Replace('\\', '/'), cancellationToken);
                }

                await _ftpClient.UploadFileAsync(file.RelativePath, localRootPath, cancellationToken);
            }

            completed++;
            progress?.Report((completed / (float)total, file.RelativePath, 1));
        }

        foreach (var fileDelete in plan.Items.Where(i => i.EntryType == SyncEntryType.File && i.Operation == SyncOperationType.Delete))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (plan.Direction == SyncDirection.DownloadFromRemote)
            {
                var localFile = Path.Combine(localRootPath, ToPlatformPath(fileDelete.RelativePath));
                if (File.Exists(localFile))
                {
                    File.Delete(localFile);
                }
            }
            else
            {
                await _ftpClient.DeleteFileAsync(fileDelete.RelativePath, cancellationToken);
            }

            completed++;
            progress?.Report((completed / (float)total, fileDelete.RelativePath, 1));
        }

        foreach (var dirDelete in plan.Items
                     .Where(i => i.EntryType == SyncEntryType.Directory && i.Operation == SyncOperationType.Delete)
                     .OrderByDescending(i => i.RelativePath.Length))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (plan.Direction == SyncDirection.DownloadFromRemote)
            {
                var localDir = Path.Combine(localRootPath, ToPlatformPath(dirDelete.RelativePath));
                if (Directory.Exists(localDir))
                {
                    Directory.Delete(localDir, recursive: true);
                }
            }
            else
            {
                await _ftpClient.DeleteDirectoryAsync(dirDelete.RelativePath, cancellationToken);
            }

            completed++;
            progress?.Report((completed / (float)total, dirDelete.RelativePath, 1));
        }
    }

    private static IReadOnlyCollection<FileSystemSnapshotEntry> BuildLocalSnapshot(string localRootPath)
    {
        if (!Directory.Exists(localRootPath))
        {
            return [];
        }

        var result = new List<FileSystemSnapshotEntry>();
        foreach (var directory in Directory.EnumerateDirectories(localRootPath, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(localRootPath, directory).Replace('\\', '/');
            result.Add(new FileSystemSnapshotEntry(rel, SyncEntryType.Directory, null, null));
        }

        foreach (var file in Directory.EnumerateFiles(localRootPath, "*", SearchOption.AllDirectories))
        {
            if (Path.GetFileName(file).Equals("StatusServer.json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var info = new FileInfo(file);
            var rel = Path.GetRelativePath(localRootPath, file).Replace('\\', '/');
            result.Add(new FileSystemSnapshotEntry(rel, SyncEntryType.File, info.Length, info.LastWriteTimeUtc));
        }

        return result;
    }

    private static string ToPlatformPath(string relativePath) => relativePath.Replace('/', Path.DirectorySeparatorChar);
}
