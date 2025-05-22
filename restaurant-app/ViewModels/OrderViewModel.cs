using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using restaurant_app.Helpers;
using restaurant_app.Models;
using restaurant_app.Services;

namespace restaurant_app.ViewModels
{
    public class OrderViewModel : ViewModelBase
    {
        private readonly OrderService _orderService;
        private readonly AuthService _authService;
        private readonly ConfigService _configService;
        private readonly NavigationService _navigationService;

        private ObservableCollection<OrderItem> _cartItems = new();
        private string _deliveryAddress = string.Empty;
        private decimal _subTotal;
        private decimal _deliveryFee;
        private decimal _discount;
        private decimal _total;
        private string _statusMessage = string.Empty;
        private bool _isProcessing;

        public ObservableCollection<OrderItem> CartItems
        {
            get => _cartItems;
            set => SetProperty(ref _cartItems, value);
        }

        public string DeliveryAddress
        {
            get => _deliveryAddress;
            set => SetProperty(ref _deliveryAddress, value);
        }

        public decimal SubTotal
        {
            get => _subTotal;
            set => SetProperty(ref _subTotal, value);
        }

        public decimal DeliveryFee
        {
            get => _deliveryFee;
            set => SetProperty(ref _deliveryFee, value);
        }

        public decimal Discount
        {
            get => _discount;
            set => SetProperty(ref _discount, value);
        }

        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public ICommand CheckoutCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ClearCartCommand { get; }

        public ICommand NavigateBackCommand { get; }

        public OrderViewModel(OrderService orderService, AuthService authService,
                         ConfigService configService, NavigationService navigationService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _navigationService = navigationService; // Allow null NavigationService

            // Initialize delivery address from current user if available
            if (_authService.IsLoggedIn && _authService.CurrentUser != null)
            {
                DeliveryAddress = _authService.CurrentUser.DeliveryAddress ?? string.Empty;
            }

            // Initialize commands
            CheckoutCommand = new RelayCommand(async _ => await ExecuteCheckoutAsync(), _ => CanCheckout());
            RemoveItemCommand = new RelayCommand(ExecuteRemoveItem, _ => CartItems.Count > 0);
            ClearCartCommand = new RelayCommand(_ => ExecuteClearCart(), _ => CartItems.Count > 0);
            NavigateBackCommand = new RelayCommand(_ => CloseCurrentWindow());

            // Calculate totals when cart items change
            CalculateTotals();
        }

        private void CloseCurrentWindow()
        {
            // Find and close the parent window that contains this view
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window.Content is System.Windows.Controls.Page page &&
                    page.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        private bool CanCheckout()
        {
            return !IsProcessing
                && CartItems.Count > 0
                && _authService.IsLoggedIn
                && !string.IsNullOrWhiteSpace(DeliveryAddress);
        }

        private async Task ExecuteCheckoutAsync()
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "Procesare comandă...";

                // Create order
                int orderId = await _orderService.CreateOrderAsync(CartItems.ToList(), DeliveryAddress);

                // Clear cart after successful order
                CartItems.Clear();
                CalculateTotals();

                StatusMessage = $"Comandă creată cu succes! ID: {orderId}";

                // Navigate to order confirmation or orders list
                // _navigationService.NavigateTo<OrderConfirmationPage>(orderId);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la procesarea comenzii: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void ExecuteRemoveItem(object parameter)
        {
            if (parameter is OrderItem item)
            {
                CartItems.Remove(item);
                CalculateTotals();
                StatusMessage = $"Produsul '{item.Name}' a fost eliminat din coș.";
            }
        }

        private void ExecuteClearCart()
        {
            CartItems.Clear();
            CalculateTotals();
            StatusMessage = "Coșul a fost golit.";
        }

        public void AddToCart(int itemId, string name, bool isProduct, decimal unitPrice, int quantity = 1)
        {
            // Check if item already exists in cart
            var existingItem = CartItems.FirstOrDefault(i => i.ItemId == itemId && i.IsProduct == isProduct);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new OrderItem
                {
                    ItemId = itemId,
                    Name = name,
                    IsProduct = isProduct,
                    UnitPrice = unitPrice,
                    Quantity = quantity
                };

                CartItems.Add(newItem);
            }

            CalculateTotals();
            StatusMessage = $"Produsul '{name}' a fost adăugat în coș.";
        }

