using System.Windows;
using System.Windows.Input;

namespace ZabgcExamsDesktop.MVVM.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для ExamWindow.xaml
    /// </summary>
    public partial class ExamWindow : Window
    {
        public ExamWindow()
        {
            InitializeComponent();
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
