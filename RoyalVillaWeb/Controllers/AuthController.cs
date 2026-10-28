using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVillaWeb.Services;

namespace RoyalVillaWeb.Controllers
{
    public class AuthController(IAuthService authService, IMapper mapper) : Controller
    {
        private readonly IAuthService _authService = authService; 
        private readonly IMapper _mapper = mapper; 

        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var response = await _authService
                    .LoginAsync<ApiResponse<LoginResponseDTO>>(loginRequestDTO);

                if (response != null && response.Success && response.Data != null)
                {
                    LoginResponseDTO loginResponseDTO = response.Data;

                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(loginResponseDTO.Token);

                    // Read the email and role claims from the JWT
                    var email = jwt.Claims.FirstOrDefault(c =>
                        c.Type == ClaimTypes.Email || c.Type == "email")?.Value;

                    var role = jwt.Claims.FirstOrDefault(c =>
                        c.Type == ClaimTypes.Role || c.Type == "role")?.Value;

                    // Make sure the required claims exist
                    if (string.IsNullOrWhiteSpace(email) ||
                        string.IsNullOrWhiteSpace(role))
                    {
                        TempData["error"] = "Login failed: email or role claim is missing.";
                        return View(loginRequestDTO);
                    }

                    // Create the identity for cookie authentication
                    var identity = new ClaimsIdentity(
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    identity.AddClaim(new Claim(ClaimTypes.Email, email));
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));

                    var principal = new ClaimsPrincipal(identity);

                    // Sign the user in
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal);

                    return RedirectToAction(nameof(Index), "Home");
                }

                TempData["error"] = response?.Message ?? "Invalid email or password.";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return View(loginRequestDTO);
        }

        public IActionResult Register()
        {
            return View(new RegistrationRequestDTO
            {
                Email = string.Empty,
                Name = string.Empty,
                Password = string.Empty, 
                Role = "Customer"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            try
            {
                var response = await _authService.RegisterAsync<ApiResponse<UserDTO>>(registrationRequestDTO);
                if(response != null && response.Success && response.Data != null)
                {
                    UserDTO model = response.Data;
                    TempData["success"] = "Registration successfull! Please login with your credentials.";
                    
                    return RedirectToAction("Login");
                }
                else
                {
                    TempData["error"] = response?.Message ?? "Registration failed. Please try again";
                    return View(registrationRequestDTO);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }
            return View();
        }

        
        public IActionResult AccessDenied()
        {
            return View();
        }


        public async Task<IActionResult> Logout()
        {
            return View();
        }
    }
}
