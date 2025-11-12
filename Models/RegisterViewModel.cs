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

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
