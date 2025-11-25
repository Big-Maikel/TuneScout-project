using System.Linq;
using System.ComponentModel.DataAnnotations;
using Logic.Models;
using Logic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;

namespace TuneScout.Pages
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;
        private readonly PasswordHasher<User> _passwordHasher;

        public LoginModel(UserService userService)
        {
            _userService = userService;
            _passwordHasher = new PasswordHasher<User>();
        }

        [BindProperty]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "Wachtwoord")]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "Naam")]
        public string RegisterName { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "E-mail")]
        public string RegisterEmail { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "Wachtwoord")]
        public string RegisterPassword { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? RegisterMessage { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            var user = _user_service_get_all_users()
                .FirstOrDefault(u => u.Email == Email);

            if (user != null)
            {
                var verification = _passwordHasher.VerifyHashedPassword(user, user.Password ?? string.Empty, Password);
                if (verification == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.Name ?? string.Empty);

                    return LocalRedirect(Url.Content("~/"));
                }
            }

            ErrorMessage = "Ongeldige e-mail of wachtwoord.";
            return Page();
        }

        public IActionResult OnPostRegister()
        {
            if (string.IsNullOrWhiteSpace(RegisterName) ||
                string.IsNullOrWhiteSpace(RegisterEmail) ||
                string.IsNullOrWhiteSpace(RegisterPassword))
            {
                RegisterMessage = "Vul alle velden in.";
                return Page();
            }

            var exists = _user_service_get_all_users().Any(u => u.Email == RegisterEmail);
            if (exists)
            {
                RegisterMessage = "E-mail is al in gebruik.";
                return Page();
            }

            var newUser = new User
            {
                Name = RegisterName,
                Email = RegisterEmail,
                Password = string.Empty
            };

            newUser.Password = _passwordHasher.HashPassword(newUser, RegisterPassword);

            _userService.CreateUser(newUser);

            var created = _user_service_get_all_users().FirstOrDefault(u => u.Email == RegisterEmail);
            if (created != null)
            {
                HttpContext.Session.SetInt32("UserId", created.Id);
                HttpContext.Session.SetString("UserName", created.Name ?? string.Empty);
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

        private System.Collections.Generic.IEnumerable<User> _user_service_get_all_users() => _userService.GetAllUsers();
    }
}