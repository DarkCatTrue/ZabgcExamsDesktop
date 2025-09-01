using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Data;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Models;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;
using Group = ZabgcExamsDesktop.MVVM.Model.DataBase.Models.Group;

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
        private Exam _exam;


        private Department _selectedDepartmentSearch;
        private Group _selectedGroupSearch;
        private Teacher _selectedTeacherSearch;
        private Audience _selectedAudienceSearch;

        public ICommand CreateExamCommand { get; set; }
        public ICommand EditDataBaseCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ClearSearchCommand { get; set; }
        public ICommand SaveRowCommand { get; set; }


        public ObservableCollection<Department> Department { get => departments; set { departments = value; OnPropertyChanged(); } }
        public ObservableCollection<Group> Group { get => groups; set { groups = value; OnPropertyChanged(); } }
        public ObservableCollection<Group> FilteredGroups { get => _filteredGroups ; set { _filteredGroups = value; OnPropertyChanged(); } }
        public ObservableCollection<Teacher> Teacher { get => teachers; set { teachers = value; OnPropertyChanged(); } }
        public ObservableCollection<Audience> Audience { get => audiences; set { audiences = value; OnPropertyChanged(); } }
        public ObservableCollection<Exam> SearchResults { get; set; }

        public Department SelectedDepartmentSearch
        {
            get => _selectedDepartment; set { _selectedDepartment = value; OnPropertyChanged(); UpdateGroups(); }
        }

        public Group SelectedGroupSearch
        {
            get => _selectedGroup; set { _selectedGroup = value; OnPropertyChanged(); }
        }
        public Teacher SelectedTeacherSearch
        {
            get => _selectedTeacher; set { _selectedTeacher = value; OnPropertyChanged(); }
        }

        public Audience SelectedAudienceSearch
        {
            get => _selectedAudience; set { _selectedAudience = value; OnPropertyChanged(); }
        }

        private Exam _selectedExam;
        public Exam SelectedExam
        {
            get => _selectedExam;
            set { _selectedExam = value; OnPropertyChanged(); }
        }

        public Department SelectedDepartment
        {
            get => _selectedDepartment; set { _selectedDepartment = value; OnPropertyChanged(); }
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
            SaveRowCommand = new RelayCommand(SaveSelectedRow);
            CreateExamCommand = new RelayCommand(CreateExam);
            EditDataBaseCommand = new RelayCommand(EditDataBase);
            SearchCommand = new RelayCommand(Search);
            ClearSearchCommand = new RelayCommand(ClearSearch);
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
                .Include(e => e.IdTeachers)
                .Include(e => e.IdAudiences)
                .Include(e => e.IdDisciplineNavigation)
                .Include(e => e.IdQualificationNavigation)
                .Include(e => e.IdTypeOfExamNavigation)
                .Include(e => e.IdTypeOfLessonNavigation)
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

        private async void Search()
        {
            using var context = new ApplicationDbContext();

            var query = context.Exams
                .Include(e => e.IdGroupNavigation)
                    .ThenInclude(g => g.IdDepartmentNavigation)
                .Include(e => e.IdTeachers)
                .Include(e => e.IdAudiences)
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
                query = query.Where(e => e.IdAudiences.Any(a => a.IdAudience == SelectedAudience.IdAudience));

            var results = await query.ToListAsync();
            SearchResults = new ObservableCollection<Exam>(results);
            OnPropertyChanged(nameof(SearchResults));
        }

        private void SaveSelectedRow()
        {
            if (SelectedExam != null)
            {
                SaveRow(SelectedExam);
            }
        }

        private async void SaveRow(Exam exam)
        {
            if (exam == null) return;

            try
            {
                using var context = new ApplicationDbContext();

                var existingExam = await context.Exams
                    .Include(e => e.IdTeachers)
                    .Include(e => e.IdAudiences)
                    .FirstOrDefaultAsync(e => e.IdExam == exam.IdExam);

                if (existingExam != null)
                {
                    existingExam.IdGroup = exam.IdGroup;
                    existingExam.IdDiscipline = exam.IdDiscipline;
                    existingExam.IdTypeOfLesson = exam.IdTypeOfLesson;
                    existingExam.IdTypeOfExam = exam.IdTypeOfExam;
                    existingExam.IdQualification = exam.IdQualification;
                    existingExam.DateEvent = exam.DateEvent;

                    existingExam.IdTeachers = exam.IdTeachers;
                    existingExam.IdAudiences = exam.IdAudiences;

                    await context.SaveChangesAsync();
                    MessageBox.Show($"Изменения для группы {exam.IdGroupNavigation?.NameOfGroup} сохранены!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        public void ClearSearch()
        {
            SelectedAudience = null;
            SelectedGroup = null;
            SelectedDepartment = null;
            SelectedTeacher = null;
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
