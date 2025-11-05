using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ZabgcExamsDesktop.MVVM.Model
{
    public class JsonHandler
    {
        string Path = @"Jsons";
        string file = $@"Jsons\configuration.json";
        public void CheckFile()
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            if (!File.Exists(file))
            {
                CreateJson();
            }
        }
        public void CreateJson()
        {
            var connectionString = "Server=ServerName\\SQLEXPRESS;Database=DataBaseName;Trusted_Connection=True;TrustServerCertificate=true;";
            ConfigurationJson configurationJson = new ConfigurationJson(connectionString);
            string json = JsonConvert.SerializeObject(configurationJson);
            File.WriteAllText(file, json);
        }
    }
}
