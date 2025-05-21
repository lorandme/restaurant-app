using System;
using System.Collections.Generic;

namespace restaurant_app.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
