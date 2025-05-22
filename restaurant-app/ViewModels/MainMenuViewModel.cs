using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using restaurant_app.Helpers;
using restaurant_app.Models;
using restaurant_app.Services;
using restaurant_app.Views;

namespace restaurant_app.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly MenuService _menuService;
        private readonly AuthService _authService;

        // Collections
        private ObservableCollection<Models.ProductWithCategoryAndAllergens> _products = new ObservableCollection<Models.ProductWithCategoryAndAllergens>();
        private ObservableCollection<MenuWithProducts> _menus = new ObservableCollection<MenuWithProducts>();
        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        private ObservableCollection<Models.ProductWithCategoryAndAllergens> _searchResults = new ObservableCollection<Models.ProductWithCategoryAndAllergens>();

        // UI Properties
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _isSearchResultsVisible;
        private string _searchKeyword = string.Empty;
        private string _searchAllergen = string.Empty;
        private bool _excludeAllergen;

        public ObservableCollection<Models.ProductWithCategoryAndAllergens> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<MenuWithProducts> Menus
        {
            get => _menus;
            set => SetProperty(ref _menus, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsSearchResultsVisible
        {
            get => _isSearchResultsVisible;
            set => SetProperty(ref _isSearchResultsVisible, value);
        }

        public ObservableCollection<Models.ProductWithCategoryAndAllergens> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }

        public string SearchAllergen
        {
            get => _searchAllergen;
            set => SetProperty(ref _searchAllergen, value);
        }

        public bool ExcludeAllergen
        {
            get => _excludeAllergen;
            set => SetProperty(ref _excludeAllergen, value);
        }

        // Auth Properties
        private bool _isUserLoggedIn;
        public bool IsUserLoggedIn
        {
            get => _isUserLoggedIn;
            set => SetProperty(ref _isUserLoggedIn, value);
        }
        public bool IsUserEmployee => _authService.IsEmployee;
        public bool IsUserClient => _authService.IsClient;
        public string LoggedInUsername => IsUserLoggedIn ? _authService.CurrentUser?.Username : "Vizitator";

        // Commands
        public ICommand SearchByKeywordCommand { get; }
        public ICommand SearchByAllergenCommand { get; }
        public ICommand ViewProductDetailsCommand { get; }
        public ICommand ViewMenuDetailsCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand ViewCartCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand RegisterCommand { get; }

        public MainMenuViewModel(MenuService menuService, AuthService authService, NavigationService navigationService)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _isUserLoggedIn = _authService.IsLoggedIn;

            // Initialize commands
            SearchByKeywordCommand = new RelayCommand(async _ => await ExecuteSearchByKeywordAsync(), _ => !string.IsNullOrWhiteSpace(SearchKeyword));
            SearchByAllergenCommand = new RelayCommand(async _ => await ExecuteSearchByAllergenAsync(), _ => !string.IsNullOrWhiteSpace(SearchAllergen));
            ViewProductDetailsCommand = new RelayCommand(ExecuteViewProductDetails);
            ViewMenuDetailsCommand = new RelayCommand(ExecuteViewMenuDetails);
            AddToCartCommand = new RelayCommand(ExecuteAddToCart, _ => IsUserClient);
            ViewCartCommand = new RelayCommand(_ => ExecuteViewCart(), _ => IsUserClient);
            LoginCommand = new RelayCommand(_ => ExecuteLogin());
            LogoutCommand = new RelayCommand(_ => ExecuteLogout(), _ => IsUserLoggedIn);
            RegisterCommand = new RelayCommand(_ => ExecuteRegister());
        }

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Se încarcă datele...";

                var productsResult = await _menuService.GetAllProductsAsync();
                Products.Clear();
                foreach (var product in productsResult)
                {
                    Products.Add(product);
                }

                var menusResult = await _menuService.GetAllMenusAsync();
                Menus.Clear();
                foreach (var menu in menusResult)
                {
                    Menus.Add(menu);
                }

                var categoriesResult = await _menuService.GetAllCategoriesAsync();
                Categories.Clear();
                foreach (var category in categoriesResult)
                {
                    Categories.Add(category);
                }

                StatusMessage = "Datele au fost încărcate cu succes.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteSearchByKeywordAsync()
        {
            try
            {
                IsLoading = true;
                var results = await _menuService.SearchProductsByKeywordAsync(SearchKeyword);

                SearchResults.Clear();
                foreach (var result in results)
                {
                    SearchResults.Add(result);
                }

                IsSearchResultsVisible = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteSearchByAllergenAsync()
        {
            try
            {
                IsLoading = true;
                var results = await _menuService.SearchProductsByAllergenAsync(SearchAllergen, ExcludeAllergen);

                SearchResults.Clear();
                foreach (var result in results)
                {
                    SearchResults.Add(result);
                }

                IsSearchResultsVisible = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteViewProductDetails(object parameter)
        {
            // Simple implementation
            StatusMessage = "Vizualizare detalii produs";
        }

        private void ExecuteViewMenuDetails(object parameter)
        {
            // Simple implementation
            StatusMessage = "Vizualizare detalii meniu";
        }

        private void ExecuteAddToCart(object parameter)
        {
            if (!IsUserClient)
            {
                MessageBox.Show("Trebuie să vă autentificați pentru această acțiune.", "Autentificare necesară");
                ExecuteLogin();
                return;
            }

            StatusMessage = "Produs adăugat în coș";
        }

        private void ExecuteViewCart()
        {
            if (!IsUserClient)
            {
                MessageBox.Show("Trebuie să vă autentificați pentru această acțiune.", "Autentificare necesară");
                return;
            }

            var orderPage = new OrderPage();
            var window = new Window
            {
                Content = orderPage,
                Title = "Order Page",
                Width = 800,
                Height = 600
            };
            window.Show();
        }


        private void ExecuteLogin()
        {
            var loginWindow = new LoginPage();
            loginWindow.Owner = Application.Current.MainWindow; // Set the owner
            loginWindow.ShowDialog();

            // Update IsUserLoggedIn property after login attempt
            IsUserLoggedIn = _authService.IsLoggedIn;

            // Update other UI properties
            OnPropertyChanged(nameof(IsUserClient));
            OnPropertyChanged(nameof(IsUserEmployee));
            OnPropertyChanged(nameof(LoggedInUsername));
        }

        private void ExecuteLogout()
        {
            _authService.Logout();

            // Update IsUserLoggedIn property after logout
            IsUserLoggedIn = _authService.IsLoggedIn;

            // Update other UI properties
            OnPropertyChanged(nameof(IsUserClient));
            OnPropertyChanged(nameof(IsUserEmployee));
            OnPropertyChanged(nameof(LoggedInUsername));
        }

        private void ExecuteRegister()
        {
            var registerWindow = new RegisterPage();
            registerWindow.ShowDialog();
        }
    }
}
