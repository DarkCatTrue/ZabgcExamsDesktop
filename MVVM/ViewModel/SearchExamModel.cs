using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Data;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Models;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class SearchExamModel : INotifyPropertyChanged
    {
        private ObservableCollection<Department> departments;
        private ObservableCollection<Group> groups;
        private ObservableCollection<Teacher> teachers;
        private ObservableCollection<Audience> audiences;
        private ObservableCollection<Group> _filteredGroups;

        private Department _selectedDepartment;
        private Group _selectedGroup;
        private Teacher _selectedTeacher;
        private Audience _selectedAudience;
        private Exam _selectedExam;
        private ObservableCollection<Exam> _searchResults;
        public ICommand CreateExamCommand { get; set; }
        public ICommand EditDataBaseCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ClearSearchCommand { get; set; }
        public ICommand SaveRowCommand { get; set; }
        public ICommand DeleteRowCommand { get; set; }
        public ICommand CreateResultCommand { get; set; }

        public ObservableCollection<Department> Department { get => departments; set { departments = value; OnPropertyChanged(); } }
        public ObservableCollection<Group> Group { get => groups; set { groups = value; OnPropertyChanged(); } }
        public ObservableCollection<Group> FilteredGroups { get => _filteredGroups; set { _filteredGroups = value; OnPropertyChanged(); } }
        public ObservableCollection<Teacher> Teacher { get => teachers; set { teachers = value; OnPropertyChanged(); } }
        public ObservableCollection<Audience> Audience { get => audiences; set { audiences = value; OnPropertyChanged(); } }

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

        private HashSet<Exam> _modifiedExams = new HashSet<Exam>();

        private void Exam_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Exam exam)
            {
                _modifiedExams.Add(exam);
            }
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

        public Department SelectedDepartment
        {
            get => _selectedDepartment; set { _selectedDepartment = value; OnPropertyChanged(); UpdateGroups(); }
        }

        public Group SelectedGroup
        {
            get => _selectedGroup; set { _selectedGroup = value; OnPropertyChanged(); }
        }
        public Teacher SelectedTeacher
        {
            get => _selectedTeacher; set { _selectedTeacher = value; OnPropertyChanged(); }
        }

        public Audience SelectedAudience
        {
            get => _selectedAudience; set { _selectedAudience = value; OnPropertyChanged(); }
        }


        public SearchExamModel()
        {
            LoadDB();
            DeleteRowCommand = new RelayCommand(DeleteExam);
            CreateExamCommand = new RelayCommand(CreateExam);
            EditDataBaseCommand = new RelayCommand(EditDataBase);
            SearchCommand = new RelayCommand(Search);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            CreateResultCommand = new RelayCommand(GoToResultPage);
        }

        public async void LoadDB()
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

        private void GoToResultPage(object parameter)
        {
            Page resultPage = new ResultPage();
            SearchExamWindow.pageManager.ChangePage(resultPage);
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

        private async void Search(object parameter)
        {
            using var context = new ApplicationDbContext();

            var query = context.Exams
                .Include(e => e.IdGroupNavigation)
                    .ThenInclude(g => g.IdDepartmentNavigation)
                .Include(e => e.IdTeachers)
                .Include(e => e.IdAudienceNavigation)
                .Include(e => e.IdDisciplineNavigation)
                .Include(e => e.IdQualificationNavigation)
                .Include(e => e.IdTypeOfExamNavigation)
                .Include(e => e.IdTypeOfLessonNavigation)
                .AsQueryable();

            if (SelectedDepartment != null)
                query = query.Where(e => e.IdGroupNavigation.IdDepartment == SelectedDepartment.IdDepartment);

            if (SelectedGroup != null)
                query = query.Where(e => e.IdGroup == SelectedGroup.IdGroup);

            if (SelectedTeacher != null)
                query = query.Where(e => e.IdTeachers.Any(t => t.IdTeacher == SelectedTeacher.IdTeacher));

            if (SelectedAudience != null)
                query = query.Where(e => e.IdAudience == SelectedAudience.IdAudience);

            var results = await query.ToListAsync();
            SearchResults = new ObservableCollection<Exam>(results);
            OnPropertyChanged(nameof(SearchResults));
        }
        private void DeleteExam(object parameter)
        {
            if (parameter is not Exam exam) return;

            var result = MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new ApplicationDbContext();
                    var existingExam = context.Exams.Find(exam.IdExam);
                    if (existingExam != null)
                    {
                        context.Exams.Remove(existingExam);
                        context.SaveChanges();
                        SearchResults.Remove(exam);
                        MessageBox.Show("Удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        public void ClearSearch(object parameter)
        {
            SelectedAudience = null;
            SelectedGroup = null;
            SelectedDepartment = null;
            SelectedTeacher = null;
        }

        public void EditDataBase(object parameter)
        {
            Page EditDB = new DataBasePage();
            SearchExamWindow.pageManager.ChangePage(EditDB);
        }

        public void CreateExam(object parameter)
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
