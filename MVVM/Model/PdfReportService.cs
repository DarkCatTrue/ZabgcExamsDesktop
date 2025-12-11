using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Collections.ObjectModel;
using System.IO;
using ZabgcExamsDesktop.API;
using ZabgcExamsDesktop.API.Models;

namespace ZabgcExamsDesktop.MVVM.Model
{
    public class PdfReportService
    {
        private readonly ApiService _apiService;
        private readonly string _fontNormalPath;
        private readonly string _fontBoldPath;
        private readonly string _fontItalicPath;

        public PdfReportService(ApiService apiService)
        {
            _apiService = apiService;
            // Сохраняем только пути к шрифтам, а не сами шрифты
            _fontNormalPath = @"C:\Windows\Fonts\times.ttf";
            _fontBoldPath = @"C:\Windows\Fonts\timesbd.ttf";
            _fontItalicPath = @"C:\Windows\Fonts\timesi.ttf";
        }

        public async Task<bool> GenerateReportAsync(string filePath, ObservableCollection<ExamDisplayDto> exams,
            DepartmentDto selectedDepartment, string selectedResult)
        {
            try
            {
                var managers = await _apiService.GetManagersAsync();
                var departmentOwners = await _apiService.GetDepartmentOwnersAsync();

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    await Task.Delay(100);
                }

                using var writer = new PdfWriter(filePath);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf, PageSize.A4);

                document.SetMargins(40, 40, 40, 40);

                // Создаем шрифты для КАЖДОГО документа
                var fontNormal = PdfFontFactory.CreateFont(_fontNormalPath, PdfEncodings.IDENTITY_H);
                var fontBold = PdfFontFactory.CreateFont(_fontBoldPath, PdfEncodings.IDENTITY_H);
                var fontItalic = PdfFontFactory.CreateFont(_fontItalicPath, PdfEncodings.IDENTITY_H);

                await AddApprovalSection(document, managers, fontNormal);
                AddReportHeader(document, selectedDepartment, selectedResult, fontBold, fontItalic, fontNormal);
                await AddExamsTable(document, exams, selectedResult, fontNormal, fontBold);
                await AddAgreementSection(document, managers, departmentOwners, selectedDepartment, fontBold, fontNormal);

