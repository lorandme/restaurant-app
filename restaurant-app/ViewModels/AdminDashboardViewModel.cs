using System;
using System.Windows;
using System.Windows.Input;
using restaurant_app.Helpers;
using restaurant_app.Services;
using restaurant_app.Views;

namespace restaurant_app.ViewModels
{
    public class AdminDashboardViewModel : ViewModelBase
    {
        private readonly NavigationService _navigationService;
        private readonly AuthService _authService;
        private string _statusMessage = string.Empty;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand NavigateToProductsCommand { get; }
        public ICommand NavigateToMenusCommand { get; }
        public ICommand NavigateToOrdersCommand { get; }
        public ICommand NavigateToLowStockReportCommand { get; }
        public ICommand NavigateToMainMenuCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminDashboardViewModel(NavigationService navigationService, AuthService authService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            // Initialize commands
            NavigateToProductsCommand = new RelayCommand(_ => HandleAdminProductsNavigation());
            NavigateToMenusCommand = new RelayCommand(_ => OpenMenusWindow());
            NavigateToOrdersCommand = new RelayCommand(_ => OpenOrdersWindow());
            NavigateToLowStockReportCommand = new RelayCommand(_ => OpenLowStockReportWindow());
            NavigateToMainMenuCommand = new RelayCommand(_ => CloseCurrentWindow());
            LogoutCommand = new RelayCommand(_ => ExecuteLogout());

            // Check if user is authorized
            if (!_authService.IsLoggedIn || !_authService.IsEmployee)
            {
                StatusMessage = "Accesul este permis doar angajaților.";
            }
            else
            {
                StatusMessage = $"Bine ați venit, {_authService.CurrentUser?.FirstName}!";
            }
        }

        private void HandleAdminProductsNavigation()
        {
            // Since AdminProductsPage doesn't exist yet, show a message
            MessageBox.Show("Funcționalitatea de gestionare produse nu este încă implementată.",
                "Funcționalitate în curs de dezvoltare",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            StatusMessage = "Gestionare produse: Funcționalitate în curs de dezvoltare";

            // Alternatively, you could implement a temporary solution:
            // OpenProductsWindow();
        }

        // This is a placeholder for when AdminProductsPage is created
        private void OpenProductsWindow()
        {
            // TODO: Implement this when AdminProductsPage is created
            StatusMessage = "Funcționalitatea de gestionare produse nu este încă implementată.";

            // Once AdminProductsPage is created, uncomment this code:
            /*
            var productsWindow = new Window
            {
                Content = new AdminProductsPage(),
                Title = "Gestionare Produse",
                Width = 1000,
                Height = 700
            };
            productsWindow.Show();
            StatusMessage = "Deschis: Gestionare Produse";
            */
        }

        private void OpenMenusWindow()
        {
            try
            {
                var menusWindow = new Window
                {
                    Content = new AdminMenusPage(),
                    Title = "Gestionare Meniuri",
                    Width = 1000,
                    Height = 700
                };
                menusWindow.Show();
                StatusMessage = "Deschis: Gestionare Meniuri";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la deschiderea ferestrei: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = $"Eroare: {ex.Message}";
            }
        }

        private void OpenOrdersWindow()
        {
            try
            {
                var ordersWindow = new Window
                {
                    Content = new OrderListPage(),
                    Title = "Gestionare Comenzi",
                    Width = 1000,
                    Height = 700
                };
                ordersWindow.Show();
                StatusMessage = "Deschis: Gestionare Comenzi";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la deschiderea ferestrei: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = $"Eroare: {ex.Message}";
            }
        }

        private void OpenLowStockReportWindow()
        {
            try
            {
                var lowStockWindow = new Window
                {
                    Content = new LowStockReportPage(),
                    Title = "Raport Stoc Scăzut",
                    Width = 900,
                    Height = 600
                };
                lowStockWindow.Show();
                StatusMessage = "Deschis: Raport Stoc Scăzut";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la deschiderea ferestrei: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = $"Eroare: {ex.Message}";
            }
        }

        private void CloseCurrentWindow()
        {
            // Find and close the current dashboard window
            foreach (Window window in Application.Current.Windows)
            {
                if (window.Content is AdminDashboardPage)
                {
                    window.Close();
                    break;
                }
            }
        }

        private void ExecuteLogout()
        {
            _authService.Logout();
            CloseCurrentWindow();
        }
    }
}
