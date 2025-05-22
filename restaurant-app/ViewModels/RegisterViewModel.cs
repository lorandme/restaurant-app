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
    public class RegisterViewModel : ViewModelBase
    {
        private readonly AuthService _authService;

        private string _username = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isProcessing;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
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

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateBackCommand { get; }

        // Constructor simplu
        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;

            RegisterCommand = new RelayCommand(async param => await ExecuteRegisterAsync(param as PasswordBox), CanRegister);
            NavigateToLoginCommand = new RelayCommand(_ => ExecuteNavigateToLogin());
            NavigateBackCommand = new RelayCommand(_ => ExecuteNavigateBack());
        }

        private bool CanRegister(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            return !IsProcessing &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   passwordBox != null &&
                   !string.IsNullOrWhiteSpace(passwordBox.Password);
        }

        private async Task ExecuteRegisterAsync(PasswordBox passwordBox)
        {
            if (passwordBox == null)
                return;

            try
            {
                IsProcessing = true;
                StatusMessage = string.Empty;

                System.Diagnostics.Debug.WriteLine($"Încercare înregistrare pentru: {Username}");

                var result = await _authService.RegisterAsync(
                    Username,
                    passwordBox.Password);

                if (result.Success)
                {
                    MessageBox.Show("Înregistrare reușită! Acum vă puteți autentifica.",
                        "Înregistrare completă",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    ExecuteNavigateToLogin();
                }
                else
                {
                    StatusMessage = result.Message;
                    // Asigurăm actualizarea UI
                    OnPropertyChanged(nameof(StatusMessage));
                    System.Diagnostics.Debug.WriteLine($"Eroare înregistrare: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare: {ex.Message}";
                // Asigurăm actualizarea UI
                OnPropertyChanged(nameof(StatusMessage));
                System.Diagnostics.Debug.WriteLine($"Excepție: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void ExecuteNavigateToLogin()
        {
            var loginWindow = new LoginPage();
            loginWindow.Show();
            CloseWindow();
        }

        private void ExecuteNavigateBack()
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            // Căutăm fereastra RegisterPage și o închidem
            foreach (Window window in Application.Current.Windows)
            {
                if (window is RegisterPage)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}