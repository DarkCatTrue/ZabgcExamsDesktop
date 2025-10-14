using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Data;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Models;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class PrintResultModel : INotifyPropertyChanged
    {

        private ObservableCollection<TypeOfLesson> lessons;
        private ObservableCollection<TypeOfExam> typeExams;
        private ObservableCollection<Department> departments;
        private ObservableCollection<Group> groups;
        private ObservableCollection<Teacher> teachers;
        private ObservableCollection<Audience> audiences;
        private ObservableCollection<Qualification> qualifications;
        private ObservableCollection<Discipline> disciplines;
        private ObservableCollection<Group> _filteredGroups;

        private Department _selectedDepartment;
        private Group _selectedGroup;
        private Exam _selectedExam;


        private ObservableCollection<Exam> _searchResults;
        private HashSet<Exam> _modifiedExams = new HashSet<Exam>();

        public ObservableCollection<Department> Department { get => departments; set { departments = value; OnPropertyChanged(); } }
        public ObservableCollection<Group> Group { get => groups; set { groups = value; OnPropertyChanged(); } }
        public ObservableCollection<Group> FilteredGroups { get => _filteredGroups; set { _filteredGroups = value; OnPropertyChanged(); } }
        public ObservableCollection<Teacher> Teacher { get => teachers; set { teachers = value; OnPropertyChanged(); } }
        public ObservableCollection<Audience> Audience { get => audiences; set { audiences = value; OnPropertyChanged(); } }
        public ObservableCollection<TypeOfLesson> Lesson { get => lessons; set { lessons = value; OnPropertyChanged(); } }
        public ObservableCollection<TypeOfExam> TypeExam { get => typeExams; set { typeExams = value; OnPropertyChanged(); } }
        public ObservableCollection<Qualification> Qualification { get => qualifications; set { qualifications = value; OnPropertyChanged(); } }
        public ObservableCollection<Discipline> Discipline { get => disciplines; set { disciplines = value; OnPropertyChanged(); } }
        
        public Department SelectedDepartment
        {
            get => _selectedDepartment; set { _selectedDepartment = value; OnPropertyChanged(); UpdateGroups(); }
        }
        public Group SelectedGroup
        {
            get => _selectedGroup; set { _selectedGroup = value; OnPropertyChanged(); }
        }
        public Exam SelectedExam
        {
            get => _selectedExam;
            set
            {
                _selectedExam = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Exam> SearchResults
        {
            get => _searchResults; set
            {
                _searchResults = value; foreach (var exam in _searchResults)
                {
                    exam.PropertyChanged += Exam_PropertyChanged;
                }
                OnPropertyChanged(nameof(SearchResults));
            }
        }
        private void Exam_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Exam exam)
            {
                _modifiedExams.Add(exam);
            }
        }

        public ICommand PrintResultCommand { get; set; }
        public ICommand SaveToPDFCommand { get; set; }
        public ICommand BackToSearch { get; set; }
        public ICommand DeleteRowCommand { get; set; }

        public PrintResultModel()
        {
            LoadDb();
            BackToSearch = new RelayCommand(BackToPage);
        }

        private void BackToPage(object parameter)
        {
            SearchExamPage searchExamPage = new SearchExamPage();
            SearchExamWindow.pageManager.ChangePage(searchExamPage);
        }

        private async void LoadDb()
        {
            try
            {
                using var context = new ApplicationDbContext();
                var departments = await context.Departments.ToListAsync();
                var groups = await context.Groups.ToListAsync();
                var teachers = await context.Teachers.ToListAsync();
                var audiences = await context.Audiences.ToListAsync();

                Department = new ObservableCollection<Department>(departments);
                Group = new ObservableCollection<Group>(groups);
                Teacher = new ObservableCollection<Teacher>(teachers);
                Audience = new ObservableCollection<Audience>(audiences);
                FilteredGroups = new ObservableCollection<Group>(groups);
                SearchResults = new ObservableCollection<Exam>(await context.Exams
                .Include(e => e.IdGroupNavigation)
                .ThenInclude(e => e.IdDepartmentNavigation)
                .Include(e => e.IdTeachers)
                .Include(e => e.IdAudienceNavigation)
                .Include(e => e.IdDisciplineNavigation)
                .Include(e => e.IdQualificationNavigation)
                .Include(e => e.IdTypeOfExamNavigation)
                .Include(e => e.IdTypeOfLessonNavigation)
                .AsNoTracking()
                .ToListAsync());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateGroups()
        {
            if (SelectedDepartment == null)
            {
                FilteredGroups = new ObservableCollection<Group>(Group);
            }
            else
            {
                var filtered = Group
                    .Where(g => g.IdDepartment == SelectedDepartment.IdDepartment)
                    .ToList();

                FilteredGroups = new ObservableCollection<Group>(filtered);
            }
            SelectedGroup = null;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
