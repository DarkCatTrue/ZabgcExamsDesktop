using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ZabgcExamsDesktop.API.Models;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.ViewModel;

namespace ZabgcExamsDesktop.MVVM.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для TeachersAddWindow.xaml
    /// </summary>
    public partial class TeachersAddWindow : Window
    {
        public TeacherSelectionViewModel ViewModel { get; }
        public TeachersAddWindow(ObservableCollection<TeacherDto> allTeachers, string currentTeachersText, Action<List<TeacherDto>, string> onSave)
        {
            InitializeComponent();
            var page = new TeachersAddPage(allTeachers, currentTeachersText, onSave);
            page.ViewModel.SetWindow(this);
            mainFrame.Navigate(page);
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
