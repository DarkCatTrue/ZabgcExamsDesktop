using Microsoft.EntityFrameworkCore;
using NLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using ZabgcExamsDesktop.API;
using ZabgcExamsDesktop.API.Models;
using ZabgcExamsDesktop.MVVM.Model;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;

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



        private Visibility _departmentsVisbility = Visibility.Collapsed;
        private Visibility _groupsVisibility = Visibility.Visible;
        private Visibility _audiencesVisibility = Visibility.Collapsed;
        private Visibility _teachersVisibility = Visibility.Collapsed;
        private Visibility _disciplinesVisibility = Visibility.Collapsed;
        private Visibility _managersVisibility = Visibility.Collapsed;
        private Visibility _departmentOwnersVisibility = Visibility.Collapsed;


        public ICommand LoadTableCommand { get; }
        public ICommand BackToExamsCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand AddNewGroupCommand { get; }


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


        public Visibility DepartmentsVisibility
        {
            get => _departmentsVisbility;
            set
            {
                _departmentsVisbility = value;
                OnPropertyChanged();
            }
        }

        public Visibility GroupsVisibility
        {
            get => _groupsVisibility;
            set
            {
                _groupsVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility DisciplinesVisibility
        {
            get => _disciplinesVisibility;
            set
            {
                _disciplinesVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility AudiencesVisibility
        {
            get => _audiencesVisibility;
            set
            {
                _audiencesVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility TeachersVisibility
        {
            get => _teachersVisibility;
            set
            {
                _teachersVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility DepartmentOwnersVisibility
        {
            get => _departmentOwnersVisibility;
            set
            {
                _departmentOwnersVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility ManagersVisibility
        {
            get => _managersVisibility;
            set
            {
                _managersVisibility = value;
                OnPropertyChanged();
            }
        }

        private void AddNewItem(object parameter)
        {
            IsEditing = true;

            if (DepartmentsVisibility == Visibility.Visible)
            {
                var newItem = new DepartmentDto { IsNew = true, IsEditing = true };
                Departments.Add(newItem);
                SelectedItem = newItem;
            }
            else if (GroupsVisibility == Visibility.Visible)
            {
                AddGroup();
            }
            else if (TeachersVisibility == Visibility.Visible)
            {
                var newItem = new TeacherDto { IsNew = true, IsEditing = true };
                Teachers.Add(newItem);
                SelectedItem = newItem;
            }
            else if (AudiencesVisibility == Visibility.Visible)
            {
                var newItem = new AudienceDto { IsNew = true, IsEditing = true };
                Audiences.Add(newItem);
                SelectedItem = newItem;
            }
            else if (DisciplinesVisibility == Visibility.Visible)
            {
                var newItem = new DisciplineDto { IsNew = true, IsEditing = true };
                Disciplines.Add(newItem);
                SelectedItem = newItem;
            }
        }

        private async void SaveItem(object parameter)
        {
            try
            {
                if (DepartmentsVisibility == Visibility.Visible)
                {
                    await SaveAllNewItems(Departments, "кафедр");
                }
                else if (GroupsVisibility == Visibility.Visible)
                {
                    await SaveAllNewItems(Groups, "групп");
                }
                else if (TeachersVisibility == Visibility.Visible)
                {
                    await SaveAllNewItems(Teachers, "преподавателей");
                }
                else if (AudiencesVisibility == Visibility.Visible)
                {
                    await SaveAllNewItems(Audiences, "аудиторий");
                }
                else if (DisciplinesVisibility == Visibility.Visible)
                {
                    await SaveAllNewItems(Disciplines, "дисциплин");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private async Task SaveAllNewItems<T>(ObservableCollection<T> collection, string entityName) where T : BaseDto
        {
            var newItems = collection.Where(x => x.IsNew).ToList();

            if (!newItems.Any())
            {
                MessageBox.Show($"Нет новых {entityName} для сохранения");
                return;
            }

            var invalidItems = newItems.Where(item => !IsItemValid(item)).ToList();
            if (invalidItems.Any())
            {
                MessageBox.Show($"Заполните все обязательные поля для новых {entityName}");
                return;
            }

            int successCount = 0;
            int errorCount = 0;

            foreach (var item in newItems)
            {
                try
                {
                    bool success = await SaveSingleItem(item);

                    if (success)
                    {
                        successCount++;
                        item.IsNew = false;
                        item.IsEditing = false;
                    }
                    else
                    {
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Console.WriteLine($"Ошибка сохранения {entityName}: {ex.Message}");
                }
            }

            await LoadData();

            string message = $"Сохранено {successCount} из {newItems.Count} {entityName}";
            if (errorCount > 0)
            {
                message += $"\nНе удалось сохранить {errorCount} {entityName}";
            }
            MessageBox.Show(message, "Результат сохранения");
        }

        private bool IsItemValid<T>(T item) where T : BaseDto
        {
            return item switch
            {
                DepartmentDto department => !string.IsNullOrWhiteSpace(department.NameOfDepartment),
                GroupDto group => !string.IsNullOrWhiteSpace(group.NameOfGroup) && group.IdDepartment > 0,
                TeacherDto teacher => !string.IsNullOrWhiteSpace(teacher.FullName),
                AudienceDto audience => !string.IsNullOrWhiteSpace(audience.NumberAudience.ToString()),
                DisciplineDto discipline => !string.IsNullOrWhiteSpace(discipline.NameDiscipline),
                _ => false
            };
        }

        private async Task<bool> SaveSingleItem<T>(T item) where T : BaseDto
        {
            return item switch
            {
                DepartmentDto department => await _apiService.CreateDepartmentAsync(new DepartmentDto
                { NameOfDepartment = department.NameOfDepartment.Trim() }),

                GroupDto group => await _apiService.CreateGroupAsync(new GroupDto
                {
                    NameOfGroup = group.NameOfGroup.Trim(),
                    IdDepartment = group.IdDepartment
                }),

                TeacherDto teacher => await _apiService.CreateTeacherAsync(new TeacherDto
                { FullName = teacher.FullName.Trim() }),

                AudienceDto audience => await _apiService.CreateAudienceAsync(new AudienceDto
                { NumberAudience = audience.NumberAudience }),

                DisciplineDto discipline => await _apiService.CreateDisciplineAsync(new DisciplineDto
                { NameDiscipline = discipline.NameDiscipline.Trim() }),

                _ => false
            };
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
                    string entityName = "";
                    string itemName = "";

                    if (parameter is DepartmentDto department)
                    {
                        success = await _apiService.DeleteDepartmentAsync(department.IdDepartment);
                        entityName = "кафедру";
                        itemName = department.NameOfDepartment;
                        Logger.Warn($"Кафедра '{itemName}' была удалена из базы данных.");
                    }
                    else if (parameter is GroupDto group)
                    {
                        success = await _apiService.DeleteGroupAsync(group.IdGroup);
                        entityName = "группу";
                        itemName = group.NameOfGroup;
                        Logger.Warn($"Группа '{itemName}' была удалена из базы данных.");
                    }
                    else if (parameter is AudienceDto audience)
                    {
                        success = await _apiService.DeleteAudienceAsync(audience.IdAudience);
                        entityName = "аудиторию";
                        itemName = audience.NumberAudience.ToString();
                        Logger.Warn($"Аудитория '{itemName}' была удалена из базы данных.");
                    }
                    else if (parameter is TeacherDto teacher)
                    {
                        success = await _apiService.DeleteTeacherAsync(teacher.IdTeacher);
                        entityName = "преподавателя";
                        itemName = teacher.FullName;
                        Logger.Warn($"Преподаватель '{itemName}' была удален из базы данных.");
                    }
                    else if (parameter is DisciplineDto discipline)
                    {
                        success = await _apiService.DeleteDisciplineAsync(discipline.IdDiscipline);
                        entityName = "дисциплину";
                        itemName = discipline.NameDiscipline;
                        Logger.Warn($"Дисциплина '{itemName}' была удалена из базы данных.");
                    }

                    if (success)
                    {
                        await LoadData();
                        MessageBox.Show($"{entityName} '{itemName}' удалена успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Не удалось удалить {entityName}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (parameter is string gridName)
            {

                GroupsVisibility = Visibility.Collapsed;
                DepartmentsVisibility = Visibility.Collapsed;
                AudiencesVisibility = Visibility.Collapsed;
                TeachersVisibility = Visibility.Collapsed;
                DisciplinesVisibility = Visibility.Collapsed;
                DepartmentOwnersVisibility = Visibility.Collapsed;
                ManagersVisibility = Visibility.Collapsed;
                switch (gridName)
                {
                    case "Groups":
                        GroupsVisibility = Visibility.Visible;
                        Logger.Info("Была открыта таблица групп");
                        break;
                    case "Departments":
                        DepartmentsVisibility = Visibility.Visible;
                        Logger.Info("Была открыта таблица отделений");
                        break;
                    case "Audiences":
                        AudiencesVisibility = Visibility.Visible;
                        Logger.Info("Была открыта таблица аудиторий");
                        break;
                    case "Teachers":
                        TeachersVisibility = Visibility.Visible;
                        Logger.Info("Была открыта таблица преподавателей");
                        break;
                    case "Disciplines":
                        DisciplinesVisibility = Visibility.Visible;
                        Logger.Info("Была открыта таблица дисциплин");
                        break;
                    case "Managers":
                        ManagersVisibility = Visibility.Visible;
                        Logger.Info("Была открыта таблица менеджеров");
                        break;
                    case "DepartmentOwners":
                        DepartmentOwnersVisibility = Visibility.Visible;
                        Logger.Info("Была открыта таблица зав.Отделений");
                        break;
                }

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
