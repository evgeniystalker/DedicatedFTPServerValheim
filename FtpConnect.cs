using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DedicatedFTPServerValheim
{
    internal class FtpConnect
    {
        private Uri _url { get; set; }

        private DirectoryModel _listFiles;
        public DirectoryModel dirModel
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
        public delegate void DateTimeHandler(DateTime lastDateChanging);
        public event DateTimeHandler DateTimeChanged;


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
        private DateTime GetDate(string url)
        {
            FtpWebRequest ftpWeb = FtpWebRequest.Create(url) as FtpWebRequest;
            ftpWeb.Method = WebRequestMethods.Ftp.GetDateTimestamp;
            try
            {
                using (FtpWebResponse response = ftpWeb.GetResponse() as FtpWebResponse)
                {
                    if (!response.WelcomeMessage.Contains("230") && response.StatusCode != FtpStatusCode.FileStatus)
                        throw new Exception("Неверный код ответа при запросе даты");

                    return response.LastModified.ToUniversalTime();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private long GetFileSize(string url)
        {
            FtpWebRequest ftpWeb = FtpWebRequest.Create(url) as FtpWebRequest;
            ftpWeb.Method = WebRequestMethods.Ftp.GetFileSize;
            try
            {
                using (FtpWebResponse response = ftpWeb.GetResponse() as FtpWebResponse)
                {
                    if (!response.WelcomeMessage.Contains("230") && response.StatusCode != FtpStatusCode.FileStatus)
                        throw new Exception("Неверный код ответа при запросе даты");

                    return response.ContentLength;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DirectoryModel LoadDirectoryModel(string pathDirectory)
        {
            DirectoryModel dir = new DirectoryModel(pathDirectory);
            List<string> listDirectory = GetResponseList(pathDirectory);
            List<string> dataFilesDetails = GetResponseList(pathDirectory, WebRequestMethods.Ftp.ListDirectoryDetails);
            foreach (var @object in listDirectory)
            {
                foreach (var objectDet in dataFilesDetails)
                {
                    if (objectDet.EndsWith(Path.GetFileName(@object)))
                    {
                        string objectUrl = new Uri(_url, @object).ToString();
                        if (objectDet.StartsWith('d'))
                        {
                            dir.Directories.Add(LoadDirectoryModel(objectUrl));
                            break;
                        }
                        else if (objectDet.StartsWith("-r"))
                        {
                            dir.Files.Add(new FileModel(Path.GetFileName(objectUrl), objectUrl.ToString(), GetDate(objectUrl), GetFileSize(objectUrl)));
                            break;
                        }
                    }
                }
            }

            return dir;
        }

        public async Task LoadFiles(string pathSave, IProgress<(float, string, float)> progress, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();
            dirModel = LoadDirectoryModel(_url.OriginalString);
            List<FileModel> filesAll = DirectoryModel.GetFilesInDirectoryRecursive(dirModel);
            filesAll = filesAll.Where(x => x.FileName != "StatusServer.json").ToList();
            DateTimeChanged.Invoke(filesAll.Select(x => x.DateTimeChangedFile).Max());
            int countFiles = 0;
            string fileName = "";
            Progress<float> progressOneFileLoading = new Progress<float>(prog =>
            {
                progress.Report(((prog + countFiles) / filesAll.Count, fileName, prog));
            });
            foreach (var file in filesAll)
            {
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();

                fileName = file.FileName;
                await Task.Run(() => GetFileFtp(file, pathSave, progressOneFileLoading));
                countFiles++;
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

        public event Func<TaskDialogPage, TaskDialogButton> SaveOwerrideShowWindow;
        public void GetFileFtp(FileModel file, string pathSave, IProgress<float> progressOneFile)
        {
            Uri fileNameUri = new Uri(file.FilePath);
            var tempPath = Path.Combine(pathSave, _url.MakeRelativeUri(fileNameUri).ToString().Replace("/", "\\"));
            var directoryName = Path.GetDirectoryName(tempPath);

            if (!Directory.Exists(directoryName))
                CreateDirRecur(directoryName);

            if (File.Exists(tempPath) && !saveOverride)
            {
                if (cancel)
                {
                    progressOneFile.Report(1);
                    return;
                }
                TaskDialogButtonCollection buttons = new TaskDialogButtonCollection() { new TaskDialogButton("Да"), new TaskDialogButton("Да для всех!"), new TaskDialogButton("Нет"), new TaskDialogButton("Нет для всех!"), new TaskDialogButton("Отмена") };
                FileInfo localFile = new FileInfo(tempPath);
                TaskDialogPage dialog = new TaskDialogPage() { Text = $"Файл {file.FileName} уже существует в локальной папке! Перезаписать?\n1. Дата изменения: {localFile.LastWriteTime.ToString("G")} ({localFile.Length} byte).\n2. Дата изменения: {file.DateTimeChangedFile} ({file.Length} byte).", Buttons = buttons };
                var result = SaveOwerrideShowWindow.Invoke(dialog);
                if (result == buttons[2])
                {
                    progressOneFile.Report(1);
                    return;
                }
                else if (result == buttons[1])
                    saveOverride = true;
                else if (result == buttons[3])
                {
                    cancel = true;
                    progressOneFile.Report(1);
                    return;
                }
                else if (result == buttons[4])
                {
                    throw new OperationCanceledException("Отмена операции");
                }
            }

            FtpWebRequest ftpWeb = FtpWebRequest.Create(fileNameUri) as FtpWebRequest;
            ftpWeb.Method = WebRequestMethods.Ftp.DownloadFile;

            ftpWeb.UseBinary = true;
            ftpWeb.ConnectionGroupName = "DownloadFTP";
            using FtpWebResponse response = ftpWeb.GetResponse() as FtpWebResponse;
            if (response.StatusDescription.Contains("150") && response.StatusCode == FtpStatusCode.OpeningData)
            {
                using Stream streamResponse = response.GetResponseStream();
                using FileStream writer = new FileStream(tempPath, FileMode.Create);

                var lenghtBytes = response.ContentLength;
                byte[] buffer = new byte[4096];

                int oldPrecent = 0;
                int numOfBytesRead = streamResponse.Read(buffer, 0, buffer.Length);
                long countBytes = numOfBytesRead;
                
                while (numOfBytesRead != 0)
                {
                    writer.Write(buffer, 0, numOfBytesRead);
                    numOfBytesRead = streamResponse.Read(buffer, 0, buffer.Length);
                    int precent = (int)((countBytes += numOfBytesRead) * 100 / lenghtBytes);
                  
                    if (precent != oldPrecent)
                    {
                        oldPrecent = precent;
                        progressOneFile.Report(precent / 100f);
                    }
                    
                }
                writer.Flush();
                writer.Close();
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

        public async Task UploadFilesBack(string pathTempDirectory, IProgress<(float, string, float)> progress, CancellationToken token)
        {
            List<string> TempDirectory = Directory.GetDirectories(pathTempDirectory, "*", SearchOption.AllDirectories).ToList();
            List<string> filesInTempDirectory = Directory.GetFiles(pathTempDirectory, "", SearchOption.AllDirectories).ToList();
            //FTPLISTS
            //List<string> filesAll = DirectoryModel.GetFilesInDirectoryRecursive(ListFiles);
            //List<string> directories = DirectoryModel.GetDirectoryRecursive(ListFiles);
            var directories = TempDirectory.Select(x => Path.GetRelativePath(pathTempDirectory, x)).Select(x => new Uri(_url, x).OriginalString).ToList();
            var filesAll = filesInTempDirectory.Select(x => Path.GetRelativePath(pathTempDirectory, x)).Select(x => new Uri(_url, x).OriginalString).ToList();

            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            foreach (var path in directories)
            {
                CreateDirectoryFtp(path);
            }

            int countFiles = 0;
            string fileName = "";
            Progress<float> progressOneFileUploading = new Progress<float>(prog =>
            {
                progress.Report((((prog + countFiles) / filesAll.Count), fileName, prog));
            });

            foreach (string file in filesAll)
            {
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
                fileName = Path.GetFileName(file);
                Uri fileNameUri = new Uri(file);
                var tempPath = Path.Combine(pathTempDirectory, _url.MakeRelativeUri(fileNameUri).ToString());
                if (!File.Exists(tempPath))
                    throw new Exception("Не найден файл " + tempPath);
                await Task.Run(() => UploadFileFtp(tempPath, fileNameUri, progressOneFileUploading));
                countFiles++;
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

        private void UploadFileFtp(string filePathTemp, Uri filePathFtp, IProgress<float> progressOneFile)
        {
            FtpWebRequest ftpWeb = FtpWebRequest.Create(filePathFtp) as FtpWebRequest;
            ftpWeb.Method = WebRequestMethods.Ftp.UploadFile;
            ftpWeb.UseBinary = true;
            ftpWeb.ConnectionGroupName = "UploadFTP";
            using FileStream reader = new FileStream(filePathTemp, FileMode.Open);
            var lenghtBytes = ftpWeb.ContentLength = reader.Length;

            byte[] buffer = new byte[4096];

            using Stream streamRequest = ftpWeb.GetRequestStream();
            int numOfBytesRead = reader.Read(buffer, 0, buffer.Length);
            long countBytes = numOfBytesRead;
            int oldPrecent = 0;
            while (numOfBytesRead != 0)
            {
                streamRequest.Write(buffer, 0, numOfBytesRead);
                numOfBytesRead = reader.Read(buffer, 0, buffer.Length);

                int precent = (int)((countBytes += numOfBytesRead) * 100 / lenghtBytes);
                if (precent != oldPrecent)
                {
                    oldPrecent = precent;
                    progressOneFile.Report(precent / 100f);
                }

            }
            streamRequest.Flush();
            streamRequest.Close();
            using FtpWebResponse response = ftpWeb.GetResponse() as FtpWebResponse;
            if (!response.StatusDescription.Contains("226") && response.StatusCode != FtpStatusCode.ClosingData)
            {
                throw new Exception("Ошибка при загрузке файла " + filePathTemp);
            }
        }
        public async Task DeleteFilesFTP(IProgress<(float, string, float)> progress, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();
            dirModel = LoadDirectoryModel(_url.OriginalString);
            List<FileModel> filesAll = DirectoryModel.GetFilesInDirectoryRecursive(dirModel);
            List<DirectoryModel> dirAll = DirectoryModel.GetDirectoryRecursive(dirModel);
            DateTimeChanged.Invoke(filesAll.Select(x => x.DateTimeChangedFile).Max());
            int count = 0;
            foreach (var file in filesAll)
            {
                FtpWebRequest ftpWeb = FtpWebRequest.Create(file.FilePath) as FtpWebRequest;
                ftpWeb.Method = WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse response = (FtpWebResponse)await ftpWeb.GetResponseAsync();
                if (!response.StatusDescription.Contains("250") && response.StatusCode != FtpStatusCode.FileActionOK)
                {
                    throw new Exception("Ошибка при удалении файла " + file.FilePath);
                }
                response.Close();

                progress.Report((++count / (float)(filesAll.Count + dirAll.Count), "Удалено: " + file.FileName, 1));
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
            }
            foreach (var dir in dirAll)
            {
                FtpWebRequest ftpWeb = FtpWebRequest.Create(dir.PathDirectory) as FtpWebRequest;
                ftpWeb.Method = WebRequestMethods.Ftp.RemoveDirectory;
                FtpWebResponse response = (FtpWebResponse)await ftpWeb.GetResponseAsync();
                if (!response.StatusDescription.Contains("250") && response.StatusCode != FtpStatusCode.FileActionOK)
                {
                    throw new Exception("Ошибка при удалении директории " + dir.NameDirectory);
                }
                response.Close();
                progress.Report((++count / (float)(filesAll.Count + dirAll.Count), "Удалено: " + dir.NameDirectory, 1));
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
            }
        }
        public async Task DeleteLocalFiles(string pathTempDirectory, IProgress<(float, string, float)> progress, CancellationToken ct)
        {
            List<string> TempDirectory = Directory.GetDirectories(pathTempDirectory, "*", SearchOption.AllDirectories).ToList();
            List<string> filesInTempDirectory = Directory.GetFiles(pathTempDirectory, "", SearchOption.AllDirectories).ToList();

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();
            int count = 0;

            foreach (var file in filesInTempDirectory)
            {
                await Task.Run(() => File.Delete(file));
                progress.Report((++count / (float)(TempDirectory.Count + filesInTempDirectory.Count), "Удалено: " + Path.GetFileName(file), 1));
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
            }

            foreach (var dir in TempDirectory)
            {
                await Task.Run(() => Directory.Delete(dir));
                progress.Report((++count / (float)(TempDirectory.Count + filesInTempDirectory.Count), "Удалено: " + Path.GetDirectoryName(dir), 1));
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
            }
        }
    }
}
