using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using ZabgcExamsDesktop.MVVM.Model;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public partial class GroupsSelectionViewModel : ObservableObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Action<List<GroupDto>, string, string> _onSave;
        private Window _currentWindow;

        [ObservableProperty] public ObservableCollection<GroupDto> _group;
        [ObservableProperty] public ObservableCollection<GroupDto> _selectedGroups;
        [ObservableProperty] public ObservableCollection<DepartmentDto> _department;
        [ObservableProperty] public ObservableCollection<GroupDto> _filteredGroup;
        
        [ObservableProperty] public DepartmentDto _selectedDepartment;
        [ObservableProperty] public GroupDto _selectedAvailableGroup;
        [ObservableProperty] public GroupDto _selectedGroupToRemove;
        [ObservableProperty] public ICollectionView _groupsView;

        partial void OnSelectedDepartmentChanged(DepartmentDto value) 
        {
            UpdateFilteredGroups();
            SelectedGroups.Clear();
        } 

        public GroupsSelectionViewModel(ObservableCollection<GroupDto> groups, ObservableCollection<DepartmentDto> departments,
                                    string currentDepartmentsText,
                                    string currentGroupsText,
                                    Action<List<GroupDto>, string, string> onSave,
                                    Window currentWindow)
        {
            InitializeCollections(groups, departments);
            _onSave = onSave;
            _currentWindow = currentWindow;
            RestoreSelectedGroupsFromText(currentGroupsText);
            SetupSorting();
        }
        public void SetWindow(Window window)
        {
            _currentWindow = window;
        }
        private void InitializeCollections(ObservableCollection<GroupDto> groups, ObservableCollection<DepartmentDto> departments)
        {
            Group = new ObservableCollection<GroupDto>(groups);
            SelectedGroups = new ObservableCollection<GroupDto>();
            Department = new ObservableCollection<DepartmentDto>(departments);
            SelectedDepartment = new DepartmentDto();
        }

        private void RestoreSelectedGroupsFromText(string currentGroupsText)
        {
            if (string.IsNullOrEmpty(currentGroupsText)) return;

            var selectedNames = currentGroupsText.Split(new[] { ", " }, StringSplitOptions.None);
            var toRemove = FilteredGroup.Where(t => selectedNames.Contains(t.NameOfGroup)).ToList();

            foreach (var group in toRemove)
            {
                FilteredGroup.Remove(group);
                SelectedGroups.Add(group);
            }
        }
       

        public void UpdateFilteredGroups()
        {
            FilteredGroup = SelectedDepartment == null
                ? new ObservableCollection<GroupDto>(Group)
                : new ObservableCollection<GroupDto>(Group.Where(g => g.IdDepartment == SelectedDepartment.IdDepartment));

            OnPropertyChanged(nameof(FilteredGroup));
        }
        private void SetupSorting()
        {
            GroupsView = CollectionViewSource.GetDefaultView(FilteredGroup);
            GroupsView.SortDescriptions.Clear();
            GroupsView.SortDescriptions.Add(new SortDescription("NameOfGroup", ListSortDirection.Ascending));
        }

        [RelayCommand]
        private void AddGroup()
        {
            if (SelectedAvailableGroup != null)
            {
                SelectedGroups.Add(SelectedAvailableGroup);
                FilteredGroup.Remove(SelectedAvailableGroup);
                SelectedAvailableGroup = null;
            }
        }

        [RelayCommand]
        private void RemoveGroup()
        {
            if (SelectedGroupToRemove != null)
            {
                FilteredGroup.Add(SelectedGroupToRemove);
                SelectedGroups.Remove(SelectedGroupToRemove);
                SelectedGroupToRemove = null;
            }
            else
            {
                MessageBox.Show("Выбранные преподаватели не найдены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void SaveAndExit()
        {
            try
            {
                string result = string.Join(", ", SelectedGroups.Select(t => t.NameOfGroup));
                string departments = SelectedDepartment.NameOfDepartment.ToString();
                _onSave?.DynamicInvoke(SelectedGroups.ToList(), result, departments);
                _currentWindow.DialogResult = true;
                _currentWindow.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении групп: {ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Ошибка при добавлении групп: {ex}");
            }
        }
    }
}