        private void CalculateTotals()
        {
            // Calculate subtotal from cart items
            SubTotal = CartItems.Sum(i => i.TotalPrice);

            // Apply delivery fee if below minimum order value
            decimal minOrderForFreeDelivery = _configService.GetMinOrderForFreeDelivery();
            DeliveryFee = (SubTotal < minOrderForFreeDelivery) ? _configService.GetDeliveryFee() : 0;

            // Calculate discount (this would typically come from the OrderService)
            // For simplicity, we'll calculate it here directly
            Discount = 0;
            if (_authService.IsLoggedIn && _authService.CurrentUser != null)
            {
                // Check if eligible for discount based on order history
                // This would normally involve a database check
                // We'll simulate it with a placeholder value
                bool isEligibleForDiscount = false; // This should come from a service

                if (isEligibleForDiscount)
                {
                    decimal discountPercentage = _configService.GetOrderDiscountPercentage();
                    Discount = SubTotal * (discountPercentage / 100);
                }
            }

            // Calculate final total
            Total = SubTotal + DeliveryFee - Discount;
        }
    }

    public class OrderListViewModel : ViewModelBase
    {
        private readonly OrderService _orderService;
        private readonly AuthService _authService;
        private readonly NavigationService _navigationService;

        private ObservableCollection<OrderWithDetails> _userOrders = new();
        private ObservableCollection<OrderWithDetails> _allOrders = new();
        private ObservableCollection<OrderWithDetails> _activeOrders = new();
        private string _statusMessage = string.Empty;
        private bool _isLoading;
        private string _selectedStatus = string.Empty;
        private OrderWithDetails? _selectedOrder;

        public ObservableCollection<OrderWithDetails> UserOrders
        {
            get => _userOrders;
            set => SetProperty(ref _userOrders, value);
        }

        public ObservableCollection<OrderWithDetails> AllOrders
        {
            get => _allOrders;
            set => SetProperty(ref _allOrders, value);
        }

        public ObservableCollection<OrderWithDetails> ActiveOrders
        {
            get => _activeOrders;
            set => SetProperty(ref _activeOrders, value);
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

        public string SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        public OrderWithDetails? SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        public bool IsUserLoggedIn => _authService.IsLoggedIn;
        public bool IsUserEmployee => _authService.IsEmployee;
        public bool IsUserClient => _authService.IsClient;

        public List<string> AvailableStatuses { get; } = new List<string>
        {
            "Registered", "Preparing", "OnDelivery", "Delivered", "Cancelled"
        };

        public ICommand UpdateOrderStatusCommand { get; }
        public ICommand CancelOrderCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; }

        public OrderListViewModel(OrderService orderService, AuthService authService, NavigationService navigationService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            // Initialize commands
            UpdateOrderStatusCommand = new RelayCommand(async _ => await ExecuteUpdateOrderStatusAsync(), _ => CanUpdateOrderStatus());
            CancelOrderCommand = new RelayCommand(async _ => await ExecuteCancelOrderAsync(), _ => CanCancelOrder());
            RefreshCommand = new RelayCommand(async _ => await LoadOrdersAsync());
            ViewOrderDetailsCommand = new RelayCommand(ExecuteViewOrderDetails, _ => SelectedOrder != null);
        }

        public async Task LoadOrdersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Încărcare comenzi...";

                if (!_authService.IsLoggedIn)
                {
                    StatusMessage = "Trebuie să fiți autentificat pentru a vedea comenzile.";
                    return;
                }

