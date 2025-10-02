namespace DBfirst.Data.DTOs
{
    public class StudentDTO
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public int? Age { get; set; }
        public bool? IsRegularStudent { get; set; }
        public List<StudentDetailDTO> StudentDetails { get; set; }

    }
    public class StudentDto
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public int? Age { get; set; }
        public bool? IsRegularStudent { get; set; }
        public string? Address { get; set; }
        public string? AdditionalInformation { get; set; }
    }
    public class EditStudentDto
    {
        public string Name { get; set; } = null!;
        public int? Age { get; set; }
        public bool? IsRegularStudent { get; set; }
        public string? Address { get; set; }
        public string? AdditionalInformation { get; set; }
    }
    public class CreateStudentDto
    {
        public string Name { get; set; } = null!;
        public int? Age { get; set; }
        public bool? IsRegularStudent { get; set; }
        public List<StudentDetailDto> StudentDetails { get; set; } = new List<StudentDetailDto>();
    }

    public class StudentDetailDto
    {
        public string? Address { get; set; }
        public string? AdditionalInformation { get; set; }
    }

}
