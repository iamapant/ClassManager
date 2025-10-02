using AutoMapper;
using DBfirst.Data.DTOs;
using DBfirst.Data.Roles;
using DBfirst.DataAccess;
using DBfirst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DBfirst.Controllers.StudentsController;

namespace DBfirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly Project_B5DBContext _context;
        private readonly IMapper _mapper;
        public StudentsController(Project_B5DBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Students
        [HttpGet]
        [Authorize(Roles = AppRole.Student + ", " + AppRole.Teacher + ", " + AppRole.Admin)]
        public IActionResult GetStudent()
        {
            var students = _context.Students
                                   .Select(s => new StudentDTOs
                                   {
                                       StudentId = s.StudentId,
                                       Name = s.Name
                                   })
                                   .ToList();

            return Ok(students);
        }

        public class StudentDTOs
        {
            public int StudentId { get; set; }
            public string Name { get; set; }
        }

        // GET: api/student/{studentId}
        [Authorize(Roles = AppRole.Student)]
        [HttpGet("{studentId}")]
        public async Task<ActionResult<StudentDTO>> GetStudentProfile(string studentId)
        {
            var student = await _context.Students
                .Include(s => s.StudentDetails)
                .Include(s => s.User)
                .Where(s => s.User.Id == studentId)
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound();
            }

            var studentDto = _mapper.Map<StudentDTO>(student);
            return Ok(studentDto);
        }

        [Authorize(Roles = AppRole.Student)]
        [HttpPut("{studentId}")]
        public async Task<IActionResult> UpdateStudent(int studentId, StudentDTO studentDto)
        {
            if (studentId != studentDto.StudentId)
            {
                return BadRequest("Student ID mismatch");
            }

            var student = await _context.Students
                .Include(s => s.StudentDetails) // Bao gồm StudentDetails nếu cần
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
            {
                return NotFound();
            }

            // Ánh xạ từ DTO vào entity
            _mapper.Map(studentDto, student);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(studentId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Trả về HTTP 204 No Content khi cập nhật thành công
        }



        // GET: api/student/{studentId}/scores
        [Authorize(Roles = AppRole.Student)]
        [HttpGet("{studentId}/scores")]
        public async Task<ActionResult<IEnumerable<EvaluationDTO>>> GetStudentScores(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.Evaluations)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
            {
                return NotFound();
            }
            try
            {
                var evaluationsDto = _mapper.Map<IEnumerable<EvaluationDTO>>(student.Evaluations);
                return Ok(evaluationsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("viewstudentdetail")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudentWithDetail()
        {
            var students = await _context.Students
            .Include(s => s.StudentDetails)
            .ToListAsync();

            var studentDtos = students.Select(student => new StudentDto
            {
                StudentId = student.StudentId,
                Name = student.Name,
                Age = student.Age,
                IsRegularStudent = student.IsRegularStudent,
                Address = student.StudentDetails.FirstOrDefault()?.Address,
                AdditionalInformation = student.StudentDetails.FirstOrDefault()?.AdditionalInformation
            }).ToList();

            return Ok(studentDtos);
        }

        // POST: api/Students
        [HttpPost("createstudent")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<ActionResult<Student>> CreateStudent(CreateStudentDto createStudentDto)
        {
            var student = _mapper.Map<Student>(createStudentDto);

            // Add the new student to the context
            _context.Students.Add(student);

            // Save changes to the database to generate the StudentId
            await _context.SaveChangesAsync();

            // Now that the StudentId is generated, update the StudentId in each StudentDetail
            foreach (var detail in student.StudentDetails)
            {
                detail.StudentId = student.StudentId;
            }

            // Save changes to the database to persist the StudentDetail updates
            await _context.SaveChangesAsync();

            // Return the newly created student object or a relevant response
            return Ok(new { student.StudentId, student.Name });
        }

        // PUT: api/Students/{id}
        [HttpPut("editstudent")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> EditStudent(int id, EditStudentDto studentDto)
        {
            var student = await _context.Students
                .Include(s => s.StudentDetails)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
            {
                return NotFound();
            }

            student.Name = studentDto.Name;
            student.Age = studentDto.Age;
            student.IsRegularStudent = studentDto.IsRegularStudent;

            var studentDetail = student.StudentDetails.FirstOrDefault();
            if (studentDetail != null)
            {
                studentDetail.Address = studentDto.Address;
                studentDetail.AdditionalInformation = studentDto.AdditionalInformation;
            }

            _context.Entry(student).State = EntityState.Modified;
            _context.Entry(studentDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
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

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }

        [HttpGet("student/{studentId}/classes")]
        [Authorize(Roles = AppRole.Student)]
        public async Task<IActionResult> GetClassesForStudent(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.Classes) // Đảm bảo bao gồm các lớp mà sinh viên đăng ký
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu sinh viên không tồn tại
            }

            var studentClasses = student.Classes.Select(c => new
            {
                ClassId = c.ClassId,
                ClassName = c.ClassName,
                TeacherId = c.TeacherId,
                SubjectId = c.SubjectId
            }).ToList();

            return Ok(studentClasses);
        }






        [HttpPost("AddStudentClass")]
        [Authorize(Roles = AppRole.Admin)]
        public IActionResult AddStudentClass([FromBody] AddStudentClassRequest request)
        {
            var classEntity = _context.Classes
                .FirstOrDefault(c => c.ClassId == request.ClassId);

            if (classEntity == null)
            {
                return NotFound("Class not found.");
            }

            var studentEntity = _context.Students
                .FirstOrDefault(s => s.StudentId == request.StudentId);

            if (studentEntity == null)
            {
                return NotFound("Student not found.");
            }

            if (classEntity.Students.Any(s => s.StudentId == request.StudentId))
            {
                return BadRequest("Student is already enrolled in this class.");
            }

            classEntity.Students.Add(studentEntity);

            try
            {
                _context.SaveChanges();
                return Ok($"Student {request.StudentId} has been added to class {request.ClassId}.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error adding student to class: {ex.Message}");
            }
        }





        [HttpPost("feedback/{StudentId}/{ClassId}")]
        [Authorize(Roles = AppRole.Student)]
        public IActionResult PostFeedBack(int StudentId, int ClassId, [FromForm] FeedBackDTO feedback)
        {
            // Kiểm tra xem sinh viên có tồn tại hay không
            var student = _context.Students
                .FirstOrDefault(s => s.StudentId == StudentId);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            // Kiểm tra xem sinh viên có tham gia lớp học này không
            var classStudent = _context.Classes
                .FirstOrDefault(c => c.ClassId == ClassId && c.Students.Any(s => s.StudentId == StudentId));

            if (classStudent == null)
            {
                return NotFound("Student is not enrolled in this class.");
            }

            // Kiểm tra xem sinh viên đã đánh giá lớp học này chưa
            var existingFeedback = _context.Feedbacks
                .FirstOrDefault(f => f.ClassId == ClassId && f.StudentId == StudentId);

            if (existingFeedback != null)
            {
                return BadRequest("Student has already provided feedback for this class.");
            }

            // Thêm đánh giá (feedback) cho lớp học
            var feedbackEntity = new Feedback
            {
                StudentId = StudentId,
                ClassId = ClassId,
                Rating = feedback.Rating,
                FeedbackText = feedback.FeedbackText,
                CreatedDate = DateTime.Now
            };

            _context.Feedbacks.Add(feedbackEntity);
            _context.SaveChanges();

            return Ok("Add thành công");
        }

    }
}