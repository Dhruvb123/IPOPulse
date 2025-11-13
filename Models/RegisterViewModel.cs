using System.ComponentModel.DataAnnotations;

namespace IPOPulse.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Contact { get; set; }

        [Required]
        public string AgeGroup { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
                
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{8,}$",
         ErrorMessage = "Password must be at least 8 characters long, contain one uppercase letter, one lowercase letter, and one special character.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
