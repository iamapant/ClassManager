using System.ComponentModel.DataAnnotations;

namespace DBfirst.Data.DTOs
{
    public class ClassDTO
    {
        [Key]
        public int ClassId { get; set; }
        public string ClassName { get; set; } = null!;
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }
    }
}
