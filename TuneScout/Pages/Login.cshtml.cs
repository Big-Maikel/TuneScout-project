using Logic.Models;
using Logic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using TuneScout.Models;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace TuneScout.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(UserService userService, ILogger<LoginModel> logger)
        {
            _userService = userService;
            _passwordHasher = new PasswordHasher<User>();
            _logger = logger;
        }

        [BindProperty]
        public LoginViewModel Login { get; set; } = new LoginViewModel();

        [BindProperty]
        public RegisterViewModel Register { get; set; } = new RegisterViewModel();

        public string? ErrorMessage { get; set; }
        public string? RegisterMessage { get; set; }

        public void OnGet() 
        {
            if (TempData["SuccessMessage"] != null)
            {
                RegisterMessage = TempData["SuccessMessage"]?.ToString();
            }
        }

        public IActionResult OnPost()
        {
            var loginErrors = ModelState
                .Where(x => x.Key.StartsWith("Login."))
                .SelectMany(x => x.Value.Errors)
                .ToList();

            if (loginErrors.Any())
            {
                _logger.LogWarning("Login validation failed");
                foreach (var error in loginErrors)
                {
                    _logger.LogWarning("Validation error: {Error}", error.ErrorMessage);
                }
                ErrorMessage = "Vul alle velden correct in.";
                return Page();
            }

            _logger.LogInformation("Login attempt for email: {Email}", Login.Email);

            var user = _userService.GetAllUsers()
                .FirstOrDefault(u => u.Email == Login.Email);

            if (user != null)
            {
                bool passwordValid = false;

                var verification = _passwordHasher.VerifyHashedPassword(user, user.Password ?? string.Empty, Login.Password);
                if (verification == PasswordVerificationResult.Success || verification == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    passwordValid = true;
                }
                else if (user.Password == Login.Password)
                {
                    passwordValid = true;
                    _logger.LogWarning("User {Email} logged in with plain text password. Consider rehashing.", user.Email);
                }

                if (passwordValid)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.Name ?? string.Empty);
                    
                    _logger.LogInformation("Login successful for user: {UserId}", user.Id);
                    return RedirectToPage("/Index");
                }
            }

            _logger.LogWarning("Login failed for email: {Email}", Login.Email);
            ErrorMessage = "Ongeldige e-mail of wachtwoord.";
            return Page();
        }

        public IActionResult OnPostRegister()
        {
            var registerErrors = ModelState
                .Where(x => x.Key.StartsWith("Register."))
                .SelectMany(x => x.Value.Errors)
                .ToList();

            if (registerErrors.Any())
            {
                _logger.LogWarning("Register validation failed");
                foreach (var error in registerErrors)
                {
                    _logger.LogWarning("Validation error: {Error}", error.ErrorMessage);
                }
                RegisterMessage = "Vul alle velden correct in.";
                return Page();
            }

            _logger.LogInformation("Registration attempt for email: {Email}", Register.Email);

            var exists = _userService.GetAllUsers().Any(u => u.Email == Register.Email);
            if (exists)
            {
                _logger.LogWarning("Registration failed: email already exists {Email}", Register.Email);
                RegisterMessage = "E-mail is al in gebruik.";
                return Page();
            }

            var newUser = new User
            {
                Name = Register.Name,
                Email = Register.Email,
                Password = string.Empty
            };

            newUser.Password = _passwordHasher.HashPassword(newUser, Register.Password);

            _userService.CreateUser(newUser);
            _logger.LogInformation("User created successfully: {Email}", Register.Email);

            TempData["SuccessMessage"] = "Registratie gelukt! Je kunt nu inloggen.";
            return RedirectToPage("/Login");
        }

        public IActionResult OnGetLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}