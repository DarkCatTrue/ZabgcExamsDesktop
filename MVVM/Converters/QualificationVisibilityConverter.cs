using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace ZabgcExamsDesktop.MVVM.Converters
{
    public class QualificationVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string typeOfExam && !string.IsNullOrEmpty(typeOfExam))
                {
                    return typeOfExam.Contains("Квалификационный", StringComparison.OrdinalIgnoreCase)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }

                return Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
