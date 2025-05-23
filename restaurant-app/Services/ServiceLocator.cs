﻿using Microsoft.EntityFrameworkCore;
using restaurant_app.Models;
using restaurant_app.Models.DataAccessLayer;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace restaurant_app.Services
{
    public class ServiceLocator
    {
        private static ServiceLocator _instance;
        private static readonly object _lock = new object();

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServiceLocator();
                        }
                    }
                }
                return _instance;
            }
        }

        private ServiceLocator()
        {
            InitializeServices();
        }

        // Servicii
        public NavigationService NavigationService { get; private set; }
        public AuthService AuthService { get; private set; }
        public MenuService MenuService { get; private set; }
        public ConfigService ConfigService { get; private set; }
        public OrderService OrderService { get; private set; }

        private RestaurantDbContext _dbContext;
        private RestaurantDALContext _dalContext;

        public void InitializeServices()
        {
            try
            {
                // Inițializăm conexiunea la baza de date
                var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>();
                optionsBuilder.UseSqlServer("Server=DESKTOP-145H9U0\\SQLEXPRESS;Database=RestaurantDB;Trusted_Connection=True;Encrypt=false;TrustServerCertificate=true");

                _dbContext = new RestaurantDbContext(optionsBuilder.Options);
                _dalContext = new RestaurantDALContext(optionsBuilder.Options);

                // Inițializăm serviciile

                // Inițializăm serviciile
                ConfigService = new ConfigService(_dbContext);
                AuthService = new AuthService(_dbContext);
                MenuService = new MenuService(_dalContext, ConfigService);
                OrderService = new OrderService(_dalContext, ConfigService, AuthService);
            }
            catch (Exception ex)
            {
                // Log this exception or show it in some way
                System.Diagnostics.Debug.WriteLine($"Error initializing services: {ex.Message}");
            }
        }

        public async Task InitializeConfigurationAsync()
        {
            try
            {
                // Verifică dacă ConfigService este inițializat
                if (ConfigService == null)
                {
                    System.Diagnostics.Debug.WriteLine("ConfigService is not initialized!");
                    return;
                }

                // Încarcă configurările asincron
                await ConfigService.LoadConfigAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
                throw; // Re-throw exception to be caught by the caller
            }
        }

        public void InitializeNavigationService(Frame navigationFrame)
        {
            NavigationService = new NavigationService(navigationFrame);
        }
    }
}
