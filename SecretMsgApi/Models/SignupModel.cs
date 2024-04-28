using System.ComponentModel.DataAnnotations;

namespace SecretMsgApi.Models
{
    public class SignupModel
    {
        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; }

        [Required, StringLength(50, MinimumLength = 1)]
        public string Name { get; set; }

        [Required, StringLength(255, MinimumLength = 8)]
        public string Password { get; set; }
    }
}
