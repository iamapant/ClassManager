using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Linq;
using System.Threading.Tasks;
using DBfirst.Models;
using DBfirst.Data.DTOs;
using DBfirst.DataAccess;
using DBfirst.Data.Roles;
using Microsoft.AspNetCore.Authorization;

namespace DBfirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly Project_B5DBContext _projectContext;

        public ClassController(Project_B5DBContext projectContext)
        {
            _projectContext = projectContext;
        }

        [EnableQuery]
        [HttpGet]
        [Authorize(Roles = AppRole.Admin)]
        public IActionResult Get()
        {
            var list = _projectContext.Classes.Select(s => new
            {
                ClassId = s.ClassId,
                ClassName = s.ClassName,
                TeacherId = s.TeacherId,
                SubjectId = s.SubjectId
            });

            return Ok(list);
        }


        [HttpPost]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> PostClass([FromBody] ClassDTO newClass)
        {
            if (newClass == null)
            {
                return BadRequest("Invalid class data.");
            }

            var classEntity = new Class
            {
                ClassName = newClass.ClassName,
                TeacherId = newClass.TeacherId,
                SubjectId = newClass.SubjectId
            };

            _projectContext.Classes.Add(classEntity);

            try
            {
                await _projectContext.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = classEntity.ClassId }, classEntity); // Assuming 'Id' is the primary key
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
