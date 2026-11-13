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
    public class AuthController(IAuthService authService, IMapper mapper, ITokenProvider tokenProvider) : Controller
    {
        private readonly IAuthService _authService = authService; 
        private readonly IMapper _mapper = mapper; 

        private readonly ITokenProvider _tokenProvider = tokenProvider; 

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
                // Get the response
                var response = await _authService.LoginAsync<ApiResponse<TokenDTO>>(loginRequestDTO);

                if (response != null && response.Success && response.Data != null)
                {
                    var principal = _tokenProvider.CreatePrincipalFromJwtToken(response.Data.AccessToken);

                    if(principal != null)
                    {
                        // Sign the user in
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        _tokenProvider.SetToken(response.Data.AccessToken, response.Data.RefreshToken);

                        // Redirect to home when successfully logged in
                        return RedirectToAction(nameof(Index), "Home");
                    }
                    else
                    {
                        TempData["error"] = "Invalid token received. Please try again";
                    }
                }
                else
                {
                    TempData["error"] = response?.Message ?? "Invalid email or password.";
                }

                return View(loginRequestDTO);
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return View();
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
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }
    }
}
