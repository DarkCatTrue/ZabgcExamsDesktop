using NLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime.CompilerServices;
using System.Windows;
using ZabgcExamsDesktop.API;
using ZabgcExamsDesktop.API.Models;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class AddExamModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ApiService _apiService;

        private ObservableCollection<TypeOfLessonDto> lessons;
        private ObservableCollection<TypeOfExamDto> typeExams;
        private ObservableCollection<DepartmentDto> departments;
        private ObservableCollection<GroupDto> groups;
        private ObservableCollection<TeacherDto> teachers;
        private ObservableCollection<AudienceDto> audiences;
        private ObservableCollection<DisciplineDto> disciplines;
        private ObservableCollection<GroupDto> _filteredGroups;

        private DepartmentDto _selectedDepartment;
        private GroupDto _selectedGroup;
        private TeacherDto _selectedTeacher;
        private AudienceDto _selectedAudience;
        private TypeOfLessonDto _selectedLesson;
        private TypeOfExamDto _selectedTypeOfExam;
        private DisciplineDto _selectedDiscipline;

        public ObservableCollection<DepartmentDto> Department { get => departments; set { departments = value; OnPropertyChanged(); } }
        public ObservableCollection<GroupDto> Group { get => groups; set { groups = value; OnPropertyChanged(); } }
        public ObservableCollection<GroupDto> FilteredGroups { get => _filteredGroups; set { _filteredGroups = value; OnPropertyChanged(); } }
        public ObservableCollection<TeacherDto> Teacher { get => teachers; set { teachers = value; OnPropertyChanged(); } }
        public ObservableCollection<AudienceDto> Audience { get => audiences; set { audiences = value; OnPropertyChanged(); } }
        public ObservableCollection<TypeOfLessonDto> Lesson { get => lessons; set { lessons = value; OnPropertyChanged(); } }
        public ObservableCollection<TypeOfExamDto> TypeExam { get => typeExams; set { typeExams = value; OnPropertyChanged(); } }
        public ObservableCollection<DisciplineDto> Discipline { get => disciplines; set { disciplines = value; OnPropertyChanged(); } }
        public DepartmentDto SelectedDepartment
        {
            get => _selectedDepartment; set { _selectedDepartment = value; OnPropertyChanged(); UpdateGroups(); }
        }
        public GroupDto SelectedGroup
        {
            get => _selectedGroup; set { _selectedGroup = value; OnPropertyChanged(); }
        }
        public DisciplineDto SelectedDiscipline
        {
            get => _selectedDiscipline; set { _selectedDiscipline = value; OnPropertyChanged(); }
        }
        public TeacherDto SelectedTeacher
        {
            get => _selectedTeacher; set { _selectedTeacher = value; OnPropertyChanged(); }
        }

        public AudienceDto SelectedAudience
        {
            get => _selectedAudience; set { _selectedAudience = value; OnPropertyChanged(); }
        }

        public TypeOfLessonDto SelectedLesson
        {
            get => _selectedLesson; set { _selectedLesson = value; OnPropertyChanged(); }
        }

        public TypeOfExamDto SelectedTypeOfExam
        {
            get => _selectedTypeOfExam; set { _selectedTypeOfExam = value; OnPropertyChanged(); }
        }

        public AddExamModel()
        {
            _apiService = new ApiService();
            LoadAllDataAsync();
        }

        public async Task LoadAllDataAsync()
        {
            try
            {
                var departments = _apiService.GetDepartmentsAsync();
                var groups = _apiService.GetGroupsAsync();
                var teachers = _apiService.GetTeachersAsync();
                var audiences = _apiService.GetAudiencesAsync();
                var typeOfLessons = _apiService.GetTypeOfLessonsAsync();
                var typeOfExams = _apiService.GetTypeOfExamsAsync();
                var discipline = _apiService.GetDisciplinesAsync();

                await Task.WhenAll(departments, groups, teachers, audiences, typeOfLessons, typeOfExams, discipline);

                Department = new ObservableCollection<DepartmentDto>(departments.Result);
                Group = new ObservableCollection<GroupDto>(groups.Result);
                Teacher = new ObservableCollection<TeacherDto>(teachers.Result);
                Audience = new ObservableCollection<AudienceDto>(audiences.Result);
                Lesson = new ObservableCollection<TypeOfLessonDto>(typeOfLessons.Result);
                TypeExam = new ObservableCollection<TypeOfExamDto>(typeOfExams.Result);
                FilteredGroups = new ObservableCollection<GroupDto>(groups.Result);
                Discipline = new ObservableCollection<DisciplineDto>(discipline.Result);

                Logger.Info("Данные успешно загружены через API");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных через API: {ex.Message}",
                    "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Ошибка при загрузке данных через API: {ex}");
            }
        }
        private void UpdateGroups()
        {
            if (SelectedDepartment == null)
            {
                FilteredGroups = new ObservableCollection<GroupDto>(Group);
            }
            else
            {
                var filtered = Group
                    .Where(g => g.IdDepartment == SelectedDepartment.IdDepartment)
                    .ToList();

                FilteredGroups = new ObservableCollection<GroupDto>(filtered);
            }

            SelectedGroup = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

