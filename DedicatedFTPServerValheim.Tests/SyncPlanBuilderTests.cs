using DedicatedFTPServerValheim.AppCore.Sync;

namespace DedicatedFTPServerValheim.Tests;

public sealed class SyncPlanBuilderTests
{
    [Fact]
    public void BuildPlan_CreatesUpdatesAndDeletes()
    {
        var source = new[]
        {
            new FileSystemSnapshotEntry("a.txt", SyncEntryType.File, 10, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            new FileSystemSnapshotEntry("new.txt", SyncEntryType.File, 20, new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)),
            new FileSystemSnapshotEntry("sub", SyncEntryType.Directory, null, null),
        };
        var target = new[]
        {
            new FileSystemSnapshotEntry("a.txt", SyncEntryType.File, 9, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            new FileSystemSnapshotEntry("old.txt", SyncEntryType.File, 5, new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)),
        };

        var plan = SyncPlanBuilder.BuildPlan(SyncDirection.UploadToRemote, source, target);

        Assert.Equal(2, plan.CreateCount); // new.txt + sub
        Assert.Equal(1, plan.UpdateCount); // a.txt
        Assert.Equal(1, plan.DeleteCount); // old.txt
    }
}
