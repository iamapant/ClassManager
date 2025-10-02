using DBfirst.DataAccess;
using DBfirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.EntityFrameworkCore;
using DBfirst.Data.Roles;
using Microsoft.AspNetCore.Authorization;

namespace DBfirst.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectsController : ControllerBase
    {
        private readonly Project_B5DBContext _context;

        public SubjectsController(Project_B5DBContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        [Authorize(Roles = AppRole.Admin)]
        public IActionResult GetAll()
        {
            if (_context.Subjects == null)
            {
                return NotFound();
            }
            return Ok(_context.Subjects
                .AsQueryable());
        }

        // GET: api/subject
        [HttpGet("viewallsubject")]
        [Authorize(Roles = AppRole.Admin + ", " + AppRole.Teacher)]
        public async Task<ActionResult<IEnumerable<Subject>>> GetSubjects()
        {
            var subs = await _context.Subjects
                                    .Select(s => new
                                    {
                                        s.SubjectId,
                                        s.SubjectName
                                    }).ToListAsync();
            return Ok(subs);
        }

        // POST: api/subject
        [HttpPost("addsubject")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> AddSubject([FromBody] string subjectName)
        {
            if (string.IsNullOrEmpty(subjectName))
            {
                return BadRequest("Subject name is required.");
            }

            if (await _context.Subjects.AnyAsync(s => s.SubjectName == subjectName))
            {
                return Conflict("A subject with this name already exists.");
            }

            var subject = new Subject
            {
                SubjectName = subjectName
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubjects), new { id = subject.SubjectId }, subject);
        }

        // PUT: api/subject/{id}
        [HttpPut("editsubject")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] string subjectName)
        {
            if (string.IsNullOrEmpty(subjectName))
            {
                return BadRequest("Subject name is required.");
            }

            if (await _context.Subjects.AnyAsync(s => s.SubjectName == subjectName && s.SubjectId != id))
            {
                return Conflict("A subject with this name already exists.");
            }

            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            subject.SubjectName = subjectName;
            _context.Subjects.Update(subject);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
