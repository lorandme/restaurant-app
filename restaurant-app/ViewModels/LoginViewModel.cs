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

        private string _email = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isProcessing;

        public Action LoginSuccessfulCallback { get; set; }

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

        public LoginViewModel()
        {
            _authService = ServiceLocator.Instance.AuthService;

            LoginCommand = new RelayCommand(async param => await ExecuteLoginAsync(param as PasswordBox), CanLogin);
            NavigateToRegisterCommand = new RelayCommand(_ => ExecuteNavigateToRegister());
            NavigateBackCommand = new RelayCommand(_ => ExecuteNavigateBack());

            // Make sure CanLogin is re-evaluated when properties change
            PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(Email) || args.PropertyName == nameof(IsProcessing))
                {
                    (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            };
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

                System.Diagnostics.Debug.WriteLine($"Încercare autentificare pentru: {Email}");

                bool result = await _authService.LoginAsync(Email, passwordBox.Password);

                if (result)
                {
                    System.Diagnostics.Debug.WriteLine("Autentificare reușită!");

                    // Executăm callback-ul dacă există
                    LoginSuccessfulCallback?.Invoke();

                    // Închidem fereastra de login
                    CloseWindow();
                }
                else
                {
                    StatusMessage = "Email sau parolă incorecte.";
                    System.Diagnostics.Debug.WriteLine("Autentificare eșuată: Email sau parolă incorecte.");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Excepție la autentificare: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void ExecuteNavigateToRegister()
        {
            var registerWindow = new RegisterPage();
            registerWindow.Show();
            CloseWindow();
        }

        private void ExecuteNavigateBack()
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            // Căutăm fereastra LoginPage și o închidem
            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginPage)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
