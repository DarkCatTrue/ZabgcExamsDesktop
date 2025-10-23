using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Data;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Models;
using ZabgcExamsDesktop.MVVM.View.Pages;
using ZabgcExamsDesktop.MVVM.View.Windows;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Layout.Element;
using iText.Layout;
using iText.Layout.Properties;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using iText.IO.Font;
using HorizontalAlignment = iText.Layout.Properties.HorizontalAlignment;

namespace ZabgcExamsDesktop.MVVM.ViewModel
{
    public class PrintResultModel : INotifyPropertyChanged
    {

        private ObservableCollection<TypeOfLesson> lessons;
        private ObservableCollection<TypeOfExam> typeExams;
        private ObservableCollection<Department> departments;
        private ObservableCollection<Group> groups;
        private ObservableCollection<Teacher> teachers;
        private ObservableCollection<Audience> audiences;
        private ObservableCollection<Qualification> qualifications;
        private ObservableCollection<Discipline> disciplines;
        private ObservableCollection<Group> _filteredGroups;

        private ObservableCollection<DepartmentOwner> _departmentOwner;
        private ObservableCollection<Manager> _managers;

        private Department _selectedDepartment;
        private Group _selectedGroup;
        private Exam _selectedExam;


        private ObservableCollection<Exam> _searchResults;
        private HashSet<Exam> _modifiedExams = new HashSet<Exam>();


        public ObservableCollection<DepartmentOwner> DepartmentOwners { get => _departmentOwner; set { _departmentOwner = value; OnPropertyChanged(); } }
        public ObservableCollection<Manager> Managers { get => _managers; set { _managers = value; OnPropertyChanged(); } }

        public ObservableCollection<Department> Department { get => departments; set { departments = value; OnPropertyChanged(); } }
        public ObservableCollection<Group> Group { get => groups; set { groups = value; OnPropertyChanged(); } }
        public ObservableCollection<Group> FilteredGroups { get => _filteredGroups; set { _filteredGroups = value; OnPropertyChanged(); } }
        public ObservableCollection<Teacher> Teacher { get => teachers; set { teachers = value; OnPropertyChanged(); } }
        public ObservableCollection<Audience> Audience { get => audiences; set { audiences = value; OnPropertyChanged(); } }
        public ObservableCollection<TypeOfLesson> Lesson { get => lessons; set { lessons = value; OnPropertyChanged(); } }
        public ObservableCollection<TypeOfExam> TypeExam { get => typeExams; set { typeExams = value; OnPropertyChanged(); } }
        public ObservableCollection<Qualification> Qualification { get => qualifications; set { qualifications = value; OnPropertyChanged(); } }
        public ObservableCollection<Discipline> Discipline { get => disciplines; set { disciplines = value; OnPropertyChanged(); } }


        private string _selectedResult;

        public List<string> ResultItems { get; } = new List<string>
        {
            "Стандартный",
            "По модулю",
            "Квалификационный"
        };

        public string SelectedResult
        {
            get => _selectedResult;
            set
            {
                _selectedResult = value;
                OnPropertyChanged(nameof(SelectedResult));
            }
        }

        public Department SelectedDepartment
        {
            get => _selectedDepartment; set { _selectedDepartment = value; OnPropertyChanged(); UpdateGroups(); }
        }
        public Group SelectedGroup
        {
            get => _selectedGroup; set { _selectedGroup = value; OnPropertyChanged(); }
        }
        public Exam SelectedExam
        {
            get => _selectedExam;
            set
            {
                _selectedExam = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Exam> SearchResults
        {
            get => _searchResults; set
            {
                _searchResults = value; foreach (var exam in _searchResults)
                {
                    exam.PropertyChanged += Exam_PropertyChanged;
                }
                OnPropertyChanged(nameof(SearchResults));
                OnPropertyChanged(nameof(CanSaveToPdf));
            }
        }
        public bool CanSaveToPdf => SearchResults?.Count > 0;

        private void Exam_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Exam exam)
            {
                _modifiedExams.Add(exam);
            }
        }

