using System.Globalization;
using System.Windows.Data;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

namespace ZabgcExamsDesktop.MVVM.Converters
{
    public class TeachersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<Teacher> teachers && teachers.Any())
                return string.Join(", ", teachers.Select(t => t.FullName));

            return "Не указано";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
