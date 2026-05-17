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

        private readonly Action<List<GroupDto>, string> _onSave;
        private Window _currentWindow;

        [ObservableProperty] public ObservableCollection<GroupDto> _group;
        [ObservableProperty] public ObservableCollection<GroupDto> _selectedGroups;
        [ObservableProperty] public GroupDto _selectedAvailableGroup;
        [ObservableProperty] public GroupDto _selectedGroupToRemove;
        [ObservableProperty] public ICollectionView _groupsView;

        public GroupsSelectionViewModel(ObservableCollection<GroupDto> groups,
                                    string currentGroupsText,
                                    Action<List<GroupDto>, string> onSave,
                                    Window currentWindow)
        {
            InitializeCollections(groups);
            _onSave = onSave;
            _currentWindow = currentWindow;
            RestoreSelectedGroupsFromText(currentGroupsText);
            SetupSorting();
        }
        public void SetWindow(Window window)
        {
            _currentWindow = window;
        }

        private void InitializeCollections(ObservableCollection<GroupDto> groups)
        {
            Group = new ObservableCollection<GroupDto>(groups);
            SelectedGroups = new ObservableCollection<GroupDto>();
        }

        private void RestoreSelectedGroupsFromText(string currentGroupsText)
        {
            if (string.IsNullOrEmpty(currentGroupsText)) return;

            var selectedNames = currentGroupsText.Split(new[] { ", " }, StringSplitOptions.None);
            var toRemove = Group.Where(t => selectedNames.Contains(t.NameOfGroup)).ToList();

            foreach (var group in toRemove)
            {
                Group.Remove(group);
                SelectedGroups.Add(group);
            }
        }
        private void SetupSorting()
        {
            GroupsView = CollectionViewSource.GetDefaultView(Group);
            GroupsView.SortDescriptions.Clear();
            GroupsView.SortDescriptions.Add(new SortDescription("NameOfGroup", ListSortDirection.Ascending));
        }

        [RelayCommand]
        private void AddGroup()
        {
            if (SelectedAvailableGroup != null)
            {
                SelectedGroups.Add(SelectedAvailableGroup);
                Group.Remove(SelectedAvailableGroup);
                SelectedAvailableGroup = null;
            }
        }

        [RelayCommand]
        private void RemoveGroup()
        {
            if (SelectedGroupToRemove != null)
            {
                Group.Add(SelectedGroupToRemove);
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
                _onSave?.DynamicInvoke(SelectedGroups.ToList(), result);
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
