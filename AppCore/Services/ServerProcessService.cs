using System.Diagnostics;

namespace DedicatedFTPServerValheim.AppCore.Services;

/// <summary>
/// Управляет процессом запуска dedicated-сервера через временный bat-файл.
/// </summary>
internal sealed class ServerProcessService
{
    private Process _process;

    public bool IsRunning => _process is { HasExited: false };

    public void Start(string tempBatPath)
    {
        Stop(force: true);
        _process = new Process
        {
            StartInfo =
            {
                FileName = "cmd.exe",
                WorkingDirectory = Path.GetDirectoryName(tempBatPath),
                Arguments = $"/C \"{tempBatPath}\""
            }
        };
        _process.Start();
    }

    public void Stop(bool force)
    {
        if (_process is null)
        {
            return;
        }

        if (!_process.HasExited)
        {
            if (force)
            {
                _process.Kill(entireProcessTree: true);
            }
            else
            {
                _process.CloseMainWindow();
            }

            _process.WaitForExit(1500);
        }

        _process.Dispose();
        _process = null;
    }
}
