using NLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZabgcExamsDesktop.API;
using ZabgcExamsDesktop.API.Models;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class SearchExamModel : INotifyPropertyChanged
    {
        private ExamWindow examWindow;
        private readonly ApiService _apiService;


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ObservableCollection<DepartmentDto> departments;
        private ObservableCollection<GroupDto> groups;
        private ObservableCollection<TeacherDto> teachers;
        private ObservableCollection<AudienceDto> audiences;
        private ObservableCollection<GroupDto> _filteredGroups;

        private DepartmentDto _selectedDepartment;
        private GroupDto _selectedGroup;
        private TeacherDto _selectedTeacher;
        private AudienceDto _selectedAudience;
        private ObservableCollection<ExamDisplayDto> _searchResults;
        public ICommand CreateExamCommand { get; set; }
        public ICommand EditDataBaseCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ClearSearchCommand { get; set; }
        public ICommand SaveRowCommand { get; set; }
        public ICommand DeleteRowCommand { get; set; }
        public ICommand CreateResultCommand { get; set; }

        public ObservableCollection<DepartmentDto> Department { get => departments; set { departments = value; OnPropertyChanged(); } }
        public ObservableCollection<GroupDto> Group { get => groups; set { groups = value; OnPropertyChanged(); } }
        public ObservableCollection<GroupDto> FilteredGroups { get => _filteredGroups; set { _filteredGroups = value; OnPropertyChanged(); } }
        public ObservableCollection<TeacherDto> Teacher { get => teachers; set { teachers = value; OnPropertyChanged(); } }
        public ObservableCollection<AudienceDto> Audience { get => audiences; set { audiences = value; OnPropertyChanged(); } }
        public ObservableCollection<ExamDisplayDto> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged();
            }
        }
        public DepartmentDto SelectedDepartment
        {
            get => _selectedDepartment; set { _selectedDepartment = value; OnPropertyChanged(); UpdateGroups(); }
        }

        public GroupDto SelectedGroup
        {
            get => _selectedGroup; set { _selectedGroup = value; OnPropertyChanged(); }
        }
        public TeacherDto SelectedTeacher
        {
            get => _selectedTeacher; set { _selectedTeacher = value; OnPropertyChanged(); }
        }

        public AudienceDto SelectedAudience
        {
            get => _selectedAudience; set { _selectedAudience = value; OnPropertyChanged(); }
        }


        public SearchExamModel()
        {
            _apiService = new ApiService();
            LoadInitialDataAsync();
            DeleteRowCommand = new RelayCommand(async (param) => await DeleteExamAsync(param as ExamDisplayDto));
            CreateExamCommand = new RelayCommand(CreateExam);
            EditDataBaseCommand = new RelayCommand(EditDataBase);
            SearchCommand = new RelayCommand(async async => await SearchAsync());
            ClearSearchCommand = new RelayCommand(ClearSearch);
            CreateResultCommand = new RelayCommand(GoToResultPage);
        }

        private async Task LoadInitialDataAsync()
        {
            await LoadReferenceDataAsync();
            await LoadAllDataAsync();
        }
        private async Task LoadReferenceDataAsync()
        {
            try
            {
                var departmentsTask = _apiService.GetDepartmentsAsync();
                var groupsTask = _apiService.GetGroupsAsync();
                var teachersTask = _apiService.GetTeachersAsync();
                var audiencesTask = _apiService.GetAudiencesAsync();

                await Task.WhenAll(departmentsTask, groupsTask, teachersTask, audiencesTask);

                Department = new ObservableCollection<DepartmentDto>(departmentsTask.Result);
                Group = new ObservableCollection<GroupDto>(groupsTask.Result);
                FilteredGroups = new ObservableCollection<GroupDto>(Group);
                Teacher = new ObservableCollection<TeacherDto>(teachersTask.Result);
                Audience = new ObservableCollection<AudienceDto>(audiencesTask.Result);

                OnPropertyChanged(nameof(Department));
                OnPropertyChanged(nameof(Group));
                OnPropertyChanged(nameof(Teacher));
                OnPropertyChanged(nameof(Audience));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ComboBox: {ex.Message}");
            }
        }
        public async Task LoadAllDataAsync()
        {
            try
            {
                var exams = await _apiService.GetExamsDisplayAsync();
                SearchResults = new ObservableCollection<ExamDisplayDto>(exams);
                Logger.Info("Данные успешно загружены через API");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных через API: {ex.Message}",
                    "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Ошибка при загрузке данных через API: {ex}");
            }
        }
        private void GoToResultPage(object parameter)
        {
            Page resultPage = new ResultPage();
            SearchExamWindow.pageManager.ChangePage(resultPage);
        }

        private async Task SearchAsync()
        {
            try
            {
                var exams = await _apiService.SearchExamsAsync(
                    departmentId: SelectedDepartment?.IdDepartment,
                    groupId: SelectedGroup?.IdGroup,
                    teacherId: SelectedTeacher?.IdTeacher,
                    audienceId: SelectedAudience?.IdAudience
                );

                SearchResults = new ObservableCollection<ExamDisplayDto>(exams);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}");
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


        private async Task DeleteExamAsync(ExamDisplayDto exam)
        {
            if (exam != null)
            {
                var result = MessageBox.Show($"Удалить экзамен для группы {exam.GroupName}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (await _apiService.DeleteExamAsync(exam.IdExam))
                    {
                        await LoadAllDataAsync();
                        MessageBox.Show("Экзамен удален", "Удаление экзамена", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
            if (examWindow == null || !examWindow.IsLoaded)
            {
                examWindow = new ExamWindow();
                examWindow.Closed += (s, args) => examWindow = null;
                examWindow.Show();
            }
            else
            {
                examWindow.Activate();
                examWindow.Focus();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
