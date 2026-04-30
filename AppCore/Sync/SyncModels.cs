namespace DedicatedFTPServerValheim.AppCore.Sync;

/// <summary>
/// Направление синхронизации.
/// </summary>
internal enum SyncDirection
{
    DownloadFromRemote,
    UploadToRemote
}

internal enum SyncEntryType
{
    File,
    Directory
}

internal enum SyncOperationType
{
    Create,
    Update,
    Delete
}

internal sealed record SyncItem(
    SyncOperationType Operation,
    SyncEntryType EntryType,
    string RelativePath,
    long? Size = null,
    DateTime? LastWriteTimeUtc = null);

internal sealed class SyncPlan
{
    public SyncDirection Direction { get; init; }
    public List<SyncItem> Items { get; } = [];

    /// <summary>
    /// Возвращает true, если в плане присутствуют удаления.
    /// Используется перед подтверждением пользователем.
    /// </summary>
    public bool HasDeletes => Items.Any(i => i.Operation == SyncOperationType.Delete);
    public int CreateCount => Items.Count(i => i.Operation == SyncOperationType.Create);
    public int UpdateCount => Items.Count(i => i.Operation == SyncOperationType.Update);
    public int DeleteCount => Items.Count(i => i.Operation == SyncOperationType.Delete);
}

internal sealed record FileSystemSnapshotEntry(
    string RelativePath,
    SyncEntryType EntryType,
    long? Size,
    DateTime? LastWriteTimeUtc);
