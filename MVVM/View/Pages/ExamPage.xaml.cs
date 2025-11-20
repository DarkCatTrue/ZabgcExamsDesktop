using Microsoft.EntityFrameworkCore;
using NLog;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ZabgcExamsDesktop.API;
using ZabgcExamsDesktop.API.Models;
using ZabgcExamsDesktop.MVVM.View.Windows;
using ZabgcExamsDesktop.MVVM.ViewModel;

namespace ZabgcExamsDesktop.MVVM.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для ExamPage.xaml
    /// </summary>
    public partial class ExamPage : Page
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ExamPage()
        {
            InitializeComponent();
            DataContext = new AddExamModel();
        }


        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTeachers = TeachersListBox.SelectedItems.Cast<TeacherDto>().ToList();

            if (Group.SelectedItem == null ||
                Discipline.SelectedItem == null ||
                TypeLesson.SelectedItem == null ||
                TypeExam.SelectedItem == null ||
                DateEvent.SelectedDate == null ||
                string.IsNullOrWhiteSpace(TimeEvent.Text) ||
                Audience.SelectedItem == null)
            {
                MessageBox.Show("Все поля должны быть заполнены", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DateTime datePart = DateEvent.SelectedDate.Value;
                TimeSpan timePart = TimeSpan.ParseExact(TimeEvent.Text, "hh\\:mm", CultureInfo.InvariantCulture);
                DateTime fullDate = datePart.Add(timePart);

                var newExam = new CreateExamDto
                {
                    IdGroup = ((GroupDto)Group.SelectedItem).IdGroup,
                    IdDiscipline = ((DisciplineDto)Discipline.SelectedItem).IdDiscipline,
                    IdTypeOfLesson = ((TypeOfLessonDto)TypeLesson.SelectedItem).IdTypeOfLesson,
                    IdTypeOfExam = ((TypeOfExamDto)TypeExam.SelectedItem).IdTypeOfExam,
                    IdAudience = ((AudienceDto)Audience.SelectedItem).IdAudience,
                    DateEvent = fullDate,
                    IdTeachers = selectedTeachers.Select(t => t.IdTeacher).ToList()
                };

                var apiService = new ApiService();
                bool isSuccess = await apiService.CreateExamAsync(newExam);

                if (isSuccess)
                {
                    await ReloadPageAsync();

                    string groupName = ((GroupDto)Group.SelectedItem).NameOfGroup;
                    MessageBox.Show("Добавлена новая запись", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Logger.Warn($"Был добавлен экзамен для группы: '{groupName}'");

                }
                else
                {
                    MessageBox.Show("Не удалось добавить экзамен. Проверьте подключение к API.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Неверный формат времени. Используйте формат ЧЧ:мм", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Не удалось добавить экзамен для группы: '{((GroupDto)Group.SelectedItem)?.NameOfGroup}' ошибка: {ex}");
            }
        }
        private async Task ReloadPageAsync()
        {
            var newPage = new SearchExamPage();
            SearchExamWindow.pageManager.ChangePage(newPage);
        }
    }
}
