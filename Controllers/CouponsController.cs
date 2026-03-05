using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gsa_api.Models;
using Microsoft.AspNetCore.Authorization;

namespace gsa_api.Controllers
{
    [Route("gsa-api/v1/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly GsaContext _context;

        public CouponsController(GsaContext context)
        {
            _context = context;
        }

        // GET: api/coupons
        [HttpGet]
        [Authorize]
        public IResult GetCoupons()
        {
            if (!User.IsInRole("admin")) return Results.Forbid();
            return Results.Json( new {
                data = _context.Coupons
                .OrderByDescending(c => c.ExpiryDate)
                .Select(c => new { id = c.Id, couponCode = c.Code, discountValue = c.DiscountPct, expiryDate = c.ExpiryDate, quota = c.Quota })
                .ToList()
            } );
        }


        // PUT: api/coupons/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public IResult PutCoupon(int id, CouponInput coupon)
        {
            if (!User.IsInRole("admin")) return Helper.errMessage("Access denied. Admin role required.", 403);
            if (_context.Coupons.Any(c => c.Code == coupon.couponCode))
            {
                return Helper.errMessage("Validation error: coupon code has been used.", 422);
            }
            if (coupon.quota < 1)
            {
                return Helper.errMessage("Validation error: quota must be a positive integer.", 422);
            }
            if (coupon.discountValue < 1)
            {
                return Helper.errMessage("Validation error: discount must be a positive integer.", 422);
            }
            if (coupon.expiryDate.Date <= DateTime.Now.Date)
            {
                return Helper.errMessage("Validation error: expiry date must be greater than today.", 422);
            }


            if (!CouponExists(id))
            {
                return Results.NotFound();
            } else
            {
                var couponRec = _context.Coupons.Find(id);
                if (couponRec.Code != coupon.couponCode) couponRec.Code = coupon.couponCode;
                if (Math.Abs(coupon.discountValue - couponRec.DiscountPct) > Convert.ToDecimal(0.000001)) couponRec.DiscountPct = coupon.discountValue;
                if (couponRec.ExpiryDate.Date != coupon.expiryDate.Date && coupon.expiryDate.Date > DateTime.Now.Date) couponRec.ExpiryDate = coupon.expiryDate;
                if(couponRec.Quota != coupon.quota) couponRec.Quota = coupon.quota;
                _context.SaveChanges();
                
                return Results.Json(new
                {
                    message = "Coupon updated successfully",
                    data = new { id = couponRec.Id, couponCode = couponRec.Code, discountValue = couponRec.DiscountPct, expiryDate = couponRec.ExpiryDate, quota = couponRec.Quota }
                });
            }

        }

        // POST: api/Coupons
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public IResult PostCoupon(CouponInput coupon)
        {
            if (!User.IsInRole("admin")) return Helper.errMessage("Access denied. Admin role required.", 403);
            if(_context.Coupons.Any(c => c.Code == coupon.couponCode))
            {
                return Helper.errMessage("Validation error: coupon code has been used.", 422);
            }
            if (coupon.quota < 1)
            {
                return Helper.errMessage("Validation error: quota must be a positive integer.", 422);
            }
            if (coupon.discountValue < 1)
            {
                return Helper.errMessage("Validation error: discount must be a positive integer.", 422);
            }
            if(coupon.expiryDate.Date <= DateTime.Now.Date)
            {
                return Helper.errMessage("Validation error: expiry date must be greater than today.", 422);
            }
            var couponRecord = coupon.toCoupon();
            _context.Coupons.Add(couponRecord);
            _context.SaveChanges();

            return Results.Json(new
            {
                message = "Coupon created successfully",
                data = new
                {
                    id = couponRecord.Id,
                    couponCode = couponRecord.Code,
                    discountValue = couponRecord.DiscountPct,
                    expiryDate = couponRecord.ExpiryDate,
                    quota = couponRecord.Quota
                }
            });
        }

        private bool CouponExists(int id)
        {
            return _context.Coupons.Any(e => e.Id == id);
        }
    }
}
