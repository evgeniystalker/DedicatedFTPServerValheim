using System.Collections.Specialized;
using System.Text;

namespace DedicatedFTPServerValheim
{
    internal class BatModel
    {
        string TempNameBat { get; set; }
        public string TempBatPath { get; set; }
        string SourceBatPath { get; set; }

        public string savedirParameter { get; set; }
        public SettingModel? SettingBatModel { get; set; }
        string Body { get; set; }

        public BatModel(string name)
        {
            TempNameBat = name;
        }

        public SettingModel? LoadBatSettings(string pathBat)
        {
            try
            {
                this.SourceBatPath = pathBat;
                using TextReader sr = new StreamReader(pathBat);
                Body = sr.ReadToEnd();

                string name = LoadParameter<string>("-name");
                string world = LoadParameter<string>("-world");
                string password = LoadParameter<string>("-password");
                int port = LoadParameter<int>("-port");
                bool nograph = LoadParameter<bool>("-nographics");
                bool batchmode = LoadParameter<bool>("-batchmode");
                bool crossplay = LoadParameter<bool>("-crossplay");

                this.SettingBatModel = new SettingModel(name, port, world, password, nograph, batchmode, crossplay, savedirParameter);
                return this.SettingBatModel;
            }
            catch (Exception)
            {
                return null;
            }

        }
        public SettingModel? LoadPropertySettings(string pathBat, StringCollection paramsBat)
        {
            this.SourceBatPath = pathBat;
            using TextReader sr = new StreamReader(pathBat);
            Body = sr.ReadToEnd();
            if (paramsBat != null)
                try
                {
                    string name = paramsBat[0];
                    string world = paramsBat[1];
                    string password = paramsBat[2];
                    int.TryParse(paramsBat[3], out int port);
                    bool.TryParse(paramsBat[4], out bool nograph);
                    bool.TryParse(paramsBat[5], out bool batchmode);
                    bool.TryParse(paramsBat[6], out bool crossplay);

                    this.SettingBatModel = new SettingModel(name, port, world, password, nograph, batchmode, crossplay, savedirParameter);
                    return this.SettingBatModel;
                }
                catch (Exception)
                {
                    return null;
                }
            else
                return null;
        }

        public string CreateTempBat()
        {
            if (!SettingBatModel.HasValue)
                throw new Exception("Ошибка. Нет параметоров!");

            var sett = this.SettingBatModel.Value;
            foreach (var param in SettingModel.GetNameAllParams())
            {
                if (SettingModel.GetNamesParams<bool>().Contains(param))
                {
                    UpdateParameter<bool>(param);
                }
                else if (SettingModel.GetNamesParams<string>().Contains(param))
                {
                    UpdateParameter<string>(param);
                }
                else if (SettingModel.GetNamesParams<int>().Contains(param))
                {
                    UpdateParameter<int>(param);
                }
            }

            string newBatPath = Path.Combine(Path.GetDirectoryName(SourceBatPath), this.TempNameBat);
            using StreamWriter wr = new StreamWriter(newBatPath);
            wr.Write(Body);
            wr.Flush();
            this.TempBatPath = newBatPath;
            return newBatPath;
        }

        private T LoadParameter<T>(string param)
        {
            var indParameter = Body.ToLower().IndexOf(param.ToLower());
            bool HasParameterInBat = indParameter != -1;

            if (HasParameterInBat && typeof(T) == typeof(bool) && SettingModel.GetNamesParams<T>().Contains(param))
            {
                return (T)(object)true;
            }
            else if (HasParameterInBat && typeof(T) == typeof(string) && SettingModel.GetNamesParams<T>().Contains(param))
            {
                var indStart = Body.IndexOf("\"", indParameter) + 1;
                var indEnd = Body.IndexOf("\"", indStart);
                var value = Body.Substring(indStart, indEnd - indStart);
                return (T)(object)value;
            }
            else if (HasParameterInBat && typeof(T) == typeof(int) && SettingModel.GetNamesParams<T>().Contains(param))
            {
                var indStart = Body.IndexOf(" ", indParameter) + 1;
                var indEnd = Body.IndexOf(" ", indStart);
                if (indEnd - indStart < 4 || indEnd - indStart > 6)
                    throw new Exception("Неверный порт");
                var value = Body.Substring(indStart, indEnd - indStart);
                int.TryParse(value, out int valueint);
                return (T)(object)valueint;
            }
            else
            {
                var ind = Body.LastIndexOf("valheim_server");
                if (ind == -1)
                    throw new Exception("Не верный файл bat!");
                return default(T);
            }
        }

