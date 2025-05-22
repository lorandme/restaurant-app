using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace restaurant_app.Helpers
{
    public class StringEqualityToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            string stringValue = value.ToString();
            string stringParameter = parameter.ToString();

            return stringValue == stringParameter ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Visible;

            string stringValue = value.ToString();
            string stringParameter = parameter.ToString();
            bool inverse = false;

            // Check if we're doing inverse logic (parameter2)
            if (parameter is string[] parameters && parameters.Length > 1)
            {
                stringParameter = parameters[0];
                inverse = parameters[1].Equals("inverse", StringComparison.OrdinalIgnoreCase);
            }

            // Split the parameter by | to check multiple values
            var valuesToCheck = stringParameter.Split('|');
            bool isInList = valuesToCheck.Contains(stringValue);

            return (isInList != inverse) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
