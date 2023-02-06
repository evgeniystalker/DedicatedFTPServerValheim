using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Net;
using System.Security.Policy;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DedicatedFTPServerValheim
{
    public partial class DedicatedFTPServerValheim : Form
    {
        Progress<int> progressFileLoading;

        readonly string NameBat = "StartDedicatedServerFromFtp.bat";
        string tempBatPath { get; set; }
        Process HandleBat { get; set; }

        FtpConnect Ftp;
        public DedicatedFTPServerValheim()
        {
            InitializeComponent();
            pathBat.Text = Properties.Settings.Default.PathBat;
            pathFTP.Text = Properties.Settings.Default.PathFTP;
            pathTemp.Text = Properties.Settings.Default.PathTemp;
            progressFileLoading = new Progress<int>(prog =>
            {
                progressBarFtp.Value = prog;
            });
            this.DesktopLocation = Properties.Settings.Default.DisplayPostion;

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
                pathBat.Text = dialog.FileName;
                Properties.Settings.Default.PathBat = dialog.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void buttonPathTemp_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Выберите папку для сохранения временных файлов.";
            dialog.SelectedPath = Properties.Settings.Default.PathTemp;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pathTemp.Text = dialog.SelectedPath;
                Properties.Settings.Default.PathTemp = dialog.SelectedPath;
                Properties.Settings.Default.Save();
            }

        }

        private async void ButtonStart_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            StateUnactive();
            progressBarFtp.Visible = true;
            progressBarFtp.Value = 0;
            var invChars = Path.GetInvalidPathChars();

            if (!File.Exists(pathBat.Text) || pathBat.Text.IndexOfAny(invChars) > 0)
            {
                MessageBox.Show("Неверный путь! PathBat");
                StateActive();
                return;
            }
            else if (!Directory.Exists(pathTemp.Text) || pathTemp.Text.IndexOfAny(invChars) > 0)
            {
                MessageBox.Show("Неверный путь! PathTemp");
                StateActive();
                return;
            }
            else if (!Uri.IsWellFormedUriString(pathFTP.Text, UriKind.RelativeOrAbsolute) || pathFTP.Text.IndexOfAny(invChars) > 0)
            {
                MessageBox.Show("Неверный путь! PathFTP");
                StateActive();
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            Ftp = new FtpConnect(pathFTP.Text);
            try
            {
                await Ftp.LoadFiles(pathTemp.Text, progressFileLoading);
            }
            catch (WebException wEx)
            {
                if ((wEx.Response as FtpWebResponse).StatusCode == FtpStatusCode.NotLoggedIn)
                {
                    MessageBox.Show("Неверный логин или пароль!");
                    StateActive();
                    return;
                }
                else
                {
                    MessageBox.Show("Ошибка подключения к ftp..." + wEx.Message);
                    StateActive();
                    return;
                }
            }
            await Task.Delay(1000);
            progressBarFtp.Visible = false; progressBarFtp.Value = 0;
            this.Cursor = Cursors.Default;
            tempBatPath = CreateTempBat(pathBat.Text, pathTemp.Text, NameBat);

            HandleBat = new Process();
            HandleBat.StartInfo.FileName = "cmd.exe";
            HandleBat.StartInfo.WorkingDirectory = Path.GetDirectoryName(tempBatPath);
            HandleBat.StartInfo.Arguments = $"/C \"{tempBatPath}\"";
            HandleBat.Start();
        }

        private string CreateTempBat(string pathBat, string pathNewTempDir, string nameBat)
        {
            using TextReader sr = new StreamReader(pathBat);
            pathNewTempDir = pathNewTempDir.Replace("\\", "/");
            string bat = sr.ReadToEnd();
            var indSave = bat.IndexOf("-savedir");
            if (indSave != -1)
            {
                var indStart = bat.IndexOf("\"", indSave) + 1;
                var indEnd = bat.IndexOf("\"", indStart);
                var oldppath = bat.Substring(indStart, indEnd - indStart);
                bat = bat.Replace(oldppath, pathNewTempDir);
            }
            else
            {
                var ind = bat.LastIndexOf("valheim_server");
                var lastInd = ind + "valheim_server".Length;
                bat = bat.Insert(lastInd, $" -savedir \"{pathNewTempDir}\"");
            }
            string newBatPath = Path.Combine(Path.GetDirectoryName(pathBat), nameBat);
            using StreamWriter wr = new StreamWriter(newBatPath);
            wr.Write(bat);
            wr.Flush();
            return newBatPath;
        }

        private void StateUnactive()
        {
            pathTemp.Enabled = pathFTP.Enabled = pathBat.Enabled = buttonPathBat.Enabled = buttonPathTemp.Enabled = ButtonStart.Enabled = false;
        }
        private void StateActive()
        {
            pathTemp.Enabled = pathFTP.Enabled = pathBat.Enabled = buttonPathBat.Enabled = buttonPathTemp.Enabled = ButtonStart.Enabled = true;
            progressBarFtp.Visible = false; progressBarFtp.Value = 0;
        }

        private async void ButtonStop_Click(object sender, EventArgs e)
        {
            if (Ftp is null) { return; }
            progressBarFtp.Visible = true; progressBarFtp.Value = 0;
            HandleBat?.CloseMainWindow();
            HandleBat?.WaitForExit();
            HandleBat?.Dispose();
            if (File.Exists(tempBatPath))
                File.Delete(tempBatPath);
            if (Ftp?.ListFiles == null)
                MessageBox.Show("Загузка не произведена, файлов нет в памяти!");

            if (MessageBox.Show("Перезаписать файлы на сервере?", null, MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            await Ftp.UploadFilesBack(pathTemp.Text, progressFileLoading);
            Ftp = null; 
            StateActive();

        }
        private void label4_Click(object sender, EventArgs e)
        {
            label4.Text = label4.Text.Replace("...", "рак)");
        }



        private void LabelCheck_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
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
            Properties.Settings.Default.Save();
        }
    }
}