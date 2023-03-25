using System.Media;
using System.Net;
using System.Text.Json;

namespace DedicatedFTPServerValheim
{
    public struct ServerStatus
    {
        public DateTime DateTimeStartServer { get; set; } 
        public string World { get; set; }
        public StatusCodeServer StatusCode { get; set; }
        public string Ip { get; set; }
        public string NameServer { get; set; }
        public string NameUser { get; set; }

        private string PathFtp;

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

        public async Task CheckInAsync()
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

                await UploadJsonListSync(statusList);
            }
            catch (Exception)
            {
                throw;
            }

        }
        /// <summary>
        /// If server not find return string empty
        /// </summary>
        /// <param name="pathFTP"></param>
        /// <returns></returns>
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
        public async Task<List<ServerStatus>> GetListServersAsync()
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
                return resultList;
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("550"))
                    return new List<ServerStatus>();
                else
                    throw ex;
            }
        }
        private async Task UploadJsonListSync(List<ServerStatus> list)
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
            catch (WebException ex)
            {
                throw ex;
            }
        }
        public async static Task DeleteJson(string pathFTP)
        {
            FtpWebRequest ftpWeb = FtpWebRequest.Create($"{pathFTP}/StatusServer.json") as FtpWebRequest;
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
