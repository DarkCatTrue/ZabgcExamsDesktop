using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using ZabgcExamsDesktop.MVVM.Model;

namespace ZabgcExamsDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            JsonHandler jsonHandler = new JsonHandler();
            jsonHandler.CreateLogsFolder();

            Logger.Info("==== Приложение запущено ====");

            jsonHandler.CheckFile();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Info("==== Приложение закрыто ====");
            LogManager.Shutdown();
            base.OnExit(e);
        }
    }

}
