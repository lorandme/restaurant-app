using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace restaurant_app.Models
{
    public class ProductWithCategoryAndAllergens
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal PortionQuantity { get; set; }
        public string PortionUnit { get; set; }
        public decimal TotalQuantity { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Allergens { get; set; }
        public bool? DatabaseIsAvailable { get; set; }

        // Property to determine if the product is available
        public bool IsAvailable => (DatabaseIsAvailable ?? true) && TotalQuantity > 0;

        // Display text for availability status
        public string AvailabilityStatus => IsAvailable ? "Disponibil" : "Indisponibil";

        // Full description for portion display
        public string PortionDescription => $"{PortionQuantity} {PortionUnit}";

        // Formatted price
        public string FormattedPrice => $"{Price:F2} lei";

        // List of individual allergens
        public IEnumerable<string> AllergenList =>
            string.IsNullOrEmpty(Allergens)
                ? Enumerable.Empty<string>()
                : Allergens.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim());

        // Helper method to check if this product contains a specific allergen
        public bool ContainsAllergen(string allergenName)
        {
            if (string.IsNullOrEmpty(Allergens) || string.IsNullOrEmpty(allergenName))
                return false;

            return AllergenList.Any(a =>
                a.Contains(allergenName, StringComparison.OrdinalIgnoreCase));
        }

        // Helper method to check if product name contains keyword
        public bool MatchesKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return true;

            return ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }
    }
}