                // Load appropriate orders based on user type
                if (_authService.IsClient)
                {
                    var orders = await _orderService.GetUserOrdersAsync(_authService.CurrentUser!.UserId);
                    UserOrders = new ObservableCollection<OrderWithDetails>(
                        orders.Select(ConvertToOrderWithDetails)
                    );
                    StatusMessage = $"Comenzile dvs. au fost încărcate. Total: {UserOrders.Count}";
                }
                else if (_authService.IsEmployee)
                {
                    var allOrdersTask = _orderService.GetAllOrdersAsync();
                    var activeOrdersTask = _orderService.GetActiveOrdersAsync();

                    await Task.WhenAll(allOrdersTask, activeOrdersTask);

                    AllOrders = new ObservableCollection<OrderWithDetails>(
                        allOrdersTask.Result.Select(ConvertToOrderWithDetails)
                    );

                    ActiveOrders = new ObservableCollection<OrderWithDetails>(
                        activeOrdersTask.Result.Select(ConvertToOrderWithDetails)
                    );

                    StatusMessage = $"Toate comenzile au fost încărcate. Total: {AllOrders.Count}, Active: {ActiveOrders.Count}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la încărcarea comenzilor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private OrderWithDetails ConvertToOrderWithDetails(Order order)
        {
            return new OrderWithDetails
            {
                OrderId = order.OrderId,
                OrderCode = order.OrderCode,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                DeliveryFee = order.DeliveryFee ?? 0,
                Discount = order.Discount ?? 0,
                FinalAmount = order.FinalAmount,
                DeliveryAddress = order.DeliveryAddress,
                EstimatedDeliveryTime = order.EstimatedDeliveryTime,
                UserName = $"{order.User.FirstName} {order.User.LastName}",
                UserEmail = order.User.Email,
                UserPhone = order.User.PhoneNumber ?? "",
                Items = order.OrderDetails.Select(od => new OrderItemDetails
                {
                    Name = od.Product?.Name ?? od.Menu?.Name ?? "Unknown",
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    TotalPrice = od.TotalPrice
                }).ToList()
            };
        }

        private bool CanUpdateOrderStatus()
        {
            return SelectedOrder != null &&
                   !string.IsNullOrEmpty(SelectedStatus) &&
                   SelectedOrder.Status != SelectedStatus &&
                   _authService.IsEmployee;
        }

        private async Task ExecuteUpdateOrderStatusAsync()
        {
            if (SelectedOrder == null || string.IsNullOrEmpty(SelectedStatus))
                return;

            try
            {
                IsLoading = true;
                StatusMessage = "Actualizare status comandă...";

                bool success = await _orderService.UpdateOrderStatusAsync(SelectedOrder.OrderId, SelectedStatus);

                if (success)
                {
                    SelectedOrder.Status = SelectedStatus;
                    StatusMessage = $"Statusul comenzii #{SelectedOrder.OrderCode} a fost actualizat la {SelectedStatus}.";

                    // Reload orders to reflect changes
                    await LoadOrdersAsync();
                }
                else
                {
                    StatusMessage = "Eroare la actualizarea statusului comenzii.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la actualizarea statusului: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanCancelOrder()
        {
            // Only clients can cancel their own orders and only if they're not delivered yet
            return SelectedOrder != null &&
                   _authService.IsClient &&
                   SelectedOrder.Status != "Delivered" &&
                   SelectedOrder.Status != "Cancelled";
        }

        private async Task ExecuteCancelOrderAsync()
        {
            if (SelectedOrder == null)
                return;

            try
            {
                IsLoading = true;
                StatusMessage = "Anulare comandă...";

                bool success = await _orderService.UpdateOrderStatusAsync(SelectedOrder.OrderId, "Cancelled");

                if (success)
                {
                    SelectedOrder.Status = "Cancelled";
                    StatusMessage = $"Comanda #{SelectedOrder.OrderCode} a fost anulată.";

                    // Reload orders to reflect changes
                    await LoadOrdersAsync();
                }
                else
                {
                    StatusMessage = "Eroare la anularea comenzii.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la anularea comenzii: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteViewOrderDetails(object parameter)
        {
            if (parameter is OrderWithDetails order)
            {
                // Navigate to order details page
                // _navigationService.NavigateTo<OrderDetailsPage>(order);
            }
        }
    }

    // Models specific to the OrderViewModel
    public class OrderWithDetails
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalAmount { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public DateTime? EstimatedDeliveryTime { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public List<OrderItemDetails> Items { get; set; } = new List<OrderItemDetails>();
    }

    public class OrderItemDetails
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
