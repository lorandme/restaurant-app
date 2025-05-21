using Microsoft.EntityFrameworkCore;
using restaurant_app.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace restaurant_app.Services
{
    public class ConfigService
    {
        private readonly RestaurantDbContext _dbContext;
        private Dictionary<string, string> _configCache = new Dictionary<string, string>();

        public ConfigService(RestaurantDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Încărcăm toate configurările în cache la pornirea aplicației
        public async Task LoadConfigAsync()
        {
            var configs = await _dbContext.AppConfigs.ToListAsync();
            _configCache.Clear();
            foreach (var config in configs)
            {
                _configCache[config.ConfigKey] = config.ConfigValue;
            }
        }

        // Obține o valoare din configurări ca string
        public string GetValue(string key, string defaultValue = "")
        {
            if (_configCache.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }

        // Obține o valoare din configurări ca int
        public int GetIntValue(string key, int defaultValue = 0)
        {
            if (_configCache.TryGetValue(key, out string value) && int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        // Obține o valoare din configurări ca decimal
        public decimal GetDecimalValue(string key, decimal defaultValue = 0)
        {
            if (_configCache.TryGetValue(key, out string value) && decimal.TryParse(value, out decimal result))
            {
                return result;
            }
            return defaultValue;
        }

        // Obține discount-ul pentru comenzi
        public decimal GetOrderDiscountPercentage()
        {
            return GetDecimalValue("OrderDiscountPercentage", 10.0m);
        }

        // Obține discount-ul pentru meniuri
        public decimal GetMenuDiscountPercentage()
        {
            return GetDecimalValue("MenuDiscountPercentage", 15.0m);
        }

        // Obține valoarea minimă a comenzii pentru livrare gratuită
        public decimal GetMinOrderForFreeDelivery()
        {
            return GetDecimalValue("MinOrderForFreeDelivery", 75.0m);
        }

        // Obține costul de livrare
        public decimal GetDeliveryFee()
        {
            return GetDecimalValue("DeliveryFee", 10.0m);
        }

        // Obține numărul minim de comenzi pentru discount
        public int GetMinOrdersForDiscount()
        {
            return GetIntValue("MinOrdersForDiscount", 5);
        }

        // Obține perioada (în zile) în care se verifică numărul de comenzi pentru discount
        public int GetOrdersPeriodForDiscount()
        {
            return GetIntValue("OrdersPeriodForDiscount", 30);
        }
    }
}