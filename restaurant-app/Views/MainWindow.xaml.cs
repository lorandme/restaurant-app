using System;
using System.Windows;
using restaurant_app.Services;
using restaurant_app.ViewModels;

namespace restaurant_app.Views
{
    public partial class MainWindow : Window
    {
        private MainMenuViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Inițializăm serviciile
                var serviceLocator = ServiceLocator.Instance;

                // Creăm ViewModel-ul utilizând serviciile din ServiceLocator
                _viewModel = new MainMenuViewModel(
                    serviceLocator.MenuService,
                    serviceLocator.AuthService,
                    serviceLocator.NavigationService);

                // Setăm DataContext
                DataContext = _viewModel;

                // Încărcăm datele când fereastra este încărcată
                Loaded += OnWindowLoaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la inițializarea aplicației: {ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Încărcăm configurația și datele
                await ServiceLocator.Instance.InitializeConfigurationAsync();
                await _viewModel.LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea datelor: {ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
