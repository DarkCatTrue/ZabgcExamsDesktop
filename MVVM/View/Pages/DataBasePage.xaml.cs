using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Windows.Data;
using ZabgcExamsDesktop.MVVM.ViewModel;

namespace ZabgcExamsDesktop.MVVM.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для DataBasePage.xaml
    /// </summary>
    public partial class DataBasePage : Page
    {
        public DataBasePage()
        {
            InitializeComponent();
            DataContext = new DbViewModel();
        }
    }
}
