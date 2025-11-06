using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

namespace ZabgcExamsDesktop.MVVM.Model
{
    public class JsonHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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

        public void CreateLogsFolder()
        {
            var folderName = "Logs";
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
        }

        public void CreateJson()
        {
            try
            {
                Logger.Info("Начата попытка создания json файла");
                var connectionString = "Server=ServerName\\SQLEXPRESS;Database=DataBaseName;Trusted_Connection=True;TrustServerCertificate=true;";
                ConfigurationJson configurationJson = new ConfigurationJson(connectionString);
                string json = JsonConvert.SerializeObject(configurationJson);
                File.WriteAllText(file, json);
                Logger.Info("Json файл конфигурации был успешно создан.");
            }
            catch (Exception ex)
            {
                Logger.Error($"При создании Json файла произошла ошибка : {ex}");
            }
        }
    }
}
