using DBfirst.Data.Roles;
using DBfirst.DataAccess;
using DBfirst.Models;
using DBfirst.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;

namespace DBfirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluationsController : ControllerBase
    {
        private readonly Project_B5DBContext _context;
        private readonly EmailService _emailService;

        public EvaluationsController(Project_B5DBContext context, EmailService service)
        {
            _context = context;
            _emailService = service;
        }

        [Authorize(Roles = AppRole.Teacher)]
        [HttpPost("/SendGrade")]
        public async Task<IActionResult> SendGrade()
        {
            List<Student> students = _context.Students
                .Include(s => s.User)
                .Include(s => s.Evaluations)
                .Where(s => s.User.Email != null)
                .ToList();
            foreach (Student student in students)
            {
                await _emailService.SendGradeAsync(student.User.Email, student);
            }
            return Ok("Send grade to all students successfully!");
        }

        [HttpGet]
        [EnableQuery]
        [Authorize(Roles = AppRole.Teacher)]
        public IQueryable<EvaluationDTOs> Get()
        {
            return _context.Evaluations
                           .Include(e => e.Student)
                           .Select(e => new EvaluationDTOs
                           {
                               EvaluationId = e.EvaluationId,
                               Grade = e.Grade,
                               AdditionExplanation = e.AdditionExplanation,
                               StudentId = e.StudentId.HasValue ? e.StudentId.Value : 0, // Trả về StudentId
                               StudentName = e.Student != null ? e.Student.Name : null // Trả về StudentName
                           })
                           .AsQueryable();
        }

        public class EvaluationDTOs
        {
            public int EvaluationId { get; set; }
            public int Grade { get; set; }
            public string AdditionExplanation { get; set; }
            public int StudentId { get; set; } // Thêm StudentId
            public string StudentName { get; set; } // Thêm StudentName
        }

        [HttpGet("{id}")]
        [EnableQuery]
        [Authorize(Roles = AppRole.Teacher)]
        public IActionResult GetById([FromRoute] int id)
        {
            var result = _context.Evaluations.Where(e => e.EvaluationId == id);
            if (!result.Any())
            {
                return NotFound();
            }
            return Ok(SingleResult.Create(result));
        }

        // POST: api/Evaluations
        [HttpPost]
        [Authorize(Roles = AppRole.Teacher)]
        public IActionResult Post([FromBody] EvaluationPostDTO evaluation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = new Evaluation
            {
                Grade = evaluation.Grade,
                AdditionExplanation = evaluation.AdditionExplanation,
                StudentId = evaluation.StudentId // Gán StudentId
            };

            _context.Evaluations.Add(result);
            _context.SaveChanges();

            return Ok("Create successfully");
        }

        public class EvaluationPostDTO
        {
            public int EvaluationId { get; set; }
            public int Grade { get; set; }
            public string AdditionExplanation { get; set; }
            public int StudentId { get; set; }
        }



        // PUT: api/Evaluations/5
        [HttpPut("{id}")]
        [Authorize(Roles = AppRole.Teacher)]
        public IActionResult PutById([FromRoute] int id, [FromBody] EvaluationPutDto evaluationDto)
        {
            if (evaluationDto == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            var evaluation = _context.Evaluations.Find(id);

            if (evaluation == null)
            {
                return NotFound();
            }

            if (evaluationDto.Grade < 0 || evaluationDto.Grade > 10)
            {
                return BadRequest("Grade is in range of 0 - 10.");
            }

            evaluation.Grade = evaluationDto.Grade;

            evaluation.AdditionExplanation = evaluationDto.AdditionExplanation;

            if (evaluationDto.StudentId == null)
            {
                return BadRequest("StudentId is required.");
            }

            evaluation.StudentId = evaluationDto.StudentId;

            _context.Entry(evaluation).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Evaluations.Any(e => e.EvaluationId == id))
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


        public class EvaluationPutDto
        {
            public int Grade { get; set; }
            public string? AdditionExplanation { get; set; }
            public int? StudentId { get; set; }
        }


        // DELETE: api/Evaluations/5
        [HttpDelete("{id}")]
        [Authorize(Roles = AppRole.Teacher)]
        public IActionResult DeleteById([FromRoute] int id)
        {
            var evaluation = _context.Evaluations.Find(id);
            if (evaluation == null)
            {
                return NotFound();
            }

            _context.Evaluations.Remove(evaluation);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
