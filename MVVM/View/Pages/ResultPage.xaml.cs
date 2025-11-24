using System.Collections.ObjectModel;
using System.Windows.Controls;
using ZabgcExamsDesktop.API.Models;
using ZabgcExamsDesktop.MVVM.ViewModel;

namespace ZabgcExamsDesktop.MVVM.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для ResultPage.xaml
    /// </summary>
    public partial class ResultPage : Page
    {
        public ResultPage()
        {
            InitializeComponent();
            DataContext = new PrintResultModel();
        }
        private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is PrintResultModel viewModel)
            {
                viewModel.SelectedGroups = new ObservableCollection<GroupDto>(
                    GroupsListBox.SelectedItems.Cast<GroupDto>());
            }
        }
    }
}
