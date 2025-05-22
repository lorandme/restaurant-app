using System;
using System.Collections.Generic;

namespace restaurant_app.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!; // Required Username field

    public string? FirstName { get; set; } // Nullable

    public string? LastName { get; set; } // Nullable

    public string? Email { get; set; } // Nullable

    public string? PhoneNumber { get; set; } // Already nullable

    public string? DeliveryAddress { get; set; } // Already nullable

    public string PasswordHash { get; set; } = null!; // Required

    public string UserType { get; set; } = null!; // Required

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
