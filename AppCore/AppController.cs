using DedicatedFTPServerValheim.AppCore.Services;
using DedicatedFTPServerValheim.AppCore.Sync;

namespace DedicatedFTPServerValheim.AppCore;

/// <summary>
/// Центральный orchestration-слой приложения.
/// Управляет сценариями запуска/остановки сервера и синхронизации,
/// а также публикует изменения состояния для UI.
/// </summary>
internal sealed class AppController
{
    private readonly AppStateMachine _stateMachine = new();
    private readonly ServerProcessService _processService = new();

    public AppState CurrentState => _stateMachine.CurrentState;
    public event Action<AppState> StateChanged;

    private void SetState(AppState state)
    {
        if (_stateMachine.TryTransition(state))
        {
            StateChanged?.Invoke(state);
        }
    }

    private void ForceState(AppState state)
    {
        _stateMachine.Force(state);
        StateChanged?.Invoke(state);
    }

    /// <summary>
    /// Полный сценарий старта:
    /// валидация -> планирование синхронизации -> применение плана -> запуск процесса -> запись статуса Run.
    /// </summary>
    public async Task StartAsync(
        BatModel bat,
        string ftpUrl,
        string tempDirectory,
        bool allowDelete,
        Func<SyncPlan, Task<bool>> confirmPlan,
        IProgress<(float Overall, string Item, float ItemProgress)> progress,
        CancellationToken cancellationToken)
    {
        try
        {
            SetState(AppState.Validating);
            var ftp = new FtpClientAdapter(ftpUrl);
            var syncService = new SyncService(ftp);
            var statusService = new ServerStatusService(ftpUrl);

            SetState(AppState.PlanningSync);
            var downloadPlan = await syncService.BuildDownloadPlanAsync(tempDirectory, cancellationToken);
            if (!allowDelete)
            {
                downloadPlan.Items.RemoveAll(x => x.Operation == SyncOperationType.Delete);
            }

            if (!await confirmPlan(downloadPlan))
            {
                ForceState(AppState.Idle);
                return;
            }

            SetState(AppState.ApplyingSyncDownload);
            await syncService.ApplyPlanAsync(downloadPlan, tempDirectory, progress, cancellationToken);

            SetState(AppState.PreparingStart);
            bat.savedirParameter = tempDirectory;
            bat.CreateTempBat();
            _processService.Start(bat.TempBatPath);

            var status = BuildStatus(bat, ftpUrl, ServerStatus.StatusCodeServer.Run);
            await statusService.CheckInAsync(status, cancellationToken);
            SetState(AppState.Running);
        }
        catch
        {
            _processService.Stop(force: true);
            ForceState(AppState.Idle);
            throw;
        }
    }

    /// <summary>
    /// Полный сценарий остановки:
    /// остановка процесса -> планирование обратной синхронизации -> применение плана -> запись статуса Stop.
    /// </summary>
    public async Task StopAsync(
        BatModel bat,
        string ftpUrl,
        string tempDirectory,
        bool allowDelete,
        Func<SyncPlan, Task<bool>> confirmPlan,
        IProgress<(float Overall, string Item, float ItemProgress)> progress,
        CancellationToken cancellationToken)
    {
        try
        {
            SetState(AppState.Stopping);
            _processService.Stop(force: true);
            if (File.Exists(bat.TempBatPath))
            {
                File.Delete(bat.TempBatPath);
            }

            var ftp = new FtpClientAdapter(ftpUrl);
            var syncService = new SyncService(ftp);
            var statusService = new ServerStatusService(ftpUrl);

            SetState(AppState.PlanningSync);
            var uploadPlan = await syncService.BuildUploadPlanAsync(tempDirectory, cancellationToken);
            if (!allowDelete)
            {
                uploadPlan.Items.RemoveAll(x => x.Operation == SyncOperationType.Delete);
            }

            if (await confirmPlan(uploadPlan))
            {
                SetState(AppState.ApplyingSyncUpload);
                await syncService.ApplyPlanAsync(uploadPlan, tempDirectory, progress, cancellationToken);
            }

            if (bat.SettingBatModel.HasValue)
            {
                var status = BuildStatus(bat, ftpUrl, ServerStatus.StatusCodeServer.Stop);
                await statusService.CheckInAsync(status, cancellationToken);
            }

            ForceState(AppState.Idle);
        }
        catch
        {
            ForceState(AppState.Idle);
            throw;
        }
    }

    /// <summary>
    /// Сценарий ручной выгрузки локальных файлов на FTP без запуска/остановки сервера.
    /// </summary>
    public async Task UploadOnlyAsync(
        string ftpUrl,
        string tempDirectory,
        bool allowDelete,
        Func<SyncPlan, Task<bool>> confirmPlan,
        IProgress<(float Overall, string Item, float ItemProgress)> progress,
        CancellationToken cancellationToken)
    {
        try
        {
            SetState(AppState.PlanningSync);
            var ftp = new FtpClientAdapter(ftpUrl);
            var syncService = new SyncService(ftp);
            var uploadPlan = await syncService.BuildUploadPlanAsync(tempDirectory, cancellationToken);
            if (!allowDelete)
            {
                uploadPlan.Items.RemoveAll(x => x.Operation == SyncOperationType.Delete);
            }

            if (!await confirmPlan(uploadPlan))
            {
                ForceState(AppState.Idle);
                return;
            }

            SetState(AppState.ApplyingSyncUpload);
            await syncService.ApplyPlanAsync(uploadPlan, tempDirectory, progress, cancellationToken);
            ForceState(AppState.Idle);
        }
        catch
        {
            ForceState(AppState.Idle);
            throw;
        }
    }

    public bool CanExecute(AppCommand command) => _stateMachine.CanExecute(command);

    private static ServerStatus BuildStatus(BatModel bat, string ftpUrl, ServerStatus.StatusCodeServer code)
    {
        var setting = bat.SettingBatModel ?? new SettingModel();
        return new ServerStatus(
            DateTime.Now,
            setting.World,
            code,
            System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())[0].MapToIPv4().ToString(),
            setting.Name,
            Environment.MachineName + "\\" + Environment.UserName,
            ftpUrl);
    }
}
