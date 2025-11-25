using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Collections.ObjectModel;
using ZabgcExamsDesktop.API;
using ZabgcExamsDesktop.API.Models;

namespace ZabgcExamsDesktop.MVVM.Model
{
    public class PdfReportService
    {
        private readonly ApiService _apiService;
        private PdfFont _fontNormal;
        private PdfFont _fontBold;
        private PdfFont _fontItalic;

        public PdfReportService(ApiService apiService)
        {
            _apiService = apiService;
            InitializeFonts();
        }

        private void InitializeFonts()
        {
            _fontNormal = PdfFontFactory.CreateFont(@"C:\Windows\Fonts\times.ttf", PdfEncodings.IDENTITY_H);
            _fontBold = PdfFontFactory.CreateFont(@"C:\Windows\Fonts\timesbd.ttf", PdfEncodings.IDENTITY_H);
            _fontItalic = PdfFontFactory.CreateFont(@"C:\Windows\Fonts\timesi.ttf", PdfEncodings.IDENTITY_H);
        }

        public async Task<bool> GenerateReportAsync(string filePath, ObservableCollection<ExamDisplayDto> exams,
            DepartmentDto selectedDepartment, string selectedResult)
        {
            try
            {
                var managers = await _apiService.GetManagersAsync();
                var departmentOwners = await _apiService.GetDepartmentOwnersAsync();

                using var writer = new PdfWriter(filePath);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf, PageSize.A4);

                document.SetMargins(40, 40, 40, 40);

                await AddApprovalSection(document, managers);
                AddReportHeader(document, selectedDepartment, selectedResult);
                await AddExamsTable(document, exams, selectedResult);
                await AddAgreementSection(document, managers, departmentOwners, selectedDepartment);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF generation error: {ex.Message}");
                return false;
            }
        }

        private async Task AddApprovalSection(Document document, List<ManagerDto> managers)
        {
            var director = managers.FirstOrDefault(m => m.IdManager == 1);

            var approveTable = new Table(1)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                .SetMarginBottom(20);

            var approvalLines = new[]
            {
            "Утверждаю:",
            "Директор",
            "ГАПОУ «ЗабГК им. М.И. Агошкова»",
            $"_________ {director?.FullName ?? "Не указан"}",
            "«___» ________ 20___ г."
        };

            foreach (var line in approvalLines)
            {
                approveTable.AddCell(new Cell()
                    .Add(new Paragraph(line).SetFont(_fontNormal).SetFontSize(12))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetPadding(2));
            }

            document.Add(approveTable);
            document.Add(new Paragraph("\n"));
        }

        private void AddReportHeader(Document document, DepartmentDto department, string reportType)
        {
            // Заголовок отчета
            var title = reportType switch
            {
                "Стандартный" => "Расписание экзаменов",
                "По модулю" => "Расписание экзаменов по модулю",
                "Квалификационный" => "Расписание экзаменов квалификационных",
                _ => "Расписание экзаменов"
            };

            document.Add(new Paragraph(title)
                .SetFont(_fontBold)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER));

            // Название отделения
            var departmentName = department.NameOfDepartment switch
            {
                "Информационное" => "Отделение информационных технологий и экономики",
                "Горное" => "Горное отделение",
                "Геолого-маркшейдерское" => "Геолого-маркшейдерское отделение",
                _ => department.NameOfDepartment
            };

