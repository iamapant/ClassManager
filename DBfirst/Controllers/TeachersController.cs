using DBfirst.Data.Roles;
using DBfirst.DataAccess;
using DBfirst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace DBfirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private readonly Project_B5DBContext _context;

        public TeachersController(Project_B5DBContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        [Authorize(Roles = AppRole.Admin)]
        public IActionResult Get()
        {
            if (_context.Teachers == null)
            {
                return NotFound();
            }
            return Ok(_context.Teachers
                .AsQueryable());
        }

        [HttpPost("create")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<ActionResult<Teacher>> CreateTeacher(TeacherPostDTO teacher)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(teacher);
            }
            if (_context.Teachers == null)
            {
                return Problem("Entity set 'Teachers' is null.");
            }

            Teacher newTeacher = new Teacher
            {
                Name = teacher.Name,
                Age = teacher.Age,
                SubjectId = teacher.SubjectId,
            };
            _context.Teachers.Add(newTeacher);
            _context.SaveChanges();
            return CreatedAtAction("Get", new { id = newTeacher.TeacherId }, teacher);
        }

        [HttpPut("edit/{id}")]
        [Authorize(Roles = AppRole.Admin)]
        public IActionResult PutById([FromRoute] int id, [FromBody] TeacherPostDTO teacherDto)
        {
            if (teacherDto == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            var teacher = _context.Teachers.Find(id);

            if (teacher == null)
            {
                return NotFound();
            }

            teacher.Name = teacherDto.Name;
            teacher.Age = teacherDto.Age;

            if (teacherDto.SubjectId == null)
            {
                return BadRequest("SubjectId is required.");
            }

            teacher.SubjectId = teacherDto.SubjectId;

            _context.Entry(teacher).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Teachers.Any(e => e.TeacherId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = AppRole.Admin)]
        public IActionResult DeleteById([FromRoute] int id)
        {
            var teacher = _context.Teachers.Find(id);
            if (teacher == null)
            {
                return NotFound();
            }

            _context.Teachers.Remove(teacher);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpPost("ChangeSubject")]
        [Authorize(Roles = AppRole.Teacher)]
        public IActionResult ChangeSubject([FromODataUri] int teacherId, [FromForm] int subjectId)
        {
            var teacher = _context.Teachers.SingleOrDefault(t => t.TeacherId == teacherId);

            if (teacher == null)
            {
                return NotFound();
            }

            teacher.SubjectId = subjectId;
            _context.SaveChanges();

            return Ok(teacher);
        }

        public class TeacherPostDTO
        {
            public string Name { get; set; } = null!;
            public int? Age { get; set; }
            public int? SubjectId { get; set; }
        }
    }
}