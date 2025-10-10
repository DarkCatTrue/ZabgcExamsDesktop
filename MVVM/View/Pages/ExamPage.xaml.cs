using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Data;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Models;
using ZabgcExamsDesktop.MVVM.ViewModel;

namespace ZabgcExamsDesktop.MVVM.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для ExamPage.xaml
    /// </summary>
    public partial class ExamPage : Page
    {
        public ExamPage()
        {
            InitializeComponent();
            DataContext = new AddExamModel();
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTeachers = TeachersListBox.SelectedItems.Cast<Teacher>().ToList();
            var selectedAudience = Audience.SelectedItem as Audience;
            var viewModel = new AddExamModel();

            if (Group.SelectedItem == null ||
                Discipline.SelectedItem == null ||
                TypeLesson.SelectedItem == null ||
                TypeExam.SelectedItem == null ||
                Qualification.SelectedItem == null ||
                DateEvent.SelectedDate == null ||
                TimeEvent.Text == null ||
                Audience.SelectedItem == null ||
                Department.SelectedItem == null)
            {
                MessageBox.Show("Все поля должны быть заполнены", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime? dateEvent = DateEvent.SelectedDate;
            DateTime Date = Convert.ToDateTime(dateEvent);
            string DateText = Date.ToString("yyyy-MM-dd");
            string TimeText = TimeEvent.Text;
            DateTime datePart = DateTime.ParseExact(DateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            TimeSpan timePart = TimeSpan.ParseExact(TimeText, "hh\\:mm", CultureInfo.InvariantCulture);
            DateTime Fulldate = datePart.Add(timePart);

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    context.ChangeTracker.Clear();

                    var newExam = new Exam
                    {
                        IdGroup = (Group.SelectedItem as Group).IdGroup,
                        IdDiscipline = (Discipline.SelectedItem as Discipline).IdDiscipline,
                        IdTypeOfLesson = (TypeLesson.SelectedItem as TypeOfLesson).IdTypeOfLesson,
                        IdTypeOfExam = (TypeExam.SelectedItem as TypeOfExam).IdTypeOfExam,
                        IdQualification = (Qualification.SelectedItem as Qualification).IdQualification,
                        IdAudience = (Audience.SelectedItem as Audience).IdAudience,
                        DateEvent = Fulldate
                    };

                    context.Exams.Add(newExam);

                    foreach (var teacher in selectedTeachers)
                    {
                        var teacherProxy = new Teacher { IdTeacher = teacher.IdTeacher };
                        context.Teachers.Attach(teacherProxy);
                        newExam.IdTeachers.Add(teacherProxy);
                    }

                    context.SaveChanges();

                    MessageBox.Show("Добавлена новая запись", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    TeachersListBox.UnselectAll();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }
}
