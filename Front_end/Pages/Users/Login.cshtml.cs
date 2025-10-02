using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DBfirst.Data.DTOs;
using DBfirst.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Front_end.Pages
{
    public class LoginModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public LoginModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<string> Errors { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var loginRequest = new UserLoginRequestDto
                {
                    Email = Input.Email,
                    Password = Input.Password
                };

                var response = await _httpClient.PostAsJsonAsync("http://localhost:5224/api/Authentication/Login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var authResult = await response.Content.ReadFromJsonAsync<AuthResult>();

                    if (authResult != null && authResult.Result)
                    {
                        if (!string.IsNullOrEmpty(authResult.Token) && !string.IsNullOrEmpty(authResult.RefreshToken))
                        {
                            var cookieOptions = new CookieOptions
                            {
                                HttpOnly = false,
                                Secure = false,
                                Expires = DateTimeOffset.UtcNow.AddHours(1),
                                SameSite = SameSiteMode.Strict
                            };

                            HttpContext.Response.Cookies.Append("JwtToken", authResult.Token, cookieOptions);
                            HttpContext.Response.Cookies.Append("RefreshToken", authResult.RefreshToken, cookieOptions);
                        }

                        return RedirectToPage("/Index");
                    }
                    else
                    {
                        Errors = authResult?.Errors ?? new List<string> { "Login failed. Please check your credentials." };
                    }
                }
                else
                {
                    Errors = new List<string> { "Login failed. Please check your credentials." };
                }
            }
            return Page();
        }

    }
}