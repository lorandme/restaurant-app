using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private readonly NavigationService _navigationService;

        // Collections
        private ObservableCollection<Models.ProductWithCategoryAndAllergens> _products = new ObservableCollection<Models.ProductWithCategoryAndAllergens>();
        private ObservableCollection<MenuWithProducts> _menus = new ObservableCollection<MenuWithProducts>();
        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        private ObservableCollection<Models.ProductWithCategoryAndAllergens> _searchResults = new ObservableCollection<Models.ProductWithCategoryAndAllergens>();
        // Update the type declaration to use fully qualified namespace for ProductWithCategoryAndAllergens
        private Dictionary<string, List<restaurant_app.Models.ProductWithCategoryAndAllergens>> _categorizedProducts;

        // UI Properties
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _isSearchResultsVisible;
        private string _searchKeyword = string.Empty;
        private string _searchAllergen = string.Empty;
        private bool _excludeAllergen;
        private Category _selectedCategory;
        private int _cartItemCount = 0;

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

        public Dictionary<string, List<restaurant_app.Models.ProductWithCategoryAndAllergens>> CategorizedProducts
        {
            get => _categorizedProducts;
            set => SetProperty(ref _categorizedProducts, value);
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

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value) && value != null)
                {
                    FilterProductsByCategory(value);
                }
            }
        }

        public int CartItemCount
        {
            get => _cartItemCount;
            set => SetProperty(ref _cartItemCount, value);
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
        public string LoggedInUsername => IsUserLoggedIn ?
            $"{_authService.CurrentUser?.FirstName} {_authService.CurrentUser?.LastName}"
            : "Vizitator";


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
        public ICommand BackToMenuCommand { get; }
        public ICommand ViewMenuCommand { get; }
        public ICommand ViewMyOrdersCommand { get; }
        public ICommand AdminDashboardCommand { get; }

        public MainMenuViewModel(MenuService menuService, AuthService authService, NavigationService navigationService)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService;
            _isUserLoggedIn = _authService.IsLoggedIn;
            _categorizedProducts = new Dictionary<string, List<restaurant_app.Models.ProductWithCategoryAndAllergens>>();

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
            BackToMenuCommand = new RelayCommand(_ => IsSearchResultsVisible = false);
            ViewMenuCommand = new RelayCommand(_ => ExecuteViewMenu());
            ViewMyOrdersCommand = new RelayCommand(_ => ExecuteViewMyOrders(), _ => IsUserClient);
            AdminDashboardCommand = new RelayCommand(_ => ExecuteAdminDashboard(), _ => IsUserEmployee);
        }


        private void ExecuteViewMenu()
        {
            IsSearchResultsVisible = false;
            StatusMessage = "Vizualizare meniu restaurant";
        }

        private void ExecuteViewMyOrders()
        {
            if (!IsUserClient)
            {
                MessageBox.Show("Trebuie să vă autentificați pentru această acțiune.", "Autentificare necesară");
                ExecuteLogin();
                return;
            }

            try
            {
                // Get required services from ServiceLocator
                var orderService = ServiceLocator.Instance.OrderService;
                var authService = ServiceLocator.Instance.AuthService;
                var configService = ServiceLocator.Instance.ConfigService;

                // Create and initialize the OrderViewModel
                // Pass null for NavigationService since it might not be initialized
                var orderViewModel = new OrderViewModel(
                    orderService,
                    authService,
                    configService,
                    null); // Pass null for NavigationService

                // Create the OrderPage and set its DataContext
                var orderPage = new OrderPage
                {
                    DataContext = orderViewModel
                };

                // Create and show the window
                var window = new Window
                {
                    Content = orderPage,
                    Title = "Comenzile Mele",
                    Width = 800,
                    Height = 600
                };

                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la deschiderea comenzilor: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void ExecuteViewCart()
        {
            if (!IsUserClient)
            {
                MessageBox.Show("Trebuie să vă autentificați pentru această acțiune.", "Autentificare necesară");
                return;
            }

            try
            {
                // Get required services from ServiceLocator
                var orderService = ServiceLocator.Instance.OrderService;
                var authService = ServiceLocator.Instance.AuthService;
                var configService = ServiceLocator.Instance.ConfigService;

                // Create and initialize the OrderViewModel
                // Pass null for NavigationService since it might not be initialized
                var orderViewModel = new OrderViewModel(
                    orderService,
                    authService,
                    configService,
                    null); // Pass null for NavigationService

                // Create the OrderPage and set its DataContext
                var orderPage = new OrderPage
                {
                    DataContext = orderViewModel
                };

                // Create and show the window
                var window = new Window
                {
                    Content = orderPage,
                    Title = "Coșul meu",
                    Width = 800,
                    Height = 600
                };

                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la deschiderea coșului: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void ExecuteAdminDashboard()
        {
            if (!IsUserEmployee)
            {
                MessageBox.Show("Acces restricționat doar pentru angajați.", "Acces restricționat");
                return;
            }

            // Open admin dashboard
            var adminPage = new AdminDashboardPage();
            var window = new Window
            {
                Content = adminPage,
                Title = "Administrare Restaurant",
                Width = 1000,
                Height = 700
            };
            window.Show();
        }

        private void FilterProductsByCategory(Category category)
        {
            if (category == null)
                return;

            try
            {
                IsLoading = true;
                var filteredProducts = Products.Where(p => p.CategoryID == category.CategoryId).ToList();
                SearchResults.Clear();
                foreach (var product in filteredProducts)
                {
                    SearchResults.Add(product);
                }
                IsSearchResultsVisible = true;
                StatusMessage = $"Produse din categoria: {category.Name}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la filtrare: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
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

                // Group products by category
                CategorizedProducts = Products
                    .GroupBy(p => p.CategoryName)
                    .ToDictionary(g => g.Key, g => g.ToList());

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
                StatusMessage = $"Căutare după: {SearchKeyword}";
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
                StatusMessage = $"Căutare după alergen: {SearchAllergen} ({(ExcludeAllergen ? "exclude" : "include")})";
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

            // Increment cart counter
            CartItemCount++;
            StatusMessage = "Produs adăugat în coș";
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

            // Reset cart when logging out
            CartItemCount = 0;

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