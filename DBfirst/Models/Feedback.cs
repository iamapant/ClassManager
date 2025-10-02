using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace DBfirst.Models
{
    public partial class Feedback
    {
        public int FeedbackId { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public int? Rating { get; set; }
        public string FeedbackText { get; set; } = null!;
        public DateTime CreatedDate { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
