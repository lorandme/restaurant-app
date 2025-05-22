using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace restaurant_app.Helpers
{
    public class InvertedBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool boolValue))
                return Visibility.Collapsed;

            // If parameter is "inverse", invert the boolean value
            if (parameter != null && parameter.ToString().Equals("inverse", StringComparison.OrdinalIgnoreCase))
            {
                boolValue = !boolValue;
            }

            // Return inverted visibility compared to standard BooleanToVisibilityConverter
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}