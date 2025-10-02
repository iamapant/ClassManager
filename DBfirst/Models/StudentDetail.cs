using System;
using System.Collections.Generic;

namespace DBfirst.Models
{
    public partial class StudentDetail
    {
        public int StudentDetailsId { get; set; }
        public string? Address { get; set; }
        public string? AdditionalInformation { get; set; }
        public int? StudentId { get; set; }

        public virtual Student? Student { get; set; }
    }
}
