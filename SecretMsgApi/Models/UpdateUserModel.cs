using System.ComponentModel.DataAnnotations;

namespace SecretMsgApi.Models
{
    public class UpdateUserModel
    {
        [StringLength(50, MinimumLength = 1)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }
        public string? Image { get; set; }
        public bool? Available { get; set; }
        public bool? ShowLastSeen { get; set; }
    }
}
