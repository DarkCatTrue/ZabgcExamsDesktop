using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Windows.Data;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Data;
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
            ApplicationDbContext context = new ApplicationDbContext();
            DataContext = new DbViewModel(context);
        }
    }
}
