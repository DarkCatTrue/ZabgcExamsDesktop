using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZabgcExamsDesktop.MVVM.Model;
using ZabgcExamsDesktop.Services.API;
using ZabgcExamsDesktop.Services.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ZabgcExamsDesktop.Services
{
    public class PdfReportService
    {
        private readonly ApiService _apiService;

        public PdfReportService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<bool> GenerateReportAsync(string filePath, List<ExamDisplayDto> exams,
                                                     DepartmentDto selectedDepartment, string reportType)
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
                        page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(10));
                        page.Content().Element(x => BuildContent(x, exams, selectedDepartment, reportType,
                                                                 managers, departmentOwners));
                    });
                });

                document.GeneratePdf(filePath);
                return true;
            }
            catch (Exception ex)
            {
                string error = $"Ошибка при генерации PDF:\n\n{ex.Message}";
                if (ex.InnerException != null)
                    error += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void BuildContent(IContainer container, List<ExamDisplayDto> exams,
                                  DepartmentDto selectedDepartment, string reportType,
                                  List<ManagerDto> managers, List<DepartmentOwnerDto> departmentOwners)
        {
            container.Column(col =>
            {
                col.Spacing(10);

                // Блок "Утверждаю"
                col.Item().ShowEntire().Element(x => BuildApprovalBlock(x, managers));

                // Заголовок
                string title = reportType switch
                {
                    "Стандартный" => "Расписание экзаменов",
                    "По модулю" => "Расписание экзаменов по модулю",
                    "Квалификационный" => "Расписание экзаменов квалификационных",
                    _ => "Расписание экзаменов"
                };
                col.Item().AlignCenter().Text(title).FontSize(14).Bold();

                string deptDisplay = selectedDepartment.NameOfDepartment switch
                {
                    "Информационных технологий и экономики" => "Отделение информационных технологий и экономики",
                    "Горное" => "Горное отделение",
                    "Геолого-маркшейдерское" => "Геолого-маркшейдерское отделение",
                    _ => selectedDepartment.NameOfDepartment
                };
                col.Item().AlignCenter().Text(deptDisplay).FontSize(12).Italic();
                col.Item().Height(10);

                // Подготовка данных и разбиение на страницы
                bool isStandard = reportType == "Стандартный";
                var paginator = new TablePaginator();
                foreach (var exam in exams.OrderBy(e => e.GroupName).ThenBy(e => e.DateEvent))
                {
                    paginator.AddRow(exam.ToTableRowData(isStandard));
                }

                // Высоты выбираем в зависимости от типа таблицы
                float firstPageHeight, otherPagesHeight;
                if (isStandard)
                {
                    firstPageHeight = 300;
                    otherPagesHeight = 450;
                }
                else
                {
                    firstPageHeight = 550;
                    otherPagesHeight = 670;
                }

                var tablePages = paginator.Paginate(firstPageHeight, otherPagesHeight);
                int totalPages = tablePages.Count;
                // Вывод таблицы по частям
                for (int i = 0; i < totalPages; i++)
                {
                    // Надпись в правом верхнем углу
                    string label = totalPages == 1 ? "Таблица" :
                                   i == 0 ? "Таблица" :
                                   i == totalPages - 1 ? "Окончание таблицы" : "Продолжение таблицы";
                    col.Item().AlignRight().Text(label).FontSize(12);

                    // Таблица с заголовком и строками данной страницы
                    col.Item().Element(x => BuildTable(x, tablePages[i], isStandard));

                    // Разрыв страницы перед следующей частью (кроме последней)
                    if (i < totalPages - 1)
                    {
                        col.Item().PageBreak();
                    }
                }

                col.Item().Height(20);

                // Блок "Согласовано" (всегда на последней странице, переносится целиком если не влезает)
                col.Item().ShowEntire().Element(x => BuildAgreementBlock(x, managers, departmentOwners, selectedDepartment));
            });
        }

        // Построение одной таблицы с заголовком и заданными строками
        private void BuildTable(IContainer container, List<TableRowData> rows, bool isStandard)
        {
            container.Table(table =>
            {
                // Определение колонок
                if (isStandard)
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);    // Дата
                        columns.RelativeColumn(2.3f); // Группа
                        columns.RelativeColumn(2.8f); // Консультация/Экзамен
                        columns.RelativeColumn(5);    // Дисциплина, МДК
                        columns.RelativeColumn(2.5f); // Аудитория
                        columns.RelativeColumn(4);    // Члены ЭК
                    });
                }
                else
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);   // Дата
                        columns.RelativeColumn(2);   // Группа
                        columns.RelativeColumn(5);   // ПМ
                        columns.RelativeColumn(2);   // Аудитория
                        columns.RelativeColumn(3);   // Члены ЭК
                    });
                }

                // Заголовок
                table.Header(header =>
                {
                    if (isStandard)
                    {
                        header.Cell().Element(CellHeaderStyle).Text("Дата").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Группа").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Экзамен").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Дисциплина, МДК").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Аудитория").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Члены ЭК").AlignCenter();
                    }
                    else
                    {
                        header.Cell().Element(CellHeaderStyle).Text("Дата").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Группа").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("ПМ").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Аудитория").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Члены ЭК").AlignCenter();
                    }
                });

                // Строки данных
                foreach (var row in rows)
                {
                    if (isStandard)
                    {
                        table.Cell().Element(CellDataStyle).Text(row.Date).AlignCenter();
                        table.Cell().Element(CellDataStyle).Text(row.GroupName).AlignCenter();
                        table.Cell().Element(CellDataStyle).Text(row.ConsultationType).AlignCenter();
                        table.Cell().Element(CellDataStyle).Text(row.DisciplineName).AlignCenter();
                        table.Cell().Element(CellDataStyle).Text(row.AudienceNumber).AlignCenter();
                        table.Cell().Element(CellDataStyle).Text(row.Teachers).AlignCenter();
                    }
                    else
                    {
                        table.Cell().Element(CellDataStyle).Text(row.Date).AlignCenter();
                        table.Cell().Element(CellDataStyle).Text(row.GroupName).AlignCenter();
                        table.Cell().Element(CellDataStyle).Text(row.DisciplineName).AlignCenter(); // колонка "ПМ"
                        table.Cell().Element(CellDataStyle).Text(row.AudienceNumber).AlignCenter();
                        table.Cell().Element(CellDataStyle).Text(row.Teachers).AlignCenter();
                    }
                }
            });
        }

        // Вспомогательные стили ячеек
        private static IContainer CellHeaderStyle(IContainer container) =>
            container.Background(Colors.Grey.Lighten2).Border(1).Padding(5)
                     .AlignCenter().AlignMiddle().DefaultTextStyle(x => x.Bold().FontSize(10));

        private static IContainer CellDataStyle(IContainer container) =>
            container.Border(1).Padding(5).AlignCenter().AlignMiddle()
                     .DefaultTextStyle(x => x.FontSize(10));

        // Блок "Утверждаю"
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

        // Блок "Согласовано"
        private void BuildAgreementBlock(IContainer container, List<ManagerDto> managers,
                                        List<DepartmentOwnerDto> departmentOwners, DepartmentDto department)
        {
            var studyWorkEmployee = managers.FirstOrDefault(m => m.IdManager == 2);
            var ownerStudyDepartment = managers.FirstOrDefault(m => m.IdManager == 3);
            var departmentOwner = departmentOwners.FirstOrDefault(d => d.IdDepartment == department.IdDepartment);

            container.Column(col =>
            {
                col.Spacing(5);
                col.Item().Text("Согласовано:").Bold().FontSize(12);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(40);
                        columns.RelativeColumn(60);
                    });

                    var noBorder = (IContainer c) => c.Border(0).Padding(2);

                    table.Cell().Element(noBorder).Text(studyWorkEmployee?.Post ?? "Должность не указана").FontSize(12);
                    table.Cell().Element(noBorder).PaddingLeft(200).Text(studyWorkEmployee?.FullName ?? "Не указан").FontSize(12);

                    table.Cell().Element(noBorder).Text(GetDepartmentOwnerPosition(department.NameOfDepartment)).FontSize(12);
                    table.Cell().Element(noBorder).PaddingLeft(200).Text(departmentOwner?.OwnerName ?? "Не указан").FontSize(12);

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
    }
}