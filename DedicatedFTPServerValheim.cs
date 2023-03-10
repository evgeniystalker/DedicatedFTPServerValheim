using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;

namespace DedicatedFTPServerValheim
{
    public partial class DedicatedFTPServerValheim : Form
    {
        /// <summary>
        /// ????? ????????, ??? ?????, ???????? ?????
        /// </summary>
        Progress<(float, string, float)> progressFileLoading;

        internal BatModel Bat { get; set; }

        Process HandleBatCmd { get; set; }


        FtpConnect Ftp;

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

            if (!Properties.Settings.Default.DisplayPostion.IsEmpty)
                this.DesktopLocation = Properties.Settings.Default.DisplayPostion;
        }

        private void buttonPathBat_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "?????(*.bat)|*.bat";
            dialog.InitialDirectory = Path.GetDirectoryName(Properties.Settings.Default.PathBat);
            dialog.RestoreDirectory = true;
            dialog.Title = "????? .bat ?????";
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
            dialog.Description = "???????? ????? ??? ?????????? ????????? ??????.";
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

            if (!File.Exists(pathBat.Text) || pathBat.Text.IndexOfAny(invChars) > 0)
            {
                MessageBox.Show("???????? ????! PathBat");
                StateActive();
                return;
            }
            else if (!Directory.Exists(pathTemp.Text) || pathTemp.Text.IndexOfAny(invChars) > 0)
            {
                MessageBox.Show("???????? ????! PathTemp");
                StateActive();
                return;
            }
            else if (!Uri.IsWellFormedUriString(pathFTP.Text, UriKind.Absolute) || pathFTP.Text.IndexOfAny(invChars) > 0)
            {
                MessageBox.Show("???????? ????! PathFTP");
                StateActive();
                return;
            }
            else if (!pathFTP.Text.EndsWith('/'))
            {
                pathFTP.Text += '/';
            }
            StateFileDownload();

            Ftp = new FtpConnect(pathFTP.Text);
            Ftp.DateTimeChanged += (data) => { fileInfoDateTime.Text = $"???? ?????????\r\n?????? ?? FTP:\r\n{data}"; fileInfoDateTime.Visible = true; };
            Ftp.SaveOwerrideShowWindow += dialog => this.Invoke(() => TaskDialog.ShowDialog(this, dialog));
            try
            {
                await Ftp.LoadFiles(pathTemp.Text, progressFileLoading);
            }
            catch (WebException wEx)
            {
                if ((wEx.Response as FtpWebResponse).StatusCode == FtpStatusCode.NotLoggedIn)
                {
                    MessageBox.Show("???????? ????? ??? ??????!");
                    StateActive();
                    return;
                }
                else
                {
                    MessageBox.Show("?????? ??????????? ? ftp..." + wEx.Message);
                    StateActive();
                    return;
                }
            }
            await Task.Delay(1000);
            StateFileDownloadComlite();
            if (!File.Exists(Path.Combine(pathTemp.Text, "worlds_local", Bat.SettingBatModel?.World + ".db")))
                if (MessageBox.Show($"??? ?? ?????? ?? ????????? ?????. ????????? ??? ????. ??? ??????? ????? ? ?????? \"{Bat.SettingBatModel?.World}\"?", null, MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    StateActive();
                    return;
                }


            Bat.CreateTempBat();
            HandleBatCmd = new Process();
            HandleBatCmd.StartInfo.FileName = "cmd.exe";
            HandleBatCmd.StartInfo.WorkingDirectory = Path.GetDirectoryName(Bat.TempBatPath);
            HandleBatCmd.StartInfo.Arguments = $"/C \"{Bat.TempBatPath}\"";
            HandleBatCmd.Start();
        }


        private void StateUnactive()
        {
            pathTemp.Enabled = pathFTP.Enabled = pathBat.Enabled = buttonPathBat.Enabled = buttonPathTemp.Enabled = ButtonStart.Enabled = false;
        }
        private void StateActive()
        {
            pathTemp.Enabled = pathFTP.Enabled = pathBat.Enabled = buttonPathBat.Enabled = buttonPathTemp.Enabled = ButtonStart.Enabled = true;
            fileInfoDateTime.Visible = false;
            StateFileDownloadComlite();
        }
        private void StateFileDownload()
        {
            progressBarFtp.Visible = true;
            progressBarFtp.Value = 0;
            //fileInfoLoading.Text = ""; fileInfoLoading.Visible = true;
            this.Cursor = Cursors.WaitCursor;
        }
        private void StateFileDownloadComlite()
        {
            progressBarFtp.Visible = false;
            progressBarFtp.Value = 0; progressBarFtp.CustomText = "";
            //fileInfoLoading.Text = ""; fileInfoLoading.Visible = false;
            this.Cursor = Cursors.Default;
        }

        private async void ButtonStop_Click(object sender, EventArgs e)
        {
            if (Ftp is null) { return; }
            StateFileDownload();
            if (HandleBatCmd != null && !HandleBatCmd.HasExited && MessageBox.Show("?????? ?? ???????? ??????.\n????????????? ????????? ?????? ????? CTRL + C.\n?????????? ??????????????", "????????", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                HandleBatCmd?.CloseMainWindow();

            HandleBatCmd?.WaitForExit();
            HandleBatCmd?.Dispose();
            if (File.Exists(Bat?.TempBatPath))
                File.Delete(Bat.TempBatPath);
            if (Ftp?.ListFiles == null)
                MessageBox.Show("??????? ?? ???????????, ?????? ??? ? ??????!");

            if (MessageBox.Show("???????????? ????? ?? ????????", null, MessageBoxButtons.YesNo) == DialogResult.Yes)
                await Ftp.UploadFilesBack(pathTemp.Text, progressFileLoading);
            Ftp = null;
            StateActive();
        }

        private void LabelCheck_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!Uri.IsWellFormedUriString(pathFTP.Text, UriKind.Absolute) || pathFTP.Text.IndexOfAny(Path.GetInvalidPathChars()) > 0)
            {
                MessageBox.Show("???????? ????! PathFTP");
                return;
            }
            Properties.Settings.Default.Save();
            FtpConnect ftp = new FtpConnect(pathFTP.Text);
            MessageBox.Show(ftp.TryConnect());
        }


        private void pathFTP_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PathFTP = pathFTP.Text;
        }

        private void DedicatedFTPServerValheim_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisplayPostion = this.DesktopLocation;
        }

        private void pathBat_TextChanged(object sender, EventArgs e)
        {
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
                MessageBox.Show("????????? ?? .bat ?? ?????????, ???????? ???????? ????! ????? ????????? ?? ?????????.");
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

        private void pathTemp_TextChanged(object sender, EventArgs e)
        {
            if (Bat is not null)
                Bat.savedirParameter = pathTemp.Text;
        }

    }
}