namespace DedicatedFTPServerValheim.Tests;

public sealed class SettingModelTests
{
    [Fact]
    public void GetParamValue_ReadsKnownParameter()
    {
        var model = new SettingModel(name: "Srv", port: 2457, world: "WorldA", password: "pwd");
        var world = model.GetParamValue<string>("-world");
        Assert.Equal("WorldA", world);
    }

    [Fact]
    public void GetNamesParams_ReturnsExpectedGroups()
    {
        Assert.Contains("-port", SettingModel.GetNamesParams<int>());
        Assert.Contains("-name", SettingModel.GetNamesParams<string>());
        Assert.Contains("-crossplay", SettingModel.GetNamesParams<bool>());
    }
}
