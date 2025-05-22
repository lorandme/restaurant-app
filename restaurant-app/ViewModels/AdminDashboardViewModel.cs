using restaurant_app.Helpers;
using restaurant_app.Models;
using restaurant_app.Services;
using restaurant_app.Models.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace restaurant_app.ViewModels
{
    public class AdminDashboardViewModel : ViewModelBase
    {
        private readonly MenuService _menuService;
        private readonly OrderService _orderService;
        private readonly ConfigService _configService;

        // Field for CurrentView - only define this once
        private string _currentView = "Dashboard";

        // Observable Collections for data display
        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        private ObservableCollection<restaurant_app.Models.ProductWithCategoryAndAllergens> _products = new ObservableCollection<restaurant_app.Models.ProductWithCategoryAndAllergens>();
        private ObservableCollection<MenuWithProducts> _menus = new ObservableCollection<MenuWithProducts>();
        private ObservableCollection<Allergen> _allergens = new ObservableCollection<Allergen>();
        private ObservableCollection<Order> _allOrders = new ObservableCollection<Order>();
        private ObservableCollection<Order> _activeOrders = new ObservableCollection<Order>();
        private ObservableCollection<ProductLowStock> _lowStockProducts = new ObservableCollection<ProductLowStock>();

        // Selected items
        private Category _selectedCategory;
        private restaurant_app.Models.ProductWithCategoryAndAllergens _selectedProduct;
        private MenuWithProducts _selectedMenu;
        private Allergen _selectedAllergen;
        private Order _selectedOrder;
        private ProductLowStock _selectedLowStockProduct;

        // UI indicators
        private bool _isLoading;
        private string _statusMessage;

        #region Properties

        // Property for CurrentView - define with the enhanced notifications
        public string CurrentView
        {
            get => _currentView;
            set
            {
                if (SetProperty(ref _currentView, value))
                {
                    // Notify visibility properties changes
                    OnPropertyChanged(nameof(IsDashboardVisible));
                    OnPropertyChanged(nameof(IsCategoriesVisible));
                    OnPropertyChanged(nameof(IsProductsVisible));
                    OnPropertyChanged(nameof(IsMenusVisible));
                    OnPropertyChanged(nameof(IsAllergensVisible));
                    OnPropertyChanged(nameof(IsAllOrdersVisible));
                    OnPropertyChanged(nameof(IsActiveOrdersVisible));
                    OnPropertyChanged(nameof(IsLowStockVisible));
                    OnPropertyChanged(nameof(CanAddEditDelete)); // Add this line
                }
            }
        }


        // Visibility helper properties
        public bool IsDashboardVisible => CurrentView == "Dashboard";
        public bool IsCategoriesVisible => CurrentView == "Categories";
        public bool IsProductsVisible => CurrentView == "Products";
        public bool IsMenusVisible => CurrentView == "Menus";
        public bool IsAllergensVisible => CurrentView == "Allergens";
        public bool IsAllOrdersVisible => CurrentView == "AllOrders";
        public bool IsActiveOrdersVisible => CurrentView == "ActiveOrders";
        public bool IsLowStockVisible => CurrentView == "LowStock";

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<restaurant_app.Models.ProductWithCategoryAndAllergens> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<MenuWithProducts> Menus
        {
            get => _menus;
            set => SetProperty(ref _menus, value);
        }

        public ObservableCollection<Allergen> Allergens
        {
            get => _allergens;
            set => SetProperty(ref _allergens, value);
        }

        public ObservableCollection<Order> AllOrders
        {
            get => _allOrders;
            set => SetProperty(ref _allOrders, value);
        }

        public ObservableCollection<Order> ActiveOrders
        {
            get => _activeOrders;
            set => SetProperty(ref _activeOrders, value);
        }

        public ObservableCollection<ProductLowStock> LowStockProducts
        {
            get => _lowStockProducts;
            set => SetProperty(ref _lowStockProducts, value);
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public restaurant_app.Models.ProductWithCategoryAndAllergens SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public MenuWithProducts SelectedMenu
        {
            get => _selectedMenu;
            set => SetProperty(ref _selectedMenu, value);
        }

        public Allergen SelectedAllergen
        {
            get => _selectedAllergen;
            set => SetProperty(ref _selectedAllergen, value);
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        public ProductLowStock SelectedLowStockProduct
        {
            get => _selectedLowStockProduct;
            set => SetProperty(ref _selectedLowStockProduct, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool CanAddEditDelete => CurrentView != "AllOrders" &&
                                CurrentView != "ActiveOrders" &&
                                CurrentView != "LowStock" &&
                                CurrentView != "Dashboard";

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        #endregion

        #region Commands

        // Navigation Commands
        public ICommand ShowCategoriesCommand { get; }
        public ICommand ShowProductsCommand { get; }
        public ICommand ShowMenusCommand { get; }
        public ICommand ShowAllergensCommand { get; }
        public ICommand ShowAllOrdersCommand { get; }
        public ICommand ShowActiveOrdersCommand { get; }
        public ICommand ShowLowStockCommand { get; }

        // CRUD Commands
        public ICommand AddItemCommand { get; }
        public ICommand EditItemCommand { get; }
        public ICommand DeleteItemCommand { get; }

        // Order Management Command
        public ICommand UpdateOrderStatusCommand { get; }

        // Utility Command
        public ICommand RefreshDataCommand { get; }

        #endregion

        public AdminDashboardViewModel(MenuService menuService, OrderService orderService, ConfigService configService)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));

            // Initialize navigation commands
            ShowCategoriesCommand = new RelayCommand(_ => { CurrentView = "Categories"; LoadCategories(); });
            ShowProductsCommand = new RelayCommand(_ => { CurrentView = "Products"; LoadProducts(); });
            ShowMenusCommand = new RelayCommand(_ => { CurrentView = "Menus"; LoadMenus(); });
            ShowAllergensCommand = new RelayCommand(_ => { CurrentView = "Allergens"; LoadAllergens(); });
            ShowAllOrdersCommand = new RelayCommand(_ => { CurrentView = "AllOrders"; LoadAllOrders(); });
            ShowActiveOrdersCommand = new RelayCommand(_ => { CurrentView = "ActiveOrders"; LoadActiveOrders(); });
            ShowLowStockCommand = new RelayCommand(_ => { CurrentView = "LowStock"; LoadLowStockProducts(); });

            // Initialize action commands that will behave differently based on current view
            AddItemCommand = new RelayCommand(_ => AddItem());
            EditItemCommand = new RelayCommand(_ => EditItem(), _ => CanEditItem());
            DeleteItemCommand = new RelayCommand(_ => DeleteItem(), _ => CanDeleteItem());

            // Order management command
            UpdateOrderStatusCommand = new RelayCommand(param => UpdateOrderStatus(param as string), _ => SelectedOrder != null);

            // Refresh command
            RefreshDataCommand = new RelayCommand(_ => RefreshCurrentView());

            // Load initial data
            LoadDashboardData();
        }


        private void LoadDashboardData()
        {
            IsLoading = true;
            StatusMessage = "Se încarcă datele...";

            Task.Run(async () =>
            {
                try
                {
                    await LoadActiveOrders();
                    await LoadLowStockProducts();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = "Datele au fost încărcate cu succes";
                        IsLoading = false;
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = $"Eroare: {ex.Message}";
                        IsLoading = false;
                    });
                }
            });
        }

        #region Data Loading Methods

        private async Task LoadCategories()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Se încarcă categoriile...";

                var categories = await _menuService.GetAllCategoriesAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Categories.Clear();
                    foreach (var category in categories)
                    {
                        Categories.Add(category);
                    }
                    StatusMessage = "Categorii încărcate";
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = $"Eroare: {ex.Message}";
                    IsLoading = false;
                });
            }
        }

        private async Task LoadProducts()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Se încarcă produsele...";

                var products = await _menuService.GetAllProductsAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Products.Clear();
                    foreach (var product in products)
                    {
                        Products.Add(product);
                    }
                    StatusMessage = "Produse încărcate";
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = $"Eroare: {ex.Message}";
                    IsLoading = false;
                });
            }
        }

        private async Task LoadMenus()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Se încarcă meniurile...";

                var menus = await _menuService.GetAllMenusAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Menus.Clear();
                    foreach (var menu in menus)
                    {
                        Menus.Add(menu);
                    }
                    StatusMessage = "Meniuri încărcate";
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = $"Eroare: {ex.Message}";
                    IsLoading = false;
                });
            }
        }

        private async Task LoadAllergens()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Se încarcă alergenii...";

                var allergens = await _menuService.GetAllAllergensAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Allergens.Clear();
                    foreach (var allergen in allergens)
                    {
                        Allergens.Add(allergen);
                    }
                    StatusMessage = "Alergeni încărcați";
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = $"Eroare: {ex.Message}";
                    IsLoading = false;
                });
            }
        }

        private async Task LoadAllOrders()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Se încarcă toate comenzile...";

                var orders = await _orderService.GetAllOrdersAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AllOrders.Clear();
                    foreach (var order in orders.OrderByDescending(o => o.OrderDate))
                    {
                        AllOrders.Add(order);
                    }
                    StatusMessage = "Comenzi încărcate";
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = $"Eroare: {ex.Message}";
                    IsLoading = false;
                });
            }
        }

        private async Task LoadActiveOrders()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Se încarcă comenzile active...";

                var orders = await _orderService.GetActiveOrdersAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ActiveOrders.Clear();
                    foreach (var order in orders.OrderByDescending(o => o.OrderDate))
                    {
                        ActiveOrders.Add(order);
                    }
                    StatusMessage = "Comenzi active încărcate";
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = $"Eroare: {ex.Message}";
                    IsLoading = false;
                });
            }
        }

        private async Task LoadLowStockProducts()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Se verifică stocul produselor...";

                var lowStockProducts = await _menuService.GetLowStockProductsAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LowStockProducts.Clear();
                    foreach (var product in lowStockProducts)
                    {
                        LowStockProducts.Add(product);
                    }
                    StatusMessage = "Produse cu stoc redus încărcate";
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = $"Eroare: {ex.Message}";
                    IsLoading = false;
                });
            }
        }

        private void RefreshCurrentView()
        {
            switch (CurrentView)
            {
                case "Categories":
                    LoadCategories();
                    break;
                case "Products":
                    LoadProducts();
                    break;
                case "Menus":
                    LoadMenus();
                    break;
                case "Allergens":
                    LoadAllergens();
                    break;
                case "AllOrders":
                    LoadAllOrders();
                    break;
                case "ActiveOrders":
                    LoadActiveOrders();
                    break;
                case "LowStock":
                    LoadLowStockProducts();
                    break;
                default:
                    LoadDashboardData();
                    break;
            }
        }

        #endregion

        #region Generic CRUD Operations

        private void AddItem()
        {
            switch (CurrentView)
            {
                case "Categories":
                    AddCategory();
                    break;
                case "Products":
                    AddProduct();
                    break;
                case "Menus":
                    AddMenu();
                    break;
                case "Allergens":
                    AddAllergen();
                    break;
            }
        }

        private bool CanEditItem()
        {
            return CurrentView switch
            {
                "Categories" => SelectedCategory != null,
                "Products" => SelectedProduct != null,
                "Menus" => SelectedMenu != null,
                "Allergens" => SelectedAllergen != null,
                _ => false
            };
        }

        private void EditItem()
        {
            switch (CurrentView)
            {
                case "Categories":
                    EditCategory();
                    break;
                case "Products":
                    EditProduct();
                    break;
                case "Menus":
                    EditMenu();
                    break;
                case "Allergens":
                    EditAllergen();
                    break;
            }
        }

        private bool CanDeleteItem()
        {
            return CurrentView switch
            {
                "Categories" => SelectedCategory != null,
                "Products" => SelectedProduct != null,
                "Menus" => SelectedMenu != null,
                "Allergens" => SelectedAllergen != null,
                _ => false
            };
        }

        private void DeleteItem()
        {
            switch (CurrentView)
            {
                case "Categories":
                    DeleteCategory();
                    break;
                case "Products":
                    DeleteProduct();
                    break;
                case "Menus":
                    DeleteMenu();
                    break;
                case "Allergens":
                    DeleteAllergen();
                    break;
            }
        }

        #endregion

        #region Entity-specific CRUD Methods

        // These methods would handle opening appropriate dialogs or performing the operations
        // I'm providing stubs that you can implement based on your existing code

        private async void AddCategory()
        {
            var name = PromptForInput("Introduceți numele categoriei:");
            if (string.IsNullOrWhiteSpace(name)) return;

            var category = new Category { Name = name };
            try
            {
                IsLoading = true;
                // Adaugă categoria în baza de date (presupunem că ai o metodă AddCategoryAsync)
                await _menuService.AddCategoryAsync(category);
                await LoadCategories();
                StatusMessage = "Categorie adăugată cu succes!";
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

        private async void EditCategory()
        {
            if (SelectedCategory == null) return;
            var name = PromptForInput("Editați numele categoriei:", SelectedCategory.Name);
            if (string.IsNullOrWhiteSpace(name)) return;

            SelectedCategory.Name = name;
            try
            {
                IsLoading = true;
                await _menuService.UpdateCategoryAsync(SelectedCategory);
                await LoadCategories();
                StatusMessage = "Categorie editată cu succes!";
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

        private async void DeleteCategory()
        {
            if (SelectedCategory == null) return;
            if (MessageBox.Show("Sigur ștergi categoria?", "Confirmare", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                await _menuService.DeleteCategoryAsync(SelectedCategory.CategoryId);
                await LoadCategories();
                StatusMessage = "Categorie ștearsă!";
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


        private async void AddProduct()
        {
            // Exemplu simplificat, poți folosi un dialog custom pentru toate detaliile
            var name = PromptForInput("Introduceți numele produsului:");
            if (string.IsNullOrWhiteSpace(name)) return;

            var product = new Product { Name = name, CategoryId = SelectedCategory?.CategoryId ?? 0 };
            try
            {
                IsLoading = true;
                await _menuService.AddProductAsync(product, new List<int>(), new List<string>());
                await LoadProducts();
                StatusMessage = "Produs adăugat!";
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

        private async void EditProduct()
        {
            if (SelectedProduct == null) return;
            var name = PromptForInput("Editați numele produsului:", SelectedProduct.ProductName);
            if (string.IsNullOrWhiteSpace(name)) return;

            // Creezi un obiect Product cu datele noi
            var product = new Product
            {
                ProductId = SelectedProduct.ProductID,
                Name = name,
                CategoryId = SelectedProduct.CategoryID
                // Completează și alte proprietăți după caz
            };

            try
            {
                IsLoading = true;
                await _menuService.UpdateProductAsync(product, new List<int>(), new List<string>());
                await LoadProducts();
                StatusMessage = "Produs editat!";
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

        private async void DeleteProduct()
        {
            if (SelectedProduct == null) return;
            if (MessageBox.Show("Sigur ștergi produsul?", "Confirmare", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                await _menuService.DeleteProductAsync(SelectedProduct.ProductID);
                await LoadProducts();
                StatusMessage = "Produs șters!";
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


        private async void AddMenu()
        {
            var name = PromptForInput("Introduceți numele meniului:");
            if (string.IsNullOrWhiteSpace(name)) return;

            var menu = new Menu { /* Completează cu alte proprietăți dacă este nevoie */ };
            // Exemplu: menu.Name = name; dacă ai proprietatea Name

            try
            {
                IsLoading = true;
                await _menuService.AddMenuAsync(menu);
                await LoadMenus();
                StatusMessage = "Meniu adăugat cu succes!";
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

        private async void EditMenu()
        {
            if (SelectedMenu == null) return;
            var name = PromptForInput("Editați numele meniului:", SelectedMenu.MenuName);
            if (string.IsNullOrWhiteSpace(name)) return;

            // Creezi un obiect Menu cu datele noi
            var menu = new Menu
            {
                MenuId = SelectedMenu.MenuID,
                // Name = name, // dacă ai proprietatea Name
                // Completează și alte proprietăți după caz
            };

            try
            {
                IsLoading = true;
                await _menuService.UpdateMenuAsync(menu);
                await LoadMenus();
                StatusMessage = "Meniu editat cu succes!";
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

        private async void DeleteMenu()
        {
            if (SelectedMenu == null) return;
            if (MessageBox.Show("Sigur ștergi meniul?", "Confirmare", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                await _menuService.DeleteMenuAsync(SelectedMenu.MenuID);
                await LoadMenus();
                StatusMessage = "Meniu șters!";
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


        private async void AddAllergen()
        {
            var name = PromptForInput("Introduceți numele alergenului:");
            if (string.IsNullOrWhiteSpace(name)) return;

            var allergen = new Allergen { Name = name };
            try
            {
                IsLoading = true;
                await _menuService.AddAllergenAsync(allergen);
                await LoadAllergens();
                StatusMessage = "Alergen adăugat cu succes!";
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

        private async void EditAllergen()
        {
            if (SelectedAllergen == null) return;
            var name = PromptForInput("Editați numele alergenului:", SelectedAllergen.Name);
            if (string.IsNullOrWhiteSpace(name)) return;

            SelectedAllergen.Name = name;
            try
            {
                IsLoading = true;
                await _menuService.UpdateAllergenAsync(SelectedAllergen);
                await LoadAllergens();
                StatusMessage = "Alergen editat cu succes!";
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

        private async void DeleteAllergen()
        {
            if (SelectedAllergen == null) return;
            if (MessageBox.Show("Sigur ștergi alergenul?", "Confirmare", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                await _menuService.DeleteAllergenAsync(SelectedAllergen.AllergenId);
                await LoadAllergens();
                StatusMessage = "Alergen șters!";
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


        private string PromptForInput(string message, string defaultValue = "")
        {
            return Microsoft.VisualBasic.Interaction.InputBox(message, "Input", defaultValue);
        }


        #endregion

        #region Order Management

        private async void UpdateOrderStatus(string newStatus)
        {
            if (SelectedOrder == null || string.IsNullOrEmpty(newStatus))
                return;

            try
            {
                IsLoading = true;
                StatusMessage = "Se actualizează statusul comenzii...";

                await _orderService.UpdateOrderStatusAsync(SelectedOrder.OrderId, newStatus);

                // Refresh the order lists
                if (CurrentView == "AllOrders")
                {
                    await LoadAllOrders();
                }
                else if (CurrentView == "ActiveOrders")
                {
                    await LoadActiveOrders();
                }

                StatusMessage = "Statusul comenzii a fost actualizat cu succes";
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

        #endregion
    }
}

