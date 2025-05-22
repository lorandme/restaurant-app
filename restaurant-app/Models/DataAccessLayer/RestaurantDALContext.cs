using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using restaurant_app.Models;

namespace restaurant_app.Models.DataAccessLayer
{
    public class RestaurantDALContext : RestaurantDbContext
    {
        public RestaurantDALContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
        }

        // Define DbSet properties for your query result models
        public DbSet<ProductWithCategoryAndAllergens> ProductsWithCategoryAndAllergens { get; set; }
        public DbSet<MenuWithProducts> MenusWithProducts { get; set; }
        public DbSet<ProductLowStock> ProductsLowStock { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call the base OnModelCreating to include all the entity configurations from RestaurantDbContext
            base.OnModelCreating(modelBuilder);

            // Configure the query result entities - mark them as having no key since they're just for queries
            modelBuilder.Entity<ProductWithCategoryAndAllergens>()
                .HasNoKey()
                .ToView("ProductWithCategoryAndAllergens"); // This is a view name, not a physical table

            modelBuilder.Entity<MenuWithProducts>()
                .HasNoKey()
                .ToView("MenuWithProducts");

            modelBuilder.Entity<ProductLowStock>()
                .HasNoKey()
                .ToView("ProductLowStock");
        }

        /// <summary>
        /// Gets all available products with their categories and allergens using the GetAvailableProducts stored procedure
        /// </summary>
        public async Task<List<ProductWithCategoryAndAllergens>> GetAvailableProductsAsync()
        {
            return await ProductsWithCategoryAndAllergens
                .FromSqlRaw("EXEC GetAvailableProducts")
                .ToListAsync();
        }

        /// <summary>
        /// Gets all available menus with their products using the GetAvailableMenus stored procedure
        /// </summary>
        public async Task<List<MenuWithProducts>> GetAvailableMenusAsync()
        {
            return await MenusWithProducts
                .FromSqlRaw("EXEC GetAvailableMenus")
                .ToListAsync();
        }

        /// <summary>
        /// Gets products with stock below the specified threshold using the GetLowStockProducts stored procedure
        /// </summary>
        /// <param name="threshold">The minimum quantity threshold</param>
        public async Task<List<ProductLowStock>> GetLowStockProductsAsync(decimal threshold)
        {
            var param = new SqlParameter("@Threshold", threshold);
            return await ProductsLowStock
                .FromSqlRaw("EXEC GetLowStockProducts @Threshold", param)
                .ToListAsync();
        }

        // All other methods remain the same...
        /// <summary>
        /// Creates a new order using the CreateOrder stored procedure
        /// </summary>
        /// <returns>The ID of the newly created order</returns>
        public async Task<int> CreateOrderAsync(int userId, string orderCode, string deliveryAddress,
            decimal totalAmount, decimal deliveryFee, decimal discount, decimal finalAmount,
            DateTime estimatedDeliveryTime)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@OrderCode", orderCode),
                new SqlParameter("@DeliveryAddress", deliveryAddress),
                new SqlParameter("@TotalAmount", totalAmount),
                new SqlParameter("@DeliveryFee", deliveryFee),
                new SqlParameter("@Discount", discount),
                new SqlParameter("@FinalAmount", finalAmount),
                new SqlParameter("@EstimatedDeliveryTime", estimatedDeliveryTime),
                new SqlParameter("@OrderID", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output }
            };

            await Database.ExecuteSqlRawAsync("EXEC CreateOrder @UserID, @OrderCode, @DeliveryAddress, @TotalAmount, @DeliveryFee, @Discount, @FinalAmount, @EstimatedDeliveryTime, @OrderID OUTPUT", parameters);

            return (int)parameters[8].Value;
        }

        /// <summary>
        /// Adds a product to an existing order using the AddProductToOrder stored procedure
        /// </summary>
        public async Task AddProductToOrderAsync(int orderId, int productId, int quantity, decimal unitPrice, decimal totalPrice)
        {
            var parameters = new[]
            {
                new SqlParameter("@OrderID", orderId),
                new SqlParameter("@ProductID", productId),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@UnitPrice", unitPrice),
                new SqlParameter("@TotalPrice", totalPrice)
            };

            await Database.ExecuteSqlRawAsync("EXEC AddProductToOrder @OrderID, @ProductID, @Quantity, @UnitPrice, @TotalPrice", parameters);
        }

        /// <summary>
        /// Adds a menu to an existing order
        /// </summary>
        public async Task AddMenuToOrderAsync(int orderId, int menuId, int quantity, decimal unitPrice, decimal totalPrice)
        {
            // Create parameters for the query
            var parameters = new[]
            {
                new SqlParameter("@OrderID", orderId),
                new SqlParameter("@MenuID", menuId),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@UnitPrice", unitPrice),
                new SqlParameter("@TotalPrice", totalPrice)
            };

            // Using a simple INSERT statement since there's no stored procedure specifically for adding menus to orders
            await Database.ExecuteSqlRawAsync(
                "INSERT INTO OrderDetails (OrderID, MenuID, Quantity, UnitPrice, TotalPrice) VALUES (@OrderID, @MenuID, @Quantity, @UnitPrice, @TotalPrice)",
                parameters);
        }

        /// <summary>
        /// Updates the status of an order
        /// </summary>
        public async Task UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var parameters = new[]
            {
                new SqlParameter("@OrderID", orderId),
                new SqlParameter("@Status", newStatus)
            };

            await Database.ExecuteSqlRawAsync(
                "UPDATE Orders SET Status = @Status WHERE OrderID = @OrderID",
                parameters);
        }

        /// <summary>
        /// Gets all orders for a specific user
        /// </summary>
        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            var param = new SqlParameter("@UserID", userId);
            return await Orders
                .FromSqlRaw("SELECT * FROM Orders WHERE UserID = @UserID ORDER BY OrderDate DESC", param)
                .Include(o => o.OrderDetails)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all active orders (not delivered or cancelled)
        /// </summary>
        public async Task<List<Order>> GetActiveOrdersAsync()
        {
            return await Orders
                .FromSqlRaw("SELECT * FROM Orders WHERE Status NOT IN ('Delivered', 'Cancelled') ORDER BY OrderDate DESC")
                .Include(o => o.OrderDetails)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all orders sorted by date descending
        /// </summary>
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await Orders
                .FromSqlRaw("SELECT * FROM Orders ORDER BY OrderDate DESC")
                .Include(o => o.OrderDetails)
                .ToListAsync();
        }
    }
}
