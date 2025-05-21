using System;
using System.Windows;
using System.Windows.Controls;
using restaurant_app.Services;
using restaurant_app.ViewModels;

namespace restaurant_app.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainMenuViewModel _viewModel;
        private Frame _mainFrame;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Create a Frame for navigation
                _mainFrame = new Frame();

                // Initialize ServiceLocator
                var serviceLocator = ServiceLocator.Instance;

                // Initialize NavigationService with the Frame
                serviceLocator.InitializeNavigationService(_mainFrame);

                // Now create and set the ViewModel with the initialized NavigationService
                _viewModel = new MainMenuViewModel(
                    serviceLocator.MenuService,
                    serviceLocator.AuthService,
                    serviceLocator.NavigationService);

                // Set the DataContext for data binding
                DataContext = _viewModel;

                // Start loading data when the window is shown
                Loaded += OnWindowLoaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing window: {ex.Message}", "Initialization Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load configuration first, then load data
                await ServiceLocator.Instance.InitializeConfigurationAsync();

                // Ensure data is loaded when the window is shown
                await _viewModel.LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.IsSearchResultsVisible = false;
            }
        }
    }
}