        private void UpdateParameter<T>(string param)
        {
            SettingModel sett = SettingBatModel.Value;
            int indParam = Body.ToLower().IndexOf(param.ToLower());
            bool paramHasBat = indParam != -1;
            if (paramHasBat)
            {
                if (typeof(T) == typeof(bool) && SettingModel.GetNamesParams<T>().Contains(param))
                {
                    var value = sett.GetParamValue<bool>(param);
                    if (!value)
                    {
                        RemoveParamInBat<T>(param, indParam);
                    }
                }
                else if (typeof(T) == typeof(string) && SettingModel.GetNamesParams<T>().Contains(param))
                {
                    var value = sett.GetParamValue<string>(param);
                    if (value == null)
                    {
                        RemoveParamInBat<T>(param, indParam);
                    }
                    else
                    {
                        ReplaceParameterInBat(param, indParam, value);
                    }

                }
                else if (typeof(T) == typeof(int) && SettingModel.GetNamesParams<T>().Contains(param))
                {
                    var value = sett.GetParamValue<int>(param);
                    if (value == 0)
                    {
                        RemoveParamInBat<T>(param, indParam);
                    }
                    else
                    {
                        ReplaceParameterInBat(param, indParam, value);
                    }
                }
                else
                {
                    throw new Exception("Неверный тип параметра. Только String || Bool || Int");
                }
            }
            else
            {
                if (typeof(T) == typeof(bool) && SettingModel.GetNamesParams<T>().Contains(param))
                {
                    var value = sett.GetParamValue<bool>(param);
                    if (value)
                    {
                        AddParameterInBat(param, indParam, value);
                    }
                }
                else if (typeof(T) == typeof(string) && SettingModel.GetNamesParams<T>().Contains(param))
                {
                    var value = sett.GetParamValue<string>(param);
                    if (value != null)
                    {
                        AddParameterInBat(param, indParam, value);
                    }
                }
                else if (typeof(T) == typeof(int) && SettingModel.GetNamesParams<T>().Contains(param))
                {
                    var value = sett.GetParamValue<int>(param);
                    if (value != 0)
                    {
                        AddParameterInBat(param, indParam, value);
                    }
                }
                else
                {
                    throw new Exception("Неверный тип параметра. Только String || Bool || Int");
                }
            }

        }
        private void RemoveParamInBat<T>(string param, int indParam)
        {
            if (indParam != -1)
                if (typeof(T) == typeof(bool) && SettingModel.GetNamesParams<T>().Contains(param))
                {
                    Body = Body.Remove(indParam, param.Length);
                }
                else if ((typeof(T) == typeof(string) && SettingModel.GetNamesParams<T>().Contains(param)) || typeof(T) == typeof(int) && SettingModel.GetNamesParams<T>().Contains(param))
                {
                    var indStart = Body.IndexOf("\"", indParam) + 1;
                    var indEnd = Body.IndexOf("\"", indStart);
                    Body = Body.Remove(indParam, indEnd - indParam);
                }
                else { throw new Exception("Неверный тип параметра"); }
            else
            { throw new Exception("Параметр не существует!"); }

        }
        private void AddParameterInBat<T>(string param, int indParam, T value)
        {
            if (indParam == -1)
                if (typeof(T) == typeof(bool) && SettingModel.GetNamesParams<T>().Contains(param))
                {
                    var ind = Body.LastIndexOf("valheim_server");
                    var lastInd = ind + "valheim_server".Length;
                    Body = Body.Insert(lastInd, $" {param}");
                }
                else if ((typeof(T) == typeof(string) && SettingModel.GetNamesParams<T>().Contains(param)) || typeof(T) == typeof(int) && SettingModel.GetNamesParams<T>().Contains(param))
                {
                    var ind = Body.LastIndexOf("valheim_server");
                    var lastInd = ind + "valheim_server".Length;
                    Body = value is string ? Body.Insert(lastInd, $" {param} \"{value}\"") : Body.Insert(lastInd, $" {param} {value}");
                }
                else { throw new Exception("Неверный тип параметра"); }
            else
            { throw new Exception("Параметр уже существует!"); }
        }
        private void ReplaceParameterInBat<T>(string param, int indParam, T value)
        {
            if (indParam != -1)
                if ((typeof(T) == typeof(string) && SettingModel.GetNamesParams<T>().Contains(param)) || typeof(T) == typeof(int) && SettingModel.GetNamesParams<T>().Contains(param))
                {
                    var indStart = Body.IndexOf(typeof(T) == typeof(string) ? "\"" : " ", indParam) + 1;
                    var indEnd = Body.IndexOf(typeof(T) == typeof(string) ? "\"" : " ", indStart);

                    if (typeof(T) == typeof(int) && (indEnd - indStart < 4 || indEnd - indStart > 6))
                        throw new Exception("Неверный порт");

                    var oldppath = Body.Substring(indStart, indEnd - indStart);
                    if (oldppath != value.ToString())
                        Body = Body.Replace(oldppath, value.ToString());
                }
                else { throw new Exception("Неверный тип параметра"); }
            else
            { throw new Exception("Параметр уже существует!"); }
        }
    }


    public struct SettingModel
    {
        public bool Nographics { get; set; }
        public bool Batchmode { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }
        public string World { get; set; }
        public string Password { get; set; }
        public bool Crossplay { get; set; }
        public string Savedir { get; set; }


        private static string[] intParams = new string[] { "-port" };

        private static string[] stringParams = new string[] { "-name", "-world", "-password", "-savedir" };

        private static string[] boolParams = new string[] { "-nographics", "-batchmode", "-crossplay" };
        public SettingModel()
        {
            Nographics = true;
            Batchmode = true; ;
            Name = "My server";
            Port = 2456;
            World = "Dedicated";
            Password = "secret";
            Crossplay = true;
            Savedir = null;
        }
        public SettingModel(string name, int port, string world, string password, bool nographics = true, bool batchmode = true, bool crossplay = true, string savedir = null)
        {
            Nographics = nographics;
            Batchmode = batchmode;
            Name = name;
            Port = port;
            World = world;
            Password = password;
            Crossplay = crossplay;
            Savedir = savedir;
        }
        public static string[] GetNameAllParams()
        {
            return intParams.Concat(stringParams).Concat(boolParams).ToArray(); ;
        }
        public static string[] GetNamesParams<T>()
        {
            if (typeof(bool) == typeof(T))
                return boolParams;
            else if (typeof(string) == typeof(T))
                return stringParams;
            else if (typeof(int) == typeof(T))
                return intParams;
            else throw new Exception("Invalid type params");
        }

        public T GetParamValue<T>(string name)
        {
            if (name.Contains("-"))
                name = name.Substring(1);

            name = char.ToUpper(name[0]) + name.Substring(1);
            return (T)typeof(SettingModel).GetProperty(name).GetValue(this);
        }
    }
}
