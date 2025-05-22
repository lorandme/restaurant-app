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

        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _email = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _deliveryAddress = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isProcessing;

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public string DeliveryAddress
        {
            get => _deliveryAddress;
            set => SetProperty(ref _deliveryAddress, value);
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

        // Constructor 
        public RegisterViewModel(AuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            RegisterCommand = new RelayCommand(async param => await ExecuteRegisterAsync(param as PasswordBox), CanRegister);
            NavigateToLoginCommand = new RelayCommand(_ => ExecuteNavigateToLogin());
            NavigateBackCommand = new RelayCommand(_ => ExecuteNavigateBack());

            // Make sure CanRegister is re-evaluated when properties change
            PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(FirstName) ||
                    args.PropertyName == nameof(LastName) ||
                    args.PropertyName == nameof(Email) ||
                    args.PropertyName == nameof(PhoneNumber) ||
                    args.PropertyName == nameof(DeliveryAddress) ||
                    args.PropertyName == nameof(IsProcessing))
                {
                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            };
        }

        private bool CanRegister(object parameter)
        {
            var passwordBox = parameter as PasswordBox;

            return !IsProcessing &&
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(PhoneNumber) &&
                   !string.IsNullOrWhiteSpace(DeliveryAddress) &&
                   passwordBox != null &&
                   !string.IsNullOrWhiteSpace(passwordBox.Password);
        }

        private async Task ExecuteRegisterAsync(PasswordBox passwordBox)
        {
            if (passwordBox == null)
                return;

            var confirmPasswordBox = GetConfirmPasswordBox();
            if (confirmPasswordBox != null && passwordBox.Password != confirmPasswordBox.Password)
            {
                StatusMessage = "Parolele nu corespund.";
                return;
            }

            try
            {
                IsProcessing = true;
                StatusMessage = string.Empty;

                System.Diagnostics.Debug.WriteLine($"Încercare înregistrare pentru: {Email}");

                var result = await _authService.RegisterClientAsync(
                    FirstName,
                    LastName,
                    Email,
                    PhoneNumber,
                    DeliveryAddress,
                    passwordBox.Password);

                if (result.Success)
                {
                    System.Diagnostics.Debug.WriteLine("Înregistrare reușită!");
                    StatusMessage = "Înregistrare reușită! Redirecționare...";

                    // Wait a moment to show success message then navigate
                    await Task.Delay(1000);
                    ExecuteNavigateToLogin();
                }
                else
                {
                    StatusMessage = result.Message;
                    System.Diagnostics.Debug.WriteLine($"Eroare înregistrare: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Excepție: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private PasswordBox GetConfirmPasswordBox()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is RegisterPage registerPage)
                {
                    return registerPage.FindName("ConfirmPasswordBox") as PasswordBox;
                }
            }
            return null;
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
