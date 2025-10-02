using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Front_end.Pages
{
    public class ForgetPasswordModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ForgetPasswordModel(HttpClient httpClient)
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
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.GetAsync($"http://localhost:5224/api/Authentication/forget-password?email={Input.Email}");

                if (response.IsSuccessStatusCode)
                {
                    Message = "Please check your email for a password reset link.";
                }
                else
                {
                    ErrorMessage = "Failed to send password reset email. Please try again.";
                }
            }
            return Page();
        }
    }
}
