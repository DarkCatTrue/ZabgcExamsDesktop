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
            SaveCommand = new RelayCommand(SaveChanges);
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
            var service = GetCurrentService();
            if (service == null) return;

            AddItemToGrid(service);
        }

        private void AddItemToGrid<T>(IEntityService<T> service) where T : BaseDto
        {
            var newItem = service.CreateNewItem();
            var collection = GetCurrentCollection<T>();

            if (collection != null)
            {
                collection.Add(newItem);
                SelectedItem = newItem;
                IsEditing = true;
            }
        }

        private ObservableCollection<T> GetCurrentCollection<T>() where T : BaseDto
        {
            return typeof(T).Name switch
            {
                nameof(DepartmentDto) => Departments as ObservableCollection<T>,
                nameof(GroupDto) => Groups as ObservableCollection<T>,
                nameof(TeacherDto) => Teachers as ObservableCollection<T>,
                nameof(AudienceDto) => Audiences as ObservableCollection<T>,
                nameof(DisciplineDto) => Disciplines as ObservableCollection<T>,
                _ => null
            };
        }

        private async void SaveChanges(object parameter)
        {
            try
            {
                if (SelectedItem is not BaseDto selectedDto) return;

                var service = GetServiceForItem(SelectedItem);
                if (service == null) return;

                if (selectedDto.IsNew)
                {
                    await SaveNewItem(service, selectedDto);
                }
                else
                {
                    await UpdateExistingItem(service, selectedDto);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
                Logger.Error($"Ошибка сохранения: {ex}");
            }
        }

        private async Task SaveNewItem<T>(IEntityService<T> service, T item) where T : BaseDto
        {
            if (!ValidateItemBeforeSave(item))
            {
                MessageBox.Show("Заполните все обязательные поля", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (await service.CreateAsync(item))
            {
                item.IsNew = false;
                item.IsEditing = false;
                item.IsPersisted = true;

                MessageBox.Show("Элемент сохранен успешно!", "Успех");
                Logger.Info($"{service.GetEntityName()} создана");
            }
        }

        private async Task UpdateExistingItem<T>(IEntityService<T> service, T item) where T : BaseDto
        {
            if (await service.UpdateAsync(item))
            {
                item.IsEditing = false;
                MessageBox.Show("Изменения сохранены успешно!", "Успех");
                Logger.Info($"{service.GetEntityName()} обновлена");
            }
        }

        private bool ValidateItemBeforeSave<T>(T item) where T : BaseDto
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
            if (parameter is not BaseDto item) return;

            // Если это временный объект - просто удаляем из коллекции
            if (item.IsNew && !item.IsPersisted)
            {
                var collection = GetCurrentCollection(item.GetType());
                collection?.Remove(item);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await DeleteItemAsync(item);
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

        private async void SaveChanges(object parameter)
        {
            try
            {
                bool allSaved = true;

                if (SelectedItem is DepartmentDto department && IsEditing)
                {
                    allSaved &= await _apiService.UpdateDepartmentAsync(department);
                }
                else if (SelectedItem is GroupDto group && IsEditing)
                {
                    allSaved &= await _apiService.UpdateGroupAsync(group);
                }
                else if (SelectedItem is TeacherDto teacher && IsEditing)
                {
                    allSaved &= await _apiService.UpdateTeacherAsync(teacher);
                }
                else if (SelectedItem is AudienceDto audience && IsEditing)
                {
                    allSaved &= await _apiService.UpdateAudienceAsync(audience);
                }
                else if (SelectedItem is DisciplineDto discipline && IsEditing)
                {
                    allSaved &= await _apiService.UpdateDisciplineAsync(discipline);
                }

                if (allSaved)
                {
                    IsEditing = false;
                    MessageBox.Show("Изменения сохранены успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Logger.Info("Изменения в базе данных были успешно выполнены.");
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить некоторые изменения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Изменения в базе данных не выполнены, ошибка: {ex}");
            }
        }

        private async Task LoadData()
        {
            try
            {

                var departments = _apiService.GetDepartmentsAsync();
                var groups = _apiService.GetGroupsAsync();
                var teachers = _apiService.GetTeachersAsync();
                var audiences = _apiService.GetAudiencesAsync();
                var typeOfExams = _apiService.GetTypeOfExamsAsync();
                var disciplines = _apiService.GetDisciplinesAsync();
                var managers = _apiService.GetManagersAsync();
                var departmentOwners = _apiService.GetDepartmentOwnersAsync();
                
                await Task.WhenAll(departments, groups, teachers, audiences, managers, typeOfExams, disciplines, departmentOwners);

                Departments = new ObservableCollection<DepartmentDto>(departments.Result);
                Groups = new ObservableCollection<GroupDto>(groups.Result);
                Audiences = new ObservableCollection<AudienceDto>(audiences.Result);
                Teachers = new ObservableCollection<TeacherDto>(teachers.Result);
                Disciplines = new ObservableCollection<DisciplineDto>(disciplines.Result);
                Managers = new ObservableCollection<ManagerDto>(managers.Result);
                DepartmentOwners = new ObservableCollection<DepartmentOwnerDto>(departmentOwners.Result);
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
