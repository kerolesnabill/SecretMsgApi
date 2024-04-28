using System.ComponentModel.DataAnnotations;

namespace SecretMsgApi.Models
{
    public class ChangeEmailModel
    {
        [Required, EmailAddress]
        public string NewEmail { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
