using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using ZabgcExamsDesktop.MVVM.ViewModel;

namespace ZabgcExamsDesktop.MVVM.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для ResultPage.xaml
    /// </summary>
    public partial class ResultPage : Page
    {
        public ResultPage()
        {
            InitializeComponent();
            DataContext = new PrintResultModel();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        private void DelFromDataGridBtn_Click(Object sender, RoutedEventArgs e) 
        {

        }

        //private async void SearchBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    var viewModel = DataContext as PrintResultModel;
        //    if (viewModel == null) return;
        //    var selectedDepartment = DepartmentComboBox.SelectedItem as Department;
        //    var selectedGroups = GroupsListBox.SelectedItems.Cast<Group>().ToList();

        //    switch (viewModel.SelectedResult)
        //    {
        //        case "Стандартный":
        //            await SearchExamAsync("Стандартный");
        //            return;
        //        case "По модулю":
        //            await SearchExamAsync("ПМ");
        //            return;
        //        case "Квалификационный":
        //            await SearchExamAsync("Квалификационный");
        //            return;
        //    }
        //}
        //private async Task SearchExamAsync(string TypeOfExamName)
        //{
        //    var viewModel = DataContext as PrintResultModel;
        //    if (viewModel == null) return;
        //    var selectedDepartment = DepartmentComboBox.SelectedItem as Department;
        //    var selectedGroups = GroupsListBox.SelectedItems.Cast<Group>().ToList();

        //    using (var context = new ApplicationDbContext())
        //    {
        //        var disciplines = await context.Disciplines
        //        .AsNoTracking()
        //        .ToDictionaryAsync(d => d.IdDiscipline, d => d);

        //        context.ChangeTracker.Clear();
        //        var query = context.Exams
        //        .Include(e => e.IdGroupNavigation)
        //        .ThenInclude(g => g.IdDepartmentNavigation)
        //        .Include(e => e.IdTeachers)
        //        .Include(e => e.IdAudienceNavigation)
        //        .Include(e => e.IdDisciplineNavigation)
        //        .Include(e => e.IdQualificationNavigation)
        //        .Include(e => e.IdTypeOfExamNavigation)
        //        .Include(e => e.IdTypeOfLessonNavigation)
        //        .AsNoTracking()
        //        .AsQueryable().Where(e => e.IdTypeOfExamNavigation.TypeOfExam1 == TypeOfExamName);

        //        if (selectedDepartment != null)
        //            query = query.Where(e => e.IdGroupNavigation.IdDepartment == selectedDepartment.IdDepartment);

        //        if (selectedGroups != null && selectedGroups.Any())
        //        {
        //            var selectedGroupIds = selectedGroups.Select(g => g.IdGroup).ToList();
        //            query = query.Where(e => selectedGroupIds.Contains(e.IdGroup));
        //        }

        //        var results = await query.ToListAsync();

        //        foreach (var exam in results)
        //        {
        //            if (disciplines.ContainsKey(exam.IdDiscipline))
        //            {
        //                exam.IdDisciplineNavigation = disciplines[exam.IdDiscipline];
        //            }
        //        }

        //        var groupedAndSortedResults = results
        //        .GroupBy(e => new
        //        {
        //            Department = e.IdGroupNavigation.IdDepartmentNavigation.NameOfDepartment,
        //            Group = e.IdGroupNavigation.NameOfGroup
        //        })
        //        .OrderBy(g => g.Key.Department)
        //        .ThenBy(g => g.Key.Group)
        //        .SelectMany(g => g
        //        .OrderBy(e => e.DateEvent)
        //        .ThenBy(e => e.DateEvent.TimeOfDay)).ToList();

        //        viewModel.SearchResults = new ObservableCollection<Exam>(groupedAndSortedResults);

        //        return;
        //    }
        //}
        //private void DelFromDataGridBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    var viewModel = DataContext as PrintResultModel;
        //    if (viewModel?.SearchResults == null) return;

        //    if (GridPrint.SelectedItems.Count > 0)
        //    {
        //        var itemsToRemove = new List<Exam>();

        //        foreach (var selectedItem in GridPrint.SelectedItems)
        //        {
        //            if (selectedItem is Exam exam)
        //            {
        //                itemsToRemove.Add(exam);
        //            }
        //        }

        //        foreach (var item in itemsToRemove)
        //        {
        //            viewModel.SearchResults.Remove(item);
        //        }

        //        viewModel.OnPropertyChanged(nameof(viewModel.SearchResults));

        //        MessageBox.Show($"Удалено {itemsToRemove.Count} строк из таблицы", "Информация",
        //                       MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    else
        //    {
        //        MessageBox.Show("Выберите строки для удаления из таблицы", "Внимание",
        //                       MessageBoxButton.OK, MessageBoxImage.Warning);
        //    }
        //}
    }
}
