using System;
using System.Collections.Generic;

namespace restaurant_app.Models;

public partial class Menu
{
    public int MenuId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public bool? IsAvailable { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<MenuProduct> MenuProducts { get; set; } = new List<MenuProduct>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
