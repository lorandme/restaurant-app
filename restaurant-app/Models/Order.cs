using System;
using System.Collections.Generic;

namespace restaurant_app.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public string OrderCode { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public decimal? DeliveryFee { get; set; }

    public decimal? Discount { get; set; }

    public decimal FinalAmount { get; set; }

    public DateTime? EstimatedDeliveryTime { get; set; }

    public string DeliveryAddress { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User User { get; set; } = null!;
}
