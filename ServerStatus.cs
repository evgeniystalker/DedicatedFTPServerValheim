using System.Media;
using System.Net;
using System.Text.Json;

namespace DedicatedFTPServerValheim
{
    /// <summary>
    /// Запись о состоянии одного экземпляра сервера в StatusServer.json.
    /// Используется для координации нескольких клиентов через FTP.
    /// </summary>
    public struct ServerStatus
    {
        public DateTime DateTimeStartServer { get; set; } 
        public string World { get; set; }
        public StatusCodeServer StatusCode { get; set; }
        public string Ip { get; set; }
        public string NameServer { get; set; }
        public string NameUser { get; set; }

        public static string PathFtp;

        /// <summary>
        /// Создает объект статуса сервера.
        /// </summary>
        public ServerStatus(DateTime dataStartServer, string world, StatusCodeServer statusCode, string ip, string nameServer, string nameUser, string pathFTP)
        {
            DateTimeStartServer = dataStartServer;
            World = world;
            StatusCode = statusCode;
            Ip = ip;
            NameServer = nameServer;
            NameUser = nameUser;
            PathFtp = pathFTP;
        }

        /// <summary>
        /// Добавляет или обновляет текущую запись в StatusServer.json.
        /// Выполняется с повторными попытками для повышения надежности.
        /// </summary>
        public async Task CheckInAsync()
        {
            const int maxRetries = 4;
            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    List<ServerStatus> statusList = await GetListServersAsync();
                    ServerStatus statusThis = this;
                    var findInd = statusList.FindIndex(x => x.World == statusThis.World && x.DateTimeStartServer == statusThis.DateTimeStartServer && x.Ip == statusThis.Ip && x.NameServer == statusThis.NameServer);
                    if (statusList.Count != 0 && findInd != -1 && !statusList[findInd].Equals(default(ServerStatus)))
                        statusList[findInd] = statusThis;
                    else
                        statusList.Add(statusThis);

                    await UploadJsonListAsync(statusList);
                    return;
                }
                catch when (attempt < maxRetries)
                {
                    await Task.Delay(150 * attempt);
                }
            }

            throw new WebException("Не удалось обновить статус сервера после нескольких попыток.");
        }
        /// <summary>
        /// Проверяет, есть ли активные серверы с тем же миром.
        /// Если нет совпадений, возвращает пустую строку.
        /// </summary>
        public async Task<string> ServerCheckRunningAsync()
        {
            var serverStatusList = await GetListServersAsync();
            string world = this.World;
            var listRuningServers = serverStatusList.Where(x => x.StatusCode != StatusCodeServer.Stop && x.World == world);
            if (listRuningServers.Any())
            {
                string result = $"Сервер{(listRuningServers.Count() > 1 ? "а" : null)} с миром {listRuningServers.Last().World} уже созданны:\n";
                foreach (var item in listRuningServers)
                {
                    result += $"Имя сервера: {item.NameServer}, IP адрес: {item.Ip}, статус: {item.StatusCode}, хост: {item.NameUser}, дата запуска: {item.DateTimeStartServer}\n";
                }
                return result;
            }

            return string.Empty;
        }
        /// <summary>
        /// Загружает список статусов серверов из StatusServer.json.
        /// </summary>
        public static async Task<List<ServerStatus>> GetListServersAsync()
        {
            FtpWebRequest ftpWeb = FtpWebRequest.Create($"{PathFtp}/StatusServer.json") as FtpWebRequest;
            ftpWeb.Method = WebRequestMethods.Ftp.DownloadFile;
            List<ServerStatus> resultList;
            ftpWeb.UseBinary = true;
            ftpWeb.ConnectionGroupName = "DownloadFTP";
            try
            {
                using FtpWebResponse response = await ftpWeb.GetResponseAsync() as FtpWebResponse;
                using Stream streamResponse = response.GetResponseStream();
                resultList = await JsonSerializer.DeserializeAsync<List<ServerStatus>>(streamResponse);
                streamResponse.Close();
                return resultList ?? new List<ServerStatus>();
            }
            catch (WebException ex)
            {
                if (IsNotFound(ex))
                    return new List<ServerStatus>();
                else
                    throw;
            }
        }
        /// <summary>
        /// Полностью перезаписывает StatusServer.json переданным списком.
        /// </summary>
        public static async Task UploadJsonListAsync(List<ServerStatus> list)
        {
            FtpWebRequest ftpWeb = FtpWebRequest.Create(PathFtp + "/StatusServer.json") as FtpWebRequest;
            ftpWeb.Method = WebRequestMethods.Ftp.UploadFile;
            ftpWeb.UseBinary = true;
            ftpWeb.ConnectionGroupName = "UploadFTPCheckInServer";
            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(list);
            ftpWeb.ContentLength = buffer.LongLength;

            try
            {
                using Stream streamRequest = await ftpWeb.GetRequestStreamAsync();
                await streamRequest.WriteAsync(buffer, 0, buffer.Length);
                streamRequest.Flush();
                streamRequest.Close();
                using FtpWebResponse response = ftpWeb.GetResponse() as FtpWebResponse;
                if (!response.StatusDescription.Contains("226") && response.StatusCode != FtpStatusCode.ClosingData)
                {
                    throw new WebException("Ошибка при загрузке файла отметки о запуске сервера. " + response.StatusDescription);
                }
            }
            catch (WebException)
            {
                throw;
            }
        }
        private static bool IsNotFound(WebException ex)
        {
            if (ex.Response is FtpWebResponse ftpEx)
            {
                return ftpEx.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable;
            }

            return ex.Message.Contains("550");
        }
        /// <summary>
        /// Удаляет файл StatusServer.json на FTP.
        /// </summary>
        public static async Task DeleteJson()
        {
            FtpWebRequest ftpWeb = FtpWebRequest.Create($"{PathFtp}/StatusServer.json") as FtpWebRequest;
            ftpWeb.Method = WebRequestMethods.Ftp.DeleteFile;
            FtpWebResponse response = (FtpWebResponse)await ftpWeb.GetResponseAsync();
            if (!response.StatusDescription.Contains("250") && response.StatusCode != FtpStatusCode.FileActionOK)
            {
                throw new WebException("Ошибка при удалении файла StatusServer.json");
            }
            response.Close();
        }
        public enum StatusCodeServer
        {
            Stop,
            Creating,
            Run,
            
        }
    }

}
