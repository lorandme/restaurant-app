using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using restaurant_app.Helpers;
using restaurant_app.Models;
using restaurant_app.Services;

namespace restaurant_app.ViewModels
{
    public class LowStockReportViewModel : ViewModelBase
    {
        private readonly MenuService _menuService;
        private readonly NavigationService _navigationService;
        private readonly AuthService _authService;
        private readonly ConfigService _configService;

        private ObservableCollection<ProductLowStock> _lowStockProducts = new();
        private string _statusMessage = string.Empty;
        private bool _isLoading;

        public ObservableCollection<ProductLowStock> LowStockProducts
        {
            get => _lowStockProducts;
            set => SetProperty(ref _lowStockProducts, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand NavigateBackCommand { get; }

        public LowStockReportViewModel(
            MenuService menuService,
            NavigationService navigationService,
            AuthService authService,
            ConfigService configService)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));

            // Check authorization
            if (!_authService.IsLoggedIn || !_authService.IsEmployee)
            {
                _navigationService.NavigateTo("Login");
                return;
            }

            // Initialize commands
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            NavigateBackCommand = new RelayCommand(_ => _navigationService.NavigateTo("AdminDashboard"));

            // Load data
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Se încarcă datele...";

                // Get products with low stock
                var products = await _menuService.GetLowStockProductsAsync();
                LowStockProducts = new ObservableCollection<ProductLowStock>(products);

                // Get the threshold value from config
                var threshold = _configService.GetDecimalValue("LowStockThreshold", 5.0m);

                StatusMessage = $"Produse cu stoc mai mic sau egal cu {threshold} {(products.Count > 0 ? products[0].PortionUnit : "")}: {LowStockProducts.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la încărcarea datelor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}