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
    public class AdminProductsViewModel : ViewModelBase
    {
        private readonly MenuService _menuService;
        private readonly NavigationService _navigationService;
        private readonly AuthService _authService;

        private ObservableCollection<Product> _products = new();
        private ObservableCollection<Category> _categories = new();
        private ObservableCollection<AllergenViewModel> _allergens = new();
        private ObservableCollection<ProductImageViewModel> _productImages = new();

        private Product _editingProduct = new();
        private Product _selectedProduct;
        private Category _selectedCategory;
        private string _searchText = string.Empty;

        private string _statusMessage = string.Empty;
        private bool _isLoading;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<AllergenViewModel> Allergens
        {
            get => _allergens;
            set => SetProperty(ref _allergens, value);
        }

        public ObservableCollection<ProductImageViewModel> ProductImages
        {
            get => _productImages;
            set => SetProperty(ref _productImages, value);
        }

        public Product EditingProduct
        {
            get => _editingProduct;
            set => SetProperty(ref _editingProduct, value);
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value) && value != null)
                {
                    LoadProductDetails(value);
                }
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public int ImageCount => ProductImages.Count;

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

        public ICommand SaveProductCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ResetSearchCommand { get; }
        public ICommand AddImageCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand NavigateBackCommand { get; }

        public AdminProductsViewModel(MenuService menuService, NavigationService navigationService, AuthService authService)
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
            SaveProductCommand = new RelayCommand(async _ => await ExecuteSaveProductAsync(), _ => CanSaveProduct());
            CancelEditCommand = new RelayCommand(_ => ExecuteCancelEdit());
            EditProductCommand = new RelayCommand(ExecuteEditProduct);
            DeleteProductCommand = new RelayCommand(async param => await ExecuteDeleteProductAsync(param as Product));
            SearchCommand = new RelayCommand(async _ => await ExecuteSearchAsync(), _ => !string.IsNullOrWhiteSpace(SearchText));
            ResetSearchCommand = new RelayCommand(async _ => await LoadDataAsync());
            AddImageCommand = new RelayCommand(_ => ExecuteAddImage());
            RemoveImageCommand = new RelayCommand(ExecuteRemoveImage);
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

                var productsTask = _menuService.GetAllProductsAsync();
                var categoriesTask = _menuService.GetAllCategoriesAsync();
                var allergensTask = _menuService.GetAllAllergensAsync();

                await Task.WhenAll(productsTask, categoriesTask, allergensTask);

                Products = new ObservableCollection<Product>(productsTask.Result.Select(p => new Product
                {
                    ProductId = p.ProductID,
                    Name = p.ProductName,
                    Price = p.Price,
                    PortionQuantity = p.PortionQuantity,
                    PortionUnit = p.PortionUnit,
                    TotalQuantity = p.TotalQuantity,
                    CategoryId = p.CategoryID,
                    IsAvailable = true // Assume available if in the result set
                }));

                Categories = new ObservableCollection<Category>(categoriesTask.Result);

                Allergens = new ObservableCollection<AllergenViewModel>(allergensTask.Result.Select(a => new AllergenViewModel
                {
                    AllergenId = a.AllergenId,
                    Name = a.Name,
                    Description = a.Description,
                    IsSelected = false
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

        private void LoadProductDetails(Product product)
        {
            // Create a copy of the product for editing
            EditingProduct = new Product
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                PortionQuantity = product.PortionQuantity,
                PortionUnit = product.PortionUnit,
                TotalQuantity = product.TotalQuantity,
                CategoryId = product.CategoryId,
                IsAvailable = product.IsAvailable
            };

            // Set selected category
            SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == product.CategoryId);

            // Reset allergen selection
            foreach (var allergen in Allergens)
            {
                allergen.IsSelected = product.Allergens.Any(a => a.AllergenId == allergen.AllergenId);
            }

            // Load product images
            ProductImages = new ObservableCollection<ProductImageViewModel>(product.ProductImages.Select(pi => new ProductImageViewModel
            {
                ImageId = pi.ImageId,
                ProductId = pi.ProductId,
                ImagePath = pi.ImagePath
            }));

            OnPropertyChanged(nameof(ImageCount));
        }

        private bool CanSaveProduct()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(EditingProduct.Name) &&
                   SelectedCategory != null &&
                   EditingProduct.Price > 0 &&
                   EditingProduct.PortionQuantity > 0 &&
                   !string.IsNullOrWhiteSpace(EditingProduct.PortionUnit);
        }

        private async Task ExecuteSaveProductAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Se salvează produsul...";

                // Set properties from selected items
                EditingProduct.CategoryId = SelectedCategory.CategoryId;

                // Get selected allergens
                var selectedAllergenIds = Allergens
                    .Where(a => a.IsSelected)
                    .Select(a => a.AllergenId)
                    .ToList();

                // Get image paths
                var imagePaths = ProductImages
                    .Select(pi => pi.ImagePath)
                    .ToList();

                // Save product
                bool result;
                if (EditingProduct.ProductId > 0)
                {
                    // Update existing product
                    result = await _menuService.UpdateProductAsync(
                        EditingProduct,
                        selectedAllergenIds,
                        imagePaths);

                    StatusMessage = result
                        ? "Produsul a fost actualizat cu succes."
                        : "Eroare la actualizarea produsului.";
                }
                else
                {
                    // Add new product
                    result = await _menuService.AddProductAsync(
                        EditingProduct,
                        selectedAllergenIds,
                        imagePaths);

                    StatusMessage = result
                        ? "Produsul a fost adăugat cu succes."
                        : "Eroare la adăugarea produsului.";
                }

                if (result)
                {
                    // Reload data to reflect changes
                    await LoadDataAsync();

                    // Reset edit state
                    ExecuteCancelEdit();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la salvarea produsului: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteCancelEdit()
        {
            // Reset editing product
            EditingProduct = new Product();
            SelectedCategory = null;

            // Reset allergen selection
            foreach (var allergen in Allergens)
            {
                allergen.IsSelected = false;
            }

            // Clear product images
            ProductImages.Clear();
            OnPropertyChanged(nameof(ImageCount));
        }

        private void ExecuteEditProduct(object parameter)
        {
            if (parameter is Product product)
            {
                SelectedProduct = product;
            }
        }

        private async Task ExecuteDeleteProductAsync(Product product)
        {
            if (product == null)
                return;

            try
            {
                IsLoading = true;
                StatusMessage = "Se șterge produsul...";

                // Delete product
                bool result = await _menuService.DeleteProductAsync(product.ProductId);

                if (result)
                {
                    // Remove from local collection
                    Products.Remove(product);
                    StatusMessage = "Produsul a fost șters cu succes.";

                    // Reset if the deleted product was being edited
                    if (EditingProduct.ProductId == product.ProductId)
                    {
                        ExecuteCancelEdit();
                    }
                }
                else
                {
                    StatusMessage = "Eroare la ștergerea produsului.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la ștergerea produsului: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteSearchAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Se caută...";

                // Search products
                var results = await _menuService.SearchProductsByKeywordAsync(SearchText);

                Products = new ObservableCollection<Product>(results.Select(p => new Product
                {
                    ProductId = p.ProductID,
                    Name = p.ProductName,
                    Price = p.Price,
                    PortionQuantity = p.PortionQuantity,
                    PortionUnit = p.PortionUnit,
                    TotalQuantity = p.TotalQuantity,
                    CategoryId = p.CategoryID,
                    IsAvailable = true // Assume available if in the result set
                }));

                StatusMessage = $"Găsite {Products.Count} rezultate pentru '{SearchText}'.";
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

        private void ExecuteAddImage()
        {
            // In a real app, you would open a file dialog and let the user select an image
            // For this example, we'll just add a placeholder image path
            var newImage = new ProductImageViewModel
            {
                ProductId = EditingProduct.ProductId,
                ImagePath = $"/Images/product_{DateTime.Now.Ticks}.jpg" // Placeholder path
            };

            ProductImages.Add(newImage);
            OnPropertyChanged(nameof(ImageCount));
            StatusMessage = "Imagine adăugată. (Notă: aceasta este doar o simulare)";
        }

        private void ExecuteRemoveImage(object parameter)
        {
            if (parameter is ProductImageViewModel image)
            {
                ProductImages.Remove(image);
                OnPropertyChanged(nameof(ImageCount));
                StatusMessage = "Imagine eliminată.";
            }
        }
    }

    // Helper view models
    public class AllergenViewModel : ViewModelBase
    {
        private int _allergenId;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private bool _isSelected;

        public int AllergenId
        {
            get => _allergenId;
            set => SetProperty(ref _allergenId, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class ProductImageViewModel : ViewModelBase
    {
        private int _imageId;
        private int _productId;
        private string _imagePath = string.Empty;

        public int ImageId
        {
            get => _imageId;
            set => SetProperty(ref _imageId, value);
        }

        public int ProductId
        {
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }
    }
}