using gsa_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace gsa_api.Controllers.Admin
{
    [Route("gsa-api/v1/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly GsaContext dbc;

        public CoursesController(GsaContext _dbc)
        {
            dbc = _dbc;
        }

        [Authorize]
        [HttpPost]
        public IResult Create(CourseInput course)
        {
            if (!User.IsInRole("admin")) return Helper.errMessage("Access denied. Admin role required.", 403);

            if(!course.isPriceValid)
            {
                return Helper.errMessage("Validation error: price must be more than zero");
            }
            if(!course.isDurationValid)
            {
                return Helper.errMessage("Validation error: duration must be more than zero");
            }
            if(course.modules.Count < 3)
            {
                return Helper.errMessage("Validation error: modules must contains 3 item or more");
            }
            var courseRec = course.toCourse();
            dbc.Courses.Add(courseRec);
            dbc.SaveChanges();
            return Results.Json(new
            {
                message = "Course created successfully",
                data = new
                {
                    courseId = courseRec.Id,
                    title = courseRec.Title,
                    description = courseRec.Description,
                    price = courseRec.Price,
                    duration = courseRec.DurationStr,
                    modules = courseRec.Modules.Select(m => m.Title)
                }
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public IResult Update(int id, CourseInput input)
        {
            if (!User.IsInRole("admin")) return Helper.errMessage("Access denied. Admin role required.", 403);

            if (!input.isPriceValid)
            {
                return Helper.errMessage("Validation error: price must be more than zero");
            }
            if (!input.isDurationValid)
            {
                return Helper.errMessage("Validation error: duration must be more than zero");
            }
            if (input.modules.Count < 3)
            {
                return Helper.errMessage("Validation error: modules must contains 3 item or more");
            }
            if (!dbc.Courses.Any(c => c.Id == id)) return Helper.errMessage("Course not found", 404);
            else
            {
                var course = dbc.Courses.Include(c => c.Modules).First(c => c.Id == id);
                if(course.Title != input.title) course.Title = input.title;
                if(course.Description != input.description) course.Description = input.description;
                if (course.Duration != input.duration) course.Duration = input.duration;
                if(Math.Abs(course.Price - input.price) > 0.000001m) course.Price = input.price;
                course.updateModules(input.modules);
                dbc.SaveChanges();
                return Results.Json(new
                {
                    message = "Course updated successfully",
                    data = new
                    {
                        courseId = course.Id,
                        title = course.Title,
                        description = course.Description,
                        price = course.Price,
                        duration = course.DurationStr,
                        modules = course.Modules.Select(m => m.Title)
                    }
                });
            }
        }
    }
}
