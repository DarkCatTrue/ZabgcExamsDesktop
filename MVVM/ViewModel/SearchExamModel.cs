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

    partial void OnSelectedDepartmentChanged(DepartmentDto value) => RefreshAvailableCollections();

    partial void OnSelectedGroupChanged(GroupDto value) => RefreshAvailableCollections();

    partial void OnSelectedTeacherChanged(TeacherDto value) => RefreshAvailableCollections();

    partial void OnSelectedAudienceChanged(AudienceDto value) => RefreshAvailableCollections();

    partial void OnDepartmentSearchTextChanged(string value) => FilterDepartments();
    partial void OnGroupSearchTextChanged(string value) => FilterGroups();
    partial void OnTeacherSearchTextChanged(string value) => FilterTeachers();
    partial void OnAudienceSearchTextChanged(string value) => FilterAudiences();

    public SearchExamModel()
    {
        _apiService = new ApiService();
        _ = LoadAllDataAsync();
    }
    private async Task LoadAllDataAsync()
    {
        await LoadExamsAsync();
        await LoadReferenceDataAsync();
        RefreshAvailableCollections();
    }

    private void ApplyFilters()
    {
        FilterDepartments();
        FilterGroups();
        FilterTeachers();
        FilterAudiences();
    }

    private void RefreshAvailableCollections()
    {
        if (SearchResults == null) return;

        IEnumerable<ExamDisplayDto> filteredExams = SearchResults;

        if (SelectedDepartment != null)
            filteredExams = filteredExams.Where(e => e.DepartmentName == SelectedDepartment.NameOfDepartment);

        if (SelectedGroup != null)
            filteredExams = filteredExams.Where(e => e.GroupName == SelectedGroup.NameOfGroup);

        if (SelectedTeacher != null)
            filteredExams = filteredExams.Where(e => e.IdTeachers.Contains(SelectedTeacher.IdTeacher));

        if (SelectedAudience != null)
            filteredExams = filteredExams.Where(e => e.AudienceNumber == SelectedAudience.NumberAudience);

        var departments = filteredExams
            .Select(e => e.DepartmentName)
            .Distinct()
            .Select(name => Department?.FirstOrDefault(d => d.NameOfDepartment == name))
            .Where(d => d != null)
            .ToList();

        var groups = filteredExams
            .Select(e => e.GroupName)
            .Distinct()
            .Select(name => Group?.FirstOrDefault(g => g.NameOfGroup == name))
            .Where(g => g != null)
            .ToList();

        var teachers = filteredExams
            .SelectMany(e => e.IdTeachers)
            .Distinct()
            .Select(id => Teacher?.FirstOrDefault(t => t.IdTeacher == id))
            .Where(t => t != null)
            .ToList();

        var audiences = filteredExams
            .Select(e => e.AudienceNumber)
            .Distinct()
            .Select(num => Audience?.FirstOrDefault(a => a.NumberAudience == num))
            .Where(a => a != null)
            .ToList();

        AvailableDepartment = new ObservableCollection<DepartmentDto>(departments);
        AvailableGroup = new ObservableCollection<GroupDto>(groups);
        AvailableTeacher = new ObservableCollection<TeacherDto>(teachers);
        AvailableAudience = new ObservableCollection<AudienceDto>(audiences);

        if (SelectedDepartment != null && !AvailableDepartment.Contains(SelectedDepartment))
            SelectedDepartment = null;

        if (SelectedGroup != null && !AvailableGroup.Contains(SelectedGroup))
            SelectedGroup = null;

        if (SelectedTeacher != null && !AvailableTeacher.Contains(SelectedTeacher))
            SelectedTeacher = null;

        if (SelectedAudience != null && !AvailableAudience.Contains(SelectedAudience))
            SelectedAudience = null;

        ApplyFilters();
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

    private void FilterDepartments()
    {
        if (AvailableDepartment == null) return;
        FilteredDepartments = string.IsNullOrEmpty(DepartmentSearchText)
            ? new ObservableCollection<DepartmentDto>(AvailableDepartment)
            : new ObservableCollection<DepartmentDto>(
                AvailableDepartment.Where(d => d.NameOfDepartment.Contains(DepartmentSearchText, StringComparison.CurrentCultureIgnoreCase))
              );
    }

    private void FilterGroups()
    {
        if (AvailableGroup == null) return;
        FilteredGroups = string.IsNullOrEmpty(GroupSearchText)
            ? new ObservableCollection<GroupDto>(AvailableGroup)
            : new ObservableCollection<GroupDto>(
                AvailableGroup.Where(g => g.NameOfGroup.Contains(GroupSearchText, StringComparison.CurrentCultureIgnoreCase))
              );
    }

    private void FilterTeachers()
    {
        if (AvailableTeacher == null) return;
        FilteredTeachers = string.IsNullOrEmpty(TeacherSearchText)
            ? new ObservableCollection<TeacherDto>(AvailableTeacher)
            : new ObservableCollection<TeacherDto>(
                AvailableTeacher.Where(t => t.FullName.Contains(TeacherSearchText, StringComparison.CurrentCultureIgnoreCase))
              );
    }

    private void FilterAudiences()
    {
        if (AvailableAudience == null) return;
        FilteredAudiences = string.IsNullOrEmpty(AudienceSearchText)
            ? new ObservableCollection<AudienceDto>(AvailableAudience)
            : new ObservableCollection<AudienceDto>(
                AvailableAudience.Where(a => a.NumberAudience.Contains(AudienceSearchText, StringComparison.CurrentCultureIgnoreCase))
              );
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
            RefreshAvailableCollections();
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
    private async Task ClearSearch()
    {
        SelectedDepartment = null;
        SelectedGroup = null;
        SelectedTeacher = null;
        SelectedAudience = null;
        DepartmentSearchText = string.Empty;
        GroupSearchText = string.Empty;
        TeacherSearchText = string.Empty;
        AudienceSearchText = string.Empty;
        await SearchAsync();
    }
}