using System.Windows.Controls;
using System.Windows.Media;
using System;
using System.Windows.Data;
using System.Globalization;

namespace restaurant_app.Views
{
    public partial class OrderPage : Page
    {
        public OrderPage()
        {
            InitializeComponent();
        }
    }
    public class OrderStatusToColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToLower())
                {
                    case "registered":
                        return new SolidColorBrush(Color.FromRgb(70, 130, 180)); // Steel Blue
                    case "preparing":
                        return new SolidColorBrush(Color.FromRgb(255, 140, 0)); // Dark Orange
                    case "ondelivery":
                        return new SolidColorBrush(Color.FromRgb(106, 90, 205)); // Slate Blue
                    case "delivered":
                        return new SolidColorBrush(Color.FromRgb(60, 179, 113)); // Medium Sea Green
                    case "cancelled":
                        return new SolidColorBrush(Color.FromRgb(220, 20, 60)); // Crimson
                    default:
                        return new SolidColorBrush(Color.FromRgb(169, 169, 169)); // Dark Gray
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToVisibleIfNotDeliveredConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() != "delivered" && status.ToLower() != "cancelled";
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}