using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedFTPServerValheim
{
    internal class FtpConnect
    {
        private Uri _url { get; set; }

        private DirectoryModel _listFiles;
        public DirectoryModel ListFiles
        {
            get
            {
                if (_listFiles == null)
                    throw new Exception("Repite LoadFiles() or error in conncetion");
                return _listFiles;
            }
            private set { _listFiles = value; }
        }

        public FtpConnect(Uri url)
        {
            _url = url;
        }
        public FtpConnect(string url)
        {
            _url = new Uri(url);
        }


        public string TryConnect()
        {
            try
            {

                if (GetResponseList(_url.OriginalString) != null)
                {
                    return "Проверка успешна!";
                }
                else
                    return "Ошибка подключения к ftp...";
            }
            catch (WebException wEx)
            {
                if ((wEx.Response as FtpWebResponse).StatusCode == FtpStatusCode.NotLoggedIn)
                {
                    return "Неверный логин или пароль!";
                }
                else
                    return "Ошибка подключения к ftp..." + wEx.Message;
            }
        }

        private DirectoryModel LoadListDirectoryAndFiles(string pathDirectory)
        {
            DirectoryModel dir = new DirectoryModel(pathDirectory);
            List<string> listDirectory = GetResponseList(pathDirectory);
            List<string> dataFilesDetails = GetResponseList(pathDirectory, WebRequestMethods.Ftp.ListDirectoryDetails);

            foreach (var file in listDirectory)
            {
                foreach (var filesDet in dataFilesDetails)
                {
                    if (filesDet.EndsWith(Path.GetFileName(file)))
                    {
                        if (filesDet.StartsWith('d'))
                        {
                            dir.Directories.Add(LoadListDirectoryAndFiles(new Uri(_url, file).ToString()));
                            break;
                        }
                        else if (filesDet.StartsWith("-r"))
                        {
                            dir.Files.Add(new Uri(_url, file).ToString());
                            break;
                        }
                    }
                }
            }

            return dir;
        }

        public async Task LoadFiles(string pathSave, IProgress<int> progress)
        {
            ListFiles = LoadListDirectoryAndFiles(_url.OriginalString);
            List<string> filesAll = DirectoryModel.GetFilesInDirectoryRecursive(ListFiles);
            int count = 0;

            foreach (var file in filesAll)
            {
                await Task.Run(() => GetFileFtp(file, pathSave));
                progress.Report(++count * 100 / filesAll.Count);
            }
        }

        bool saveOverride = false;
        bool cancel = false;

        private List<string> GetResponseList(string url, string listDirectoryOrDetails = WebRequestMethods.Ftp.ListDirectory)
        {
            if (WebRequestMethods.Ftp.ListDirectory != listDirectoryOrDetails && WebRequestMethods.Ftp.ListDirectoryDetails != listDirectoryOrDetails)
                throw new Exception("Неверный метод Ftp. Передайте только ListDirectory или ListDerectoryDetails");
            FtpWebRequest ftpWeb = FtpWebRequest.Create(url) as FtpWebRequest;
            ftpWeb.Method = listDirectoryOrDetails;
            List<string> listDirectory = null;
            using (FtpWebResponse response = ftpWeb.GetResponse() as FtpWebResponse)
            {
                if (response.WelcomeMessage.Contains("230") && response.StatusCode == FtpStatusCode.OpeningData)
                {
                    using StreamReader sr = new StreamReader(response.GetResponseStream());
                    string dataFiles = sr.ReadToEnd();
                    listDirectory = dataFiles.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }
            return listDirectory;
        }
        public void GetFileFtp(string file, string pathSave)
        {
            Uri fileNameUri = new Uri(file);
            var tempPath = Path.Combine(pathSave, _url.MakeRelativeUri(fileNameUri).OriginalString.Replace("/", "\\"));
            var directoryName = Path.GetDirectoryName(tempPath);

            if (File.Exists(tempPath) && !saveOverride)
            {
                if (cancel)
                    return;
                TaskDialogButtonCollection buttons = new TaskDialogButtonCollection() { new TaskDialogButton("Да"), new TaskDialogButton("Да для всех!"), new TaskDialogButton("Нет"), new TaskDialogButton("Нет для всех!") };
                TaskDialogPage dialog = new TaskDialogPage() { Text = $"Файл {Path.GetFileName(file)} существует! Перезаписать?", Buttons = buttons };
                var result = TaskDialog.ShowDialog(dialog);
                if (result == buttons[2])
                    return;
                else if (result == buttons[1])
                    saveOverride = true;
                else if (result == buttons[3])
                    cancel = true;
            }

            CreateDirRecur(directoryName);

            FtpWebRequest ftpWeb = FtpWebRequest.Create(fileNameUri) as FtpWebRequest;
            ftpWeb.Method = WebRequestMethods.Ftp.DownloadFile;

            ftpWeb.UseBinary = true;
            using FtpWebResponse response = ftpWeb.GetResponse() as FtpWebResponse;
            if (response.StatusDescription.Contains("150") && response.StatusCode == FtpStatusCode.OpeningData)
            {
                using Stream streamResponse = response.GetResponseStream();
                using FileStream writer = new FileStream(tempPath, FileMode.Create);
                int byt = streamResponse.ReadByte();
                while (byt != -1)
                {
                    writer.WriteByte((byte)byt);
                    byt = streamResponse.ReadByte();
                }
                writer.Flush();
            }

            void CreateDirRecur(string path)
            {
                if (!Directory.Exists(path))
                {
                    CreateDirRecur(Path.GetDirectoryName(path));
                    Directory.CreateDirectory(path);
                }
                else
                    return;
            }
        }

        public async Task UploadFilesBack(string pathTempDirectory, IProgress<int> progress)
        {
            List<string> filesAll = DirectoryModel.GetFilesInDirectoryRecursive(ListFiles);
            List<string> directories = DirectoryModel.GetDirectoryRecursive(ListFiles);
            foreach (var path in directories)
            {
                CreateDirectoryFtp(path);
            }

            int count = 0;
            foreach (string file in filesAll)
            {
                Uri fileNameUri = new Uri(file);
                var tempPath = Path.Combine(pathTempDirectory, _url.MakeRelativeUri(fileNameUri).OriginalString.Replace("/", "\\"));
                if (!File.Exists(tempPath))
                    throw new Exception("Не найден файл " + tempPath);
                await Task.Run(()=>UploadFileFtp(tempPath, fileNameUri));
                progress.Report(++count * 100 / filesAll.Count);
            }

        }

        private void CreateDirectoryFtp(string directoryPath)
        {
            try
            {
                FtpWebRequest ftpWeb = FtpWebRequest.Create(directoryPath) as FtpWebRequest;
                ftpWeb.Method = WebRequestMethods.Ftp.MakeDirectory;
                ftpWeb.GetResponse();
            }
            catch (WebException wEx)
            {
                if (!wEx.Message.Contains("550"))
                    throw;
            }

        }

        private void UploadFileFtp(string filePathTemp, Uri filePathFtp)
        {
            FtpWebRequest ftpWeb = FtpWebRequest.Create(filePathFtp) as FtpWebRequest;
            ftpWeb.Method = WebRequestMethods.Ftp.UploadFile;
            ftpWeb.UseBinary = true;

            using Stream streamRequest = ftpWeb.GetRequestStream();
            using FileStream reader = new FileStream(filePathTemp, FileMode.Open);
            ftpWeb.ContentLength = reader.Length;
            int bit = reader.ReadByte();
            while (bit != -1)
            {
                streamRequest.WriteByte((byte)bit);
                bit = reader.ReadByte();
            }
            streamRequest.Flush();
            streamRequest.Close();
            using FtpWebResponse response = ftpWeb.GetResponse() as FtpWebResponse;
            if (!response.StatusDescription.Contains("226") && response.StatusCode != FtpStatusCode.ClosingData)
            {
                throw new Exception("Ошибка при загрзке файла " + filePathTemp);
            }
        }
    }
}
