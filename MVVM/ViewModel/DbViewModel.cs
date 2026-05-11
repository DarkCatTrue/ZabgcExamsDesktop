using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZabgcExamsDesktop.MVVM.Model;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;
using ZabgcExamsDesktop.Services.API;
using static ZabgcExamsDesktop.MVVM.View.Pages.DataGridColumnsBehavior;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public partial class DbViewModel : ObservableObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ApiService _apiService;
        public int? SelectedDepartmentId { get; set; }
        public bool IsNotEditing => !IsEditing;

        [ObservableProperty] private ObservableCollection<GroupDto> _group;
        [ObservableProperty] private ObservableCollection<DepartmentDto> _department;
        [ObservableProperty] private ObservableCollection<DepartmentOwnerDto> _departmentOwner;
        [ObservableProperty] private ObservableCollection<ManagerDto> _manager;
        [ObservableProperty] private ObservableCollection<TeacherDto> _teacher;
        [ObservableProperty] private ObservableCollection<AudienceDto> _audience;
        [ObservableProperty] private ObservableCollection<DisciplineDto> _discipline;

        [ObservableProperty] private object _currentItems;
        [ObservableProperty] private object _selectedItem;
        [ObservableProperty] private string _enterGroup;
        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private ViewType _currentViewType;

        partial void OnIsEditingChanged(bool value) => OnPropertyChanged(nameof(IsNotEditing));
        partial void OnCurrentViewTypeChanged(ViewType value) => UpdateItems();
        partial void OnSelectedItemChanged(object value) => CommandManager.InvalidateRequerySuggested();

        private void UpdateItems()
        {
            CurrentItems = CurrentViewType switch
            {
                ViewType.Departments => Department,
                ViewType.Groups => Group,
                ViewType.Audiences => Audience,
                ViewType.Teachers => Teacher,
                ViewType.Disciplines => Discipline,
                ViewType.Managers => Manager,
                ViewType.DepartmentOwners => DepartmentOwner,
                _ => null
            };
        }

        public DbViewModel()
        {
            _apiService = new ApiService();
            _ = FirstOpen();
        }

        [RelayCommand]
        private async Task AddNewGroup(object parameter)
        {
            if (string.IsNullOrWhiteSpace(EnterGroup))
            {
                MessageBox.Show("Введите название группы!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedDepartmentId == null)
            {
                MessageBox.Show("Выберите отделение!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var department = Department.FirstOrDefault(d => d.IdDepartment == SelectedDepartmentId);
                if (department == null)
                {
                    MessageBox.Show("Выбранное отделение не найдено!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newGroup = new GroupDto
                {
                    NameOfGroup = EnterGroup.Trim(),
                    IdDepartment = SelectedDepartmentId.Value
                };

                if (await _apiService.CreateGroupAsync(newGroup))
                {
                    await LoadData();
                    MessageBox.Show($"Группа '{EnterGroup}' с отделением: '{department.NameOfDepartment}' успешно добавлена!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    Logger.Info($"Группа '{EnterGroup}' с отделением: '{department.NameOfDepartment}' была добавлена в базу данных.");

                    EnterGroup = string.Empty;
                    OnPropertyChanged(nameof(EnterGroup));
                    ReloadPage();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить группу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Произошла ошибка добавления новой группы: '{EnterGroup}', ошибка: {ex}");
            }
        }

        private void ReloadPage()
        {
            var newPage = new DataBasePage();
            SearchExamWindow.pageManager.ChangePage(newPage);
        }

        [RelayCommand]
        public void BackToExams(object parameter)
        {
            Page SearchExam = new SearchExamPage();
            SearchExamWindow.pageManager.ChangePage(SearchExam);
        }

        [RelayCommand]
        private void AddNewItem(object parameter)
        {
            IsEditing = true;

            switch (CurrentViewType)
            {
                case ViewType.Groups:
                    AddGroup();
                    break;

                case ViewType.Teachers:
                    AddItem(Teacher, new TeacherDto());
                    break;

                case ViewType.Audiences:
                    AddItem(Audience, new AudienceDto());
                    break;

                case ViewType.Disciplines:
                    AddItem(Discipline, new DisciplineDto());
                    break;
            }
        }
        private void AddItem<T>(ObservableCollection<T> collection, T item) where T : BaseDto
        {
            item.IsNew = true;
            item.IsEditing = true;

            collection.Add(item);
            SelectedItem = item;
        }

        [RelayCommand]
        private async Task SaveItem(object parameter)
        {
            try
            {
                switch (CurrentViewType)
                {
                    case ViewType.Managers:
                        await SaveAllItems(Manager, _apiService.CreateManagerAsync, _apiService.UpdateManagerAsync);
                        break;

                    case ViewType.DepartmentOwners:
                        await SaveAllItems(DepartmentOwner, _apiService.CreateDepartmentOwnerAsync, _apiService.UpdateDepartmentOwnerAsync);
                        break;

                    case ViewType.Departments:
                        await SaveAllItems(Department, _apiService.CreateDepartmentAsync, _apiService.UpdateDepartmentAsync);
                        break;

                    case ViewType.Groups:
                        await SaveAllItems(Group, _apiService.CreateGroupAsync, _apiService.UpdateGroupAsync);
                        break;

                    case ViewType.Teachers:
                        await SaveAllItems(Teacher, _apiService.CreateTeacherAsync, _apiService.UpdateTeacherAsync);
                        break;

                    case ViewType.Audiences:
                        await SaveAllItems(Audience, _apiService.CreateAudienceAsync, _apiService.UpdateAudienceAsync);
                        break;

                    case ViewType.Disciplines:
                        await SaveAllItems(Discipline, _apiService.CreateDisciplineAsync, _apiService.UpdateDisciplineAsync);
                        break;
                }

                MessageBox.Show("Изменения успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task SaveAllItems<T>(
         ObservableCollection<T> collection,
         Func<T, Task<bool>> createFunc,
         Func<T, Task<bool>> updateFunc)
         where T : BaseDto
        {
            if (!collection.Any())
            {
                MessageBox.Show("Нет элементов для сохранения");
                return;
            }

            foreach (var item in collection)
            {
                try
                {
                    bool success;

                    if (item.IsNew)
                        success = await createFunc(item);
                    else
                        success = await updateFunc(item);

                    if (success)
                    {
                        item.IsEditing = false;
                        item.IsNew = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}");
                }
            }
        }

        private void AddGroup()
        {
            GroupAddWindow groupAddWindow = new GroupAddWindow();
            groupAddWindow.ShowDialog();
        }

        [RelayCommand]
        private async Task DeleteItem(object parameter)
        {
            if (parameter == null) return;

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = false;

                    switch (parameter)
                    {
                        case GroupDto g:
                            Group.Remove(g);
                            success = await _apiService.DeleteGroupAsync(g.IdGroup);
                            Logger.Warn($"Группа '{g.IdGroup}' была удалена из базы данных.");
                            break;

                        case AudienceDto a:
                            Audience.Remove(a);
                            success = await _apiService.DeleteAudienceAsync(a.IdAudience);
                            Logger.Warn($"Аудитория '{a.IdAudience}' была удалена из базы данных.");
                            break;

                        case TeacherDto t:
                            Teacher.Remove(t);
                            success = await _apiService.DeleteTeacherAsync(t.IdTeacher);
                            Logger.Warn($"Преподаватель '{t.IdTeacher}' был удален из базы данных.");
                            break;

                        case DisciplineDto d:
                            Discipline.Remove(d);
                            success = await _apiService.DeleteDisciplineAsync(d.IdDiscipline);
                            Logger.Warn($"Дисциплина '{d.IdDiscipline}' была удалена из базы данных.");
                            break;
                    }

                    if (success)
                    {
                        await LoadData();
                        MessageBox.Show($"Запись успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Не удалось удалить запись.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Logger.Error($"Ошибка при удалении данных из базы данных: {ex}");
                }
            }
        }

        [RelayCommand]
        private void EditItem(object parameter)
        {
            SelectedItem = parameter;
            if (SelectedItem != null)
            {
                IsEditing = true;
            }
        }
        private async Task FirstOpen()
        {
            await LoadData();
            ShowGrid("Groups");
        }
        private async Task LoadData()
        {
            try
            {
                var departmentsTask = _apiService.GetDepartmentsAsync();
                var groupsTask = _apiService.GetGroupsAsync();
                var teachers = _apiService.GetTeachersAsync();
                var audiences = _apiService.GetAudiencesAsync();
                var disciplines = _apiService.GetDisciplinesAsync();
                var managers = _apiService.GetManagersAsync();
                var departmentOwnersTask = _apiService.GetDepartmentOwnersAsync();

                await Task.WhenAll(departmentsTask, groupsTask, teachers, audiences, managers, disciplines, departmentOwnersTask);

                var departments = departmentsTask.Result;
                var groups = groupsTask.Result;
                var departmentOwners = departmentOwnersTask.Result;


                foreach (var group in groups)
                {
                    var department = departments.FirstOrDefault(d => d.IdDepartment == group.IdDepartment);
                    group.DepartmentName = department?.NameOfDepartment ?? "Не указано";
                }

                foreach (var departmentOwner in departmentOwners)
                {
                    var department = departments.FirstOrDefault(d => d.IdDepartment == departmentOwner.IdDepartment);
                    departmentOwner.DepartmentName = department?.NameOfDepartment ?? "Не указано";
                }


                Department = new ObservableCollection<DepartmentDto>(departments);
                Group = new ObservableCollection<GroupDto>(groups);
                Audience = new ObservableCollection<AudienceDto>(audiences.Result);
                Teacher = new ObservableCollection<TeacherDto>(teachers.Result);
                Discipline = new ObservableCollection<DisciplineDto>(disciplines.Result);
                Manager = new ObservableCollection<ManagerDto>(managers.Result);
                DepartmentOwner = new ObservableCollection<DepartmentOwnerDto>(departmentOwners);
                Logger.Info("Данные для редактирования базы данных успешно загружены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Данные для редактирования базы данных не были загружены, ошибка : {ex}", "Ошибка загрузки данных", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Данные для редактирования базы данных не были загружены, ошибка : {ex}");
            }
        }

        [RelayCommand]
        private void ShowGrid(object parameter)
        {
            if (parameter is string gridName &&
                Enum.TryParse<ViewType>(gridName, out var viewType))
            {
                CurrentViewType = viewType;

                Logger.Info($"Открыта таблица: {viewType}");
            }
        }
    }
}
