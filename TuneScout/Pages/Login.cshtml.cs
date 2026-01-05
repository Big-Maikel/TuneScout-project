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

        public void OnGet() { }

        public IActionResult OnPost()
        {
            ModelState.Clear();
            if (!TryValidateModel(Login, nameof(Login)))
            {
                _logger.LogWarning("Login validation failed");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
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
                var verification = _passwordHasher.VerifyHashedPassword(user, user.Password ?? string.Empty, Login.Password);
                if (verification == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.Name ?? string.Empty);
                    
                    _logger.LogInformation("Login successful for user: {UserId}", user.Id);
                    return LocalRedirect(Url.Content("~/"));
                }
            }

            _logger.LogWarning("Login failed for email: {Email}", Login.Email);
            ErrorMessage = "Ongeldige e-mail of wachtwoord.";
            return Page();
        }

        public IActionResult OnPostRegister()
        {
            ModelState.Clear();
            if (!TryValidateModel(Register, nameof(Register)))
            {
                _logger.LogWarning("Register validation failed");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
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

            var created = _userService.GetAllUsers().FirstOrDefault(u => u.Email == Register.Email);
            if (created != null)
            {
                HttpContext.Session.SetInt32("UserId", created.Id);
                HttpContext.Session.SetString("UserName", created.Name ?? string.Empty);
                _logger.LogInformation("Registration successful, user logged in: {UserId}", created.Id);
                return LocalRedirect(Url.Content("~/"));
            }

            RegisterMessage = "Registratie gelukt! Je kunt nu inloggen.";
            return Page();
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