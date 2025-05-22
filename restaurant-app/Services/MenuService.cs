using Microsoft.EntityFrameworkCore;
using restaurant_app.Models;
using restaurant_app.Models.DataAccessLayer;
using restaurant_app.Helpers; // Add this to import the extension methods
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace restaurant_app.Services
{
    public class MenuService
    {
        private readonly RestaurantDALContext _context;
        private readonly ConfigService _configService;

        public MenuService(RestaurantDALContext context, ConfigService configService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        public async Task<List<restaurant_app.Models.ProductWithCategoryAndAllergens>> GetAllProductsAsync()
        {
            return await _context.GetAvailableProductsAsync();
        }


        public async Task<List<restaurant_app.Models.ProductWithCategoryAndAllergens>> SearchProductsByKeywordAsync(string keyword)
        {
            var products = await GetAllProductsAsync();
            return products.FilterByKeyword(keyword).ToList();
        }

        public async Task<List<restaurant_app.Models.ProductWithCategoryAndAllergens>> SearchProductsByAllergenAsync(string allergen, bool exclude = false)
        {
            var products = await GetAllProductsAsync();
            return products.FilterByAllergen(allergen, exclude).ToList();
        }


        public async Task<List<MenuWithProducts>> GetAllMenusAsync()
        {
            return await _context.GetAvailableMenusAsync();
        }

        public async Task<List<ProductLowStock>> GetLowStockProductsAsync()
        {
            // Get low stock threshold from configuration
            decimal threshold = _configService.GetDecimalValue("LowStockThreshold", 5.0m);
            return await _context.GetLowStockProductsAsync(threshold);
        }

        public async Task<bool> AddProductAsync(Product product, List<int> allergenIds, List<string> imagePaths)
        {
            try
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product));

                // Initialize collections if they're null
                product.Allergens ??= new List<Allergen>();
                product.ProductImages ??= new List<ProductImage>();

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Add allergens
                if (allergenIds?.Count > 0)
                {
                    var allergens = await _context.Allergens
                        .Where(a => allergenIds.Contains(a.AllergenId))
                        .ToListAsync();

                    foreach (var allergen in allergens)
                    {
                        product.Allergens.Add(allergen);
                    }

                    await _context.SaveChangesAsync();
                }

                // Add images
                if (imagePaths?.Count > 0)
                {
                    foreach (var path in imagePaths)
                    {
                        product.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImagePath = path
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding product: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product, List<int> allergenIds, List<string> imagePaths)
        {
            try
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product));

                var existingProduct = await _context.Products
                    .Include(p => p.Allergens)
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);

                if (existingProduct == null)
                    return false;

                // Update product properties
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.PortionQuantity = product.PortionQuantity;
                existingProduct.PortionUnit = product.PortionUnit;
                existingProduct.TotalQuantity = product.TotalQuantity;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.IsAvailable = product.IsAvailable;

                // Update allergens - use a safer approach
                var allergenList = existingProduct.Allergens.ToList();
                foreach (var allergen in allergenList)
                {
                    existingProduct.Allergens.Remove(allergen);
                }

                if (allergenIds?.Count > 0)
                {
                    var allergens = await _context.Allergens
                        .Where(a => allergenIds.Contains(a.AllergenId))
                        .ToListAsync();

                    foreach (var allergen in allergens)
                    {
                        existingProduct.Allergens.Add(allergen);
                    }
                }

                // Update images
                if (imagePaths?.Count > 0)
                {
                    // Remove old images one by one to avoid EF Core tracking issues
                    var imagesToDelete = existingProduct.ProductImages.ToList();
                    foreach (var image in imagesToDelete)
                    {
                        _context.ProductImages.Remove(image);
                    }

                    await _context.SaveChangesAsync();

                    // Add new images
                    foreach (var path in imagePaths)
                    {
                        existingProduct.ProductImages.Add(new ProductImage
                        {
                            ProductId = existingProduct.ProductId,
                            ImagePath = path
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .Include(p => p.MenuProducts)
                    .Include(p => p.OrderDetails)
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                    return false;

                // Check if product is used in any order
                if (product.OrderDetails.Any())
                {
                    // If product is in orders, just mark as unavailable instead of deleting
                    product.IsAvailable = false;
                    await _context.SaveChangesAsync();
                    return true;
                }

                // Otherwise, proceed with deletion
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<List<Allergen>> GetAllAllergensAsync()
        {
            return await _context.Allergens.ToListAsync();
        }

        // --- MENU METHODS ---
        public async Task<bool> AddMenuAsync(Menu menu)
        {
            try
            {
                if (menu == null) throw new ArgumentNullException(nameof(menu));
                _context.Menus.Add(menu);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding menu: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateMenuAsync(Menu menu)
        {
            try
            {
                if (menu == null) throw new ArgumentNullException(nameof(menu));
                var existingMenu = await _context.Menus.FindAsync(menu.MenuId);
                if (existingMenu == null) return false;

                existingMenu.CategoryId = menu.CategoryId;
                // Update other properties as needed
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating menu: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteMenuAsync(int menuId)
        {
            try
            {
                var menu = await _context.Menus.FindAsync(menuId);
                if (menu == null) return false;
                _context.Menus.Remove(menu);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting menu: {ex.Message}");
                return false;
            }
        }

        // --- CATEGORY METHODS ---
        public async Task<bool> AddCategoryAsync(Category category)
        {
            try
            {
                if (category == null) throw new ArgumentNullException(nameof(category));
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding category: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                if (category == null) throw new ArgumentNullException(nameof(category));
                var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
                if (existingCategory == null) return false;

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating category: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            try
            {
                var category = await _context.Categories.FindAsync(categoryId);
                if (category == null) return false;
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting category: {ex.Message}");
                return false;
            }
        }

        // --- ALLERGEN METHODS ---
        public async Task<bool> AddAllergenAsync(Allergen allergen)
        {
            try
            {
                if (allergen == null) throw new ArgumentNullException(nameof(allergen));
                _context.Allergens.Add(allergen);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding allergen: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAllergenAsync(Allergen allergen)
        {
            try
            {
                if (allergen == null) throw new ArgumentNullException(nameof(allergen));
                var existingAllergen = await _context.Allergens.FindAsync(allergen.AllergenId);
                if (existingAllergen == null) return false;

                existingAllergen.Name = allergen.Name;
                existingAllergen.Description = allergen.Description;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating allergen: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAllergenAsync(int allergenId)
        {
            try
            {
                var allergen = await _context.Allergens.FindAsync(allergenId);
                if (allergen == null) return false;
                _context.Allergens.Remove(allergen);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting allergen: {ex.Message}");
                return false;
            }
        }
    }
}
