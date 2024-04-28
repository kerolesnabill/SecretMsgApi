using System.ComponentModel.DataAnnotations;

namespace SecretMsgApi.Models
{
    public class ChangePasswordModel
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required, StringLength(255, MinimumLength = 8)]
        public string NewPassword { get; set; }
    }
}
