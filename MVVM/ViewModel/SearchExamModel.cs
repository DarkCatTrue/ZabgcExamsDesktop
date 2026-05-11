using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ZabgcExamsDesktop.MVVM.Model;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;
using ZabgcExamsDesktop.Services.API;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public partial class SearchExamModel : ObservableObject
    {
        private readonly ApiService _apiService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [ObservableProperty] private ObservableCollection<GroupDto> _group;
        [ObservableProperty] private ObservableCollection<GroupDto> _filteredGroup;
        [ObservableProperty] private ObservableCollection<TeacherDto> _teacher;
        [ObservableProperty] private ObservableCollection<AudienceDto> _audience;
        [ObservableProperty] private ObservableCollection<DepartmentDto> _department;
        [ObservableProperty] private ObservableCollection<ExamDisplayDto> _searchResults;

        [ObservableProperty] private DepartmentDto _selectedDepartment;
        [ObservableProperty] private GroupDto _selectedGroup;
        [ObservableProperty] private TeacherDto _selectedTeacher;
        [ObservableProperty] private AudienceDto _selectedAudience;

        partial void OnSelectedDepartmentChanged(DepartmentDto value) => UpdateGroups();

        public SearchExamModel()
        {
            _apiService = new ApiService();
            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            await LoadAllDataAsync();
            await LoadReferenceDataAsync();
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
                FilteredGroup = new ObservableCollection<GroupDto>(Group);
                Teacher = new ObservableCollection<TeacherDto>(teachersTask.Result);
                Audience = new ObservableCollection<AudienceDto>(audiencesTask.Result);

                OnPropertyChanged(nameof(Department));
                OnPropertyChanged(nameof(Group));
                OnPropertyChanged(nameof(Teacher));
                OnPropertyChanged(nameof(Audience));
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка загрузки ComboBox: {ex.Message}");
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

        [RelayCommand]
        private void GoToResult(object parameter)
        {
            Page resultPage = new ResultPage();
            SearchExamWindow.pageManager.ChangePage(resultPage);
        }

        [RelayCommand]
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
                MessageBox.Show($"Ошибка при поиске: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateGroups()
        {
            try
            {

                if (SelectedDepartment == null || Group == null)
                    FilteredGroup = new ObservableCollection<GroupDto>();
                else
                    FilteredGroup = new ObservableCollection<GroupDto>(
                        Group.Where(g => g.IdDepartment == SelectedDepartment.IdDepartment));
            }
            catch { }
        }

        [RelayCommand]
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

        [RelayCommand]
        public void ClearSearch(object parameter)
        {
            SelectedAudience = null;
            SelectedGroup = null;
            SelectedDepartment = null;
            SelectedTeacher = null;
        }

        [RelayCommand]
        public void EditDataBase(object parameter)
        {
            Page EditDB = new DataBasePage();
            SearchExamWindow.pageManager.ChangePage(EditDB);
        }
        
        [RelayCommand]        
        public void CreateExam(object parameter)
        {
            ExamWindow examWindow = new ExamWindow();
            examWindow.ShowDialog();
        }
    }
}
