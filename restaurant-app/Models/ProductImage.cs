﻿using System;
using System.Collections.Generic;

namespace restaurant_app.Models;

public partial class ProductImage
{
    public int ImageId { get; set; }

    public int ProductId { get; set; }

    public string ImagePath { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
