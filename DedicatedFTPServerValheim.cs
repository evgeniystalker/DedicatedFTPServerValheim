using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;

namespace DedicatedFTPServerValheim
{
    public partial class DedicatedFTPServerValheim : Form
    {
        /// <summary>
        /// Общий прогресс, имя файла, прогресс файла
        /// </summary>
        Progress<(float, string, float)> progressFileLoading;

        internal BatModel Bat { get; set; }

        Process HandleBatCmd { get; set; }

        CancellationTokenSource tokenSource;

        FtpConnect Ftp;
        ServerStatus status;
        Task statusTask;

        public DedicatedFTPServerValheim()
        {
            InitializeComponent();
            pathBat.Text = Properties.Settings.Default.PathBat;
            pathTemp.Text = Properties.Settings.Default.PathTemp;
            pathFTP.Text = Properties.Settings.Default.PathFTP;

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

        private void buttonPathBat_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Файлы(*.bat)|*.bat";
            dialog.InitialDirectory = Path.GetDirectoryName(Properties.Settings.Default.PathBat);
            dialog.RestoreDirectory = true;
            dialog.Title = "Выбор .bat файла";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pathBat.Clear();
                pathBat.Text = dialog.FileName;
                Properties.Settings.Default.PathBat = dialog.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void buttonPathTemp_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Выберите папку для сохранения временных файлов.";
            dialog.ShowNewFolderButton = true;
            dialog.InitialDirectory = Properties.Settings.Default.PathTemp;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pathTemp.Clear();
                pathTemp.Text = dialog.SelectedPath;
                Properties.Settings.Default.PathTemp = dialog.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        private async void ButtonStart_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            StateUnactive();

            var invChars = Path.GetInvalidPathChars();

            if (!CheckPathBat(invChars))
                return;
            else if (!CheckPathTemp(invChars))
                return;
            else if (!CheckPathFTP(invChars))
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

            Ftp = new FtpConnect(pathFTP.Text);
            Ftp.DateTimeChanged += (data) => { fileInfoDateTime.Text = $"Дата изменения\r\nфайлов на FTP:\r\n{data}"; fileInfoDateTime.Visible = true; };
            Ftp.SaveOwerrideShowWindow += dialog => this.Invoke(() => TaskDialog.ShowDialog(this, dialog));

            try
            {
                status = new ServerStatus(DateTime.Now, Bat.SettingBatModel.Value.World, ServerStatus.StatusCodeServer.Creating, Dns.GetHostAddresses(Dns.GetHostName())[0].MapToIPv4().ToString(), Bat.SettingBatModel.Value.Name, Environment.MachineName + "\\" + Environment.UserName, pathFTP.Text);
                string runingServers = await status.ServerCheckRunningAsync();
                if (runingServers != string.Empty && MessageBox.Show(this, runingServers + "Запустить свой мир?", "Внимание!", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                { StateStopServer(sendStatus: false); return; }
                else if (cleanFilesCheckBox.Checked)
                {
                    DialogResult dresult = MessageBox.Show(this, "Удалить все файлы в временной папке?", "Внимание!", MessageBoxButtons.OKCancel);
                    if (dresult == DialogResult.OK)
                        await Ftp.DeleteLocalFiles(pathTemp.Text, progressFileLoading, ctFileDownload);
                    else if (dresult == DialogResult.Cancel)
                    { StateStopServer(sendStatus: false); return; }
                }

                statusTask = status.CheckInAsync();

                await Ftp.LoadFiles(pathTemp.Text, progressFileLoading, ctFileDownload);
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

            if (!File.Exists(Path.Combine(pathTemp.Text, "worlds_local", Bat.SettingBatModel?.World + ".db")))
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
            pathTemp.Enabled = pathFTP.Enabled = pathBat.Enabled = buttonPathBat.Enabled = buttonPathTemp.Enabled = ButtonStart.Enabled = buttonUpload.Enabled = false;
        }
        private void StateActive()
        {
            pathTemp.Enabled = pathFTP.Enabled = pathBat.Enabled = buttonPathBat.Enabled = buttonPathTemp.Enabled = ButtonStart.Enabled = buttonUpload.Enabled = true;
            fileInfoDateTime.Visible = false;
            fileInfoDateTime.Text = "Дата изменения\r\nфайлов на FTP:\r\n";
            StateFileDownloadComlite();
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
                status.StatusCode = ServerStatus.StatusCodeServer.Stop;
                statusTask = status.CheckInAsync();
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
                                await Ftp.DeleteFilesFTP(progressFileLoading, tokenSource.Token);
                            else if (dresult == DialogResult.Cancel)
                                tokenSource.Cancel();
                        }
                        await Ftp.UploadFilesBack(pathTemp.Text, progressFileLoading, tokenSource.Token);
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
            if (!CheckPathFTP())
                return;

            Properties.Settings.Default.Save();
            FtpConnect ftp = new FtpConnect(pathFTP.Text);
            MessageBox.Show(this, ftp.TryConnect());
        }

        private void DedicatedFTPServerValheim_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisplayPostion = this.DesktopLocation;
        }

        private void pathFTP_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PathFTP = pathFTP.Text;
        }

