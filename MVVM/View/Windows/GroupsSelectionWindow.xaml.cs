using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ZabgcExamsDesktop.MVVM.Model;
using ZabgcExamsDesktop.MVVM.View.Pages;

namespace ZabgcExamsDesktop.MVVM.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для GroupsSelectionWindow.xaml
    /// </summary>
    public partial class GroupsSelectionWindow : Window
    {
        public GroupsSelectionWindow(ObservableCollection<GroupDto> groups, string currentGroupsText, Action<List<GroupDto>, string> onSave)
        {
            InitializeComponent();
            var page = new SelectionGroupsPage(groups, currentGroupsText, onSave);
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
