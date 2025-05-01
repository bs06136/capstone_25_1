using System;
using System.Globalization;
using System.Windows.Data;

namespace overcloud.Converters
{
    public class BoolToActiveTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? "Active" : "Inactive";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
                return s.Equals("Active", StringComparison.OrdinalIgnoreCase);
            return false;
        }
    }
}
