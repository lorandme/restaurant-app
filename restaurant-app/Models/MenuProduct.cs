using System;
using System.Collections.Generic;

namespace restaurant_app.Models;

public partial class MenuProduct
{
    public int MenuId { get; set; }

    public int ProductId { get; set; }

    public decimal ProductQuantity { get; set; }

    public string ProductUnit { get; set; } = null!;

    public virtual Menu Menu { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
