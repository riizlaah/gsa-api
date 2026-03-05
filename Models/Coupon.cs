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

public class CouponInput
{
    public string couponCode { get; set; } = null!;

    public decimal discountValue { get; set; }

    public int quota { get; set; }

    public DateTime expiryDate { get; set; }

    public Coupon toCoupon()
    {
        return new Coupon { Code = couponCode, DiscountPct = discountValue, Quota = quota, ExpiryDate = expiryDate};
    }
}