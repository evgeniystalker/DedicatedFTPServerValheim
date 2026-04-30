namespace DedicatedFTPServerValheim
{
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
            return boolParams.Concat(stringParams).Concat(intParams).ToArray();
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
            if (name.Contains('-'))
                name = name.Substring(1);

            name = char.ToUpper(name[0]) + name.Substring(1);
            return (T)typeof(SettingModel).GetProperty(name).GetValue(this);
        }
        public override string ToString()
        {
            List<string> comandLine = new List<string>();
            foreach (string name in intParams)
            {
                comandLine.Add(name + " " + GetParamValue<int>(name));
            }
            foreach (string name in stringParams)
            {
                comandLine.Add(name + " " + GetParamValue<string>(name));
            }
            foreach (string name in boolParams)
            {
                comandLine.Add(name);
            }
            return string.Join(" ", comandLine);
        }
    }
}
