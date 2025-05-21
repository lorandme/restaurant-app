using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using restaurant_app.Helpers;
using restaurant_app.Services;
using restaurant_app.Views;


namespace restaurant_app.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly NavigationService _navigationService;

        private string _email = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isProcessing;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand NavigateBackCommand { get; }

        public LoginViewModel(AuthService authService, NavigationService navigationService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            LoginCommand = new RelayCommand(async param => await ExecuteLoginAsync(param as PasswordBox), CanLogin);
            NavigateToRegisterCommand = new RelayCommand(_ => ExecuteNavigateToRegister());
            NavigateBackCommand = new RelayCommand(_ => ExecuteNavigateBack());
        }

        private bool CanLogin(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            return !IsProcessing &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   passwordBox != null &&
                   !string.IsNullOrWhiteSpace(passwordBox.Password);
        }

        private async Task ExecuteLoginAsync(PasswordBox passwordBox)
        {
            if (passwordBox == null)
                return;

            try
            {
                IsProcessing = true;
                StatusMessage = string.Empty;

                bool result = await _authService.LoginAsync(Email, passwordBox.Password);

                if (result)
                {
                    // Create and show the appropriate window based on user type
                    Window targetWindow;

                    if (_authService.IsEmployee)
                    {
                        // Navigate to the admin dashboard page instead of opening a window
                        _navigationService.NavigateTo("AdminDashboard");
                        // Optionally close the login window if needed
                        if (System.Windows.Application.Current.MainWindow is LoginPage loginWindow)
                        {
                            loginWindow.Close();
                        }
                    }
                    else
                    {
                        targetWindow = new MainWindow(); // Create the main window for regular users
                        targetWindow.Show();
                        if (System.Windows.Application.Current.MainWindow is LoginPage loginWindow)
                        {
                            loginWindow.Close();
                        }
                    }

                }
                else
                {
                    StatusMessage = "Email sau parolă incorecte.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la autentificare: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void ExecuteNavigateToRegister()
        {
            _navigationService.NavigateTo("Register");
        }

        private void ExecuteNavigateBack()
        {
            _navigationService.NavigateTo("MainMenu");
        }
    }
}