        private void pathTemp_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PathTemp = pathTemp.Text;
            if (Bat is not null)
                Bat.savedirParameter = pathTemp.Text;
            if (!Directory.Exists(pathTemp.Text) || pathTemp.Text.IndexOfAny(Path.GetInvalidPathChars()) > 0)
                buttonUpload.Enabled = false;
            else
                buttonUpload.Enabled = true;
        }

        private void pathBat_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PathBat = pathBat.Text;
            if (File.Exists(pathBat.Text) && Path.GetExtension(pathBat.Text) == ".bat")
            {
                Bat = new BatModel("StartDedicatedServerFromFtp.bat") { savedirParameter = pathTemp.Text };
                if (!Properties.Settings.Default.SaveParamFlag)
                {
                    Bat.LoadBatSettings(pathBat.Text);
                }
                else
                {
                    Bat.LoadPropertySettings(pathBat.Text, Properties.Settings.Default.ParamsBat);
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
            if (!CheckPathTemp())
                return;
            else if (!CheckPathFTP())
                return;

            Ftp = new FtpConnect(pathFTP.Text);
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
                            await Ftp.DeleteFilesFTP(progressFileLoading, tokenSource.Token);
                        else if (dresult == DialogResult.Cancel)
                            tokenSource.Cancel();
                    }

                    await Ftp.UploadFilesBack(pathTemp.Text, progressFileLoading, tokenSource.Token);
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
            if (File.Exists(Bat?.TempBatPath))
                File.Delete(Bat.TempBatPath);
            if (status.StatusCode == ServerStatus.StatusCodeServer.Run)
                try
                {
                    status.StatusCode = ServerStatus.StatusCodeServer.Stop;
                    await status.CheckInAsync();
                }
                catch (Exception)
                {
                    System.Media.SystemSounds.Beep.Play();
                }
            e.Cancel = false;
            this.FormClosing -= DedicatedFTPServerValheim_FormClosing;
            this.Close();
        }

        private async void resetJsonServerListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckPathFTP())
                return;
            try
            {
                await ServerStatus.DeleteJson(pathFTP.Text);
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("550"))
                    MessageBox.Show(this, "На сервере FTP нет json файла.", "Нет файла!");

                MessageBox.Show(this, ex.Message, "Внимание ошибка!");
            }
        }

        private bool CheckPathTemp(char[] invChars = null)
        {
            if (!Directory.Exists(pathTemp.Text) || pathTemp.Text.IndexOfAny(invChars ?? Path.GetInvalidPathChars()) > 0)
            {
                MessageBox.Show(this, "Неверный путь! PathTemp");
                StateActive();
                return false;
            }
            return true;
        }

        private bool CheckPathFTP(char[] invChars = null)
        {
            if (!Uri.IsWellFormedUriString(pathFTP.Text, UriKind.Absolute) || pathFTP.Text.IndexOfAny(invChars ?? Path.GetInvalidPathChars()) > 0)
            {
                MessageBox.Show(this, "Неверный путь! PathFTP");
                StateActive();
                return false;
            }
            else if (!pathFTP.Text.EndsWith('/'))
            {
                pathFTP.Text += '/';
            }
            return true;
        }
        private bool CheckPathBat(char[] invChars = null)
        {
            if (!File.Exists(pathBat.Text) || pathBat.Text.IndexOfAny(invChars ?? Path.GetInvalidPathChars()) > 0)
            {
                MessageBox.Show(this, "Неверный путь! PathBat");
                StateActive();
                return false;
            }
            return true;
        }

    }
}