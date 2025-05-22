using Microsoft.EntityFrameworkCore;
using restaurant_app.Models;
using restaurant_app.Models.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace restaurant_app.Services
{
    public class OrderService
    {
        private readonly RestaurantDALContext _context;
        private readonly ConfigService _configService;
        private readonly AuthService _authService;

        public OrderService(RestaurantDALContext context, ConfigService configService, AuthService authService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }


        // Add this to OrderService.cs
        private List<OrderItem> _tempCart = new List<OrderItem>();

        public List<OrderItem> TempCart => _tempCart;

        public void InitializeTempCart()
        {
            _tempCart = new List<OrderItem>();
        }

        public void AddToTempCart(int itemId, string name, bool isProduct, decimal unitPrice, int quantity = 1)
        {
            // Check if item already exists in cart
            var existingItem = _tempCart.FirstOrDefault(i => i.ItemId == itemId && i.IsProduct == isProduct);

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

                _tempCart.Add(newItem);
            }
        }


        public async Task<int> CreateOrderAsync(
    List<OrderItem> items,
    string deliveryAddress)
        {
            if (!_authService.IsLoggedIn || _authService.CurrentUser == null)
                throw new InvalidOperationException("Utilizatorul trebuie să fie autentificat pentru a plasa o comandă.");

            // Generăm un cod unic pentru comandă (format: ORD-YYMMDD-XXXX)
            string orderCode = GenerateOrderCode();

            // Calculăm sumele pentru comandă
            decimal subtotal = items.Sum(i => i.TotalPrice);
            // Use the async version of the method with await
            var (deliveryFee, discount, finalAmount) = await CalculateOrderAmountsAsync(subtotal);

            // Adăugăm 30 de minute pentru timpul estimat de livrare
            DateTime estimatedDeliveryTime = DateTime.Now.AddMinutes(30);

            // Creăm comanda
            int orderId = await _context.CreateOrderAsync(
                _authService.CurrentUser.UserId,
                orderCode,
                deliveryAddress,
                subtotal,
                deliveryFee,
                discount,
                finalAmount,
                estimatedDeliveryTime);

            // Adăugăm detaliile comenzii
            foreach (var item in items)
            {
                if (item.IsProduct)
                {
                    await _context.AddProductToOrderAsync(
                        orderId,
                        item.ItemId,
                        item.Quantity,
                        item.UnitPrice,
                        item.TotalPrice);
                }
                else
                {
                    await _context.AddMenuToOrderAsync(
                        orderId,
                        item.ItemId,
                        item.Quantity,
                        item.UnitPrice,
                        item.TotalPrice);
                }
            }

            return orderId;
        }


        private string GenerateOrderCode()
        {
            // Format: ORD-YYMMDD-XXXX (XXXX: număr aleator între 1000 și 9999)
            string datePart = DateTime.Now.ToString("yyMMdd");
            Random rand = new Random();
            int randomPart = rand.Next(1000, 10000);
            return $"ORD-{datePart}-{randomPart}";
        }

        private async Task<(decimal DeliveryFee, decimal Discount, decimal FinalAmount)> CalculateOrderAmountsAsync(decimal subtotal)
        {
            decimal deliveryFee = 0;
            decimal discount = 0;
            decimal finalAmount = subtotal;

            // Verificăm dacă se aplică taxa de livrare
            decimal minOrderForFreeDelivery = _configService.GetMinOrderForFreeDelivery();
            if (subtotal < minOrderForFreeDelivery)
            {
                deliveryFee = _configService.GetDeliveryFee();
            }

            // Verificăm dacă utilizatorul are dreptul la discount (bazat pe numărul de comenzi anterioare)
            if (_authService.IsLoggedIn && _authService.CurrentUser != null)
            {
                var userId = _authService.CurrentUser.UserId;
                // Use await here instead of .Result
                bool isEligibleForDiscount = await CheckDiscountEligibility(userId);

                if (isEligibleForDiscount)
                {
                    decimal discountPercentage = _configService.GetOrderDiscountPercentage();
                    discount = subtotal * (discountPercentage / 100);
                }
            }

            // Calculăm suma finală
            finalAmount = subtotal + deliveryFee - discount;

            return (deliveryFee, discount, finalAmount);
        }

        private async Task<bool> CheckDiscountEligibility(int userId)
        {
            // Verificăm dacă utilizatorul are un număr minim de comenzi într-o perioadă specificată
            int minOrders = _configService.GetMinOrdersForDiscount();
            int periodInDays = _configService.GetOrdersPeriodForDiscount();

            DateTime startDate = DateTime.Now.AddDays(-periodInDays);

            int orderCount = await _context.Orders
                .CountAsync(o => o.UserId == userId && o.OrderDate >= startDate);

            return orderCount >= minOrders;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            try
            {
                await _context.UpdateOrderStatusAsync(orderId, newStatus);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            return await _context.GetOrdersByUserIdAsync(userId);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.GetAllOrdersAsync();
        }

        public async Task<List<Order>> GetActiveOrdersAsync()
        {
            return await _context.GetActiveOrdersAsync();
        }
    }

    // Clasă pentru reprezentarea unui item din comandă (poate fi produs sau meniu)
    public class OrderItem
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public bool IsProduct { get; set; } // true pentru produs, false pentru meniu
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}