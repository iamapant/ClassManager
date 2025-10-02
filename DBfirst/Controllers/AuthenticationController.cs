using DBfirst.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DBfirst.Data.DTOs;
using DBfirst.Data;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;
using DBfirst.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Http.Extensions;
using DBfirst.Helper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net;
using DBfirst.Services;
using Org.BouncyCastle.Asn1.Ocsp;
using DBfirst.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace DBfirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly Project_B5DBContext _context;
        private readonly IEmailHelper _emailHelper;
        private readonly IEmailTemplateReader _emailTemplateReader;
        private readonly EmailService _emailService;
        private readonly RoleManager<IdentityRole> _roleManager;
        //private readonly JwtConfig _jwtConfig;

        public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration,
            Project_B5DBContext context, TokenValidationParameters tokenValidationParameters, IEmailHelper emailHelper,
            IEmailTemplateReader emailTemplateReader, EmailService emailService, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
            _tokenValidationParameters = tokenValidationParameters;
            _emailHelper = emailHelper;
            _emailTemplateReader = emailTemplateReader;
            _emailService = emailService;
            _roleManager = roleManager;
            //_jwtConfig = jwtConfig;
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(3));
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto request)
        {
            if (ModelState.IsValid)
            {
                var userExist = await _userManager.FindByEmailAsync(request.Email);
                if (userExist != null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "An have account already exists associated with this email!"
                        }
                    });
                }
                var newUser = new User()
                {
                    Email = request.Email,
                    UserName = request.Email,
                    EmailConfirmed = false,
                    ActiveCode = CreateRandomToken(),
                    IsActive = true
                };

                var isCreated = await _userManager.CreateAsync(newUser, request.Password);
                if (isCreated.Succeeded)
                {
                    var userRole = await _userManager.AddToRoleAsync(newUser, "Student");
                    if (userRole.Succeeded)
                    {
                        await _emailService.SendEmailAsync(newUser.Email, newUser.ActiveCode);
                        return Ok("User created successfully!");
                    }
                    else
                    {
                        var errorMessages = string.Join(", ", isCreated.Errors.Select(e => e.Description));
                        return BadRequest(new AuthResult()
                        {
                            Errors = new List<string> { $"Server error: {errorMessages}" },
                            Result = false
                        });
                    }
                }
                else
                {
                    var errorMessages = string.Join(", ", isCreated.Errors.Select(e => e.Description));
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string> { $"Server error: {errorMessages}" },
                        Result = false
                    });
                }
            }
            return BadRequest();
        }

        [HttpPost("Verify")]
        public async Task<IActionResult> ConfirmEmail([FromBody] UserVerifyRequestDto request)
        {
            var user = _context.User.FirstOrDefault(s => s.Email == request.Email);
            if (user == null)
            {
                return BadRequest("Account isn't exist in the system");
            }

            if (user.EmailConfirmed)
            {
                return Ok("The email has already been confirmed");
            }
            if (user.ActiveCode == request.Code)
            {
                user.ActiveCode = null;
                user.EmailConfirmed = true;
                Student student = new Student
                {
                    Name = user.UserName,
                    AccountId = user.Id
                };
                _context.Students.Add(student);
                _context.User.Update(user);
                _context.SaveChanges();
                return Ok("Your account has been activated");
            }
            return BadRequest("Confirm email failed");
        }

        [HttpGet("forget-password")]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return BadRequest("Account is exist in the system");
            }
            string host = _configuration.GetValue<string>("ApplicationUrl");

            string tokenConfirm = await _userManager.GeneratePasswordResetTokenAsync(user);

            string encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenConfirm));
            string resetPasswordUrl = $"{host}Users/ResetPassword?email={email}&token={encodedToken}";

            string body = $"Please reset your password by clicking here: <a href=\"{resetPasswordUrl}\">link</a>";

            await _emailHelper.SendEmailAsync(new EmailRequest
            {
                To = user.Email,
                Subject = "Reset Password",
                Content = body
            });

            return Ok("Please check your email");
        }



        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user is null)
            {
                return BadRequest("Email does not exist");
            }
            if (string.IsNullOrEmpty(resetPasswordDto.Token))
            {
                return BadRequest("Token is invalid");
            }

            string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordDto.Token));

            var identityResult = await _userManager.ResetPasswordAsync(user, decodedToken, resetPasswordDto.Password);

            if (identityResult.Succeeded)
            {
                return Ok("Reset password successful");
            }
            else
            {
                var errorMessage = identityResult.Errors.FirstOrDefault()?.Description ?? "Reset password failed";
                return BadRequest(errorMessage);
            }
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginRequest)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(loginRequest.Email);
                if (existingUser == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string> { "Invalid payload" },
                        Result = false
                    });
                }

                if (!existingUser.EmailConfirmed)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string> { "Email needs to be confirmed" },
                        Result = false
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, loginRequest.Password);
                if (!isCorrect)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string> { "Invalid credentials" },
                        Result = false
                    });
                }

                var authResult = await GenerateJwtToken(existingUser);
                authResult.Roles = await _userManager.GetRolesAsync(existingUser);

                return Ok(authResult);
            }

            return BadRequest(new AuthResult()
            {
                Errors = new List<string> { "Invalid payload" },
                Result = false
            });
        }
        private async Task<AuthResult> GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);

            var userRoles = await _userManager.GetRolesAsync(user);
            var roleClaims = userRoles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            var claims = new List<Claim>
            {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
            }.Concat(roleClaims);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TimeSpan.Parse(_configuration.GetSection("JwtConfig:ExpiryTimeFrame").Value)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                Token = RandomStringGeneration(23), // generate a refresh token
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                IsRevoked = false,
                IsUsed = false,
                UserId = user.Id,
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResult()
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                Result = true
            };
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (ModelState.IsValid)
            {
                var result = await VerifyAndGenerateToken(tokenRequest);
                if (result == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>
                        {
                        "Invalid parameters"
                        },
                        Result = false
                    });
                }
                return Ok(result);

            }
            return BadRequest(new AuthResult()
            {
                Errors = new List<string>
                {
                    "Invalid parameters"
                },
                Result = false
            });
        }

        private async Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            try
            {
                _tokenValidationParameters.ValidateLifetime = false;

                var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                    if (result == false)
                    {
                        return null;
                    }
                }
                var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

                if (expiryDate > DateTime.Now)
                {
                    return new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Expired token"
                        }
                    };
                }
                var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);
                if (storedToken == null)
                {
                    return new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Invalid token"
                        }
                    };
                }
                if (storedToken.IsUsed)
                {
                    return new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Invalid token"
                        }
                    };
                }
                if (storedToken.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Invalid token"
                        }
                    };
                }
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    return new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Invalid token"
                        }
                    };
                }
                if (storedToken.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>
                        {
                            "Expired token"
                        }
                    };
                }
                storedToken.IsUsed = true;
                _context.RefreshTokens.Update(storedToken);
                await _context.SaveChangesAsync();

                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
                return await GenerateJwtToken(dbUser);

            }
            catch (Exception ex)
            {
                return new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>
                        {
                            "Server error"
                        }
                };
            }

        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTimeVal;
        }

        /*private bool sendEmail(string body, string email)
        {
            var client = new RestClient("https://api.mailgun.net/v3");
            var request = new RestRequest("", Method.Post);
            client.Authenticator = new HttpBasicAuthenticator("api", _configuration.GetSection("EmailConfig:API_KEY").Value);

            request.AddParameter("domain", "sandboxbad99ec661494b92b9f3216339249870.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Excited User <mailgun@sandboxbad99ec661494b92b9f3216339249870.mailgun.org>");
            request.AddParameter("to", email);
            request.AddParameter("subject", "Email verification");
            request.AddParameter("text", body);
            request.Method = Method.Post;

            var response = client.Execute(request);
            return response.IsSuccessful;
        }*/
        /*private bool sendEmail(string body, string email)
        {
            var client = new RestClient("https://api.mailgun.net/v3/sandboxbad99ec661494b92b9f3216339249870.mailgun.org/messages");

            var request = new RestRequest("",Method.Post);

            // Add authentication header
            var apiKey = _configuration.GetSection("EmailConfig:API_KEY").Value;
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{apiKey}"));
            request.AddHeader("Authorization", $"Basic {authValue}");

            // Add email parameters
            request.AddParameter("from", "Excited User <mailgun@sandboxbad99ec661494b92b9f3216339249870.mailgun.org>");
            request.AddParameter("to", "dunghoangf7.yhd@gmail.com");
            request.AddParameter("subject", "Email verification");
            request.AddParameter("text", body);

            // Execute the request
            var response = client.Execute(request);
            return response.IsSuccessful;
        }*/

        private string RandomStringGeneration(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLNMOPQRSTUVWXYZ1234567890abcdefghijklnmopqrstuvwxyz_";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
