using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class SearchExamModel
    {
        public ICommand CreateExamCommand { get; set; }
        public ICommand EditDataBaseCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ClearSearchCommand { get; set; }

        public SearchExamModel()
        {
            CreateExamCommand = new RelayCommand(CreateExam);
            EditDataBaseCommand = new RelayCommand(EditDataBase);
            SearchCommand = new RelayCommand(Search);
            ClearSearchCommand = new RelayCommand(ClearSearch);
        }


        public void Search()
        {

        }

        public void ClearSearch()
        {

        }

        public void EditDataBase()
        {
            Page EditDB = new DataBasePage();
            SearchExamWindow.pageManager.ChangePage(EditDB);
        }

        public void CreateExam()
        {
            var ExamWindow = new ExamWindow();
            ExamWindow.Show();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
