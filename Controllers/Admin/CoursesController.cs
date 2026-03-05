using gsa_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            //if()
        }
    }
}
