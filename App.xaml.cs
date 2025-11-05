using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using ZabgcExamsDesktop.MVVM.Model;

namespace ZabgcExamsDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            JsonHandler jsonHandler = new JsonHandler();
            jsonHandler.CheckFile();
            base.OnStartup(e);
        }
    }

}
