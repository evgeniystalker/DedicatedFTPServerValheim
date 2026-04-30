using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;
using DedicatedFTPServerValheim.States;

namespace DedicatedFTPServerValheim
{
    public partial class DedicatedFTPServerValheim : Form
    {
        /// <summary>
        /// Общий прогресс, имя файла, прогресс файла
        /// </summary>
        Progress<(float, string, float)> progressFileLoading;

        IFormState _state;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal BatModel Bat { get; set; }

        Process HandleBatCmd { get; set; }

        CancellationTokenSource tokenSource;

        FtpConnect Ftp;
        ServerStatus status;
        Task statusTask;
        Task loadingUploadingTask;

        public DedicatedFTPServerValheim()
        {
            InitializeComponent();
            startFilePath.Text = String.IsNullOrEmpty(Properties.Settings.Default.StartFilePath) ? FindPathExe() : Properties.Settings.Default.StartFilePath;
            tempDirectoryPath.Text = Properties.Settings.Default.TempDirectoryPath;
            ftpUrl.Text = Properties.Settings.Default.FtpUrl;
            pathFTP_Validated(ftpUrl, EventArgs.Empty);

            progressFileLoading = new Progress<(float, string, float)>(prog =>
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
            StateUnactive();

            var invChars = Path.GetInvalidPathChars();

            if (!CheckStartFilePath(invChars))
                return;
            else if (!CheckTempDirectoryPath(invChars))
                return;
            else if (!CheckFtpUrl(invChars))
                return;
            else if (!Bat.SettingBatModel.HasValue)
            {
                linkLabelSettings_LinkClicked(sender, null);
                StateActive();
                return;
            }

            tokenSource = new CancellationTokenSource();
            var ctFileDownload = tokenSource.Token;
            ctFileDownload.Register(() => { fileInfoDateTime.Text = "\r\n\r\nОтмена..."; fileInfoDateTime.Visible = true; });

            StateFileDownload();

            Ftp = new FtpConnect(ftpUrl.Text);
            Ftp.DateTimeChanged += (data) => { fileInfoDateTime.Text = $"Дата изменения\r\nфайлов на FTP:\r\n{data}"; fileInfoDateTime.Visible = true; };
            Ftp.SaveOverrideShowWindow += dialog => this.Invoke(() => TaskDialog.ShowDialog(this, dialog));

            try
            {
                status = new ServerStatus(DateTime.Now, Bat.SettingBatModel.Value.World, ServerStatus.StatusCodeServer.Creating, Dns.GetHostAddresses(Dns.GetHostName())[0].MapToIPv4().ToString(), Bat.SettingBatModel.Value.Name, Environment.MachineName + "\\" + Environment.UserName, ftpUrl.Text);
                string runingServers = await status.ServerCheckRunningAsync();
                if (runingServers != string.Empty && MessageBox.Show(this, runingServers + "Запустить свой мир?", "Внимание!", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                { StateStopServer(sendStatus: false); return; }
                else if (cleanFilesCheckBox.Checked)
                {
                    DialogResult dresult = MessageBox.Show(this, "Удалить все файлы в временной папке?", "Внимание!", MessageBoxButtons.OKCancel);
                    if (dresult == DialogResult.OK)
                        await (loadingUploadingTask = Ftp.DeleteLocalFiles(tempDirectoryPath.Text, progressFileLoading, ctFileDownload));
                    else if (dresult == DialogResult.Cancel)
                    { StateStopServer(sendStatus: false); return; }
                }

                statusTask = status.CheckInAsync();

                await (loadingUploadingTask = Ftp.LoadFiles(tempDirectoryPath.Text, progressFileLoading, ctFileDownload));
            }
            catch (OperationCanceledException)
            {
                StateStopServer();
                return;
            }
            catch (WebException wEx)
            {
                StateStopServer();
                if ((wEx.Response as FtpWebResponse).StatusCode == FtpStatusCode.NotLoggedIn)
                    MessageBox.Show(this, "Неверный логин или пароль!");
                else
                    MessageBox.Show(this, "Ошибка подключения к ftp..." + wEx.Message);

                return;
            }
            finally
            {
                tokenSource.Dispose();
                tokenSource = null;
                fileInfoDateTime.Visible = false;
            }

            await Task.Delay(1000);
            StateFileDownloadComlite();

            if (!File.Exists(Path.Combine(tempDirectoryPath.Text, "worlds_local", Bat.SettingBatModel?.World + ".db")))
                if (MessageBox.Show(this, $"Мир не найден во временной папке. Проверьте имя мира. Или создать новый с именем \"{Bat.SettingBatModel?.World}\"?", "Внимание!", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    StateStopServer();
                    return;
                }


            Bat.CreateTempBat();
            HandleBatCmd = new Process();
            HandleBatCmd.StartInfo.FileName = "cmd.exe";
            HandleBatCmd.StartInfo.WorkingDirectory = Path.GetDirectoryName(Bat.TempBatPath);
            HandleBatCmd.StartInfo.Arguments = $"/C \"{Bat.TempBatPath}\"";
            HandleBatCmd.Start();

            status.StatusCode = ServerStatus.StatusCodeServer.Run;
            statusTask = status.CheckInAsync();
        }

        private void StateUnactive()
        {
            tempDirectoryPath.Enabled = ftpUrl.Enabled = startFilePath.Enabled = buttonPathBat.Enabled = buttonPathTemp.Enabled = ButtonStart.Enabled = buttonUpload.Enabled = false;
        }
        private void StateActive()
        {
            tempDirectoryPath.Enabled = ftpUrl.Enabled = startFilePath.Enabled = buttonPathBat.Enabled = buttonPathTemp.Enabled = ButtonStart.Enabled = buttonUpload.Enabled = true;
            fileInfoDateTime.Visible = false;
            fileInfoDateTime.Text = "Дата изменения\r\nфайлов на FTP:\r\n";
            StateFileDownloadComlite();
        }
        private void StateBlockedUrlFtp()
        {
           
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
        private void StateStopServer(bool sendStatus = true)
        {
            if (sendStatus)
            {
                status.StatusCode = ServerStatus.StatusCodeServer.Stop;
                statusTask = status.CheckInAsync();
            }
            Ftp = null;
            StateActive();
        }

        private async void ButtonStop_Click(object sender, EventArgs e)
        {
            if (tokenSource != null && !tokenSource.IsCancellationRequested)
            {
                if (status.StatusCode != ServerStatus.StatusCodeServer.Stop)
                {
                    status.StatusCode = ServerStatus.StatusCodeServer.Stop;
                    statusTask = status.CheckInAsync();
                }
                tokenSource.Cancel();
                Ftp = null;
                return;
            }

            if (Ftp is null) { return; }
            StateFileDownload();
            if (HandleBatCmd != null && !HandleBatCmd.HasExited && MessageBox.Show(this, "Сервер не завершил работу.\nРекомендуется завершить работу через CTRL + C.\nЗвавершить принудительно?", "Внимание!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                HandleBatCmd?.CloseMainWindow();

            HandleBatCmd?.WaitForExit();
            HandleBatCmd?.Dispose();
            HandleBatCmd = null;
            if (File.Exists(Bat?.TempBatPath))
                File.Delete(Bat.TempBatPath);
            if (Ftp != null)
                if (MessageBox.Show(this, "Загрузить файлы на сервер?", null, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        tokenSource = new CancellationTokenSource();
                        tokenSource.Token.Register(() => { fileInfoDateTime.Text = "\r\n\r\nОтмена..."; fileInfoDateTime.Visible = true; });
                        if (cleanFilesCheckBox.Checked)
                        {
                            DialogResult dresult = MessageBox.Show(this, "Удалить все файлы в папке на сервере FTP?", "Внимание!", MessageBoxButtons.OKCancel);
                            if (dresult == DialogResult.OK)
                                await (loadingUploadingTask = Ftp.DeleteFilesFTP(progressFileLoading, tokenSource.Token));
                            else if (dresult == DialogResult.Cancel)
                                tokenSource.Cancel();
                        }
                        await (loadingUploadingTask = Ftp.UploadFilesBack(tempDirectoryPath.Text, progressFileLoading, tokenSource.Token));
                    }
                    catch (OperationCanceledException) { }
                    finally
                    {
                        tokenSource.Dispose();
                        tokenSource = null;
                    }
                }

            StateStopServer();
        }

        private void LabelCheck_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!CheckFtpUrl())
                return;

            Properties.Settings.Default.Save();
            FtpConnect ftp = new FtpConnect(ftpUrl.Text);
            MessageBox.Show(this, ftp.TryConnect());
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
            StateUnactive();
            if (!CheckTempDirectoryPath())
                return;
            else if (!CheckFtpUrl())
                return;

            Ftp = new FtpConnect(ftpUrl.Text);
            Ftp.DateTimeChanged += (data) => { fileInfoDateTime.Text = $"Дата изменения\r\nфайлов на FTP:\r\n{data}"; fileInfoDateTime.Visible = true; };
            StateFileDownload();
            if (MessageBox.Show(this, "Загрузить файлы на сервер FTP?", "Внимание!", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    tokenSource = new CancellationTokenSource();
                    tokenSource.Token.Register(() => { { fileInfoDateTime.Text = "\r\n\r\nОтмена..."; fileInfoDateTime.Visible = true; } });

                    if (cleanFilesCheckBox.Checked)
                    {
                        DialogResult dresult = MessageBox.Show(this, "Удалить все файлы в папке на сервере на FTP?", "Внимание!", MessageBoxButtons.OKCancel);
                        if (dresult == DialogResult.OK)
                            await (loadingUploadingTask = Ftp.DeleteFilesFTP(progressFileLoading, tokenSource.Token));
                        else if (dresult == DialogResult.Cancel)
                            tokenSource.Cancel();
                    }

                    await (loadingUploadingTask = Ftp.UploadFilesBack(tempDirectoryPath.Text, progressFileLoading, tokenSource.Token));
                }
                catch (OperationCanceledException) { }
                catch (WebException wEx)
                {
                    if ((wEx.Response as FtpWebResponse).StatusCode == FtpStatusCode.NotLoggedIn)
                    {
                        MessageBox.Show(this, "Неверный логин или пароль!");
                    }
                    else
                    {
                        MessageBox.Show(this, "Ошибка подключения к FTP..." + wEx.Message);
                    }
                }
                finally
                {
                    tokenSource.Dispose();
                    tokenSource = null;
                }
            }
            StateActive();
            Ftp = null;
        }

        private async void DedicatedFTPServerValheim_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            if (HandleBatCmd != null && !HandleBatCmd.HasExited && MessageBox.Show(this, "Сервер не завершил работу.\nРекомендуется завершить работу через CTRL + C.\nЗвавершить принудительно?", "Внимание!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                HandleBatCmd?.CloseMainWindow();

            HandleBatCmd?.WaitForExit(60 * 100);
            HandleBatCmd?.Dispose();
            HandleBatCmd = null;

            if (tokenSource != null && !tokenSource.IsCancellationRequested && MessageBox.Show(this, "Отменить загрузку?", "Внимание!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                tokenSource.Cancel();
            try
            {
                if (loadingUploadingTask != null)
                    await loadingUploadingTask.WaitAsync(new TimeSpan(0, 0, 60));
                if (statusTask != null)
                    await statusTask.WaitAsync(new TimeSpan(0, 0, 60));
            }
            catch (OperationCanceledException) { }
            catch (Exception) { System.Media.SystemSounds.Beep.Play(); }
            Ftp = null;

            if (File.Exists(Bat?.TempBatPath))
                File.Delete(Bat.TempBatPath);
            if (status.StatusCode != ServerStatus.StatusCodeServer.Stop)
                try
                {
                    status.StatusCode = ServerStatus.StatusCodeServer.Stop;
                    await (statusTask = status.CheckInAsync());
                }
                catch (Exception)
                {
                    System.Media.SystemSounds.Beep.Play();
                }
            this.FormClosing -= DedicatedFTPServerValheim_FormClosing;
            this.Close();
        }

        private async void resetJsonServerListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckFtpUrl())
                return;
            try
            {
                ServerStatus.PathFtp = ftpUrl.Text;
                await (statusTask = ServerStatus.DeleteJson());
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
                ServerStatus.PathFtp = ftpUrl.Text;
                List<ServerStatus> serverStatusList;
                serverStatusList = await ServerStatus.GetListServersAsync();
                JournalGames journal = new JournalGames(serverStatusList);
                if (journal.ShowDialog(this) == DialogResult.OK)
                {
                    var list = journal.serverStatusBindingSource.DataSource as List<ServerStatus>;
                    await (statusTask = ServerStatus.UploadJsonListAsync(list));
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
    }
}