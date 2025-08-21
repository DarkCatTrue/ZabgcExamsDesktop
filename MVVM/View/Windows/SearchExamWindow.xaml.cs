using System.Windows;
using System.Windows.Input;
using ZabgcExamsDesktop.MVVM.Model;

namespace ZabgcExamsDesktop.MVVM.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для SearchExamWindow.xaml
    /// </summary>
    public partial class SearchExamWindow : Window
    {
        static public PageManager pageManager;
        public SearchExamWindow()
        {
            InitializeComponent();
            pageManager = new PageManager(mainFrame);
        }
        private void colapseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void closeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void ToolBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
