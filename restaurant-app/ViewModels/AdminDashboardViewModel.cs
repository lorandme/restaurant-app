using System;
using System.Windows.Input;
using restaurant_app.Helpers;
using restaurant_app.Services;

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
            NavigateToProductsCommand = new RelayCommand(_ => NavigateTo("AdminProducts"));
            NavigateToMenusCommand = new RelayCommand(_ => NavigateTo("AdminMenus"));
            NavigateToOrdersCommand = new RelayCommand(_ => NavigateTo("OrderList"));
            NavigateToLowStockReportCommand = new RelayCommand(_ => NavigateTo("LowStockReport"));
            NavigateToMainMenuCommand = new RelayCommand(_ => NavigateTo("MainMenu"));
            LogoutCommand = new RelayCommand(_ => ExecuteLogout());

            // Check if user is authorized
            if (!_authService.IsLoggedIn || !_authService.IsEmployee)
            {
                StatusMessage = "Accesul este permis doar angajaților. Redirecționare...";
                NavigateTo("Login");
            }
            else
            {
                StatusMessage = $"Bine ați venit, {_authService.CurrentUser?.FirstName}!";
            }
        }

        private void NavigateTo(string viewName)
        {
            _navigationService.NavigateTo(viewName);
        }

        private void ExecuteLogout()
        {
            _authService.Logout();
            _navigationService.NavigateTo("MainMenu");
        }
    }
}