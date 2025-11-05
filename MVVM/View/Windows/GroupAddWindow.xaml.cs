using System.Windows;
using System.Windows.Input;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Data;
using ZabgcExamsDesktop.MVVM.ViewModel;

namespace ZabgcExamsDesktop.MVVM.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для GroupAddWindow.xaml
    /// </summary>
    public partial class GroupAddWindow : Window
    {
        public GroupAddWindow()
        {
            InitializeComponent();
            DataContext = new DbViewModel();
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
