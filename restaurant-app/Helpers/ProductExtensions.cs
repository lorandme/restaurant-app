using restaurant_app.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace restaurant_app.Helpers
{
    public static class ProductExtensions
    {
        // Filter products by keyword in name
        public static IEnumerable<ProductWithCategoryAndAllergens> FilterByKeyword(
            this IEnumerable<ProductWithCategoryAndAllergens> products, string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return products;

            return products.Where(p => p.ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        // Filter products that contain a specific allergen
        public static IEnumerable<ProductWithCategoryAndAllergens> FilterByAllergen(
            this IEnumerable<ProductWithCategoryAndAllergens> products, string allergen, bool exclude = false)
        {
            if (string.IsNullOrEmpty(allergen))
                return products;

            return exclude
                ? products.Where(p => string.IsNullOrEmpty(p.Allergens) ||
                                     !p.Allergens.Contains(allergen, StringComparison.OrdinalIgnoreCase))
                : products.Where(p => !string.IsNullOrEmpty(p.Allergens) &&
                                     p.Allergens.Contains(allergen, StringComparison.OrdinalIgnoreCase));
        }
    }
}
