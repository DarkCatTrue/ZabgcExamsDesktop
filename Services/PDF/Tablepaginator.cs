using System;
using System.Collections.Generic;
using System.Linq;
using ZabgcExamsDesktop.MVVM.Model;

namespace ZabgcExamsDesktop.Services.PDF
{
    public class TablePaginator
    {
        private readonly List<TableRowData> _rows = new();
        private const float HeaderHeight = 45;      // примерная высота заголовка таблицы (подбирается)
        private const float RowBaseHeight = 25;     // базовая высота строки
        private const float RowLineHeight = 15;     // добавочная высота на каждую строку текста в ячейке

        public void AddRow(TableRowData row) => _rows.Add(row);
        public void AddRows(IEnumerable<TableRowData> rows) => _rows.AddRange(rows);

        private float GetRowHeight(TableRowData row)
        {
            int maxLines = 1;
            string[] fields = { row.Date, row.GroupName, row.ConsultationType,
                              row.DisciplineName, row.AudienceNumber, row.Teachers };
            foreach (var f in fields.Where(f => !string.IsNullOrEmpty(f)))
            {
                // Переносы строк учитываем по символу \n
                int lines = f.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                if (lines > maxLines) maxLines = lines;
            }
            return RowBaseHeight + (maxLines - 1) * RowLineHeight;
        }

        /// <summary>
        /// Разбиение строк на страницы с учётом разной доступной высоты на первой и последующих страницах.
        /// </summary>
        /// <param name="firstPageHeight">Доступная высота для таблицы на первой странице (в поинтах)</param>
        /// <param name="otherPagesHeight">Доступная высота на всех последующих страницах</param>
        public List<List<TableRowData>> Paginate(float firstPageHeight, float otherPagesHeight)
        {
            var pages = new List<List<TableRowData>>();
            if (_rows.Count == 0) return pages;

            var enumerator = _rows.GetEnumerator();
            enumerator.MoveNext();

            var currentPage = new List<TableRowData>();
            float usedHeight = HeaderHeight;
            float currentLimit = firstPageHeight; // начинаем с ограничения первой страницы

            while (true)
            {
                var row = enumerator.Current;
                float rowHeight = GetRowHeight(row);

                if (usedHeight + rowHeight > currentLimit && currentPage.Count > 0)
                {
                    // Сохраняем текущую страницу и начинаем новую
                    pages.Add(currentPage);
                    currentPage = new List<TableRowData>();
                    usedHeight = HeaderHeight;
                    // На всех следующих страницах используем otherPagesHeight
                    currentLimit = otherPagesHeight;
                    continue; // не переходим к следующей строке, пробуем эту же строку на новой странице
                }

                currentPage.Add(row);
                usedHeight += rowHeight;

                if (!enumerator.MoveNext())
                {
                    if (currentPage.Count > 0) pages.Add(currentPage);
                    break;
                }
            }
            return pages;
        }
    }

    public class TableRowData
    {
        public string Date { get; set; }
        public string GroupName { get; set; }
        public string ConsultationType { get; set; }
        public string DisciplineName { get; set; }
        public string AudienceNumber { get; set; }
        public string Teachers { get; set; }
    }

    public static class ExamExtensions
    {
        public static TableRowData ToTableRowData(this ExamDisplayDto exam, bool isStandard)
        {
            return new TableRowData
            {
                Date = exam.DateEvent.ToString("dd.MM.yyyy HH:mm") ?? "",
                GroupName = exam.GroupName ?? "",
                ConsultationType = isStandard ? (exam.TypeOfLessonName ?? "") : "",
                DisciplineName = exam.DisciplineName ?? "",
                AudienceNumber = exam.AudienceNumber ?? "",
                Teachers = FormatTeachers(exam.TeachersDisplay ?? "")
            };
        }

        private static string FormatTeachers(string teachers)
        {
            if (string.IsNullOrWhiteSpace(teachers)) return "";
            return string.Join("\n", teachers.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t)));
        }
    }
}