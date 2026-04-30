namespace DedicatedFTPServerValheim.AppCore;

/// <summary>
/// Состояния жизненного цикла главной формы/приложения.
/// Используются для управления доступностью UI и допустимыми командами.
/// </summary>
internal enum AppState
{
    Idle,
    Validating,
    PlanningSync,
    ApplyingSyncDownload,
    PreparingStart,
    Running,
    Stopping,
    ApplyingSyncUpload,
    Error
}
