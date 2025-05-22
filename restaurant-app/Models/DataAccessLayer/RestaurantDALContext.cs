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
            // Call the base implementation first to include all configurations from parent context
            base.OnModelCreating(modelBuilder);

            // Configure all result entities as keyless
            modelBuilder.Entity<ProductWithCategoryAndAllergens>()
                .HasNoKey()
                .ToView(null);

            // Add this configuration for MenuWithProducts
            modelBuilder.Entity<MenuWithProducts>()
                .HasNoKey()
                .ToView(null);

            // Add this configuration for ProductLowStock
            modelBuilder.Entity<ProductLowStock>()
                .HasNoKey()
                .ToView(null);
        }

        /// <summary>
        /// Gets all available products with their categories and allergens using the GetAvailableProducts stored procedure
        /// </summary>
        // Make this change in your RestaurantDALContext.cs
        public async Task<List<ProductWithCategoryAndAllergens>> GetAvailableProductsAsync()
        {
            try
            {
                // Add debug logging
                Console.WriteLine("Fetching products from database using stored procedure...");

                // Check if we can use the stored procedure
                var products = await ProductsWithCategoryAndAllergens
                    .FromSqlRaw("EXEC GetAvailableProducts")
                    .ToListAsync();

                Console.WriteLine($"Found {products.Count} products from stored procedure");

                // If stored procedure doesn't return DatabaseIsAvailable, try to use direct SQL query
                if (products.Count > 0 && products.All(p => p.DatabaseIsAvailable == null))
                {
                    Console.WriteLine("DatabaseIsAvailable not found in stored procedure results. Trying direct SQL...");
                    products = await GetProductsDirectSqlAsync();
                }

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching products: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Fallback to direct SQL if stored procedure fails
                Console.WriteLine("Falling back to direct SQL...");
                return await GetProductsDirectSqlAsync();
            }
        }


        /// <summary>
        /// Gets all available menus with their products using the GetAvailableMenus stored procedure
        /// </summary>
        public async Task<List<MenuWithProducts>> GetAvailableMenusAsync()
        {
            try
            {
                // Add debug logging
                Console.WriteLine("Fetching menus from database...");
                var menus = await MenusWithProducts
                    .FromSqlRaw("EXEC GetAvailableMenus")
                    .ToListAsync();

                Console.WriteLine($"Found {menus.Count} menu items");
                return menus;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching menus: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<MenuWithProducts>();
            }
        }

        /// <summary>
        /// Gets products with stock below the specified threshold using the GetLowStockProducts stored procedure
        /// </summary>
        /// <param name="threshold">The minimum quantity threshold</param>
        public async Task<List<ProductLowStock>> GetLowStockProductsAsync(decimal threshold)
        {
            try
            {
                var param = new SqlParameter("@Threshold", threshold);
                var products = await ProductsLowStock
                    .FromSqlRaw("EXEC GetLowStockProducts @Threshold", param)
                    .ToListAsync();

                Console.WriteLine($"Found {products.Count} low stock products");
                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching low stock products: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<ProductLowStock>();
            }
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

        /// <summary>
        /// Gets products with a direct SQL query instead of stored procedure
        /// </summary>
        /// <summary>
        /// Gets products with a direct SQL query instead of stored procedure
        /// </summary>
        public async Task<List<ProductWithCategoryAndAllergens>> GetProductsDirectSqlAsync()
        {
            try
            {
                Console.WriteLine("Executing direct SQL query for products with allergens...");

                string sqlQuery = @"
            SELECT 
                p.ProductId AS ProductID, 
                p.Name AS ProductName, 
                p.Price, 
                p.PortionQuantity, 
                p.PortionUnit, 
                p.TotalQuantity, 
                p.CategoryId AS CategoryID, 
                c.Name AS CategoryName,
                STUFF((
                    SELECT ', ' + a.Name
                    FROM ProductAllergens pa
                    JOIN Allergens a ON pa.AllergenId = a.AllergenId
                    WHERE pa.ProductId = p.ProductId
                    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Allergens,
                p.IsAvailable AS DatabaseIsAvailable
            FROM Products p
            JOIN Categories c ON p.CategoryId = c.CategoryId";

                var products = await ProductsWithCategoryAndAllergens
                    .FromSqlRaw(sqlQuery)
                    .ToListAsync();

                Console.WriteLine($"Direct SQL found {products.Count} products");

                // Log the names of first few products for debugging
                if (products.Count > 0)
                {
                    foreach (var p in products.Take(3))
                    {
                        Console.WriteLine($"  - Product: {p.ProductID}, {p.ProductName}, Allergens: {p.Allergens}");
                    }
                }

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in direct SQL query: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Option 2: If Option 1 fails, try manual mapping approach
                try
                {
                    Console.WriteLine("Attempting alternative SQL approach with allergens...");
                    var products = new List<ProductWithCategoryAndAllergens>();

                    using (var command = Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = @"
                    SELECT 
                        p.ProductId, 
                        p.Name, 
                        p.Price, 
                        p.PortionQuantity, 
                        p.PortionUnit, 
                        p.TotalQuantity, 
                        p.CategoryId, 
                        c.Name AS CategoryName,
                        (
                            SELECT STRING_AGG(a.Name, ', ')
                            FROM ProductAllergens pa
                            JOIN Allergens a ON pa.AllergenId = a.AllergenId
                            WHERE pa.ProductId = p.ProductId
                        ) AS Allergens,
                        p.IsAvailable
                    FROM Products p
                    JOIN Categories c ON p.CategoryId = c.CategoryId";

                        if (Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                        {
                            await Database.GetDbConnection().OpenAsync();
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                products.Add(new ProductWithCategoryAndAllergens
                                {
                                    ProductID = reader.GetInt32(0),
                                    ProductName = reader.GetString(1),
                                    Price = reader.GetDecimal(2),
                                    PortionQuantity = reader.GetDecimal(3),
                                    PortionUnit = reader.GetString(4),
                                    TotalQuantity = reader.GetDecimal(5),
                                    CategoryID = reader.GetInt32(6),
                                    CategoryName = reader.GetString(7),
                                    Allergens = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                                    DatabaseIsAvailable = reader.IsDBNull(9) ? null : (bool?)reader.GetBoolean(9)
                                });
                            }
                        }
                    }

                    Console.WriteLine($"Alternative query found {products.Count} products with allergens");
                    return products;
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Alternative SQL approach failed: {ex2.Message}");
                    return new List<ProductWithCategoryAndAllergens>();
                }
            }
        }




    }


}
