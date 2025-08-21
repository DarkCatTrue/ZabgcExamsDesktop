using System.Windows.Controls;
using ZabgcExamsDesktop.MVVM.ViewModel;

namespace ZabgcExamsDesktop.MVVM.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для SearchExamPage.xaml
    /// </summary>
    public partial class SearchExamPage : Page
    {
        public SearchExamPage()
        {
            InitializeComponent();
            DataContext = new SearchExamModel();
        }
    }
}
