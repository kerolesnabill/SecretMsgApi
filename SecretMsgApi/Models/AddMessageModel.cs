using System.ComponentModel.DataAnnotations;

namespace SecretMsgApi.Models
{
    public class AddMessageModel
    {
        [Required]
        public int? UserId { get; set; }

        [Required, StringLength(1000, MinimumLength = 1)]
        public string Body { get; set; }
    }
}
