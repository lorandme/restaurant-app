using System;
using System.Collections.Generic;

namespace restaurant_app.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal PortionQuantity { get; set; }

    public string PortionUnit { get; set; } = null!;

    public decimal TotalQuantity { get; set; }

    public int CategoryId { get; set; }

    public bool? IsAvailable { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<MenuProduct> MenuProducts { get; set; } = new List<MenuProduct>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<Allergen> Allergens { get; set; } = new List<Allergen>();
}
