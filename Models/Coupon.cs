using System;
using System.Collections.Generic;

namespace gsa_api.Models;

public partial class Coupon
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public decimal DiscountPct { get; set; }

    public int Quota { get; set; }

    public DateTime ExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
