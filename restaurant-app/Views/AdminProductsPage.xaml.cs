using System.Windows;
using System.Windows.Controls;
using restaurant_app.Services;
using restaurant_app.ViewModels;

namespace restaurant_app.Views
{
    /// <summary>
    /// Interaction logic for AdminProductsPage.xaml
    /// </summary>
    public partial class AdminProductsPage : Page
    {
        public AdminProductsPage()
        {
            InitializeComponent();

            try
            {
                var menuService = ServiceLocator.Instance.MenuService;
                var navigationService = ServiceLocator.Instance.NavigationService;
                var authService = ServiceLocator.Instance.AuthService;

                var viewModel = new AdminProductsViewModel(menuService, navigationService, authService);
                DataContext = viewModel;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Eroare: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
