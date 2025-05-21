using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using restaurant_app.Helpers;
using restaurant_app.Models;
using restaurant_app.Services;

namespace restaurant_app.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly MenuService _menuService;
        private readonly AuthService _authService;
        private readonly NavigationService _navigationService;

        // Fully qualify the type to avoid ambiguity
        private ObservableCollection<Models.ProductWithCategoryAndAllergens> _products = new();
        private ObservableCollection<MenuWithProducts> _menus = new();
        private ObservableCollection<Category> _categories = new();
        private string _searchKeyword = string.Empty;
        private string _searchAllergen = string.Empty;
        private bool _excludeAllergen;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _isSearchResultsVisible;
        private ObservableCollection<Models.ProductWithCategoryAndAllergens> _searchResults = new();

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

        public bool IsUserLoggedIn => _authService.IsLoggedIn;
        public bool IsUserEmployee => _authService.IsEmployee;
        public bool IsUserClient => _authService.IsClient;

        public ICommand SearchByKeywordCommand { get; }
        public ICommand SearchByAllergenCommand { get; }
        public ICommand ViewProductDetailsCommand { get; }
        public ICommand ViewMenuDetailsCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand ViewCartCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public MainMenuViewModel(MenuService menuService, AuthService authService, NavigationService navigationService)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            // These initializations are now redundant due to the field initializers above
            // but keeping them for clarity
            Products = new ObservableCollection<Models.ProductWithCategoryAndAllergens>();
            Menus = new ObservableCollection<MenuWithProducts>();
            Categories = new ObservableCollection<Category>();
            SearchResults = new ObservableCollection<Models.ProductWithCategoryAndAllergens>();

            SearchByKeywordCommand = new RelayCommand(async param => await ExecuteSearchByKeywordAsync(), _ => !string.IsNullOrWhiteSpace(SearchKeyword));
            SearchByAllergenCommand = new RelayCommand(async param => await ExecuteSearchByAllergenAsync(), _ => !string.IsNullOrWhiteSpace(SearchAllergen));

            // Fix null warnings - cast will throw if param is null so no need for the null check
            ViewProductDetailsCommand = new RelayCommand(ExecuteViewProductDetails);
            ViewMenuDetailsCommand = new RelayCommand(ExecuteViewMenuDetails);
            AddToCartCommand = new RelayCommand(ExecuteAddToCart, _ => IsUserClient);

            ViewCartCommand = new RelayCommand(_ => ExecuteViewCart(), _ => IsUserClient);
            LoginCommand = new RelayCommand(_ => ExecuteLogin(), _ => !IsUserLoggedIn);
            RegisterCommand = new RelayCommand(_ => ExecuteRegister(), _ => !IsUserLoggedIn);
        }

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Încărcare date...";

                // Fix for ENC0046: Split the await from the assignment
                var productsResult = await _menuService.GetAllProductsAsync();
                var menusResult = await _menuService.GetAllMenusAsync();
                var categoriesResult = await _menuService.GetAllCategoriesAsync();

                // Convert the results to ObservableCollection
                Products = new ObservableCollection<Models.ProductWithCategoryAndAllergens>(
                    productsResult.Select(p => new Models.ProductWithCategoryAndAllergens
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        Price = p.Price,
                        PortionQuantity = p.PortionQuantity,
                        PortionUnit = p.PortionUnit,
                        TotalQuantity = p.TotalQuantity,
                        CategoryID = p.CategoryID,
                        CategoryName = p.CategoryName,
                        Allergens = p.Allergens
                    }));

                Menus = new ObservableCollection<MenuWithProducts>(menusResult);
                Categories = new ObservableCollection<Category>(categoriesResult);

                StatusMessage = $"Încărcare completă: {Products.Count} produse și {Menus.Count} meniuri";
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

        private async Task ExecuteSearchByKeywordAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Căutare...";
                IsSearchResultsVisible = true;

                // Fix for ENC0046: Split the await from the assignment
                var resultsData = await _menuService.SearchProductsByKeywordAsync(SearchKeyword);

                // Convert the results to properly typed ObservableCollection
                SearchResults = new ObservableCollection<Models.ProductWithCategoryAndAllergens>(
                    resultsData.Select(p => new Models.ProductWithCategoryAndAllergens
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        Price = p.Price,
                        PortionQuantity = p.PortionQuantity,
                        PortionUnit = p.PortionUnit,
                        TotalQuantity = p.TotalQuantity,
                        CategoryID = p.CategoryID,
                        CategoryName = p.CategoryName,
                        Allergens = p.Allergens
                    }));

                StatusMessage = $"Rezultate găsite: {SearchResults.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la căutare: {ex.Message}";
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
                StatusMessage = "Căutare...";
                IsSearchResultsVisible = true;

                // Fix for ENC0046: Split the await from the assignment
                var resultsData = await _menuService.SearchProductsByAllergenAsync(SearchAllergen, ExcludeAllergen);

                // Convert the results to properly typed ObservableCollection
                SearchResults = new ObservableCollection<Models.ProductWithCategoryAndAllergens>(
                    resultsData.Select(p => new Models.ProductWithCategoryAndAllergens
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        Price = p.Price,
                        PortionQuantity = p.PortionQuantity,
                        PortionUnit = p.PortionUnit,
                        TotalQuantity = p.TotalQuantity,
                        CategoryID = p.CategoryID,
                        CategoryName = p.CategoryName,
                        Allergens = p.Allergens
                    }));

                string searchType = ExcludeAllergen ? "nu conțin" : "conțin";
                StatusMessage = $"Rezultate găsite care {searchType} '{SearchAllergen}': {SearchResults.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la căutare: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteViewProductDetails(object parameter)
        {
            // Use pattern matching with null check to eliminate warning
            if (parameter is Models.ProductWithCategoryAndAllergens product)
            {
                StatusMessage = $"Vizualizare detalii pentru produsul: {product.ProductName}";
            }
        }

        private void ExecuteViewMenuDetails(object parameter)
        {
            // Use pattern matching with null check to eliminate warning
            if (parameter is MenuWithProducts menu)
            {
                StatusMessage = $"Vizualizare detalii pentru meniul: {menu.MenuName}";
            }
        }

        private void ExecuteAddToCart(object parameter)
        {
            if (!_authService.IsLoggedIn)
            {
                StatusMessage = "Trebuie să vă autentificați pentru a adăuga produse în coș!";
                return;
            }

            // Use pattern matching with null check to eliminate warning
            if (parameter is Models.ProductWithCategoryAndAllergens product)
            {
                StatusMessage = $"Produsul '{product.ProductName}' a fost adăugat în coș";
            }
            else if (parameter is MenuWithProducts menu)
            {
                StatusMessage = $"Meniul '{menu.MenuName}' a fost adăugat în coș";
            }
        }

        private void ExecuteViewCart()
        {
            StatusMessage = "Navigare către coșul de cumpărături";
        }

        private void ExecuteLogin()
        {
            StatusMessage = "Navigare către pagina de autentificare";
        }

        private void ExecuteRegister()
        {
            StatusMessage = "Navigare către pagina de înregistrare";
        }
    }
}
