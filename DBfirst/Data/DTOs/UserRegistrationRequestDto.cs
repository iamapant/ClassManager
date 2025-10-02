using System.ComponentModel.DataAnnotations;

namespace DBfirst.Data.DTOs
{
    public class UserRegistrationRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [PasswordValidation(ErrorMessage = "Passwords must have at least one non alphanumeric character, one digit ('0'-'9') and one uppercase ('A'-'Z').")]
        public string Password { get; set; } = string.Empty;
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UserVerifyRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
