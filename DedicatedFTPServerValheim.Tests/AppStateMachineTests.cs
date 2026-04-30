using DedicatedFTPServerValheim.AppCore;

namespace DedicatedFTPServerValheim.Tests;

public sealed class AppStateMachineTests
{
    [Fact]
    public void Idle_AllowsStartCommand()
    {
        var stateMachine = new AppStateMachine();
        Assert.True(stateMachine.CanExecute(AppCommand.Start));
    }

    [Fact]
    public void Running_AllowsOnlyStopRelatedCommands()
    {
        var stateMachine = new AppStateMachine();
        Assert.True(stateMachine.TryTransition(AppState.Validating));
        Assert.True(stateMachine.TryTransition(AppState.PlanningSync));
        Assert.True(stateMachine.TryTransition(AppState.ApplyingSyncDownload));
        Assert.True(stateMachine.TryTransition(AppState.PreparingStart));
        Assert.True(stateMachine.TryTransition(AppState.Running));

        Assert.True(stateMachine.CanExecute(AppCommand.Stop));
        Assert.False(stateMachine.CanExecute(AppCommand.Start));
        Assert.False(stateMachine.CanExecute(AppCommand.UploadOnly));
    }

    [Fact]
    public void InvalidTransition_IsRejected()
    {
        var stateMachine = new AppStateMachine();
        Assert.False(stateMachine.TryTransition(AppState.Running));
        Assert.Equal(AppState.Idle, stateMachine.CurrentState);
    }
}
