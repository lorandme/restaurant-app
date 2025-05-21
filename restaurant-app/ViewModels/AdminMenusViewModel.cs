using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using restaurant_app.Helpers;
using restaurant_app.Models;
using restaurant_app.Services;

namespace restaurant_app.ViewModels
{
    public class AdminMenusViewModel : ViewModelBase
    {
        private readonly MenuService _menuService;
        private readonly NavigationService _navigationService;
        private readonly AuthService _authService;

        private ObservableCollection<Menu> _menus = new();
        private ObservableCollection<Category> _categories = new();
        private ObservableCollection<Product> _availableProducts = new();
        private ObservableCollection<MenuProduct> _selectedMenuProducts = new();

        private Menu _editingMenu = new();
        private Menu _selectedMenu;
        private Category _selectedCategory;
        private Product _selectedProductToAdd;

        private string _statusMessage = string.Empty;
        private bool _isLoading;

        public ObservableCollection<Menu> Menus
        {
            get => _menus;
            set => SetProperty(ref _menus, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<Product> AvailableProducts
        {
            get => _availableProducts;
            set => SetProperty(ref _availableProducts, value);
        }

        public ObservableCollection<MenuProduct> SelectedMenuProducts
        {
            get => _selectedMenuProducts;
            set => SetProperty(ref _selectedMenuProducts, value);
        }

        public Menu EditingMenu
        {
            get => _editingMenu;
            set => SetProperty(ref _editingMenu, value);
        }

        public Menu SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                if (SetProperty(ref _selectedMenu, value) && value != null)
                {
                    LoadMenuDetails(value);
                }
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public Product SelectedProductToAdd
        {
            get => _selectedProductToAdd;
            set => SetProperty(ref _selectedProductToAdd, value);
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

        public ICommand SaveMenuCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand EditMenuCommand { get; }
        public ICommand DeleteMenuCommand { get; }
        public ICommand AddProductToMenuCommand { get; }
        public ICommand RemoveProductFromMenuCommand { get; }
        public ICommand NavigateBackCommand { get; }

        public AdminMenusViewModel(MenuService menuService, NavigationService navigationService, AuthService authService)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            // Check authorization
            if (!_authService.IsLoggedIn || !_authService.IsEmployee)
            {
                _navigationService.NavigateTo("Login");
                return;
            }

            // Initialize commands
            SaveMenuCommand = new RelayCommand(async _ => await ExecuteSaveMenuAsync(), _ => CanSaveMenu());
            CancelEditCommand = new RelayCommand(_ => ExecuteCancelEdit());
            EditMenuCommand = new RelayCommand(ExecuteEditMenu);
            DeleteMenuCommand = new RelayCommand(async param => await ExecuteDeleteMenuAsync(param as Menu));
            AddProductToMenuCommand = new RelayCommand(_ => ExecuteAddProductToMenu(), _ => CanAddProductToMenu());
            RemoveProductFromMenuCommand = new RelayCommand(ExecuteRemoveProductFromMenu);
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

                var menusTask = _menuService.GetAllMenusAsync();
                var categoriesTask = _menuService.GetAllCategoriesAsync();
                var productsTask = _menuService.GetAllProductsAsync();

                await Task.WhenAll(menusTask, categoriesTask, productsTask);

                Menus = new ObservableCollection<Menu>(menusTask.Result.Select(m => new Menu
                {
                    // Fix property names to match what actually exists in MenuWithProducts
                    MenuId = m.MenuID,         // Note: MenuID not MenuId
                    Name = m.MenuName,         // Use MenuName instead of Name
                    Description = m.Description,
                    CategoryId = m.CategoryID, // Note: CategoryID not CategoryId
                    IsAvailable = true         // If IsAvailable doesn't exist, provide a default value
                }));

                Categories = new ObservableCollection<Category>(categoriesTask.Result);

                AvailableProducts = new ObservableCollection<Product>(productsTask.Result.Select(p => new Product
                {
                    ProductId = p.ProductID,
                    Name = p.ProductName,
                    Price = p.Price
                }));

                StatusMessage = "Date încărcate cu succes.";
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

        private void LoadMenuDetails(Menu menu)
        {
            // Create a copy of the menu for editing
            EditingMenu = new Menu
            {
                MenuId = menu.MenuId,
                Name = menu.Name,
                Description = menu.Description,
                CategoryId = menu.CategoryId,
                IsAvailable = menu.IsAvailable
            };

            // Set selected category
            SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == menu.CategoryId);

            // Load menu products
            SelectedMenuProducts = new ObservableCollection<MenuProduct>(menu.MenuProducts);
        }

        private bool CanSaveMenu()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(EditingMenu.Name) &&
                   SelectedCategory != null;
        }

        private async Task ExecuteSaveMenuAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Se salvează meniul...";

                // Set properties from selected items
                EditingMenu.CategoryId = SelectedCategory.CategoryId;

                // Save menu
                // This would call a method like _menuService.SaveMenuAsync(EditingMenu, SelectedMenuProducts.ToList());

                // For now, we'll just update the local collections
                var existingMenu = Menus.FirstOrDefault(m => m.MenuId == EditingMenu.MenuId);

                if (existingMenu != null)
                {
                    // Update existing menu
                    existingMenu.Name = EditingMenu.Name;
                    existingMenu.Description = EditingMenu.Description;
                    existingMenu.CategoryId = EditingMenu.CategoryId;
                    existingMenu.IsAvailable = EditingMenu.IsAvailable;
                    existingMenu.MenuProducts = SelectedMenuProducts.ToList();
                    StatusMessage = "Meniul a fost actualizat cu succes.";
                }
                else
                {
                    // Add new menu
                    Menus.Add(EditingMenu);
                    StatusMessage = "Meniul a fost adăugat cu succes.";
                }

                // Reset edit state
                ExecuteCancelEdit();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la salvarea meniului: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteCancelEdit()
        {
            // Reset editing menu
            EditingMenu = new Menu();
            SelectedCategory = null;
            SelectedMenuProducts.Clear();
        }

        private void ExecuteEditMenu(object parameter)
        {
            if (parameter is Menu menu)
            {
                SelectedMenu = menu;
            }
        }

        private async Task ExecuteDeleteMenuAsync(Menu menu)
        {
            if (menu == null)
                return;

            try
            {
                IsLoading = true;
                StatusMessage = "Se șterge meniul...";

                // Delete menu
                // This would call a method like await _menuService.DeleteMenuAsync(menu.MenuId);

                // For now, we'll just remove it from the local collection
                Menus.Remove(menu);

                StatusMessage = "Meniul a fost șters cu succes.";

                // Reset if the deleted menu was being edited
                if (EditingMenu.MenuId == menu.MenuId)
                {
                    ExecuteCancelEdit();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la ștergerea meniului: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanAddProductToMenu()
        {
            return SelectedProductToAdd != null;
        }

        private void ExecuteAddProductToMenu()
        {
            if (SelectedProductToAdd == null)
                return;

            // Create a new menu product and add it to the list
            var menuProduct = new MenuProduct
            {
                MenuId = EditingMenu.MenuId,
                ProductId = SelectedProductToAdd.ProductId,
                Product = SelectedProductToAdd,
                ProductQuantity = 1, // Default quantity
                ProductUnit = "g"    // Default unit
            };

            SelectedMenuProducts.Add(menuProduct);
            StatusMessage = $"Produs adăugat la meniu: {SelectedProductToAdd.Name}";
        }

        private void ExecuteRemoveProductFromMenu(object parameter)
        {
            if (parameter is MenuProduct menuProduct)
            {
                SelectedMenuProducts.Remove(menuProduct);
                StatusMessage = $"Produs eliminat din meniu.";
            }
        }
    }
}