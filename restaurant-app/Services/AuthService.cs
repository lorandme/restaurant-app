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

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == username);

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
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(
    string username, string password)
        {
            try
            {
                // Verificăm dacă există deja un utilizator cu acest username
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (existingUser != null)
                    return (false, "Există deja un cont cu acest nume de utilizator.");

                // Creăm un nou utilizator
                var newUser = new User
                {
                    Username = username,
                    PasswordHash = HashPassword(password),
                    UserType = "Client", // Utilizatorii noi sunt întotdeauna clienți
                    Email = null, // Null instead of empty string
                    FirstName = null,
                    LastName = null,
                    PhoneNumber = null,
                    DeliveryAddress = null
                };

                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();

                // După înregistrare autentificăm automat utilizatorul
                CurrentUser = newUser;

                return (true, "Înregistrare realizată cu succes!");
            }
            catch (Exception ex)
            {
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
    }
}