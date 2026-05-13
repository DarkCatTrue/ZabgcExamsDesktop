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

public partial class SearchExamModel : ObservableObject
{
    private readonly ApiService _apiService;
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    [ObservableProperty] private ObservableCollection<ExamDisplayDto> _searchResults;

    [ObservableProperty] private ObservableCollection<DepartmentDto> _department;
    [ObservableProperty] private ObservableCollection<GroupDto> _group;
    [ObservableProperty] private ObservableCollection<TeacherDto> _teacher;
    [ObservableProperty] private ObservableCollection<AudienceDto> _audience;

    [ObservableProperty] private ObservableCollection<DepartmentDto> _filteredDepartments;
    [ObservableProperty] private ObservableCollection<GroupDto> _filteredGroups;
    [ObservableProperty] private ObservableCollection<TeacherDto> _filteredTeachers;
    [ObservableProperty] private ObservableCollection<AudienceDto> _filteredAudiences;

    [ObservableProperty] private string _departmentSearchText = string.Empty;
    [ObservableProperty] private string _groupSearchText = string.Empty;
    [ObservableProperty] private string _teacherSearchText = string.Empty;
    [ObservableProperty] private string _audienceSearchText = string.Empty;

    [ObservableProperty] private DepartmentDto _selectedDepartment;
    [ObservableProperty] private GroupDto _selectedGroup;
    [ObservableProperty] private TeacherDto _selectedTeacher;
    [ObservableProperty] private AudienceDto _selectedAudience;

    [ObservableProperty] private ObservableCollection<DepartmentDto> _availableDepartment;
    [ObservableProperty] private ObservableCollection<GroupDto> _availableGroup;
    [ObservableProperty] private ObservableCollection<TeacherDto> _availableTeacher;
    [ObservableProperty] private ObservableCollection<AudienceDto> _availableAudience;

    public SearchExamModel()
    {
        _apiService = new ApiService();
        _ = LoadAllDataAsync();
    }

    private async Task LoadAllDataAsync()
    {
        await LoadExamsAsync();
        await LoadReferenceDataAsync();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        FilterDepartments();
        FilterGroups();
        FilterTeachers();
        FilterAudiences();
    }

    private async Task LoadExamsAsync()
    {
        try
        {
            var exams = await _apiService.GetExamsDisplayAsync();
            SearchResults = new ObservableCollection<ExamDisplayDto>(exams);
            Logger.Info("Экзамены загружены");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки экзаменов: {ex.Message}");
            Logger.Error(ex);
        }
    }

    private async Task LoadReferenceDataAsync()
    {
        try
        {
            var depsTask = _apiService.GetDepartmentsAsync();
            var groupsTask = _apiService.GetGroupsAsync();
            var teachersTask = _apiService.GetTeachersAsync();
            var audsTask = _apiService.GetAudiencesAsync();
            await Task.WhenAll(depsTask, groupsTask, teachersTask, audsTask);

            Department = new ObservableCollection<DepartmentDto>(depsTask.Result);
            Group = new ObservableCollection<GroupDto>(groupsTask.Result);
            Teacher = new ObservableCollection<TeacherDto>(teachersTask.Result);
            Audience = new ObservableCollection<AudienceDto>(audsTask.Result);

            FilteredDepartments = new ObservableCollection<DepartmentDto>(Department);
            FilteredGroups = new ObservableCollection<GroupDto>(Group);
            FilteredTeachers = new ObservableCollection<TeacherDto>(Teacher);
            FilteredAudiences = new ObservableCollection<AudienceDto>(Audience);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки справочников: {ex.Message}");
            Logger.Error(ex);
        }
    }

    partial void OnDepartmentSearchTextChanged(string value) => FilterDepartments();
    partial void OnGroupSearchTextChanged(string value) => FilterGroups();
    partial void OnTeacherSearchTextChanged(string value) => FilterTeachers();
    partial void OnAudienceSearchTextChanged(string value) => FilterAudiences();

    private void FilterDepartments()
    {
        if (Department == null) return;
        var filtered = string.IsNullOrEmpty(DepartmentSearchText)
            ? Department
            : new ObservableCollection<DepartmentDto>(
                Department.Where(d => d.NameOfDepartment.Contains(DepartmentSearchText, StringComparison.CurrentCultureIgnoreCase))
              );
        FilteredDepartments = new ObservableCollection<DepartmentDto>(filtered);
    }

    private void FilterGroups()
    {
        if (Group == null) return;
        var filtered = string.IsNullOrEmpty(GroupSearchText)
            ? Group
            : new ObservableCollection<GroupDto>(
                Group.Where(g => g.NameOfGroup.Contains(GroupSearchText, StringComparison.CurrentCultureIgnoreCase))
              );
        FilteredGroups = new ObservableCollection<GroupDto>(filtered);
    }

    private void FilterTeachers()
    {
        if (Teacher == null) return;
        var filtered = string.IsNullOrEmpty(TeacherSearchText)
            ? Teacher
            : new ObservableCollection<TeacherDto>(
                Teacher.Where(t => t.FullName.Contains(TeacherSearchText, StringComparison.CurrentCultureIgnoreCase))
              );
        FilteredTeachers = new ObservableCollection<TeacherDto>(filtered);
    }

    private void FilterAudiences()
    {
        if (Audience == null) return;
        var filtered = string.IsNullOrEmpty(AudienceSearchText)
            ? Audience
            : new ObservableCollection<AudienceDto>(
                Audience.Where(a => a.NumberAudience.Contains(AudienceSearchText, StringComparison.CurrentCultureIgnoreCase))
              );
        FilteredAudiences = new ObservableCollection<AudienceDto>(filtered);
    }

    partial void OnSelectedDepartmentChanged(DepartmentDto value)
    {
        if (Group == null) return;
        var filtered = Group.Where(g => g.IdDepartment == value?.IdDepartment);
        FilteredGroups = new ObservableCollection<GroupDto>(filtered);
    }

    [RelayCommand]
    private async Task DeleteOldExamsAsync()
    {

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
            MessageBox.Show($"Ошибка поиска: {ex.Message}");
        }
    }

    [RelayCommand]
    private void GoToResult(object parameter)
    {
        Page resultPage = new ResultPage();
        SearchExamWindow.pageManager.ChangePage(resultPage);
    }

    [RelayCommand]
    private async Task DeleteItemAsync(ExamDisplayDto exam)
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

    [RelayCommand]
    private void ClearSearch()
    {
        SelectedDepartment = null;
        SelectedGroup = null;
        SelectedTeacher = null;
        SelectedAudience = null;
        DepartmentSearchText = string.Empty;
        GroupSearchText = string.Empty;
        TeacherSearchText = string.Empty;
        AudienceSearchText = string.Empty;
    }
}