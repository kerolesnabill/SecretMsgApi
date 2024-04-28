using System.ComponentModel.DataAnnotations;

namespace SecretMsgApi.Models
{
    public class UsernameModel
    {
        [Required, StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }
    }
}
