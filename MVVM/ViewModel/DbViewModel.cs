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
using static ZabgcExamsDesktop.MVVM.View.Pages.DataGridColumnsBehavior;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class DbViewModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        GroupAddWindow groupAddWindow;
        private readonly ApiService _apiService;

        private ObservableCollection<DepartmentDto> _departments;
        private ObservableCollection<GroupDto> _groups;
        private ObservableCollection<AudienceDto> _audiences;
        private ObservableCollection<TeacherDto> _teachers;
        private ObservableCollection<DisciplineDto> _disciplines;
        private ObservableCollection<ManagerDto> _managers;
        private ObservableCollection<DepartmentOwnerDto> _departmentOwners;

        private object _selectedItem;
        private string _enterGroup;
        private bool _isEditing = false;

        private object _currentItems;
        public object CurrentItems
        {
            get => _currentItems;
            set { _currentItems = value; OnPropertyChanged(); }
        }

        private ViewType _currentViewType;
        public ViewType CurrentViewType
        {
            get => _currentViewType;
            set
            {
                _currentViewType = value;
                OnPropertyChanged();
                UpdateItems();
            }
        }


        public ICommand LoadTableCommand { get; }
        public ICommand BackToExamsCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand AddNewGroupCommand { get; }

        private void UpdateItems()
        {
            CurrentItems = CurrentViewType switch
            {
                ViewType.Departments => Departments,
                ViewType.Groups => Groups,
                ViewType.Audiences => Audiences,
                ViewType.Teachers => Teachers,
                ViewType.Disciplines => Disciplines,
                ViewType.Managers => Managers,
                ViewType.DepartmentOwners => DepartmentOwners,
                _ => null
            };
        }
        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }
        public int? SelectedDepartmentId { get; set; }

        public string EnterGroup
        {
            get => _enterGroup;
            set
            {
                _enterGroup = value;
                OnPropertyChanged();
            }
        }
        public DbViewModel()
        {
            _apiService = new ApiService();
            LoadData();
            BackToExamsCommand = new RelayCommand(BackToExamsPage);
            LoadTableCommand = new RelayCommand(ShowGrid);
            DeleteCommand = new RelayCommand(DeleteItem);
            EditCommand = new RelayCommand(EditItem);
            SaveCommand = new RelayCommand(SaveItem);
            AddCommand = new RelayCommand(AddNewItem);
            AddNewGroupCommand = new RelayCommand(AddNewGroup);
        }

        private async void AddNewGroup(object parameter)
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
                var department = Departments.FirstOrDefault(d => d.IdDepartment == SelectedDepartmentId);
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

        public void BackToExamsPage(object parameter)
        {
            Page SearchExam = new SearchExamPage();
            SearchExamWindow.pageManager.ChangePage(SearchExam);
        }

        public ObservableCollection<DepartmentDto> Departments
        {
            get => _departments;
            set
            {
                _departments = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DisciplineDto> Disciplines
        {
            get => _disciplines;
            set
            {
                _disciplines = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ManagerDto> Managers
        {
            get => _managers;
            set
            {
                _managers = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DepartmentOwnerDto> DepartmentOwners
        {
            get => _departmentOwners;
            set
            {
                _departmentOwners = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<GroupDto> Groups
        {
            get => _groups;
            set
            {
                _groups = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AudienceDto> Audiences
        {
            get => _audiences;
            set
            {
                _audiences = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<TeacherDto> Teachers
        {
            get => _teachers;
            set
            {
                _teachers = value;
                OnPropertyChanged();
            }
        }
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEditing));
            }
        }
        public bool IsNotEditing => !IsEditing;

        private void AddNewItem(object parameter)
        {
            IsEditing = true;

            switch (CurrentViewType)
            {
                case ViewType.Groups:
                    AddGroup();
                    break;

                case ViewType.Teachers:
                    AddItem(Teachers, new TeacherDto());
                    break;

                case ViewType.Audiences:
                    AddItem(Audiences, new AudienceDto());
                    break;

                case ViewType.Disciplines:
                    AddItem(Disciplines, new DisciplineDto());
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

        private async void SaveItem(object parameter)
        {
            try
            {
                switch (CurrentViewType)
                {
                    case ViewType.Managers:
                        await SaveAllItems(Managers, _apiService.CreateManagerAsync, _apiService.UpdateManagerAsync); 
                        break;
                    
                    case ViewType.DepartmentOwners:
                        await SaveAllItems(DepartmentOwners, _apiService.CreateDepartmentOwnerAsync, _apiService.UpdateDepartmentOwnerAsync); 
                        break;

                    case ViewType.Departments:
                        await SaveAllItems(Departments, _apiService.CreateDepartmentAsync, _apiService.UpdateDepartmentAsync);
                        break;

                    case ViewType.Groups:
                        await SaveAllItems(Groups, _apiService.CreateGroupAsync, _apiService.UpdateGroupAsync);
                        break;

                    case ViewType.Teachers:
                        await SaveAllItems(Teachers, _apiService.CreateTeacherAsync, _apiService.UpdateTeacherAsync);
                        break;

                    case ViewType.Audiences:
                        await SaveAllItems(Audiences, _apiService.CreateAudienceAsync, _apiService.UpdateAudienceAsync);
                        break;

                    case ViewType.Disciplines:
                        await SaveAllItems(Disciplines, _apiService.CreateDisciplineAsync, _apiService.UpdateDisciplineAsync);
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
            if (groupAddWindow == null || !groupAddWindow.IsLoaded)
            {
                groupAddWindow = new GroupAddWindow();
                groupAddWindow.Closed += (s, args) => groupAddWindow = null;
                groupAddWindow.Show();
            }
            else
            {
                groupAddWindow.Activate();
                groupAddWindow.Focus();
            }
        }

        private async void DeleteItem(object parameter)
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
                            Groups.Remove(g);
                            success = await _apiService.DeleteGroupAsync(g.IdGroup);
                            Logger.Warn($"Группа '{g.IdGroup}' была удалена из базы данных.");
                            break;

                        case AudienceDto a:
                            Audiences.Remove(a);
                            success = await _apiService.DeleteAudienceAsync(a.IdAudience);
                            Logger.Warn($"Аудитория '{a.IdAudience}' была удалена из базы данных.");
                            break;

                        case TeacherDto t:
                            Teachers.Remove(t);
                            success = await _apiService.DeleteTeacherAsync(t.IdTeacher);
                            Logger.Warn($"Преподаватель '{t.IdTeacher}' был удален из базы данных.");
                            break;

                        case DisciplineDto d:
                            Disciplines.Remove(d);
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
        private void EditItem(object parameter)
        {
            SelectedItem = parameter;
            if (SelectedItem != null)
            {
                IsEditing = true;
            }
        }
        private async Task LoadData()
        {
            try
            {

                var departmentsTask = _apiService.GetDepartmentsAsync();
                var groupsTask = _apiService.GetGroupsAsync();
                var teachers = _apiService.GetTeachersAsync();
                var audiences = _apiService.GetAudiencesAsync();
                var typeOfExams = _apiService.GetTypeOfExamsAsync();
                var disciplines = _apiService.GetDisciplinesAsync();
                var managers = _apiService.GetManagersAsync();
                var departmentOwnersTask = _apiService.GetDepartmentOwnersAsync();

                await Task.WhenAll(departmentsTask, groupsTask, teachers, audiences, managers, typeOfExams, disciplines, departmentOwnersTask);

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

                Departments = new ObservableCollection<DepartmentDto>(departments);
                Groups = new ObservableCollection<GroupDto>(groups);
                Audiences = new ObservableCollection<AudienceDto>(audiences.Result);
                Teachers = new ObservableCollection<TeacherDto>(teachers.Result);
                Disciplines = new ObservableCollection<DisciplineDto>(disciplines.Result);
                Managers = new ObservableCollection<ManagerDto>(managers.Result);
                DepartmentOwners = new ObservableCollection<DepartmentOwnerDto>(departmentOwners);
                Logger.Info("Данные для редактирования базы данных успешно загружены.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Данные для редактирования базы данных не были загружены, ошибка : {ex}");
            }
        }

        private void ShowGrid(object parameter)
        {
            if (parameter is string gridName &&
                Enum.TryParse<ViewType>(gridName, out var viewType))
            {
                CurrentViewType = viewType;

                Logger.Info($"Открыта таблица: {viewType}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
