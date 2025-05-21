using System;
using System.Collections.Generic;

namespace restaurant_app.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? DeliveryAddress { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