                document.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF generation error: {ex.Message}");
                return false;
            }
        }

        private async Task AddApprovalSection(Document document, List<ManagerDto> managers, PdfFont fontNormal)
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
                    .Add(new Paragraph(line).SetFont(fontNormal).SetFontSize(12))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetPadding(2));
            }

            document.Add(approveTable);
            document.Add(new Paragraph("\n"));
        }

        private void AddReportHeader(Document document, DepartmentDto department, string reportType,
            PdfFont fontBold, PdfFont fontItalic, PdfFont fontNormal)
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
                .SetFont(fontBold)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER));

            // Название отделения
            var departmentName = department.NameOfDepartment switch
            {
                "Информационных технологий и экономики" => "Отделение информационных технологий и экономики",
                "Горное" => "Горное отделение",
                "Геолого-маркшейдерское" => "Геолого-маркшейдерское отделение",
                _ => department.NameOfDepartment
            };

            document.Add(new Paragraph(departmentName)
                .SetFont(fontItalic)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("Таблица")
                .SetFont(fontNormal)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(5)
                .SetMarginBottom(10));
        }

        private async Task AddExamsTable(Document document, ObservableCollection<ExamDisplayDto> exams, string reportType,
            PdfFont fontNormal, PdfFont fontBold)
        {
            Table table = reportType switch
            {
                "Стандартный" => CreateStandardExamsTable(exams, fontNormal, fontBold),
                "По модулю" or "Квалификационный" => CreateModuleExamsTable(exams, fontNormal, fontBold),
                _ => CreateStandardExamsTable(exams, fontNormal, fontBold)
            };

            document.Add(table);
        }

        private async Task AddAgreementSection(Document document, List<ManagerDto> managers,
            List<DepartmentOwnerDto> departmentOwners, DepartmentDto department,
            PdfFont fontBold, PdfFont fontNormal)
        {
            var studyWorkEmployee = managers.FirstOrDefault(m => m.IdManager == 2);
            var ownerStudyDepartment = managers.FirstOrDefault(m => m.IdManager == 3);
            var departmentOwner = departmentOwners.FirstOrDefault(d => d.IdDepartment == department.IdDepartment);

            document.Add(new Paragraph("Согласовано:")
                .SetFont(fontBold)
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
                    .Add(new Paragraph(item.Position).SetFont(fontNormal))
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetPadding(0));

                lineTable.AddCell(new Cell()
                    .Add(new Paragraph(item.Name).SetFont(fontNormal))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetPadding(0));

                document.Add(lineTable);
            }
        }

        private Table CreateStandardExamsTable(ObservableCollection<ExamDisplayDto> exams, PdfFont fontNormal, PdfFont fontBold)
        {
            var table = new Table(new float[] { 2, 2, 2, 3, 2, 3 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(10)
                .SetMarginBottom(10);

            AddTableHeaders(table, new[] { "Дата", "Группа", "Консультация/Экзамен", "Дисциплина, МДК", "Аудитория", "Члены ЭК" }, fontBold);
            AddTableData(table, exams, includeLessonType: true, fontNormal);

            return table;
        }

        private Table CreateModuleExamsTable(ObservableCollection<ExamDisplayDto> exams, PdfFont fontNormal, PdfFont fontBold)
        {
            var table = new Table(new float[] { 2, 2, 3, 2, 3 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(10)
                .SetMarginBottom(10);

            AddTableHeaders(table, new[] { "Дата", "Группа", "ПМ", "Аудитория", "Члены ЭК" }, fontBold);
            AddTableData(table, exams, includeLessonType: false, fontNormal);

            return table;
        }

        private void AddTableHeaders(Table table, string[] headers, PdfFont fontBold)
        {
            foreach (var header in headers)
            {
                table.AddHeaderCell(new Cell()
                    .Add(new Paragraph(header).SetFont(fontBold).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                    .SetPadding(8));
            }
        }

        private void AddTableData(Table table, ObservableCollection<ExamDisplayDto> exams, bool includeLessonType, PdfFont fontNormal)
        {
            var groupedExams = exams
                .GroupBy(e => e.GroupName)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in groupedExams)
            {
                var sortedExams = group.OrderBy(e => e.DateEvent).ToList();

                foreach (var exam in sortedExams)
                {
                    AddTableCell(table, exam.DateEvent.ToString("dd.MM.yyyy HH:mm"), fontNormal);
                    AddTableCell(table, exam.GroupName, fontNormal);

                    if (includeLessonType)
                    {
                        AddTableCell(table, exam.TypeOfLessonName, fontNormal);
                    }

                    AddTableCell(table, exam.DisciplineName, fontNormal);
                    AddTableCell(table, exam.AudienceNumber, fontNormal);
                    AddTableCell(table, exam.TeachersDisplay, fontNormal, isTeachersCell: true);
                }
            }
        }

        private void AddTableCell(Table table, string text, PdfFont fontNormal, bool isTeachersCell = false)
        {
            var cell = new Cell()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6);

            if (isTeachersCell && !string.IsNullOrEmpty(text))
            {
                var teachers = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(t => t.Trim())
                                  .Where(t => !string.IsNullOrEmpty(t))
                                  .ToList();

                var paragraph = new Paragraph();
                for (int i = 0; i < teachers.Count; i++)
                {
                    paragraph.Add(teachers[i]);
                    if (i < teachers.Count - 1)
                    {
                        paragraph.Add(new Text("\n"));
                    }
                }
                paragraph.SetFont(fontNormal).SetFontSize(9);
                cell.Add(paragraph);
            }
            else
            {
                cell.Add(new Paragraph(text).SetFont(fontNormal).SetFontSize(9));
            }

            table.AddCell(cell);
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
