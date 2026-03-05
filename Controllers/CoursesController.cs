using gsa_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Drawing;
using System.Security.Claims;

namespace gsa_api.Controllers
{
    [Route("gsa-api/v1/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly GsaContext dbc;
        public CoursesController(GsaContext ctx) {
            dbc = ctx;
        }
        [HttpGet]
        [Authorize]
        public IResult GetAll(string title = "", string sort = "desc", string page = "1", string size = "10")
        {
            if(!int.TryParse(page, out int currPage))
            {
                return Helper.errMessage("Validation error: 'page' must be a positive integer.");
            }
            if(currPage < 1)
            {
                return Helper.errMessage("Validation error: 'page' must be a positive integer.");
            }
            if (!int.TryParse(size, out int itemCount))
            {
                return Helper.errMessage("Validation error: 'size' must be a positive integer.");
            }
            if (itemCount < 1)
            {
                return Helper.errMessage("Validation error: 'size' must be a positive integer.");
            }
            var query = dbc.Courses.Select(c => new {c.Id, c.Title, c.Description, c.Price, c.CreatedAt}).AsQueryable();

            if(title.Trim() != "")
            {
                query = query.Where(c => EF.Functions.Like(c.Title, $"%{title}%"));
            }
            if(sort == "desc")
            {
                query = query.OrderByDescending(c => c.CreatedAt);
            } else
            {
                query = query.OrderBy(c => c.CreatedAt);
            }
            var totalPages = (int)Math.Ceiling((double)query.Count() / 10);
            var data = query.Skip((currPage - 1) * itemCount).Take(itemCount).ToList();

            return Results.Json(new
            {
                data = data.Select(c => new { c.Id, c.Title, c.Description, c.Price }),
                pagination = new
                {
                    page = page,
                    size = size,
                    totalPages = totalPages
                }
            });
        }
        [Authorize]
        [HttpGet("{id}")]
        public IResult Get(string id)
        {
            if(!int.TryParse(id, out var courseId))
            {
                return Helper.errMessage("Validation error: 'courseId' must be a numeric.");
            }
            var course = dbc.Courses.Include(c => c.Modules).Select(c => new {c.Id, c.Title, c.Description, c.Price, duration = c.DurationStr, modules = c.Modules.Select(m => m.Title)}).FirstOrDefault(c => c.Id == courseId);
            if(course is null)
            {
                return Helper.errMessage("Course not found.", 404);
            }
            return Results.Json(new
            {
                data = course
            });
        }

        [Authorize]
        [HttpPost("{id}/purchase")]
        public IResult Purchase(string id, PurchaseData purchaseData)
        {
            if (!int.TryParse(id, out var courseId))
            {
                return Helper.errMessage("Validation error: 'courseId' must be a numeric.");
            }
            var supportedPayments = new List<string> { "card", "debit_card", "paypal" };
            if (!supportedPayments.Contains(purchaseData.PaymentMethod))
            {
                return Helper.errMessage("Validation error: 'paymentMethod' must be 'card' or 'credit_card' or 'paypal'.");
            }
            var course = dbc.Courses.Include(c => c.Modules).Select(c => new { c.Id, c.Title, c.Description, c.Price, duration = c.DurationStr, modules = c.Modules.Select(m => m.Title) }).FirstOrDefault(c => c.Id == courseId);
            if (course is null)
            {
                return Helper.errMessage("Course not found.", 404);
            }
            var price = course.Price;
            var discountApplied = Convert.ToDecimal(0.0);
            int couponId = -1;
            if(purchaseData.CouponCode != "")
            {
                var coupon = dbc.Coupons.Include(c => c.Purchases).FirstOrDefault(c => c.Code == purchaseData.CouponCode);
                if(coupon is null)
                {
                    return Helper.errMessage("Coupon not found.", 404);
                }
                if(coupon.Quota == coupon.Purchases.Count || coupon.ExpiryDate < DateTime.Now)
                {
                    return Helper.errMessage("Coupon has expired or quota exceeded.", 404);
                }
                couponId = coupon.Id;
                discountApplied = coupon.DiscountPct;
                var scale = Convert.ToDecimal(100.0) - coupon.DiscountPct;
                price = price * scale;
            }
            if(!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return Helper.errMessage($"User id not valid.");
            }
            var purchase = new Purchase
            {
                UserId = userId,
                CouponId = couponId > 0 ? couponId : null,
                CourseId = course.Id,
                PricePaid = price,
                PaymentMethod = purchaseData.PaymentMethod,
            };
            dbc.Purchases.Add(purchase);
            dbc.SaveChanges();
            return Results.Json(new
            {
                message = "Course purchased successfully.",
                data = new
                {
                    purchaseId = purchase.Id,
                    courseId = courseId,
                    userId = userId,
                    purchaseDate = purchase.PurchasedAt,
                    originalPrice = course.Price,
                    discountApplied = discountApplied,
                    paidAmount = price
                }
            });
        }
    }
}

