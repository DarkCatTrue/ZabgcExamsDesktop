using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ZabgcExamsDesktop.MVVM.Model;
using ZabgcExamsDesktop.MVVM.ViewModel;

namespace ZabgcExamsDesktop.MVVM.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для SelectionGroupsPage.xaml
    /// </summary>
    public partial class SelectionGroupsPage : Page
    {
        public GroupsSelectionViewModel ViewModel { get; }
        public SelectionGroupsPage(ObservableCollection<GroupDto> groups, ObservableCollection<DepartmentDto> departments, string currentDepartmentsText, string currentGroupsText, Action<List<GroupDto>, string, string> onSave)
        {
            InitializeComponent();
            var ownerWindow = Window.GetWindow(this);
            ViewModel = new GroupsSelectionViewModel(groups, departments, currentDepartmentsText, currentGroupsText, onSave, ownerWindow);
            DataContext = ViewModel;
        }
    }
}
