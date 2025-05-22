using Microsoft.EntityFrameworkCore;
using restaurant_app.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace restaurant_app.Services
{
    public class AuthService
    {
        private readonly RestaurantDbContext _dbContext;

        // Proprietate pentru utilizatorul autentificat
        public User? CurrentUser { get; private set; }

        // Proprietăți pentru a verifica tipul de utilizator
        public bool IsLoggedIn => CurrentUser != null;
        public bool IsEmployee => IsLoggedIn && CurrentUser?.UserType == "Employee";
        public bool IsClient => IsLoggedIn && CurrentUser?.UserType == "Client";

        public AuthService(RestaurantDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                    return false;

                // Verificăm parola folosind hash
                if (VerifyPassword(password, user.PasswordHash))
                {
                    CurrentUser = user;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare la autentificare: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool Success, string Message)> RegisterClientAsync(
    string firstName, string lastName, string email,
    string phoneNumber, string deliveryAddress, string password)
        {
            try
            {
                // Verificăm dacă există deja un utilizator cu acest email
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (existingUser != null)
                    return (false, "Există deja un cont cu acest email.");

                // Validări simple
                if (string.IsNullOrWhiteSpace(firstName))
                    return (false, "Numele este obligatoriu.");

                if (string.IsNullOrWhiteSpace(lastName))
                    return (false, "Prenumele este obligatoriu.");

                if (string.IsNullOrWhiteSpace(email))
                    return (false, "Email-ul este obligatoriu.");

                if (string.IsNullOrWhiteSpace(phoneNumber))
                    return (false, "Numărul de telefon este obligatoriu.");

                if (string.IsNullOrWhiteSpace(deliveryAddress))
                    return (false, "Adresa de livrare este obligatorie.");

                if (password.Length < 6)
                    return (false, "Parola trebuie să aibă cel puțin 6 caractere.");

                // Verificăm formatul email-ului
                if (!IsValidEmail(email))
                    return (false, "Formatul adresei de email nu este valid.");

                // Creăm un nou utilizator
                var newUser = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    DeliveryAddress = deliveryAddress,
                    PasswordHash = HashPassword(password),
                    UserType = "Client" // Utilizatorii noi sunt întotdeauna clienți
                                        // Removed Username = email line
                };

                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();

                // După înregistrare autentificăm automat utilizatorul
                CurrentUser = newUser;

                return (true, "Înregistrare realizată cu succes!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare la înregistrare: {ex.Message}");
                return (false, $"Eroare la înregistrare: {ex.Message}");
            }
        }


        public void Logout()
        {
            CurrentUser = null;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == storedHash;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
