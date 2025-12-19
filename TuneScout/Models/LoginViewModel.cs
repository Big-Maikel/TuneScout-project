using System.ComponentModel.DataAnnotations;

namespace TuneScout.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-mail is verplicht")]
        [EmailAddress(ErrorMessage = "Ongeldig e-mailadres")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [Display(Name = "Wachtwoord")]
        public string Password { get; set; } = string.Empty;
    }
}