using Microsoft.Win32;
using NLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ZabgcExamsDesktop.API;
using ZabgcExamsDesktop.API.Models;
using ZabgcExamsDesktop.MVVM.Model;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class PrintResultModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ApiService _apiService;
        private readonly PdfReportService _pdfReportService;

        public ICommand SaveToPDFCommand { get; }
        public ICommand LoadDbCommand { get; }
        public ICommand BackToSearch { get; }
        public ICommand SearchCommand { get; }
        public ICommand DeleteSingleItemCommand { get; }

        public PrintResultModel()
        {
            _apiService = new ApiService();
            _pdfReportService = new PdfReportService(_apiService);
            BackToSearch = new RelayCommand(BackToPage);
            SearchCommand = new RelayCommand(async (param) => await SearchAsync());
            SaveToPDFCommand = new RelayCommand(async (param) => await SaveToPdfAsync());
            DeleteSingleItemCommand = new RelayCommand(DeleteSingleItem);
            LoadDbAsync();
        }

        public ObservableCollection<ExamDisplayDto> SearchResults { get; set; } = new();
        public ObservableCollection<DepartmentDto> Departments { get; set; } = new();
        public ObservableCollection<GroupDto> Groups { get; set; } = new();
        public ObservableCollection<GroupDto> FilteredGroups { get; set; } = new();
        public string SelectedResult { get; set; }

        private ObservableCollection<GroupDto> _selectedGroups = new();
        private DepartmentDto _selectedDepartment = new();

        public DepartmentDto SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                _selectedDepartment = value;
                OnPropertyChanged();
                UpdateFilteredGroups();
            }
        }

        public ObservableCollection<GroupDto> SelectedGroups
        {
            get => _selectedGroups;
            set
            {
                _selectedGroups = value;
                OnPropertyChanged();
            }
        }

        public List<string> ResultItems { get; } = new List<string>
        {
            "Стандартный",
            "По модулю",
            "Квалификационный"
        };

        public async Task<List<ExamDisplayDto>> SearchExamsAsync(string typeOfExamName, int? departmentId = null, List<int> groupIds = null)
        {
            try
            {
                var allExams = await _apiService.GetExamsDisplayAsync();

                var filteredExams = allExams.AsEnumerable();

                filteredExams = typeOfExamName switch
                {
                    "Стандартный" => filteredExams.Where(e => e.TypeOfExamName.Contains("стандарт", StringComparison.OrdinalIgnoreCase)),
                    "ПМ" => filteredExams.Where(e => e.TypeOfExamName.Contains("модуль", StringComparison.OrdinalIgnoreCase) ||
                                                     e.TypeOfExamName.Contains("ПМ", StringComparison.OrdinalIgnoreCase)),
                    "Квалификационный" => filteredExams.Where(e => e.TypeOfExamName.Contains("квалификац", StringComparison.OrdinalIgnoreCase)),
                    _ => filteredExams
                };

                if (departmentId.HasValue)
                {
                    filteredExams = filteredExams.Where(e =>
                        Departments.FirstOrDefault(d => d.NameOfDepartment == e.DepartmentName)?.IdDepartment == departmentId);
                }

                if (groupIds != null && groupIds.Any())
                {
                    filteredExams = filteredExams.Where(e => groupIds.Contains(e.IdGroup));
                }

                var result = filteredExams
                    .GroupBy(e => new { e.DepartmentName, e.GroupName })
                    .OrderBy(g => g.Key.DepartmentName)
                    .ThenBy(g => g.Key.GroupName)
                    .SelectMany(g => g
                        .OrderBy(e => e.DateEvent)
                        .ThenBy(e => e.DateEvent.TimeOfDay))
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Search error: {ex.Message}");
                return new List<ExamDisplayDto>();
            }
        }

        private List<int> GetSelectedGroupIds()
        {
            if (FilteredGroups == null || !FilteredGroups.Any())
            {
                return new List<int>();
            }
            var selectedGroups = FilteredGroups.Where(g => g.IsSelected).ToList();
            return selectedGroups.Select(g => g.IdGroup).ToList();
        }

        private async Task SearchAsync()
        {

            try
            {
                if (!ValidateSearchParameters())
                    return;

                var typeOfExamName = GetTypeOfExamName();
                var departmentId = SelectedDepartment?.IdDepartment;
                var selectedGroupIds = GetSelectedGroupIds();
                var results = await SearchExamsAsync(typeOfExamName, departmentId, selectedGroupIds);

                foreach (var item in results)
                {
                    item.IsSelected = false;
                }

                SearchResults = new ObservableCollection<ExamDisplayDto>(results);
                OnPropertyChanged(nameof(SearchResults));

                ShowSearchResultMessage(results.Count);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка поиска: {ex.Message}");
            }
        }

        private bool ValidateSearchParameters()
        {
            if (string.IsNullOrEmpty(SelectedResult))
            {
                MessageBox.Show("Выберите тип экзамена", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private string GetTypeOfExamName()
        {
            return SelectedResult switch
            {
                "Стандартный" => "Стандартный",
                "По модулю" => "ПМ",
                "Квалификационный" => "Квалификационный",
                _ => SelectedResult
            };
        }
        public void UpdateFilteredGroups()
        {
            FilteredGroups = SelectedDepartment == null
                ? new ObservableCollection<GroupDto>(Groups)
                : new ObservableCollection<GroupDto>(Groups.Where(g => g.IdDepartment == SelectedDepartment.IdDepartment));

            OnPropertyChanged(nameof(FilteredGroups));
        }

        private void ShowSearchResultMessage(int count)
        {
            var message = count > 0
                ? $"Найдено {count} экзаменов"
                : "Экзамены по заданным критериям не найдены";

            ShowInfoMessage(message);
        }

        private void ShowInfoMessage(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task SaveToPdfAsync()
        {

            if (!ValidatePdfData()) return;

            var filePath = GetSaveFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            var success = await _pdfReportService.GenerateReportAsync(filePath, SearchResults, SelectedDepartment, SelectedResult);

            if (success)
            {
                ShowSuccessMessage($"Файл успешно сохранен: {filePath}");
                Logger.Info("PDF отчет успешно создан");
            }
            else
            {
                ShowErrorMessage("Ошибка при создании PDF файла");
            }
        }

        private async Task LoadDbAsync()
        {
            try
            {
                Logger.Info("Загрузка данных для печати отчета");

                var filteredGroup = _apiService.GetGroupsAsync();
                await Task.WhenAll(filteredGroup);
                FilteredGroups = new ObservableCollection<GroupDto>(filteredGroup.Result);

                var (departments, groups, exams) = await LoadBasicDataAsync();
                await ProcessLoadedDataAsync(departments, groups, exams);

                Logger.Info("Данные для редактирования базы данных успешно загружены.");
            }
            catch (Exception ex)
            {
                HandleLoadError(ex);
            }
        }

        private void BackToPage(object parameter)
        {
            var searchExamPage = new SearchExamPage();
            SearchExamWindow.pageManager.ChangePage(searchExamPage);
        }

        private bool ValidatePdfData()
        {
            if (SearchResults == null || !SearchResults.Any())
            {
                ShowWarningMessage("Нет данных для сохранения в PDF");
                return false;
            }

            if (SelectedDepartment == null || SelectedResult == null)
            {
                ShowWarningMessage("Выберите отделение и тип отчета");
                return false;
            }

            return true;
        }

        private string GetSaveFilePath()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Расписание экзаменов {DateTime.Now:yyyy-MM-dd}",
                DefaultExt = ".pdf"
            };

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }

        private async Task<(List<DepartmentDto>, List<GroupDto>, List<ExamDisplayDto>)> LoadBasicDataAsync()
        {
            var departmentsTask = _apiService.GetDepartmentsAsync();
            var groupsTask = _apiService.GetGroupsAsync();
            var examsTask = _apiService.GetExamsDisplayAsync();

            await Task.WhenAll(departmentsTask, groupsTask, examsTask);

            return (departmentsTask.Result, groupsTask.Result, examsTask.Result);
        }

        private async Task ProcessLoadedDataAsync(List<DepartmentDto> departments, List<GroupDto> groups, List<ExamDisplayDto> exams)
        {
            foreach (var group in groups)
            {
                group.DepartmentName = departments.FirstOrDefault(d => d.IdDepartment == group.IdDepartment)?.NameOfDepartment ?? "Не указано";
            }

            Departments = new ObservableCollection<DepartmentDto>(departments);
            Groups = new ObservableCollection<GroupDto>(groups);
            SearchResults = new ObservableCollection<ExamDisplayDto>(exams);

            NotifyCollectionsChanged();
        }

        private void DeleteSingleItem(object parameter)
        {
            if (parameter is ExamDisplayDto itemToRemove)
            {
                if (SearchResults.Contains(itemToRemove))
                {
                    SearchResults.Remove(itemToRemove);
                    OnPropertyChanged(nameof(SearchResults));
                    ShowInfoMessage("Строка удалена из отчёта");
                }
            }
        }
        private void NotifyCollectionsChanged()
        {
            OnPropertyChanged(nameof(Departments));
            OnPropertyChanged(nameof(Groups));
            OnPropertyChanged(nameof(SearchResults));
        }

        private void HandleLoadError(Exception ex)
        {
            ShowErrorMessage($"Ошибка загрузки данных: {ex.Message}");
            Logger.Error($"Ошибка загрузки данных: {ex}");
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
