using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class DataBaseModel
    {
        public ICommand BackToExamsCommand { get; set; }
        public ICommand DepartmentCommand { get; set; }
        public ICommand GroupCommand { get; set; }
        public ICommand AudienceCommand { get; set; }
        public ICommand TeacherCommand { get; set; }
        public ICommand DisciplineCommand { get; set; }
        public ICommand QualificationCommand { get; set; }
        public ICommand TypeExamCommand { get; set; }

        public DataBaseModel()
        {
            BackToExamsCommand = new RelayCommand(BackToSearch);

        }

        public void BackToSearch()
        {
            Page SearchExam = new SearchExamPage();
            SearchExamWindow.pageManager.ChangePage(SearchExam);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
