using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using DedicatedFTPServerValheim.AppCore;
using DedicatedFTPServerValheim.AppCore.Services;
using DedicatedFTPServerValheim.AppCore.Sync;

namespace DedicatedFTPServerValheim
{
    public partial class DedicatedFTPServerValheim : Form
    {
        private readonly AppController _controller = new();
        private readonly Progress<(float, string, float)> _progressFileLoading;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal BatModel Bat { get; set; }

        private CancellationTokenSource _tokenSource;

        public DedicatedFTPServerValheim()
        {
            InitializeComponent();
            startFilePath.Text = String.IsNullOrEmpty(Properties.Settings.Default.StartFilePath) ? FindPathExe() : Properties.Settings.Default.StartFilePath;
            tempDirectoryPath.Text = Properties.Settings.Default.TempDirectoryPath;
            ftpUrl.Text = Properties.Settings.Default.FtpUrl;
            pathFTP_Validated(ftpUrl, EventArgs.Empty);

            _controller.StateChanged += ApplyUiState;
            ApplyUiState(_controller.CurrentState);

            _progressFileLoading = new Progress<(float, string, float)>(prog =>
            {
                progressBarFtp.CustomText = $"{prog.Item2} {(int)(prog.Item3 * 100)}%";
                progressBarFtp.Value = (int)(prog.Item1 * progressBarFtp.Maximum);
                progressBarFtp.Refresh();
            });

            foreach (var screen in Screen.AllScreens)
            {
                if (!Properties.Settings.Default.DisplayPostion.IsEmpty && screen.Bounds.Contains(Properties.Settings.Default.DisplayPostion))
                {
                    this.DesktopLocation = Properties.Settings.Default.DisplayPostion;
                    break;
                }
            }
        }

        private string FindPathExe()
        {
            var drives = DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed && x.IsReady);
            foreach (var drive in drives)
            {
                var findedDirecories = Directory.EnumerateDirectories(drive.RootDirectory.FullName, "Valheim dedicated server", new EnumerationOptions() {IgnoreInaccessible = true, RecurseSubdirectories = true, MaxRecursionDepth = 10 });
                foreach (var findedDirecory in findedDirecories)
                {
                    var findResult = Directory.EnumerateFiles(findedDirecory, "valheim_server.exe", SearchOption.TopDirectoryOnly);
                    if (findResult.Any())
                        return findResult.First();
                }
            }
            return string.Empty;
        }

        private void buttonPathStartFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Все файлы|*.bat;*.exe|Файлы(*.exe)|*.exe|Файлы(*.bat)|*.bat";
            dialog.InitialDirectory = Path.GetDirectoryName(Properties.Settings.Default.StartFilePath);
            dialog.RestoreDirectory = true;
            dialog.Title = "Выбор .bat файла";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                startFilePath.Clear();
                startFilePath.Text = dialog.FileName;
                Properties.Settings.Default.StartFilePath = dialog.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void buttonPathTemp_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Выберите папку для сохранения временных файлов.";
            dialog.ShowNewFolderButton = true;
            dialog.InitialDirectory = Properties.Settings.Default.TempDirectoryPath;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tempDirectoryPath.Clear();
                tempDirectoryPath.Text = dialog.SelectedPath;
                Properties.Settings.Default.TempDirectoryPath = dialog.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        private async void ButtonStart_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            if (!ValidateForStart())
            {
                return;
            }

