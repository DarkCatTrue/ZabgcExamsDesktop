using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZabgcExamsDesktop.MVVM.Model;
using ZabgcExamsDesktop.Services.API;

namespace ZabgcExamsDesktop.Services
{
    public class PdfReportService
    {
        private readonly ApiService _apiService;

        public PdfReportService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<bool> GenerateReportAsync(string filePath, List<ExamDisplayDto> exams, DepartmentDto selectedDepartment, string reportType)
        {
            try
            {
                var managers = await _apiService.GetManagersAsync();
                var departmentOwners = await _apiService.GetDepartmentOwnersAsync();

                QuestPDF.Settings.License = LicenseType.Community;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontFamily("Times New Roman"));

                        page.Header().Element(x => BuildHeader(x, managers));
                        page.Content().Element(x => BuildContent(x, exams, selectedDepartment, reportType, managers, departmentOwners));
                        // Footer – удалён (нумерации нет)
                    });
                });

                document.GeneratePdf(filePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF generation error: {ex.Message}");
                return false;
            }
        }

        private void BuildHeader(IContainer container, List<ManagerDto> managers)
        {
            var director = managers.FirstOrDefault(m => m.IdManager == 1);
            var directorName = director?.FullName ?? "Не указан";

            container.Column(col =>
            {
                col.Spacing(2);
                col.Item().AlignRight().Text("Утверждаю:").FontSize(12);
                col.Item().AlignRight().Text("Директор").FontSize(12);
                col.Item().AlignRight().Text("ГАПОУ «ЗабГК им. М.И. Агошкова»").FontSize(12);
                col.Item().AlignRight().Text($"_________ {directorName}").FontSize(12);
                col.Item().AlignRight().Text("«___» ________ 20___ г.").FontSize(12);
                col.Item().Height(20);
            });
        }

        private void BuildContent(IContainer container, List<ExamDisplayDto> exams, DepartmentDto selectedDepartment,
                                   string reportType, List<ManagerDto> managers, List<DepartmentOwnerDto> departmentOwners)
        {
            container.Column(col =>
            {
                col.Spacing(10);

                // Заголовок отчёта
                string titleText = reportType switch
                {
                    "Стандартный" => "Расписание экзаменов",
                    "По модулю" => "Расписание экзаменов по модулю",
                    "Квалификационный" => "Расписание экзаменов квалификационных",
                    _ => "Расписание экзаменов"
                };
                col.Item().AlignCenter().Text(titleText).Bold().FontSize(14);

                // Название отделения
                string departmentDisplay = selectedDepartment.NameOfDepartment switch
                {
                    "Информационных технологий и экономики" => "Отделение информационных технологий и экономики",
                    "Горное" => "Горное отделение",
                    "Геолого-маркшейдерское" => "Геолого-маркшейдерское отделение",
                    _ => selectedDepartment.NameOfDepartment
                };
                col.Item().AlignCenter().Text(departmentDisplay).Italic().FontSize(12);

                // Слово "Таблица" перед таблицей (выравнивание вправо)
                col.Item().AlignRight().Text("Таблица").FontSize(12);

                // Таблица
                col.Item().Element(x => BuildTable(x, exams, reportType));

                // "Окончание таблицы" после таблицы, справа
                col.Item().AlignRight().Text("Окончание таблицы").FontSize(9).Italic();

                col.Item().Height(20);

                // Блок "Согласовано"
                col.Item().Element(x => BuildAgreement(x, managers, departmentOwners, selectedDepartment));
            });
        }

        private void BuildTable(IContainer container, List<ExamDisplayDto> exams, string reportType)
        {
            bool isStandard = reportType == "Стандартный";
            var sortedExams = exams
                .OrderBy(e => e.GroupName)
                .ThenBy(e => e.DateEvent)
                .ToList();

            container.Table(table =>
            {
                // Определяем колонки в зависимости от типа отчёта
                if (isStandard)
                {
                    // Стандартный: Дата, Группа, Консультация/Экзамен, Дисциплина, МДК, Аудитория, Члены ЭК
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);   // Дата
                        columns.RelativeColumn(2);   // Группа
                        columns.RelativeColumn(3);   // Консультация/Экзамен
                        columns.RelativeColumn(5);   // Дисциплина, МДК
                        columns.RelativeColumn(3);   // Аудитория
                        columns.RelativeColumn(3);   // Члены ЭК
                    });
                }
                else
                {
                    // Модульный / Квалификационный: Дата, Группа, ПМ, Дисциплина, МДК, Аудитория, Члены ЭК
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);   // Дата
                        columns.RelativeColumn(2);   // Группа
                        columns.RelativeColumn(5);   // ПМ
                        columns.RelativeColumn(2);   // Аудитория
                        columns.RelativeColumn(3);   // Члены ЭК
                    });
                }

                // Заголовки с серым фоном
                table.Header(header =>
                {
                    if (isStandard)
                    {
                        header.Cell().Element(CellHeaderStyle).Text("Дата");
                        header.Cell().Element(CellHeaderStyle).Text("Группа");
                        header.Cell().Element(CellHeaderStyle).Text("Консультация/\rЭкзамен");
                        header.Cell().Element(CellHeaderStyle).Text("Дисциплина, МДК");
                        header.Cell().Element(CellHeaderStyle).Text("Аудитория");
                        header.Cell().Element(CellHeaderStyle).Text("Члены ЭК");
                    }
                    else
                    {
                        header.Cell().Element(CellHeaderStyle).Text("Дата");
                        header.Cell().Element(CellHeaderStyle).Text("Группа");
                        header.Cell().Element(CellHeaderStyle).Text("ПМ");
                        header.Cell().Element(CellHeaderStyle).Text("Аудитория");
                        header.Cell().Element(CellHeaderStyle).Text("Члены ЭК");
                    }
                });

                // Данные
                foreach (var exam in sortedExams)
                {
                    // Дата
                    table.Cell().Element(CellDataStyle).Text(exam.DateEvent.ToString("dd.MM.yyyy HH:mm"));
                    // Группа
                    table.Cell().Element(CellDataStyle).Text(exam.GroupName);

                    if (isStandard)
                        // Консультация/Экзамен
                        table.Cell().Element(CellDataStyle).Text(exam.TypeOfLessonName);
                    // Дисциплина, МДК
                    table.Cell().Element(CellDataStyle).Text(exam.DisciplineName);
                    // Аудитория
                    table.Cell().Element(CellDataStyle).Text(exam.AudienceNumber);
                    // Члены ЭК – с переносом каждого преподавателя на новую строку
                    table.Cell().Element(CellDataStyle).AlignLeft().Text(FormatTeachers(exam.TeachersDisplay));
                }

                // Стили ячеек
                static IContainer CellHeaderStyle(IContainer container)
                {
                    return container
                        .Background(Colors.Grey.Lighten2)
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .DefaultTextStyle(x => x.Bold());
                }

                static IContainer CellDataStyle(IContainer container)
                {
                    return container
                        .Border(1)
                        .Padding(5)
                        .AlignCenter();
                }
            });
        }

        // Вспомогательный метод: разбивает строку преподавателей на отдельные строки
        private string FormatTeachers(string teachersDisplay)
        {
            if (string.IsNullOrWhiteSpace(teachersDisplay))
                return "";

            var teachers = teachersDisplay.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(t => t.Trim())
                                          .Where(t => !string.IsNullOrEmpty(t))
                                          .ToList();
            return string.Join("\n", teachers);
        }

        private void BuildAgreement(IContainer container, List<ManagerDto> managers, List<DepartmentOwnerDto> departmentOwners, DepartmentDto department)
        {
            var studyWorkEmployee = managers.FirstOrDefault(m => m.IdManager == 2);
            var ownerStudyDepartment = managers.FirstOrDefault(m => m.IdManager == 3);
            var departmentOwner = departmentOwners.FirstOrDefault(d => d.IdDepartment == department.IdDepartment);

            container.Column(col =>
            {
                col.Spacing(5);
                col.Item().Text("Согласовано:").Bold();

                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text(studyWorkEmployee?.Post ?? "Должность не указана");
                    row.RelativeItem().AlignRight().Text(studyWorkEmployee?.FullName ?? "Не указан");
                });
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text(GetDepartmentOwnerPosition(department.NameOfDepartment));
                    row.RelativeItem().AlignRight().Text(departmentOwner?.OwnerName ?? "Не указан");
                });
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text(ownerStudyDepartment?.Post ?? "Должность не указана");
                    row.RelativeItem().AlignRight().Text(ownerStudyDepartment?.FullName ?? "Не указан");
                });
            });
        }

        private string GetDepartmentOwnerPosition(string departmentName)
        {
            return departmentName switch
            {
                "Информационных технологий и экономики" => "Зав. отделением ИТ и Э",
                "Горное" => "Зав. горным отделением",
                "Геолого-маркшейдерское" => "Зав. Г-М отделением",
                _ => "Зав. отделением"
            };
        }
    }
}