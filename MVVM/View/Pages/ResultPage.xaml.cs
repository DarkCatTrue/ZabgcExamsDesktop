using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Data;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Models;
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

        private async void SearchBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as PrintResultModel;
            if (viewModel == null) return;
            var selectedDepartment = DepartmentComboBox.SelectedItem as Department;
            var selectedGroups = GroupsListBox.SelectedItems.Cast<Group>().ToList();
            using var context = new ApplicationDbContext();
            
            var disciplines = await context.Disciplines
            .AsNoTracking()
            .ToDictionaryAsync(d => d.IdDiscipline, d => d);

            context.ChangeTracker.Clear();

            switch (viewModel.SelectedResult)
            {
                case "Стандартный":
                    var query = context.Exams
                    .Include(e => e.IdGroupNavigation)
                    .ThenInclude(g => g.IdDepartmentNavigation)
                    .Include(e => e.IdTeachers)
                    .Include(e => e.IdAudienceNavigation)
                    .Include(e => e.IdDisciplineNavigation)
                    .Include(e => e.IdQualificationNavigation)     
                    .Include(e => e.IdTypeOfExamNavigation)
                    .Include(e => e.IdTypeOfLessonNavigation)
                    .AsQueryable().Where(e => e.IdTypeOfExamNavigation.TypeOfExam1 == "Стандартный");


                    if (selectedDepartment != null)
                        query = query.Where(e => e.IdGroupNavigation.IdDepartment == selectedDepartment.IdDepartment);

                    if (selectedGroups != null && selectedGroups.Any())
                    {
                        var selectedGroupIds = selectedGroups.Select(g => g.IdGroup).ToList();
                        query = query.Where(e => selectedGroupIds.Contains(e.IdGroup));
                    }

                    var results = await query.ToListAsync();

                    foreach (var exam in results)
                    {
                        if (disciplines.ContainsKey(exam.IdDiscipline))
                        {
                            exam.IdDisciplineNavigation = disciplines[exam.IdDiscipline];
                        }
                    }

                    viewModel.SearchResults = new ObservableCollection<Exam>(results);

                    return;

                case "По модулю":
                    query = context.Exams
                   .Include(e => e.IdGroupNavigation)
                   .ThenInclude(g => g.IdDepartmentNavigation)
                   .Include(e => e.IdTeachers)
                   .Include(e => e.IdAudienceNavigation)
                   .Include(e => e.IdDisciplineNavigation)
                   .Include(e => e.IdQualificationNavigation)
                   .Include(e => e.IdTypeOfExamNavigation)
                   .Include(e => e.IdTypeOfLessonNavigation)
                   .AsNoTracking()
                   .AsQueryable().Where(e => e.IdTypeOfExamNavigation.TypeOfExam1 == "ПМ")
                   ;


                    if (selectedDepartment != null)
                        query = query.Where(e => e.IdGroupNavigation.IdDepartment == selectedDepartment.IdDepartment);

                    if (selectedGroups != null && selectedGroups.Any())
                    {
                        var selectedGroupIds = selectedGroups.Select(g => g.IdGroup).ToList();
                        query = query.Where(e => selectedGroupIds.Contains(e.IdGroup));
                    }

                    results = await query.ToListAsync();

                    foreach (var exam in results)
                    {
                        if (disciplines.ContainsKey(exam.IdDiscipline))
                        {
                            exam.IdDisciplineNavigation = disciplines[exam.IdDiscipline];
                        }
                    }

                    viewModel.SearchResults = new ObservableCollection<Exam>(results);

                    return;

                case "Квалификационный":
                    query = context.Exams
                   .Include(e => e.IdGroupNavigation)
                   .ThenInclude(g => g.IdDepartmentNavigation)
                   .Include(e => e.IdTeachers)
                   .Include(e => e.IdAudienceNavigation)
                   .Include(e => e.IdDisciplineNavigation)
                   .Include(e => e.IdQualificationNavigation)
                   .Include(e => e.IdTypeOfExamNavigation)
                   .Include(e => e.IdTypeOfLessonNavigation)
                   .AsNoTracking()
                   .AsQueryable().Where(e => e.IdTypeOfExamNavigation.TypeOfExam1 == "Квалификационный");

                    if (selectedDepartment != null)
                        query = query.Where(e => e.IdGroupNavigation.IdDepartment == selectedDepartment.IdDepartment);

                    if (selectedGroups != null && selectedGroups.Any())
                    {
                        var selectedGroupIds = selectedGroups.Select(g => g.IdGroup).ToList();
                        query = query.Where(e => selectedGroupIds.Contains(e.IdGroup));
                    }

                    results = await query.ToListAsync();

                    foreach (var exam in results)
                    {
                        if (disciplines.ContainsKey(exam.IdDiscipline))
                        {
                            exam.IdDisciplineNavigation = disciplines[exam.IdDiscipline];
                        }
                    }

                    viewModel.SearchResults = new ObservableCollection<Exam>(results);

                    return;

            }
        }
        private void DelFromDataGridBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as PrintResultModel;
            if (viewModel?.SearchResults == null) return;

            if (GridPrint.SelectedItems.Count > 0)
            {
                var itemsToRemove = new List<Exam>();

                foreach (var selectedItem in GridPrint.SelectedItems)
                {
                    if (selectedItem is Exam exam)
                    {
                        itemsToRemove.Add(exam);
                    }
                }

                foreach (var item in itemsToRemove)
                {
                    viewModel.SearchResults.Remove(item);
                }

                viewModel.OnPropertyChanged(nameof(viewModel.SearchResults));

                MessageBox.Show($"Удалено {itemsToRemove.Count} строк из таблицы", "Информация",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Выберите строки для удаления из таблицы", "Внимание",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