        public ICommand PrintResultCommand { get; set; }
        public ICommand SaveToPDFCommand { get; set; }
        public ICommand BackToSearch { get; set; }
        public ICommand DeleteRowCommand { get; set; }

        public PrintResultModel()
        {
            LoadDb();
            BackToSearch = new RelayCommand(BackToPage);
            SaveToPDFCommand = new RelayCommand(ExecuteSaveToPdf);
        }
        private void ExecuteSaveToPdf(object parameter)
        {
            SaveToPdf(parameter);
        }

        private void SaveToPdf(object parameter)
        {
            if (SearchResults == null || !SearchResults.Any())
            {
                MessageBox.Show("Нет данных для сохранения в PDF", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Расписание экзаменов {DateTime.Now:yyyy-MM-dd}",
                DefaultExt = ".pdf"
            };

            if (SelectedDepartment == null || SelectedResult == null)
            {
                MessageBox.Show("Поле с выбранным отделением и поле с типом отчёта должны быть заполнены", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        using var context = new ApplicationDbContext();

                        using (var writer = new PdfWriter(saveFileDialog.FileName))
                        {
                            using (var pdf = new PdfDocument(writer))
                            {
                                var document = new Document(pdf, PageSize.A4);
                                document.SetMargins(40, 40, 40, 40);

                                PdfFont fontNormal = PdfFontFactory.CreateFont(
                                    @"C:\Windows\Fonts\times.ttf",
                                    PdfEncodings.IDENTITY_H
                                );

                                PdfFont fontBold = PdfFontFactory.CreateFont(
                                    @"C:\Windows\Fonts\timesbd.ttf",
                                    PdfEncodings.IDENTITY_H
                                );

                                PdfFont fontItalic = PdfFontFactory.CreateFont(
                                    @"C:\Windows\Fonts\timesi.ttf",
                                    PdfEncodings.IDENTITY_H
                                );

                                // БЛОК "УТВЕРЖДАЮ" - ВЫРАВНИВАНИЕ ВПРАВО
                                var approveTable = new Table(1)
                                    .SetWidth(UnitValue.CreatePercentValue(100))
                                    .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                                    .SetMarginBottom(20);

                                // Все элементы блока выравниваем вправо
                                approveTable.AddCell(new Cell()
                                    .Add(new Paragraph("Утверждаю:").SetFont(fontNormal).SetFontSize(12))
                                    .SetTextAlignment(TextAlignment.RIGHT)
                                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                    .SetPadding(2));


                                approveTable.AddCell(new Cell()
                                    .Add(new Paragraph("Директор").SetFont(fontNormal).SetFontSize(12))
                                    .SetTextAlignment(TextAlignment.RIGHT)
                                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                    .SetPadding(2));

                                approveTable.AddCell(new Cell()
                                    .Add(new Paragraph("ГАПОУ «ЗабГК им. М.И. Агошкова»").SetFont(fontNormal).SetFontSize(12))
                                    .SetTextAlignment(TextAlignment.RIGHT)
                                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                    .SetPadding(2));

                                var Director = context.Managers.Where(m => m.IdManager == 1).Select(m => m.FullName).First();

                                approveTable.AddCell(new Cell()
                                    .Add(new Paragraph($"_________ {Director}").SetFont(fontNormal).SetFontSize(12))
                                    .SetTextAlignment(TextAlignment.RIGHT)
                                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                    .SetPadding(2));

                                approveTable.AddCell(new Cell()
                                    .Add(new Paragraph("«___» ________ 20___ г.").SetFont(fontNormal).SetFontSize(12))
                                    .SetTextAlignment(TextAlignment.RIGHT)
                                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                    .SetPadding(2));

                                document.Add(approveTable);

                                document.Add(new Paragraph("\n"));

                                switch (SelectedResult)
                                {
                                    case "Стандартный":
                                        document.Add(new Paragraph("Расписание экзаменов")
                                        .SetFont(fontBold)
                                        .SetFontSize(14)
                                        .SetTextAlignment(TextAlignment.CENTER));
                                        break;
                                    case "По модулю":
                                        document.Add(new Paragraph("Расписание экзаменов по модулю")
                                        .SetFont(fontBold)
                                        .SetFontSize(14)
                                        .SetTextAlignment(TextAlignment.CENTER));
                                        break;
                                    case "Квалификационный":
                                        document.Add(new Paragraph("Расписание экзаменов квалификационных")
                                        .SetFont(fontBold)
                                        .SetFontSize(14)
                                        .SetTextAlignment(TextAlignment.CENTER));
                                        break;
                                }


                                switch (SelectedDepartment.NameOfDepartment)
                                {
                                    case "Информационное":
                                        document.Add(new Paragraph("Отделение информационных технологий и экономики")
                                        .SetFont(fontItalic)
                                        .SetFontSize(12)
                                        .SetTextAlignment(TextAlignment.CENTER));
                                        break;

                                    case "Горное":
                                        document.Add(new Paragraph("Горное отделение")
                                        .SetFont(fontItalic)
                                        .SetFontSize(12)
                                        .SetTextAlignment(TextAlignment.CENTER));
                                        break;

                                    case "Геолого-маркшейдерское":
                                        document.Add(new Paragraph("Геолого-маркшейдерское отделение")
                                        .SetFont(fontItalic)
                                        .SetFontSize(12)
                                        .SetTextAlignment(TextAlignment.CENTER));
                                        break;
                                }


                                document.Add(new Paragraph("Таблица")
                                 .SetFont(fontNormal)
                                 .SetFontSize(12)
                                 .SetTextAlignment(TextAlignment.RIGHT)
                                 .SetMarginTop(5)
                                 .SetMarginBottom(10));


                                // Основная таблица с экзаменами
                                switch (SelectedResult)
                                {
                                    case "Стандартный":
                                        var mainTable = CreateExamsTable(SearchResults, fontNormal, fontBold);
                                        document.Add(mainTable);
                                        break;
                                    case "По модулю":
                                        mainTable = CreateExamsTableModules(SearchResults, fontNormal, fontBold);
                                        document.Add(mainTable);
                                        break;
                                    case "Квалификационный":
                                        mainTable = CreateExamsTableQualification(SearchResults, fontNormal, fontBold);
                                        document.Add(mainTable);
                                        break;
                                }
                                // Блок "Согласовано"
                                document.Add(new Paragraph("Согласовано:")
                                    .SetFont(fontBold)
                                    .SetFontSize(12)
                                    .SetMarginTop(20));


                                var StudyWorkEmployeeName = context.Managers.Where(m => m.IdManager == 2).Select(m => m.FullName).First();
                                var OwnerStudyDepartmentName = context.Managers.Where(m => m.IdManager == 3).Select(m => m.FullName).First();

                                var StudyWorkEmployee = context.Managers.Where(m => m.IdManager == 2).Select(m => m.Post).First();
                                var OwnerStudyDepartment = context.Managers.Where(m => m.IdManager == 3).Select(m => m.Post).First();


                                switch (SelectedDepartment.NameOfDepartment)
                                {
                                    case "Информационное":
                                        var DepartmentOwner = context.DepartmentOwners.Where(d => d.IdDepartment == 1).Select(d => d.OwnerName).First();
                                        var agreements = new[]
                                {
                        new { Name = StudyWorkEmployeeName, Position = StudyWorkEmployee },
                        new { Name = DepartmentOwner, Position = "Зав. отделением ИТ и Э" },
                        new { Name = OwnerStudyDepartmentName, Position = OwnerStudyDepartment }
                        };
                                        foreach (var item in agreements)
                                        {
                                            // Создаем таблицу для каждой строки
                                            var lineTable = new Table(UnitValue.CreatePercentArray(new float[] { 40f, 60f }))
                                                .SetWidth(UnitValue.CreatePercentValue(100))
                                                .SetMarginTop(5);

                                            // Имя слева
                                            lineTable.AddCell(new Cell()
                                                .Add(new Paragraph(item.Position).SetFont(fontNormal))
                                                .SetTextAlignment(TextAlignment.LEFT)
                                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                                .SetPadding(0));

                                            // Должность справа
                                            lineTable.AddCell(new Cell()
                                                .Add(new Paragraph(item.Name).SetFont(fontNormal))
                                                .SetTextAlignment(TextAlignment.RIGHT)
                                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                                .SetPadding(0));

                                            document.Add(lineTable);
                                        }

                                        document.Close();
                                        break;

                                    case "Горное":
                                        DepartmentOwner = context.DepartmentOwners.Where(d => d.IdDepartment == 2).Select(d => d.OwnerName).First();
                                        agreements = new[]
                                {
                         new { Name = StudyWorkEmployeeName, Position = StudyWorkEmployee },
                        new { Name = DepartmentOwner, Position = "Зав. горным отделением" },
                        new { Name = OwnerStudyDepartmentName, Position = OwnerStudyDepartment }
                        };
                                        foreach (var item in agreements)
                                        {
                                            // Создаем таблицу для каждой строки
                                            var lineTable = new Table(UnitValue.CreatePercentArray(new float[] { 40f, 60f }))
                                                .SetWidth(UnitValue.CreatePercentValue(100))
                                                .SetMarginTop(5);

                                            // Имя слева
                                            lineTable.AddCell(new Cell()
                                                .Add(new Paragraph(item.Position).SetFont(fontNormal))
                                                .SetTextAlignment(TextAlignment.LEFT)
                                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                                .SetPadding(0));

                                            // Должность справа
                                            lineTable.AddCell(new Cell()
                                                .Add(new Paragraph(item.Name).SetFont(fontNormal))
                                                .SetTextAlignment(TextAlignment.RIGHT)
                                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                                .SetPadding(0));

                                            document.Add(lineTable);
                                        }

                                        document.Close();
                                        break;

                                    case "Геолого-маркшейдерское":
                                        DepartmentOwner = context.DepartmentOwners.Where(d => d.IdDepartment == 3).Select(d => d.OwnerName).First();
                                        agreements = new[]
                                {
                        new { Name = StudyWorkEmployeeName, Position = StudyWorkEmployee },
                        new { Name = DepartmentOwner, Position = "Зав. Г-М отделением" },
                        new { Name = OwnerStudyDepartmentName, Position = OwnerStudyDepartment }
                        };
                                        foreach (var item in agreements)
                                        {
                                            // Создаем таблицу для каждой строки
                                            var lineTable = new Table(UnitValue.CreatePercentArray(new float[] { 40f, 60f }))
                                                .SetWidth(UnitValue.CreatePercentValue(100))
                                                .SetMarginTop(5);

                                            // Имя слева
                                            lineTable.AddCell(new Cell()
                                                .Add(new Paragraph(item.Position).SetFont(fontNormal))
                                                .SetTextAlignment(TextAlignment.LEFT)
                                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                                .SetPadding(0));

                                            // Должность справа
                                            lineTable.AddCell(new Cell()
                                                .Add(new Paragraph(item.Name).SetFont(fontNormal))
                                                .SetTextAlignment(TextAlignment.RIGHT)
                                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                                .SetPadding(0));

                                            document.Add(lineTable);
                                        }

                                        document.Close();
                                        break;
                                }
                            }
                        }

                        MessageBox.Show($"Файл успешно сохранен: {saveFileDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении PDF: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private Table CreateExamsTableQualification(ObservableCollection<Exam> exams, PdfFont fontNormal, PdfFont fontBold)
        {
            // Таблица с фиксированными ширинами колонок
            var table = new Table(new float[] { 2, 2, 3, 2, 2 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(10)
                .SetMarginBottom(10);

            // Заголовки таблицы
            var headers = new[]
            {
        "Дата", "Группа", "ПМ", "Аудитория", "Члены ЭК"
    };

            foreach (var header in headers)
            {
                var cell = new Cell()
                    .Add(new Paragraph(header).SetFont(fontBold).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                    .SetPadding(8);
                table.AddHeaderCell(cell);
            }

            // Данные таблицы
            foreach (var exam in exams.OrderBy(e => e.DateEvent))
            {
                // Дата (по центру)
                table.AddCell(new Cell()
                    .Add(new Paragraph(exam.DateEvent.ToString("dd.MM.yyyy HH:mm"))
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6));

                // Группа (по центру)
                table.AddCell(new Cell()
                    .Add(new Paragraph(exam.IdGroupNavigation?.NameOfGroup ?? ""))
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6);

                // Дисциплина (по центру)
                var disciplineText = $"{exam.IdDisciplineNavigation?.NameDiscipline ?? ""}";
                table.AddCell(new Cell()
                    .Add(new Paragraph(disciplineText)
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6));

                // Аудитория (по центру)
                table.AddCell(new Cell()
                    .Add(new Paragraph(exam.IdAudienceNavigation?.NumberAudience?.ToString() ?? ""))
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6);

                // Преподаватели (по центру)
                var teachers = exam.IdTeachers?.Any() == true ?
                    string.Join(", ", exam.IdTeachers.Select(t => t.FullName)) : "";
                table.AddCell(new Cell()
                .Add(new Paragraph(teachers)
                    .SetFont(fontNormal)
                    .SetFontSize(9))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6));
            }

            return table;
        }

        private Table CreateExamsTableModules(ObservableCollection<Exam> exams, PdfFont fontNormal, PdfFont fontBold)
        {
            // Таблица с фиксированными ширинами колонок
            var table = new Table(new float[] { 2, 2, 3, 2, 2 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(10)
                .SetMarginBottom(10);

            // Заголовки таблицы
            var headers = new[]
            {
        "Дата", "Группа", "ПМ", "Аудитория", "Члены ЭК"
    };

            foreach (var header in headers)
            {
                var cell = new Cell()
                    .Add(new Paragraph(header).SetFont(fontBold).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                    .SetPadding(8);
                table.AddHeaderCell(cell);
            }

            // Данные таблицы
            foreach (var exam in exams.OrderBy(e => e.DateEvent))
            {
                // Дата (по центру)
                table.AddCell(new Cell()
                    .Add(new Paragraph(exam.DateEvent.ToString("dd.MM.yyyy HH:mm"))
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6));

                // Группа (по центру)
                table.AddCell(new Cell()
                    .Add(new Paragraph(exam.IdGroupNavigation?.NameOfGroup ?? ""))
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6);

                // Дисциплина (по центру)
                var disciplineText = $"{exam.IdDisciplineNavigation?.NameDiscipline ?? ""}";
                table.AddCell(new Cell()
                    .Add(new Paragraph(disciplineText)
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6));

                // Аудитория (по центру)
                table.AddCell(new Cell()
                    .Add(new Paragraph(exam.IdAudienceNavigation?.NumberAudience?.ToString() ?? ""))
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6);

                // Преподаватели (по центру)
                var teachers = exam.IdTeachers?.Any() == true ?
                    string.Join(", ", exam.IdTeachers.Select(t => t.FullName)) : "";
                table.AddCell(new Cell()
                .Add(new Paragraph(teachers)
                    .SetFont(fontNormal)
                    .SetFontSize(9))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6));
            }

            return table;
        }



        private Table CreateExamsTable(ObservableCollection<Exam> exams, PdfFont fontNormal, PdfFont fontBold)
        {
            // Таблица с фиксированными ширинами колонок
            var table = new Table(new float[] { 2, 2, 2, 3, 2, 2 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(10)
                .SetMarginBottom(10);

            // Заголовки таблицы
            var headers = new[]
            {
        "Дата", "Группа", "Консультация/Экзамен", "Дисциплина, МДК", "Аудитория", "Члены ЭК"
    };

            foreach (var header in headers)
            {
                var cell = new Cell()
                    .Add(new Paragraph(header).SetFont(fontBold).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                    .SetPadding(8);
                table.AddHeaderCell(cell);
            }

            // Данные таблицы
            foreach (var exam in exams.OrderBy(e => e.DateEvent))
            {
                // Дата (по центру)
                table.AddCell(new Cell()
                    .Add(new Paragraph(exam.DateEvent.ToString("dd.MM.yyyy HH:mm"))
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6));

                // Группа (по центру)
                table.AddCell(new Cell()
                    .Add(new Paragraph(exam.IdGroupNavigation?.NameOfGroup ?? ""))
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6);

                // Тип занятия
                table.AddCell(new Cell()
                    .Add(new Paragraph(exam.IdTypeOfLessonNavigation.TypeOfLesson1 ?? ""))
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6);

                // Дисциплина (по центру)
                var disciplineText = $"{exam.IdDisciplineNavigation?.NameDiscipline ?? ""}";
                table.AddCell(new Cell()
                    .Add(new Paragraph(disciplineText)
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6));

                // Аудитория (по центру)
                table.AddCell(new Cell()
                    .Add(new Paragraph(exam.IdAudienceNavigation?.NumberAudience?.ToString() ?? ""))
                        .SetFont(fontNormal)
                        .SetFontSize(9))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6);

                // Преподаватели (по центру)
                var teachers = exam.IdTeachers?.Any() == true ?
                    string.Join(", ", exam.IdTeachers.Select(t => t.FullName)) : "";
                table.AddCell(new Cell()
                .Add(new Paragraph(teachers)
                    .SetFont(fontNormal)
                    .SetFontSize(9))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6));
            }

            return table;
        }

        private void BackToPage(object parameter)
        {
            SearchExamPage searchExamPage = new SearchExamPage();
            SearchExamWindow.pageManager.ChangePage(searchExamPage);
        }

        private async void LoadDb()
        {
            try
            {
                using var context = new ApplicationDbContext();
                var departments = await context.Departments.ToListAsync();
                var groups = await context.Groups.ToListAsync();
                var teachers = await context.Teachers.ToListAsync();
                var audiences = await context.Audiences.ToListAsync();

                context.ChangeTracker.Clear();

                var disciplines = context.Disciplines.ToDictionary(d => d.IdDiscipline, d => d);

                Department = new ObservableCollection<Department>(departments);
                Group = new ObservableCollection<Group>(groups);
                Teacher = new ObservableCollection<Teacher>(teachers);
                Audience = new ObservableCollection<Audience>(audiences);
                FilteredGroups = new ObservableCollection<Group>(groups);
                var exams = context.Exams
                .Include(e => e.IdGroupNavigation)
                .ThenInclude(e => e.IdDepartmentNavigation)
                .Include(e => e.IdTypeOfLessonNavigation)
                .Include(e => e.IdTypeOfExamNavigation)
                .Include(e => e.IdQualificationNavigation)
                .Include(e => e.IdAudienceNavigation)
                .Include(e => e.IdTeachers)
                .AsNoTracking()
                .ToList();

                foreach (var exam in exams)
                {
                    if (disciplines.ContainsKey(exam.IdDiscipline))
                    {
                        exam.IdDisciplineNavigation = disciplines[exam.IdDiscipline];
                    }
                }

                SearchResults = new ObservableCollection<Exam>(exams);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void UpdateGroups()
        {
            if (SelectedDepartment == null)
            {
                FilteredGroups = new ObservableCollection<Group>(Group);
            }
            else
            {
                var filtered = Group
                    .Where(g => g.IdDepartment == SelectedDepartment.IdDepartment)
                    .ToList();

                FilteredGroups = new ObservableCollection<Group>(filtered);
            }
            SelectedGroup = null;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
