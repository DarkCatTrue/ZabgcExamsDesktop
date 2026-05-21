using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZabgcExamsDesktop.MVVM.Model;
using ZabgcExamsDesktop.Services.API;
using iText.Kernel.Pdf;
using System.IO;

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

                // 1. Подсчитываем количество страниц, занимаемых таблицей (без блока "Согласовано")
                int totalTablePages = GetTablePagesCount(exams, selectedDepartment, reportType, managers, departmentOwners);

                // 2. Финальная генерация с правильными надписями и блоком "Согласовано"
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(10));
                        page.Content().Element(x => BuildFinalContent(x, exams, selectedDepartment, reportType, managers, departmentOwners, totalTablePages));
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

        // ---- Вспомогательный метод для подсчёта страниц таблицы ----
        private int GetTablePagesCount(List<ExamDisplayDto> exams, DepartmentDto selectedDepartment, string reportType,
                                       List<ManagerDto> managers, List<DepartmentOwnerDto> departmentOwners)
        {
            using (var tempStream = new MemoryStream())
            {
                var tempDocument = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(10));
                        page.Content().Element(x => BuildContentWithoutAgreement(x, exams, selectedDepartment, reportType, managers, departmentOwners));
                    });
                });
                tempDocument.GeneratePdf(tempStream);
                tempStream.Position = 0;
                using (var pdfReader = new PdfReader(tempStream))
                using (var pdfDoc = new PdfDocument(pdfReader))
                {
                    return pdfDoc.GetNumberOfPages();
                }
            }
        }

        // ---- Содержимое без блока "Согласовано" (только для подсчёта страниц) ----
        private void BuildContentWithoutAgreement(IContainer container, List<ExamDisplayDto> exams, DepartmentDto selectedDepartment,
                                                   string reportType, List<ManagerDto> managers, List<DepartmentOwnerDto> departmentOwners)
        {
            container.Column(col =>
            {
                col.Spacing(10);
                col.Item().ShowEntire().Element(x => BuildApprovalBlock(x, managers));

                string titleText = reportType switch
                {
                    "Стандартный" => "Расписание экзаменов",
                    "По модулю" => "Расписание экзаменов по модулю",
                    "Квалификационный" => "Расписание экзаменов квалификационных",
                    _ => "Расписание экзаменов"
                };
                col.Item().AlignCenter().Text(titleText).FontSize(14).Bold();

                string departmentDisplay = selectedDepartment.NameOfDepartment switch
                {
                    "Информационных технологий и экономики" => "Отделение информационных технологий и экономики",
                    "Горное" => "Горное отделение",
                    "Геолого-маркшейдерское" => "Геолого-маркшейдерское отделение",
                    _ => selectedDepartment.NameOfDepartment
                };
                col.Item().AlignCenter().Text(departmentDisplay).FontSize(12).Italic();

                col.Item().Element(x => BuildTable(x, exams, reportType));
                // "Согласовано" не добавляем
            });
        }

        // ---- Финальное содержимое (с надписями и блоком "Согласовано") ----
        private void BuildFinalContent(IContainer container, List<ExamDisplayDto> exams, DepartmentDto selectedDepartment,
                                        string reportType, List<ManagerDto> managers, List<DepartmentOwnerDto> departmentOwners,
                                        int totalTablePages)
        {
            container.Column(col =>
            {
                col.Spacing(10);
                col.Item().ShowEntire().Element(x => BuildApprovalBlock(x, managers));

                string titleText = reportType switch
                {
                    "Стандартный" => "Расписание экзаменов",
                    "По модулю" => "Расписание экзаменов по модулю",
                    "Квалификационный" => "Расписание экзаменов квалификационных",
                    _ => "Расписание экзаменов"
                };
                col.Item().AlignCenter().Text(titleText).FontSize(14).Bold();

                string departmentDisplay = selectedDepartment.NameOfDepartment switch
                {
                    "Информационных технологий и экономики" => "Отделение информационных технологий и экономики",
                    "Горное" => "Горное отделение",
                    "Геолого-маркшейдерское" => "Геолого-маркшейдерское отделение",
                    _ => selectedDepartment.NameOfDepartment
                };
                col.Item().AlignCenter().Text(departmentDisplay).FontSize(12).Italic();

                // Таблица с декорацией
                col.Item().Decoration(dec =>
                {
                    dec.Before().Element(x =>
                    {
                        x.Column(colBefore =>
                        {
                            colBefore.Item()
                                .ShowIf(ctx => ctx.PageNumber == 1)
                                .AlignRight().Text("Таблица").FontSize(12);
                            colBefore.Item()
                                .ShowIf(ctx => ctx.PageNumber > 1 && ctx.PageNumber < totalTablePages)
                                .AlignRight().Text("Продолжение таблицы").FontSize(12);
                            colBefore.Item()
                                .ShowIf(ctx => ctx.PageNumber == totalTablePages && totalTablePages > 1)
                                .AlignRight().Text("Окончание таблицы").FontSize(12);
                        });
                    });
                    dec.Content().Element(x => BuildTable(x, exams, reportType));
                });

                // Блок "Согласовано" с выравниванием: должности слева, ФИО справа, но текст ФИО выровнен по левому краю
                col.Item().ShowEntire().Element(x => BuildAgreementBlock(x, managers, departmentOwners, selectedDepartment));
            });
        }

        // ---- Блок "Утверждаю" ----
        private void BuildApprovalBlock(IContainer container, List<ManagerDto> managers)
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

        // ---- Таблица (без изменений) ----
        private void BuildTable(IContainer container, List<ExamDisplayDto> exams, string reportType)
        {
            bool isStandard = reportType == "Стандартный";
            var sortedExams = exams.OrderBy(e => e.GroupName).ThenBy(e => e.DateEvent).ToList();

            container.Table(table =>
            {
                if (isStandard)
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2.3f);
                        columns.RelativeColumn(2.8f);
                        columns.RelativeColumn(5);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(4);
                    });
                }
                else
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(5);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                    });
                }

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

                foreach (var exam in sortedExams)
                {
                    table.Cell().Element(CellDataStyle).Text(exam.DateEvent.ToString("dd.MM.yyyy HH:mm"));
                    table.Cell().Element(CellDataStyle).Text(exam.GroupName);
                    if (isStandard)
                        table.Cell().Element(CellDataStyle).Text(exam.TypeOfLessonName);
                    table.Cell().Element(CellDataStyle).Text(exam.DisciplineName);
                    table.Cell().Element(CellDataStyle).Text(exam.AudienceNumber);
                    table.Cell().Element(CellDataStyle).Text(FormatTeachers(exam.TeachersDisplay));
                }
            });
        }

        private string FormatTeachers(string teachersDisplay)
        {
            if (string.IsNullOrWhiteSpace(teachersDisplay)) return "";
            return string.Join("\n", teachersDisplay.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)));
        }

        private void BuildAgreementBlock(IContainer container, List<ManagerDto> managers, List<DepartmentOwnerDto> departmentOwners, DepartmentDto department)
        {
            var studyWorkEmployee = managers.FirstOrDefault(m => m.IdManager == 2);
            var ownerStudyDepartment = managers.FirstOrDefault(m => m.IdManager == 3);
            var departmentOwner = departmentOwners.FirstOrDefault(d => d.IdDepartment == department.IdDepartment);

            container.Column(col =>
            {
                col.Spacing(5);
                col.Item().Text("Согласовано:").Bold().FontSize(12);

                // Таблица на всю ширину
                col.Item().Table(table =>
                {
                    // Две колонки: левая (должности) – 40%, правая (ФИО) – 60%
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(40);
                        columns.RelativeColumn(60);
                    });

                    // Убираем границы ячеек
                    var noBorder = (IContainer c) => c.Border(0).Padding(2);

                    // Строка 1
                    table.Cell().Element(noBorder).Text(studyWorkEmployee?.Post ?? "Должность не указана").FontSize(12);
                    table.Cell().Element(noBorder).PaddingLeft(200).Text(studyWorkEmployee?.FullName ?? "Не указан").FontSize(12);

                    // Строка 2
                    table.Cell().Element(noBorder).Text(GetDepartmentOwnerPosition(department.NameOfDepartment)).FontSize(12);
                    table.Cell().Element(noBorder).PaddingLeft(200).Text(departmentOwner?.OwnerName ?? "Не указан").FontSize(12);

                    // Строка 3
                    table.Cell().Element(noBorder).Text(ownerStudyDepartment?.Post ?? "Должность не указана").FontSize(12);
                    table.Cell().Element(noBorder).PaddingLeft(200).Text(ownerStudyDepartment?.FullName ?? "Не указан").FontSize(12);
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

        private static IContainer CellHeaderStyle(IContainer container) =>
            container.Background(Colors.Grey.Lighten2).Border(1).Padding(5).AlignCenter().DefaultTextStyle(x => x.Bold());

        private static IContainer CellDataStyle(IContainer container) =>
            container.Border(1).Padding(5).AlignCenter();
    }
}