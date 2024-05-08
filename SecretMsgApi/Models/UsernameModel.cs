using System.ComponentModel.DataAnnotations;

namespace SecretMsgApi.Models
{
    public class UsernameModel
    {
        [Required, StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username must contain only alphabets and numbers")]
        public string Username { get; set; }
    }
}
