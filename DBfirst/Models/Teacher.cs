namespace DBfirst.Models
{
    public partial class Teacher
    {
        public int TeacherId { get; set; }
        public string Name { get; set; } = null!;
        public int? Age { get; set; }
        public int? SubjectId { get; set; }
        public string? AccountId { get; set; }

        public virtual User User { get; set; }
        public virtual Subject? Subject { get; set; }
        public virtual ICollection<Class> Classes { get; set; }

    }
}
