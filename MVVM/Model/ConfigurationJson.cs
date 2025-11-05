using System.IO;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace ZabgcExamsDesktop.MVVM.Model
{
    public class ConfigurationJson
    {
        public string ConnectionString { get; set; }
       
        public ConfigurationJson(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
