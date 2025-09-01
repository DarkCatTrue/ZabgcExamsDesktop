using System.Globalization;
using System.Windows.Data;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

namespace ZabgcExamsDesktop.MVVM.Converters
{
    public class AudiencesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<Audience> audiences && audiences.Any())
                return string.Join(", ", audiences.Select(a => a.NumberAudience));

            return "Не указано";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