            document.Add(new Paragraph(departmentName)
                .SetFont(_fontItalic)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("Таблица")
                .SetFont(_fontNormal)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(5)
                .SetMarginBottom(10));
        }

        private async Task AddExamsTable(Document document, ObservableCollection<ExamDisplayDto> exams, string reportType)
        {
            Table table = reportType switch
            {
                "Стандартный" => CreateStandardExamsTable(exams),
                "По модулю" or "Квалификационный" => CreateModuleExamsTable(exams),
                _ => CreateStandardExamsTable(exams)
            };

            document.Add(table);
        }

        private async Task AddAgreementSection(Document document, List<ManagerDto> managers,
            List<DepartmentOwnerDto> departmentOwners, DepartmentDto department)
        {
            var studyWorkEmployee = managers.FirstOrDefault(m => m.IdManager == 2);
            var ownerStudyDepartment = managers.FirstOrDefault(m => m.IdManager == 3);
            var departmentOwner = departmentOwners.FirstOrDefault(d => d.IdDepartment == department.IdDepartment);

            document.Add(new Paragraph("Согласовано:")
                .SetFont(_fontBold)
                .SetFontSize(12)
                .SetMarginTop(20));

            var agreements = new[]
            {
            new { Name = studyWorkEmployee?.FullName ?? "Не указан", Position = studyWorkEmployee?.Post ?? "Должность не указана" },
            new { Name = departmentOwner?.OwnerName ?? "Не указан", Position = GetDepartmentOwnerPosition(department.NameOfDepartment) },
            new { Name = ownerStudyDepartment?.FullName ?? "Не указан", Position = ownerStudyDepartment?.Post ?? "Должность не указана" }
        };

            foreach (var item in agreements)
            {
                var lineTable = new Table(UnitValue.CreatePercentArray(new float[] { 40f, 60f }))
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginTop(5);

                lineTable.AddCell(new Cell()
                    .Add(new Paragraph(item.Position).SetFont(_fontNormal))
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetPadding(0));

                lineTable.AddCell(new Cell()
                    .Add(new Paragraph(item.Name).SetFont(_fontNormal))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetPadding(0));

                document.Add(lineTable);
            }
        }

        private Table CreateStandardExamsTable(ObservableCollection<ExamDisplayDto> exams)
        {
            var table = new Table(new float[] { 2, 2, 2, 3, 2, 3 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(10)
                .SetMarginBottom(10);

            AddTableHeaders(table, new[] { "Дата", "Группа", "Консультация/Экзамен", "Дисциплина, МДК", "Аудитория", "Члены ЭК" });
            AddTableData(table, exams, includeLessonType: true);

            return table;
        }

        private Table CreateModuleExamsTable(ObservableCollection<ExamDisplayDto> exams)
        {
            var table = new Table(new float[] { 2, 2, 3, 2, 3 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(10)
                .SetMarginBottom(10);

            AddTableHeaders(table, new[] { "Дата", "Группа", "ПМ", "Аудитория", "Члены ЭК" });
            AddTableData(table, exams, includeLessonType: false);

            return table;
        }

        private void AddTableHeaders(Table table, string[] headers)
        {
            foreach (var header in headers)
            {
                table.AddHeaderCell(new Cell()
                    .Add(new Paragraph(header).SetFont(_fontBold).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                    .SetPadding(8));
            }
        }

        private void AddTableData(Table table, ObservableCollection<ExamDisplayDto> exams, bool includeLessonType)
        {
            foreach (var exam in exams.OrderBy(e => e.DateEvent))
            {
                AddTableCell(table, exam.DateEvent.ToString("dd.MM.yyyy HH:mm"));
                AddTableCell(table, exam.GroupName);

                if (includeLessonType)
                {
                    AddTableCell(table, exam.TypeOfLessonName);
                }

                AddTableCell(table, exam.DisciplineName);
                AddTableCell(table, exam.AudienceNumber);
                AddTableCell(table, exam.TeachersDisplay);
            }
        }

        private void AddTableCell(Table table, string text)
        {
            table.AddCell(new Cell()
                .Add(new Paragraph(text).SetFont(_fontNormal).SetFontSize(9))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6));
        }

        private string GetDepartmentOwnerPosition(string departmentName)
        {
            return departmentName switch
            {
                "Информационное" => "Зав. отделением ИТ и Э",
                "Горное" => "Зав. горным отделением",
                "Геолого-маркшейдерское" => "Зав. Г-М отделением",
                _ => "Зав. отделением"
            };
        }
    }
}
