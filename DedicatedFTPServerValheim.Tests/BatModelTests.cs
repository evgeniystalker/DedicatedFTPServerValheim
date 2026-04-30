namespace DedicatedFTPServerValheim.Tests;

public sealed class BatModelTests
{
    [Fact]
    public void LoadBatSettings_ParsesKeyParameters()
    {
        var root = CreateTempDir();
        var batPath = Path.Combine(root, "start_server.bat");
        File.WriteAllText(
            batPath,
            """
            @echo off
            valheim_server -name "MySrv" -world "Dedicated" -password "secret" -port 2456 -nographics -batchmode -crossplay
            """);

        var bat = new BatModel("temp_start.bat") { savedirParameter = root };
        var model = bat.LoadBatSettings(batPath);

        Assert.True(model.HasValue);
        Assert.Equal("MySrv", model.Value.Name);
        Assert.Equal("Dedicated", model.Value.World);
        Assert.Equal(2456, model.Value.Port);
    }

    [Fact]
    public void CreateTempBat_CreatesUpdatedScript()
    {
        var root = CreateTempDir();
        var batPath = Path.Combine(root, "start_server.bat");
        File.WriteAllText(
            batPath,
            """
            @echo off
            valheim_server -name "OldSrv" -world "OldWorld" -password "secret" -port 2456 -nographics -batchmode -crossplay
            """);

        var bat = new BatModel("temp_start.bat") { savedirParameter = root };
        bat.LoadBatSettings(batPath);
        bat.SettingBatModel = bat.SettingBatModel!.Value with { Name = "NewSrv", World = "NewWorld", Port = 2460 };

        var tempPath = bat.CreateTempBat();
        var content = File.ReadAllText(tempPath);

        Assert.Contains("NewSrv", content);
        Assert.Contains("NewWorld", content);
        Assert.Contains("2460", content);
    }

    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "DedicatedFTPServerValheim.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}
