using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DBfirst.Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Front_end.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ResetPasswordModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string Message { get; set; }
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string Email { get; set; }

            [Required]
            public string Token { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Invalid password reset request.";
                return Page();
            }

            Input = new InputModel
            {
                Email = email,
                Token = token
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resetPasswordDto = new ResetPasswordDto
            {
                Email = Input.Email,
                Token = Input.Token,
                Password = Input.Password
            };

            var response = await _httpClient.PostAsJsonAsync("http://localhost:5224/api/Authentication/reset-password", resetPasswordDto);

            if (response.IsSuccessStatusCode)
            {
                Message = "Your password has been reset successfully!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Error: {error}";
            }

            return Page();
        }
    }
}
