namespace DedicatedFTPServerValheim.AppCore;

/// <summary>
/// Автомат состояний приложения.
/// Определяет допустимые переходы и то, какие команды можно выполнить в каждом состоянии.
/// </summary>
internal sealed class AppStateMachine
{
    private static readonly Dictionary<AppState, AppState[]> AllowedTransitions = new()
    {
        [AppState.Idle] = [AppState.Validating, AppState.PlanningSync, AppState.ApplyingSyncUpload, AppState.Error],
        [AppState.Validating] = [AppState.PlanningSync, AppState.Error, AppState.Idle],
        [AppState.PlanningSync] = [AppState.ApplyingSyncDownload, AppState.ApplyingSyncUpload, AppState.Idle, AppState.Error],
        [AppState.ApplyingSyncDownload] = [AppState.PreparingStart, AppState.Idle, AppState.Error],
        [AppState.PreparingStart] = [AppState.Running, AppState.Idle, AppState.Error],
        [AppState.Running] = [AppState.Stopping, AppState.ApplyingSyncUpload, AppState.Error],
        [AppState.Stopping] = [AppState.ApplyingSyncUpload, AppState.Idle, AppState.Error],
        [AppState.ApplyingSyncUpload] = [AppState.Idle, AppState.Error],
        [AppState.Error] = [AppState.Idle, AppState.Validating]
    };

    public AppState CurrentState { get; private set; } = AppState.Idle;

    /// <summary>
    /// Пытается выполнить переход в новое состояние.
    /// Возвращает false, если переход запрещен таблицей переходов.
    /// </summary>
    public bool TryTransition(AppState nextState)
    {
        if (!AllowedTransitions.TryGetValue(CurrentState, out var states) || !states.Contains(nextState))
        {
            return false;
        }

        CurrentState = nextState;
        return true;
    }

    /// <summary>
    /// Проверяет, разрешена ли команда в текущем состоянии.
    /// Используется UI для блокировки кнопок и действий.
    /// </summary>
    public bool CanExecute(AppCommand command) => (CurrentState, command) switch
    {
        (AppState.Idle, AppCommand.Start) => true,
        (AppState.Idle, AppCommand.UploadOnly) => true,
        (AppState.Idle, AppCommand.OpenJournal) => true,
        (AppState.Idle, AppCommand.OpenSettings) => true,
        (AppState.Running, AppCommand.Stop) => true,
        (AppState.ApplyingSyncDownload, AppCommand.Cancel) => true,
        (AppState.ApplyingSyncUpload, AppCommand.Cancel) => true,
        (AppState.Stopping, AppCommand.Cancel) => true,
        (AppState.Validating, AppCommand.Cancel) => true,
        (AppState.PlanningSync, AppCommand.Cancel) => true,
        _ => false
    };
}
