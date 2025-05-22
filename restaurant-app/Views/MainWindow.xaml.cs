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

                // Creăm ViewModel-ul
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
                MessageBox.Show($"Eroare: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Eroare: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.IsSearchResultsVisible = false;
            }
        }
    }
}