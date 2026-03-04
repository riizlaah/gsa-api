using System;
using System.Collections.Generic;

namespace gsa_api.Models;

public partial class Purchase
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public int? CouponId { get; set; }

    public decimal PricePaid { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime PurchasedAt { get; set; }

    public virtual Coupon? Coupon { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
