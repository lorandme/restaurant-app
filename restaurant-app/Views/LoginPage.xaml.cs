using System.Windows;
using restaurant_app.Services;
using restaurant_app.ViewModels;

namespace restaurant_app.Views
{
    public partial class LoginPage : Window
    {
        private LoginViewModel _viewModel;

        public LoginPage()
        {
            InitializeComponent();

            try
            {
                // Initialize ServiceLocator
                var serviceLocator = ServiceLocator.Instance;

                // Create a frame for navigation that will be used after login
                var navigationFrame = new System.Windows.Controls.Frame();

                // Initialize navigation service
                serviceLocator.InitializeNavigationService(navigationFrame);

                // Create and set the ViewModel
                _viewModel = new LoginViewModel(
                    serviceLocator.AuthService,
                    serviceLocator.NavigationService);

                // Set the DataContext
                DataContext = _viewModel;

                // Initialize configuration when window loads
                Loaded += OnWindowLoaded;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error initializing login: {ex.Message}", "Initialization Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load configuration
                await ServiceLocator.Instance.InitializeConfigurationAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
