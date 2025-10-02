using DBfirst.Models;
using DBfirst.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Table;
using OfficeOpenXml;
using DBfirst.DataAccess;
using DBfirst.Data.Roles;
using Microsoft.AspNetCore.Authorization;

namespace DBfirst.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DashboardController : ControllerBase
	{
		private readonly Project_B5DBContext _context;

		public DashboardController(Project_B5DBContext context)
		{
			_context = context;
		}

        [Authorize(Roles = AppRole.Admin)]
        [HttpGet("subject_analysis")]
		public IActionResult GetSubjectAnalytics()
		{
			var subjectAnalytics = _context.Subjects
				.Select(s => new
				{
                    s.SubjectId,
					s.SubjectName,
					AverageGrade = _context.Evaluations
						.Where(e => e.AdditionExplanation == s.SubjectName)
						.Average(e => (double?)e.Grade) ?? 0
				})
				.ToList();

			return Ok(subjectAnalytics);
		}

        [HttpGet("student_analysis")]
        [Authorize(Roles = AppRole.Admin)]
        public IActionResult Top10AverageGrades()
        {
            var top10AverageGrades = _context.Students
                .Select(student => new
                {
                    StudentName = student.Name,
                    AverageGrade = _context.Evaluations
                        .Where(e => e.StudentId == student.StudentId)
                        .Average(e => (double?)e.Grade) ?? 0
                })
                .OrderByDescending(student => student.AverageGrade)
                .Take(10)
                .ToList();

            return Ok(top10AverageGrades);
        }

		[HttpGet("export_subject_analysis")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> ExportSubjectAnalysis(int subjectId)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            Subject subject = await _context.Subjects.FindAsync(subjectId);

            if (subject == null)
            {
                return NotFound($"Subject with ID {subjectId} not found.");
            }

            var students = await _context.Students
                .Include(s => s.Evaluations)
                .Include(s => s.StudentSubjects)
                .ThenInclude(s => s.Subject)
                .SelectMany(student => student.Evaluations
                    .Where(evaluation => student.StudentSubjects.Any(subject => subject.SubjectId == subjectId))
                    .Select(evaluation => new
                    {
                        StudentName = student.Name,
                        Grade = evaluation.Grade
                    }))
                .ToListAsync();

            if (students == null || !students.Any())
            {
                return NotFound($"No evaluations found for Subject ID {subjectId}.");
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(subject.SubjectName);

                worksheet.Cells[1, 1].Value = "Index";
                worksheet.Cells[1, 2].Value = "Student";
                worksheet.Cells[1, 3].Value = "Score";

                int row = 2;
                int count = 1;
                foreach (var student in students)
                {
                    worksheet.Cells[row, 1].Value = count;
                    worksheet.Cells[row, 2].Value = student.StudentName;
                    worksheet.Cells[row, 3].Value = student.Grade;
                    row++;
                    count++;
                }

                var range = worksheet.Cells[1, 1, row - 1, 3];
                var table = worksheet.Tables.Add(range, $"Table_{subject.SubjectName}");
                table.TableStyle = TableStyles.Medium9;

                worksheet.Cells.AutoFitColumns();
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                var fileName = $"SubjectAnalysis_{subject.SubjectName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}
