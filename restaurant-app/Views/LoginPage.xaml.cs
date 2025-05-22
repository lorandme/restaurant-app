using System.Windows;
using restaurant_app.Services;
using restaurant_app.ViewModels;

namespace restaurant_app.Views
{
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();

            try
            {
                // Setăm ViewModel-ul
                var viewModel = new LoginViewModel();
                DataContext = viewModel;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Eroare: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // In LoginPage.xaml.cs, after successful login
                if (this.Owner is MainWindow mainWindow && mainWindow.DataContext is MainMenuViewModel viewModel)
                {
                    viewModel.IsUserLoggedIn = true;
                }

            }
        }
    }
}