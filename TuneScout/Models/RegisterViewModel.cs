using System.ComponentModel.DataAnnotations;

namespace TuneScout.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Naam is verplicht")]
        [Display(Name = "Naam")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail is verplicht")]
        [EmailAddress(ErrorMessage = "Ongeldig e-mailadres")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [Display(Name = "Wachtwoord")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bevestig wachtwoord is verplicht")]
        [Compare("Password", ErrorMessage = "Wachtwoorden komen niet overeen")]
        [Display(Name = "Bevestig wachtwoord")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
