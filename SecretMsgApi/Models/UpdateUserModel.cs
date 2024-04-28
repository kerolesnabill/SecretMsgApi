using System.ComponentModel.DataAnnotations;

namespace SecretMsgApi.Models
{
    public class UpdateUserModel
    {
        public int Id { get; set; }

        [StringLength(50, MinimumLength = 1)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }
        public string? Image { get; set; }
        public bool? NotAvailable { get; set; }
    }
}
