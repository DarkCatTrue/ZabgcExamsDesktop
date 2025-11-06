using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using NLog;
using ZabgcExamsDesktop.MVVM.Model;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Data;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Models;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class DbViewModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        GroupAddWindow groupAddWindow;
        ApplicationDbContext _context;

        private ObservableCollection<Department> _departments;
        private ObservableCollection<Group> _groups;
        private ObservableCollection<Audience> _audiences;
        private ObservableCollection<Teacher> _teachers;
        private ObservableCollection<Discipline> _disciplines;
        private ObservableCollection<Manager> _managers;
        private ObservableCollection<DepartmentOwner> _departmentOwners;
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
            ApplicationDbContext context = new ApplicationDbContext();
            _context = context;
            LoadData();
            BackToExamsCommand = new RelayCommand(BackToExamsPage);
            LoadTableCommand = new RelayCommand(ShowGrid);
            DeleteCommand = new RelayCommand(DeleteItem);
            EditCommand = new RelayCommand(EditItem);
            SaveCommand = new RelayCommand(SaveChanges);
            AddCommand = new RelayCommand(AddNewItem);
            AddNewGroupCommand = new RelayCommand(AddNewGroup);
        }

        private void AddNewGroup(object parameter)
        {
            var department = Departments.FirstOrDefault(d => d.IdDepartment == SelectedDepartmentId);

            if (string.IsNullOrWhiteSpace(EnterGroup))
            {
                MessageBox.Show("Введите название группы!");
                return;
            }

            if (SelectedDepartmentId == null)
            {
                MessageBox.Show("Выберите отделение!");
                return;
            }

            try
            {
                var newGroup = new Group
                {
                    NameOfGroup = EnterGroup.Trim(),
                    IdDepartment = department.IdDepartment,
                };

                _context.Groups.Add(newGroup);
                _context.SaveChanges();
               
                ReloadPage();

                MessageBox.Show($"Группа '{EnterGroup}' успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
               
                Logger.Info($"Группа, {EnterGroup} была добавлена в базу данных.");

                EnterGroup = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Произошла ошибка добавления новой группы: {EnterGroup}, id отделения : {SelectedDepartmentId}");
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

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set
            {
                _departments = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Discipline> Disciplines
        {
            get => _disciplines;
            set
            {
                _disciplines = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Manager> Managers
        {
            get => _managers;
            set
            {
                _managers = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DepartmentOwner> DepartmentOwners
        {
            get => _departmentOwners;
            set
            {
                _departmentOwners = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Group> Groups
        {
            get => _groups;
            set
            {
                _groups = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Audience> Audiences
        {
            get => _audiences;
            set
            {
                _audiences = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Teacher> Teachers
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
            if (DepartmentsVisibility == Visibility.Visible)
                AddItem<Department>(_context.Departments, Departments);
            else if (GroupsVisibility == Visibility.Visible)
                AddGroup();
            else if (AudiencesVisibility == Visibility.Visible)
                AddItem<Audience>(_context.Audiences, Audiences);
            else if (TeachersVisibility == Visibility.Visible)
                AddItem<Teacher>(_context.Teachers, Teachers);
            else if (DisciplinesVisibility == Visibility.Visible)
                AddItem<Discipline>(_context.Disciplines, Disciplines);
            else if (ManagersVisibility == Visibility.Visible)
                AddItem<Manager>(_context.Managers, Managers);
            else if (DepartmentOwnersVisibility == Visibility.Visible)
                AddItem<DepartmentOwner>(_context.DepartmentOwners, DepartmentOwners);
        }

        private void AddItem<T>(DbSet<T> dbSet, ObservableCollection<T> collection) where T : class, new()
        {
            var newItem = new T();
            dbSet.Add(newItem);
            collection.Add(newItem);
            SelectedItem = newItem;
            IsEditing = true;
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

        private void DeleteItem(object parameter)
        {
            var itemToDelete = parameter;
            if (itemToDelete == null) return;

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Remove(itemToDelete);

                    if (itemToDelete is Department department)
                    {
                        Departments.Remove(department);
                        if (SelectedItem == department) SelectedItem = null;
                    }
                    else if (itemToDelete is Group group)
                    {
                        Groups.Remove(group);
                        if (SelectedItem == group) SelectedItem = null;
                    }
                    else if (itemToDelete is Audience audience)
                    {
                        Audiences.Remove(audience);
                        if (SelectedItem == audience) SelectedItem = null;
                    }
                    else if (itemToDelete is DepartmentOwner departmentOwner)
                    {
                        DepartmentOwners.Remove(departmentOwner);
                        if (SelectedItem == departmentOwner) SelectedItem = null;
                    }
                    else if (itemToDelete is Manager managers)
                    {
                        Managers.Remove(managers);
                        if (SelectedItem == managers) SelectedItem = null;
                    }
                    else if (itemToDelete is Discipline discipline)
                    {
                        Disciplines.Remove(discipline);
                        if (SelectedItem == discipline) SelectedItem = null;
                    }

                    _context.SaveChanges();
                    MessageBox.Show("Запись удалена успешно!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Logger.Info($"Запись {itemToDelete} была удалена из базы данных.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    _context.ChangeTracker.Clear();
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

        private void SaveChanges(object parameter)
        {
            try
            {
                _context.SaveChanges();
                IsEditing = false;
                MessageBox.Show("Изменения сохранены успешно!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Logger.Info("Изменения в базе данных были успешно выполнены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Изменения в базе данных не выполнены, ошибка: {ex}");
            }
        }

        private void LoadData()
        {
            try
            {
                Departments = new ObservableCollection<Department>(_context.Departments.ToList());
                Groups = new ObservableCollection<Group>(_context.Groups.ToList());
                Audiences = new ObservableCollection<Audience>(_context.Audiences.ToList());
                Teachers = new ObservableCollection<Teacher>(_context.Teachers.ToList());
                Disciplines = new ObservableCollection<Discipline>(_context.Disciplines.ToList());
                Managers = new ObservableCollection<Manager>(_context.Managers.ToList());
                DepartmentOwners = new ObservableCollection<DepartmentOwner>(_context.DepartmentOwners.ToList());
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
