using NLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ZabgcExamsDesktop.MVVM.Model;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class TeacherSelectionViewModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Action<List<TeacherDto>, string> _onSave;
        private Window _currentWindow;

        public ObservableCollection<TeacherDto> Teacher { get; set; }
        public ObservableCollection<TeacherDto> SelectedTeachers { get; set; }

        private TeacherDto _selectedAvailableTeacher;
        public TeacherDto SelectedAvailableTeacher
        {
            get => _selectedAvailableTeacher;
            set { _selectedAvailableTeacher = value; OnPropertyChanged(); }
        }

        private TeacherDto _selectedTeacherToRemove;
        public TeacherDto SelectedTeacherToRemove
        {
            get => _selectedTeacherToRemove;
            set { _selectedTeacherToRemove = value; OnPropertyChanged(); }
        }

        private ICollectionView _teachersView;
        public ICollectionView TeachersView
        {
            get => _teachersView;
            set { _teachersView = value; OnPropertyChanged(); }
        }

        public ICommand AddTeacherCommand { get; set; }
        public ICommand RemoveTeacherCommand { get; set; }
        public ICommand SaveNewTeachersCommand { get; set; }

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
            InitializeCommands();
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

        private void InitializeCommands()
        {
            AddTeacherCommand = new RelayCommand(_ => AddTeacher());
            RemoveTeacherCommand = new RelayCommand(_ => RemoveTeacher());
            SaveNewTeachersCommand = new RelayCommand(_ => SaveAndClose());
        }

        private void AddTeacher()
        {
            if (SelectedAvailableTeacher != null)
            {
                SelectedTeachers.Add(SelectedAvailableTeacher);
                Teacher.Remove(SelectedAvailableTeacher);
                SelectedAvailableTeacher = null;
            }
        }

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

        private void SaveAndClose()
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
