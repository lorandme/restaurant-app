using System;
using System.Collections.Generic;

namespace restaurant_app.Models;

public partial class AppConfig
{
    public string ConfigKey { get; set; } = null!;

    public string ConfigValue { get; set; } = null!;

    public string? Description { get; set; }
}
