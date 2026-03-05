using gsa_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace gsa_api.Controllers
{
    [Route("gsa-api/v1/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly GsaContext dbc;

        public TransactionsController(GsaContext ctx)
        {
            dbc = ctx;
        }

        [Authorize]
        [HttpGet]
        public IResult GetAll(string courseName = "", string sortBy = "asc", string userEmail = "", string page = "1", string size = "10")
        {
            if (!int.TryParse(page, out int currPage))
            {
                return Helper.errMessage("Validation error: 'page' must be a positive integer.");
            }
            if (currPage < 1)
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
            if (!dbc.Courses.Any(c => c.Title == courseName) && courseName.Trim() != "") return Helper.errMessage("Course name not found!.", 404);
            if (!User.IsInRole("admin") && userEmail.Trim() != "") return Helper.errMessage("This features require admin privileges!.", 403);
            var query = dbc.Purchases.Include(p => p.Course).Include(p => p.Coupon).Include(p => p.User).AsQueryable();
            if(courseName.Trim() != "")
            {
                query = query.Where(c => c.Course.Title == courseName);
            }
            if(userEmail.Trim() != "")
            {
                query = query.Where(c => c.User.Email == userEmail);
            }
            if (sortBy == "asc")
            {
                query = query.OrderBy(c => c.PurchasedAt);
            }
            else
            {
                query = query.OrderByDescending(c => c.PurchasedAt);
            }
            var totalPages = (int)Math.Ceiling((double)query.Count() / 10);
            var data = query.Skip((currPage - 1) * itemCount).Take(itemCount).ToList();
            if(User.IsInRole("admin"))
            {
                return Results.Json(new
                {
                    data = data.Select(p => new { transactionId = p.Id, userEmail = p.User.Email, courseId = p.CourseId, courseTitle = p.Course.Title, purchaseDate = p.PurchasedAt, amount = p.Course.Price, couponCode = p.Coupon?.Code ?? "", paidAmount = p.PricePaid }),
                    pagination = new
                    {
                        page = page,
                        size = size,
                        totalPages = totalPages
                    }
                });
            }
            return Results.Json(new
            {
                data = data.Select(p => new { transactionId = p.Id, courseId = p.CourseId, courseTitle = p.Course.Title, purchaseDate = p.PurchasedAt, amount = p.Course.Price, couponCode = p.Coupon?.Code ?? "", paidAmount = p.PricePaid }),
                pagination = new
                {
                    page = page,
                    size = size,
                    totalPages = totalPages
                }
            });
        }
    }
}
