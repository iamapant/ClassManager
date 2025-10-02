using System;
using System.Collections.Generic;

namespace DBfirst.Models
{
    public partial class Subject
    {
        public Subject()
        {
            Teachers = new HashSet<Teacher>();
            StudentSubjects = new HashSet<StudentSubject>();
        }

        public int SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public virtual ICollection<StudentSubject> StudentSubjects { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<Teacher> Teachers { get; set; }

    }
}
