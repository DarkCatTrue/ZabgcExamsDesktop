using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NLog;
using System.Collections.ObjectModel;
using System.Windows;
using ZabgcExamsDesktop.MVVM.Model;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;
using ZabgcExamsDesktop.Services;
using ZabgcExamsDesktop.Services.API;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public partial class PrintResultModel : ObservableObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ApiService _apiService;
        private readonly PdfReportService _pdfReportService;
        public string SelectedResult { get; set; }

        [ObservableProperty] public ObservableCollection<ExamDisplayDto> _searchResult;
        [ObservableProperty] public ObservableCollection<GroupDto> _group;
        [ObservableProperty] public ObservableCollection<DepartmentDto> _department;
        [ObservableProperty] public ObservableCollection<GroupDto> _filteredGroup;
        [ObservableProperty] private ObservableCollection<GroupDto> _selectedGroup;

        [ObservableProperty] private List<GroupDto> _selectedGroups = new();
        [ObservableProperty] private string _groupsText = string.Empty;
        [ObservableProperty] private string _departmentsText = string.Empty;

        [ObservableProperty] public DepartmentDto _selectedDepartment;

        public PrintResultModel()
        {
            _apiService = new ApiService();
            _pdfReportService = new PdfReportService(_apiService);
            _ = LoadDbAsync();
        }

        public List<string> ResultItems { get; } = new List<string>
        {
            "Стандартный",
            "По модулю",
            "Квалификационный"
        };

        [RelayCommand]
        private void SelectionGroups()
        {
            var dialog = new GroupsSelectionWindow(Group, Department, GroupsText, DepartmentsText, (selectedGroups, groupsText, departmentsText) =>
            {
                SelectedGroups = selectedGroups;
                GroupsText = groupsText;
                DepartmentsText = departmentsText;
            });
            dialog.ShowDialog();
        }
        [RelayCommand]
        private async Task SearchAsync()
        {
            try
            {
                if (!ValidateSearchParameters())
                    return;

                var typeOfExamName = GetTypeOfExamName();
                var departmentId = SelectedDepartment?.IdDepartment;
                var groupIds = SelectedGroups?.Select(g => g.IdGroup).ToList() ?? new List<int>();

                var results = await SearchExamsAsync(typeOfExamName, departmentId, groupIds);
                SearchResult = new ObservableCollection<ExamDisplayDto>(results);
                OnPropertyChanged(nameof(SearchResult));
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка поиска: {ex.Message}");
            }
        }

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
                        Department.FirstOrDefault(d => d.NameOfDepartment == e.DepartmentName)?.IdDepartment == departmentId);
                }

                if (groupIds != null && groupIds.Any())
                {
                    filteredExams = filteredExams.Where(e => groupIds.Contains(e.IdGroup));
                }

                var result = filteredExams
                    .GroupBy(e => new { e.DepartmentName, e.GroupName })
                    .OrderBy(g => g.Key.DepartmentName)
                    .ThenBy(g => g.Key.GroupName)
                    .SelectMany(g => g.OrderBy(e => e.DateEvent).ThenBy(e => e.DateEvent.TimeOfDay))
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Search error: {ex.Message}");
                return new List<ExamDisplayDto>();
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

        private void ShowInfoMessage(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task SavePdfAsync()
        {

            if (!ValidatePdfData()) return;

            var filePath = GetSaveFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            var success = await _pdfReportService.GenerateReportAsync(filePath, SearchResult, DepartmentsText, SelectedResult);

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

                var (departments, groups, exams) = await LoadBasicDataAsync();
                await ProcessLoadedDataAsync(departments, groups, exams);

                SelectedGroups = new List<GroupDto>();
                GroupsText = string.Empty;
                DepartmentsText = string.Empty;
            }
            catch (Exception ex)
            {
                HandleLoadError(ex);
            }
        }

        [RelayCommand]
        private void BackToSearch(object parameter)
        {
            var searchExamPage = new SearchExamPage();
            SearchExamWindow.pageManager.ChangePage(searchExamPage);
        }

        private bool ValidatePdfData()
        {
            if (SearchResult == null || !SearchResult.Any())
            {
                ShowWarningMessage("Нет данных для сохранения в PDF");
                return false;
            }

            if (DepartmentsText == null || SelectedResult == null)
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

            Department = new ObservableCollection<DepartmentDto>(departments);
            Group = new ObservableCollection<GroupDto>(groups);
            SearchResult = new ObservableCollection<ExamDisplayDto>(exams);

            NotifyCollectionsChanged();
        }

        [RelayCommand]
        private void DeleteItem(object parameter)
        {
            if (parameter is ExamDisplayDto itemToRemove)
            {
                if (SearchResult.Contains(itemToRemove))
                {
                    SearchResult.Remove(itemToRemove);
                    OnPropertyChanged(nameof(SearchResult));
                    ShowInfoMessage("Строка удалена из отчёта");
                }
            }
        }
        private void NotifyCollectionsChanged()
        {
            OnPropertyChanged(nameof(Department));
            OnPropertyChanged(nameof(Group));
            OnPropertyChanged(nameof(SearchResult));
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
    }
}
