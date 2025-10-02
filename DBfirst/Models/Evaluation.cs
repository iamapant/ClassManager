using System;
using System.Collections.Generic;

namespace DBfirst.Models
{
    public partial class Evaluation
    {
        public int EvaluationId { get; set; }
        public int Grade { get; set; }
        public string? AdditionExplanation { get; set; }
        public int? StudentId { get; set; }

        public virtual Student? Student { get; set; }
    }
}
