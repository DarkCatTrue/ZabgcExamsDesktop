using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using NLog;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Data;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class AddExamModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
        private Teacher _selectedTeacher;
        private Audience _selectedAudience;
        private TypeOfLesson _selectedLesson;
        private TypeOfExam _selectedTypeOfExam;
        private Qualification _selectedQualification;
        private Discipline _selectedDiscipline;

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
        public Discipline SelectedDiscipline
        {
            get => _selectedDiscipline; set { _selectedDiscipline = value; OnPropertyChanged(); }
        }
        public Teacher SelectedTeacher
        {
            get => _selectedTeacher; set { _selectedTeacher = value; OnPropertyChanged(); }
        }

        public Audience SelectedAudience
        {
            get => _selectedAudience; set { _selectedAudience = value; OnPropertyChanged(); }
        }

        public TypeOfLesson SelectedLesson
        {
            get => _selectedLesson; set { _selectedLesson = value; OnPropertyChanged(); }
        }

        public TypeOfExam SelectedTypeOfExam
        {
            get => _selectedTypeOfExam; set { _selectedTypeOfExam = value; OnPropertyChanged(); UpdateQualifications(); }
        }

        public Qualification SelectedQualification
        {
            get => _selectedQualification; set { _selectedQualification = value; OnPropertyChanged(); }
        }

        public AddExamModel()
        {
            LoadDB();
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
                var typeOfLessons = await context.TypeOfLessons.ToListAsync();
                var typeOfExams = await context.TypeOfExams.ToListAsync();
                var qualifications = await context.Qualifications.ToListAsync();
                var discpline = await context.Disciplines.ToListAsync();

                Department = new ObservableCollection<Department>(departments);
                Group = new ObservableCollection<Group>(groups);
                Teacher = new ObservableCollection<Teacher>(teachers);
                Audience = new ObservableCollection<Audience>(audiences);
                Qualification = new ObservableCollection<Qualification>(qualifications);
                Lesson = new ObservableCollection<TypeOfLesson>(typeOfLessons);
                TypeExam = new ObservableCollection<TypeOfExam>(typeOfExams);
                FilteredGroups = new ObservableCollection<Group>(groups);
                Discipline = new ObservableCollection<Discipline>(discpline);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных из базы данных: {ex}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Ошибка загрузки данных из базы данных: {ex}");

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

        private void UpdateQualifications()
        {
            if (SelectedTypeOfExam?.TypeOfExam1.Contains("Квалификационный") == false)
            {
                SelectedQualification = Qualification.FirstOrDefault(q => q.NameQualification == "Нет");
            }
            else
            {
                SelectedQualification = Qualification.FirstOrDefault(q => q.NameQualification == null);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

