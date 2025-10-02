using System;
using System.Collections.Generic;

namespace DBfirst.Models
{
    public partial class Class
    {
        public Class()
        {
            Feedbacks = new HashSet<Feedback>();
            Students = new HashSet<Student>();
        }

        public int ClassId { get; set; }
        public string ClassName { get; set; } = null!;
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }

        public virtual Subject Subject { get; set; } = null!;
        public virtual Teacher Teacher { get; set; } = null!;
        public virtual ICollection<Feedback> Feedbacks { get; set; }

        public virtual ICollection<Student> Students { get; set; }
    }
}