            var statusService = new ServerStatusService(ftpUrl.Text);
            var running = await statusService.CheckRunningAsync(Bat.SettingBatModel.Value.World, CancellationToken.None);
            if (!string.IsNullOrWhiteSpace(running) && MessageBox.Show(this, running + "Запустить свой мир?", "Внимание!", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
            {
                return;
            }

            _tokenSource = new CancellationTokenSource();
            _tokenSource.Token.Register(() =>
            {
                fileInfoDateTime.Text = "\r\n\r\nОтмена...";
                fileInfoDateTime.Visible = true;
            });

            try
            {
                await _controller.StartAsync(
                    Bat,
                    ftpUrl.Text,
                    tempDirectoryPath.Text,
                    cleanFilesCheckBox.Checked,
                    ConfirmSyncPlanAsync,
                    _progressFileLoading,
                    _tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(this, "Операция отменена.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Ошибка запуска: " + ex.Message);
            }
            finally
            {
                _tokenSource?.Dispose();
                _tokenSource = null;
            }
        }

        private void StateActive()
        {
            ApplyUiState(AppState.Idle);
        }

        private void StateBlockedUrlFtp()
        {
            ButtonStart.Enabled = buttonUpload.Enabled = linkLabelJournal.Enabled = false;
        }

        private void StateUnBlockedUrlFtp()
        {
            ButtonStart.Enabled = buttonUpload.Enabled = linkLabelJournal.Enabled = true;
        }

        private void StateFileDownload()
        {
            progressBarFtp.Visible = true;
            progressBarFtp.Value = 0;
            this.Cursor = Cursors.WaitCursor;
            ButtonStop.Cursor = Cursors.Default;
            cleanFilesCheckBox.Cursor = Cursors.Default;
        }
        private void StateFileDownloadComlite()
        {
            progressBarFtp.Visible = false;
            progressBarFtp.Value = 0; progressBarFtp.CustomText = "";
            this.Cursor = Cursors.Default;
        }
        private async void ButtonStop_Click(object sender, EventArgs e)
        {
            if (_tokenSource is not null && !_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel();
                return;
            }

            // Без запущенного сервера Stop не должен стартовать выгрузку на FTP.
            if (_controller.CurrentState != AppState.Running || Bat is null)
            {
                return;
            }

            _tokenSource = new CancellationTokenSource();
            try
            {
                await _controller.StopAsync(
                    Bat,
                    ftpUrl.Text,
                    tempDirectoryPath.Text,
                    cleanFilesCheckBox.Checked,
                    ConfirmSyncPlanAsync,
                    _progressFileLoading,
                    _tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(this, "Остановка отменена.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Ошибка остановки: " + ex.Message);
            }
            finally
            {
                _tokenSource.Dispose();
                _tokenSource = null;
            }
        }

        private async void LabelCheck_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!CheckFtpUrl())
                return;

            Properties.Settings.Default.Save();
            var ftp = new FtpClientAdapter(ftpUrl.Text);
            MessageBox.Show(this, await ftp.TryConnectAsync(CancellationToken.None));
        }

        private void DedicatedFTPServerValheim_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisplayPostion = this.DesktopLocation;
        }

