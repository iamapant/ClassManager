using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DBfirst.Data
{
    public class PasswordValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult("Password is required.");
            }

            if (password.Length < 6)
            {
                return new ValidationResult("Password must be at least 6 characters long.");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return new ValidationResult("Password must have at least one uppercase ('A'-'Z').");
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return new ValidationResult("Password must have at least one digit ('0'-'9').");
            }

            if (!Regex.IsMatch(password, @"[\W_]")) 
            {
                return new ValidationResult("Password must have at least one non-alphanumeric character.");
            }

            return ValidationResult.Success;
        }
    }
}
