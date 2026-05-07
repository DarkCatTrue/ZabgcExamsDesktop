using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using ZabgcExamsDesktop.API;
using ZabgcExamsDesktop.API.Models;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public partial class CreateExamModel : ObservableObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ApiService _apiService;

        [ObservableProperty] private ObservableCollection<GroupDto> _group;
        [ObservableProperty] private ObservableCollection<DepartmentDto> _department;
        [ObservableProperty] private ObservableCollection<TeacherDto> _teacher;
        [ObservableProperty] private ObservableCollection<AudienceDto> _audience;
        [ObservableProperty] private ObservableCollection<TypeOfLessonDto> _lesson;
        [ObservableProperty] private ObservableCollection<TypeOfExamDto> _typeOfExam;
        [ObservableProperty] private ObservableCollection<DisciplineDto> _discipline;

        [ObservableProperty] private string _searchGroupsText = string.Empty;
        [ObservableProperty] private string _searchAudienceText = string.Empty;
        [ObservableProperty] private string _searchDisciplineText = string.Empty;
        [ObservableProperty] private DepartmentDto _selectedDepartment;

        [ObservableProperty] private GroupDto _selectedGroup; 
        [ObservableProperty] private DisciplineDto _selectedDiscipline;
        [ObservableProperty] private List<TeacherDto> _selectedTeachers = new();
        [ObservableProperty] private AudienceDto _selectedAudience;
        [ObservableProperty] private TypeOfLessonDto _selectedLesson;
        [ObservableProperty] private TypeOfExamDto _selectedTypeOfExam;
        [ObservableProperty] private DateTime? _selectedDate;
        [ObservableProperty] private TimeSpan? _selectedTime;
        [ObservableProperty] private string _teachersText = string.Empty;

        partial void OnSearchGroupsTextChanged(string value) => FilterGroupsView();
        partial void OnSearchAudienceTextChanged(string value) => FilterAudienceView();
        partial void OnSearchDisciplineTextChanged(string value) => FilterDisciplineView();
        partial void OnSelectedDepartmentChanged(DepartmentDto value) => FilterGroupsView();

        public ICollectionView GroupsView { get; private set; }
        public ICollectionView AudienceView { get; private set; }
        public ICollectionView DisciplineView { get; private set; }

        public CreateExamModel()
        {
            _apiService = new ApiService();
            _ = LoadAllDataAsync();
        }

        [RelayCommand]
        private void SelectionTeachers()
        {
            var dialog = new TeachersAddWindow(Teacher, TeachersText, (selectedTeachers, teachersText) =>
            {
                SelectedTeachers = selectedTeachers;
                TeachersText = teachersText;
            });
            dialog.ShowDialog();
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

                Department = new ObservableCollection<DepartmentDto>(await departments);
                Group = new ObservableCollection<GroupDto>(await groups);
                Teacher = new ObservableCollection<TeacherDto>(await teachers);
                Audience = new ObservableCollection<AudienceDto>(await audiences);
                Lesson = new ObservableCollection<TypeOfLessonDto>(await typeOfLessons);
                TypeOfExam = new ObservableCollection<TypeOfExamDto>(await typeOfExams);
                Discipline = new ObservableCollection<DisciplineDto>(await discipline);

                InitializeViews();

                Logger.Info("Данные успешно загружены через API в окне создания экзамена.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных через API: {ex.Message}", "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Ошибка при загрузке данных через API в окне создания экзамена: {ex}");
            }
        }

        private void FilterGroupsView() => GroupsView?.Refresh();
        private void FilterAudienceView() => AudienceView?.Refresh();
        private void FilterDisciplineView() => DisciplineView?.Refresh();
        private void InitializeViews()
        {
            GroupsView = CollectionViewSource.GetDefaultView(Group);
            GroupsView.Filter = item =>
            {
                var group = item as GroupDto;
                if (group == null) return false;

                if (SelectedDepartment != null && group.IdDepartment != SelectedDepartment.IdDepartment)
                    return false;

                if (string.IsNullOrEmpty(SearchGroupsText))
                    return true;

                return group.NameOfGroup?.IndexOf(SearchGroupsText, StringComparison.CurrentCultureIgnoreCase) >= 0;
            };

            AudienceView = CollectionViewSource.GetDefaultView(Audience);
            AudienceView.Filter = item =>
            {
                var audience = item as AudienceDto;
                if (audience == null) return false;

                if (string.IsNullOrEmpty(SearchAudienceText))
                    return true;

                return audience.NumberAudience?.IndexOf(SearchAudienceText, StringComparison.CurrentCultureIgnoreCase) >= 0;
            };

            DisciplineView = CollectionViewSource.GetDefaultView(Discipline);
            DisciplineView.Filter = item =>
            {
                var discipline = item as DisciplineDto;
                if (discipline == null) return false;

                if (string.IsNullOrEmpty(SearchDisciplineText))
                    return true;

                return discipline.NameDiscipline?.IndexOf(SearchDisciplineText, StringComparison.CurrentCultureIgnoreCase) >= 0;
            };
        }

        private bool CheckValidation()
        {
            if (SelectedGroup == null ||
                SelectedDiscipline == null ||
                SelectedLesson == null ||
                SelectedTypeOfExam == null ||
                SelectedDate == null ||
                SelectedTime == null ||
                SelectedAudience == null)
            {
                MessageBox.Show("Все поля должны быть заполнены", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        [RelayCommand]
        private async Task SaveExamsAsync()
        {
            if (!CheckValidation()) return;

            try
            {
                DateTime fullDate = SelectedDate.Value.Add(SelectedTime.Value);

                var newExam = new CreateExamDto
                {
                    IdGroup = SelectedGroup.IdGroup,
                    IdDiscipline = SelectedDiscipline.IdDiscipline,
                    IdTypeOfLesson = SelectedLesson.IdTypeOfLesson,
                    IdTypeOfExam = SelectedTypeOfExam.IdTypeOfExam,
                    IdAudience = SelectedAudience.IdAudience,
                    DateEvent = fullDate,
                    IdTeachers = SelectedTeachers.Select(t => t.IdTeacher).ToList()
                };

                bool isSuccess = await _apiService.CreateExamAsync(newExam);

                if (isSuccess)
                {
                    ReloadPage();
                    MessageBox.Show("Добавлена новая запись", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Logger.Info($"Был добавлен экзамен для группы: '{SelectedGroup.NameOfGroup}'");
                }
                else
                {
                    MessageBox.Show("Не удалось добавить экзамен. Проверьте подключение к API.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Неверный формат времени.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Не удалось добавить экзамен для группы: '{SelectedGroup?.NameOfGroup}' ошибка: {ex}");
            }
        }
        private void ReloadPage()
        {
            var newPage = new SearchExamPage();
            SearchExamWindow.pageManager.ChangePage(newPage);
        }
    }
}