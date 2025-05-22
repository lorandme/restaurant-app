using System.Windows;
using restaurant_app.Services;
using restaurant_app.ViewModels;

namespace restaurant_app.Views
{
    public partial class RegisterPage : Window
    {
        public RegisterPage()
        {
            InitializeComponent();

            try
            {
                // Obținem AuthService din ServiceLocator
                var authService = ServiceLocator.Instance.AuthService;

                // Creăm și setăm ViewModel-ul
                var viewModel = new RegisterViewModel(authService);
                DataContext = viewModel;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Eroare: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}