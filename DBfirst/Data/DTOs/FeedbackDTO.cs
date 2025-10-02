using System.ComponentModel.DataAnnotations;

namespace DBfirst.Data.DTOs
{
    public class FeedBackDTO
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int? Rating { get; set; }

        [Required(ErrorMessage = "Feedback text is required.")]
        public string FeedbackText { get; set; } = null!;
    }
}