        private void pathFTP_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FtpUrl = ftpUrl.Text;
        }

        private void pathTemp_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TempDirectoryPath = tempDirectoryPath.Text;
            if (Bat is not null)
                Bat.savedirParameter = tempDirectoryPath.Text;
            if (!Directory.Exists(tempDirectoryPath.Text) || tempDirectoryPath.Text.IndexOfAny(Path.GetInvalidPathChars()) > 0)
                buttonUpload.Enabled = false;
            else
                buttonUpload.Enabled = true;
        }

        private void pathBat_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.StartFilePath = startFilePath.Text;
            if (File.Exists(startFilePath.Text) && Path.GetExtension(startFilePath.Text) == ".bat")
            {
                Bat = new BatModel("StartDedicatedServerFromFtp.bat") { savedirParameter = tempDirectoryPath.Text };
                if (!Properties.Settings.Default.SaveParamFlag)
                {
                    Bat.LoadBatSettings(startFilePath.Text);
                }
                else
                {
                    Bat.LoadPropertySettings(startFilePath.Text, Properties.Settings.Default.ParamsBat);
                }
                linkLabelSettings.Enabled = true;
            }
            else
                linkLabelSettings.Enabled = false;
        }

        private void linkLabelSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!Bat.SettingBatModel.HasValue)
            {
                MessageBox.Show(this, "Настройки из .bat не загружены, возможно неверный файл! Будут загружены по умолчанию.");
                Bat.SettingBatModel = new SettingModel();
            }
            SettingsBatForm settingBatForm = new SettingsBatForm(Bat.SettingBatModel.Value);
            settingBatForm.Owner = this;
            if (settingBatForm.ShowDialog() == DialogResult.OK && Properties.Settings.Default.SaveParamFlag)
            {
                StringCollection sett = new StringCollection
                {
                    settingBatForm.SettingModel.Name,
                    settingBatForm.SettingModel.World,
                    settingBatForm.SettingModel.Password,
                    settingBatForm.SettingModel.Port.ToString(),
                    settingBatForm.SettingModel.Nographics.ToString(),
                    settingBatForm.SettingModel.Batchmode.ToString(),
                    settingBatForm.SettingModel.Crossplay.ToString()
                };
                Properties.Settings.Default.ParamsBat = sett;
                Properties.Settings.Default.Save();
            }
        }

        private void resetSettingAplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            System.Windows.Forms.Application.Restart();
        }


        private async void buttonUpload_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            if (!CheckTempDirectoryPath() || !CheckFtpUrl())
                return;

            if (MessageBox.Show(this, "Загрузить файлы на сервер FTP?", "Внимание!", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _tokenSource = new CancellationTokenSource();
                try
                {
                    await _controller.UploadOnlyAsync(
                        ftpUrl.Text,
                        tempDirectoryPath.Text,
                        cleanFilesCheckBox.Checked,
                        ConfirmSyncPlanAsync,
                        _progressFileLoading,
                        _tokenSource.Token);
                }
                catch (OperationCanceledException) { }
                catch (Exception wEx)
                {
                    MessageBox.Show(this, "Ошибка подключения к FTP..." + wEx.Message);
                }
                finally
                {
                    _tokenSource.Dispose();
                    _tokenSource = null;
                }
            }
        }

        private void DedicatedFTPServerValheim_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (File.Exists(Bat?.TempBatPath))
                File.Delete(Bat.TempBatPath);
        }

        private async void resetJsonServerListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckFtpUrl())
                return;
            try
            {
                var statusService = new ServerStatusService(ftpUrl.Text);
                await statusService.ResetAsync(CancellationToken.None);
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("550"))
                    MessageBox.Show(this, "На сервере FTP нет json файла.", "Нет файла!");

                MessageBox.Show(this, ex.Message, "Внимание ошибка!");
            }
        }

        private bool CheckTempDirectoryPath(char[] invChars = null)
        {
            if (!Directory.Exists(tempDirectoryPath.Text) || tempDirectoryPath.Text.IndexOfAny(invChars ?? Path.GetInvalidPathChars()) > 0)
            {
                MessageBox.Show(this, "Неверный путь! PathTemp");
                StateActive();
                return false;
            }
            return true;
        }

        private bool CheckFtpUrl(char[] invChars = null)
        {
            if (!Uri.IsWellFormedUriString(ftpUrl.Text, UriKind.Absolute) || ftpUrl.Text.IndexOfAny(invChars ?? Path.GetInvalidPathChars()) > 0)
            {
                MessageBox.Show(this, "Неверный путь ссылки Ftp!");
                StateActive();
                return false;
            }
            else if (!ftpUrl.Text.EndsWith('/'))
            {
                ftpUrl.Text += '/';
            }
            return true;
        }
        private bool CheckStartFilePath(char[] invChars = null)
        {
            if (!File.Exists(startFilePath.Text) || startFilePath.Text.IndexOfAny(invChars ?? Path.GetInvalidPathChars()) > 0)
            {
                MessageBox.Show(this, "Неверный путь! Программы сервера.");
                StateActive();
                return false;
            }
            return true;
        }

        private async void linkLabelJournal_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!CheckFtpUrl())
                return;
            try
            {
                var statusService = new ServerStatusService(ftpUrl.Text);
                var serverStatusList = await statusService.GetStatusesAsync(CancellationToken.None);
                JournalGames journal = new JournalGames(serverStatusList);
                if (journal.ShowDialog(this) == DialogResult.OK)
                {
                    var list = journal.serverStatusBindingSource.DataSource as List<ServerStatus>;
                    await statusService.UploadStatusesAsync(list, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pathFTP_Validated(object sender, EventArgs e)
        {
            if (ftpUrl.Text.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
            {
                StateUnBlockedUrlFtp();
                return;
            }
            if (ftpUrl.Text.Contains("://"))
            {
                var shema = ftpUrl.Text.Substring(0, ftpUrl.Text.IndexOf("://") + 3);
                StateBlockedUrlFtp();
                MessageBox.Show($"Неверная схема ссылки на ftp ({shema}). Требуется ftp://");
                return;
            }

            ftpUrl.Text = "ftp://" + ftpUrl.Text;
            StateUnBlockedUrlFtp();
            return;
        }

        private bool ValidateForStart()
        {
            var invChars = Path.GetInvalidPathChars();
            if (!CheckStartFilePath(invChars) || !CheckTempDirectoryPath(invChars) || !CheckFtpUrl(invChars))
            {
                return false;
            }

            if (!Bat.SettingBatModel.HasValue)
            {
                linkLabelSettings_LinkClicked(this, null!);
                return Bat.SettingBatModel.HasValue;
            }

            return true;
        }

        private Task<bool> ConfirmSyncPlanAsync(SyncPlan plan)
        {
            var text =
                $"Операций: {plan.Items.Count}\n" +
                $"Создать: {plan.CreateCount}\n" +
                $"Обновить: {plan.UpdateCount}\n" +
                $"Удалить: {plan.DeleteCount}\n\n" +
                "Применить план синхронизации?";
            var result = MessageBox.Show(this, text, "План синхронизации", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return Task.FromResult(result == DialogResult.Yes);
        }

        private void ApplyUiState(AppState state)
        {
            var busy = state is not AppState.Idle and not AppState.Running;
            tempDirectoryPath.Enabled = !busy;
            ftpUrl.Enabled = !busy;
            startFilePath.Enabled = !busy;
            buttonPathBat.Enabled = !busy;
            buttonPathTemp.Enabled = !busy;
            cleanFilesCheckBox.Enabled = !busy;
            linkLabelSettings.Enabled = !busy && Bat is not null;
            linkLabelJournal.Enabled = !busy;
            LabelCheck.Enabled = !busy;
            ButtonStart.Enabled = state == AppState.Idle;
            buttonUpload.Enabled = state == AppState.Idle;
            ButtonStop.Enabled = state == AppState.Running || busy;

            if (busy)
            {
                StateFileDownload();
            }
            else
            {
                StateFileDownloadComlite();
                fileInfoDateTime.Visible = false;
                fileInfoDateTime.Text = "Дата изменения\r\nфайлов на FTP:\r\n";
            }
        }
    }
}
