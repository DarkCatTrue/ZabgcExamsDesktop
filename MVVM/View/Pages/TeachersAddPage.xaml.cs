using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ZabgcExamsDesktop.MVVM.Model;
using ZabgcExamsDesktop.MVVM.ViewModel;

namespace ZabgcExamsDesktop.MVVM.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для TeachersAddPage.xaml
    /// </summary>
    public partial class TeachersAddPage : Page
    {
        public TeacherSelectionViewModel ViewModel { get; }
        public TeachersAddPage(ObservableCollection<TeacherDto> allTeachers, string currentTeachersText, Action<List<TeacherDto>, string> onSave)
        {
            InitializeComponent();
            var ownerWindow = Window.GetWindow(this);
            ViewModel = new TeacherSelectionViewModel(allTeachers, currentTeachersText, onSave, ownerWindow);
            DataContext = ViewModel;
        }
    }
}
