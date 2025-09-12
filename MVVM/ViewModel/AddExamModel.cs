using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Data;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Models;
using Group = ZabgcExamsDesktop.MVVM.Model.DataBase.Models.Group;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class AddExamModel : INotifyPropertyChanged
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
        private Teacher _selectedTeacher;
        private Audience _selectedAudience;
        private TypeOfLesson _selectedLesson;
        private TypeOfExam _selectedTypeOfExam;
        private Qualification _selectedQualification;
        private Discipline _selectedDiscipline;

        public ICommand AddExamCommand { get; set; }

        public ObservableCollection<Department> Department { get => departments; set { departments = value; OnPropertyChanged(); } }
        public ObservableCollection<Group> Group { get => groups; set { groups = value; OnPropertyChanged(); } }
        public ObservableCollection<Group> FilteredGroups { get => _filteredGroups; set { _filteredGroups = value; OnPropertyChanged(); } }
        public ObservableCollection<Teacher> Teacher { get => teachers; set { teachers = value; OnPropertyChanged(); } }
        public ObservableCollection<Audience> Audience { get => audiences; set { audiences = value; OnPropertyChanged(); } }
        public ObservableCollection<TypeOfLesson> Lesson { get => lessons; set { lessons = value; OnPropertyChanged(); } }
        public ObservableCollection<TypeOfExam> TypeExam { get => typeExams; set { typeExams = value; OnPropertyChanged(); } }
        public ObservableCollection<Qualification> Qualification { get => qualifications; set { qualifications = value; OnPropertyChanged(); } }
        public ObservableCollection<Discipline> Discipline { get => disciplines; set { disciplines = value; OnPropertyChanged(); } }

        private DateTime? _datePickerText;
        private string _timePickerText;

        public DateTime? DatePickerText
        {
            get => _datePickerText; set
            {
                _datePickerText = value; OnPropertyChanged();
            }
        }
        public string TimePickerText
        {
            get => _timePickerText; set { _timePickerText = value; OnPropertyChanged(); }
        }

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
            AddExamCommand = new RelayCommand(CreateExam);
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
                MessageBox.Show($"Ошибка базы данных: {ex}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CreateExam(object parameter)
        {
            try
            {
                using var context = new ApplicationDbContext();
                DateTime Date = Convert.ToDateTime(DatePickerText);
                string DateText = Date.ToString("yyyy-MM-dd");
                string TimeText = TimePickerText;
                DateTime datePart = DateTime.ParseExact(DateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                TimeSpan timePart = TimeSpan.ParseExact(TimeText, "hh\\:mm", CultureInfo.InvariantCulture);
                DateTime Fulldate = datePart.Add(timePart);
                var newExam = new Exam
                {
                    IdGroup = SelectedGroup?.IdGroup,
                    IdDiscipline = SelectedDiscipline?.IdDiscipline,
                    IdTypeOfLesson = SelectedLesson?.IdTypeOfLesson,
                    IdTypeOfExam = SelectedTypeOfExam?.IdTypeOfExam,
                    IdQualification = SelectedQualification?.IdQualification,
                    DateEvent = Fulldate,
                };

                if (SelectedTeacher != null)
                {
                    var teacher = context.Teachers.Find(SelectedTeacher.IdTeacher);
                    if (teacher != null)
                        newExam.IdTeachers.Add(teacher);
                }

                if (SelectedAudience != null)
                {
                    var audience = context.Audiences.Find(SelectedAudience.IdAudience);
                    if (audience != null)
                        newExam.IdAudiences.Add(audience);
                }
                context.Exams.Add(newExam);
                context.SaveChanges();
                MessageBox.Show("Добавлена новая запись", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

