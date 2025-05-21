using System.Windows;

namespace restaurant_app.Helpers
{
    public static class WindowNavigationHelper
    {
        public static void NavigateToWindow<T>() where T : Window, new()
        {
            var window = new T();
            window.Show();

            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Close();
            }

            Application.Current.MainWindow = window;
        }

        public static void ShowWindow<T>() where T : Window, new()
        {
            var window = new T();
            window.Show();
        }
    }
}