using System;
using System.Collections.Generic;

namespace restaurant_app.Models;

public partial class Allergen
{
    public int AllergenId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
