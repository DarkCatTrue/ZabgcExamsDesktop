using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ZabgcExamsDesktop.MVVM.Model;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public partial class TeacherSelectionViewModel : ObservableObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Action<List<TeacherDto>, string> _onSave;
        private Window _currentWindow;

        [ObservableProperty] public ObservableCollection<TeacherDto> _teacher;
        [ObservableProperty] public ObservableCollection<TeacherDto> _selectedTeachers;
        [ObservableProperty] public TeacherDto _selectedAvailableTeacher;
        [ObservableProperty] public TeacherDto _selectedTeacherToRemove;
        [ObservableProperty] public ICollectionView _teachersView;

        public TeacherSelectionViewModel(ObservableCollection<TeacherDto> allTeachers,
                                    string currentTeachersText,
                                    Action<List<TeacherDto>, string> onSave,
                                    Window currentWindow)
        {
            InitializeCollections(allTeachers);
            _onSave = onSave;
            _currentWindow = currentWindow;

            RestoreSelectedTeachersFromText(currentTeachersText);
            SetupSorting();
        }
        public void SetWindow(Window window)
        {
            _currentWindow = window;
        }

        private void InitializeCollections(ObservableCollection<TeacherDto> allTeachers)
        {
            Teacher = new ObservableCollection<TeacherDto>(allTeachers);
            SelectedTeachers = new ObservableCollection<TeacherDto>();
        }

        private void RestoreSelectedTeachersFromText(string currentTeachersText)
        {
            if (string.IsNullOrEmpty(currentTeachersText)) return;

            var selectedNames = currentTeachersText.Split(new[] { ", " }, StringSplitOptions.None);
            var toRemove = Teacher.Where(t => selectedNames.Contains(t.FullName)).ToList();

            foreach (var teacher in toRemove)
            {
                Teacher.Remove(teacher);
                SelectedTeachers.Add(teacher);
            }
        }

        private void SetupSorting()
        {
            TeachersView = CollectionViewSource.GetDefaultView(Teacher);
            TeachersView.SortDescriptions.Clear();
            TeachersView.SortDescriptions.Add(new SortDescription("FullName", ListSortDirection.Ascending));
        }

        [RelayCommand]
        private void AddTeacher()
        {
            if (SelectedAvailableTeacher != null)
            {
                SelectedTeachers.Add(SelectedAvailableTeacher);
                Teacher.Remove(SelectedAvailableTeacher);
                SelectedAvailableTeacher = null;
            }
        }

        [RelayCommand]
        private void RemoveTeacher()
        {
            if (SelectedTeacherToRemove != null)
            {
                Teacher.Add(SelectedTeacherToRemove);
                SelectedTeachers.Remove(SelectedTeacherToRemove);
                SelectedTeacherToRemove = null;
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
                string result = string.Join(", ", SelectedTeachers.Select(t => t.FullName));
                _onSave?.DynamicInvoke(SelectedTeachers.ToList(), result);
                _currentWindow.DialogResult = true;
                _currentWindow.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении членов ЭК: {ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Ошибка при добавлении членов ЭК: {ex}");
            }
        }
    }
}
