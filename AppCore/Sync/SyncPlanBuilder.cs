namespace DedicatedFTPServerValheim.AppCore.Sync;

/// <summary>
/// Строит план синхронизации (create/update/delete) на основе снимков source/target.
/// </summary>
internal static class SyncPlanBuilder
{
    /// <summary>
    /// Рассчитывает список операций для приведения target к source.
    /// </summary>
    public static SyncPlan BuildPlan(
        SyncDirection direction,
        IReadOnlyCollection<FileSystemSnapshotEntry> sourceEntries,
        IReadOnlyCollection<FileSystemSnapshotEntry> targetEntries)
    {
        var plan = new SyncPlan { Direction = direction };
        var sourceMap = sourceEntries.ToDictionary(x => NormalizePath(x.RelativePath), x => x, StringComparer.OrdinalIgnoreCase);
        var targetMap = targetEntries.ToDictionary(x => NormalizePath(x.RelativePath), x => x, StringComparer.OrdinalIgnoreCase);

        foreach (var source in sourceMap.Values)
        {
            if (!targetMap.TryGetValue(NormalizePath(source.RelativePath), out var target))
            {
                plan.Items.Add(new SyncItem(SyncOperationType.Create, source.EntryType, NormalizePath(source.RelativePath), source.Size, source.LastWriteTimeUtc));
                continue;
            }

            if (source.EntryType == SyncEntryType.File && target.EntryType == SyncEntryType.File)
            {
                if (NeedsUpdate(source, target))
                {
                    plan.Items.Add(new SyncItem(SyncOperationType.Update, source.EntryType, NormalizePath(source.RelativePath), source.Size, source.LastWriteTimeUtc));
                }
            }
        }

        foreach (var target in targetMap.Values)
        {
            if (!sourceMap.ContainsKey(NormalizePath(target.RelativePath)))
            {
                plan.Items.Add(new SyncItem(SyncOperationType.Delete, target.EntryType, NormalizePath(target.RelativePath), target.Size, target.LastWriteTimeUtc));
            }
        }

        return plan;
    }

    /// <summary>
    /// Правило определения необходимости обновления файла:
    /// размер отличается или время изменения отличается больше, чем на 2 секунды.
    /// </summary>
    private static bool NeedsUpdate(FileSystemSnapshotEntry source, FileSystemSnapshotEntry target)
    {
        if (source.Size != target.Size)
        {
            return true;
        }

        if (!source.LastWriteTimeUtc.HasValue || !target.LastWriteTimeUtc.HasValue)
        {
            return false;
        }

        var diff = (source.LastWriteTimeUtc.Value - target.LastWriteTimeUtc.Value).Duration();
        return diff > TimeSpan.FromSeconds(2);
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/').Trim('/');
}
