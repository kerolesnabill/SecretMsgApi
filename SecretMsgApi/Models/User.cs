using System.ComponentModel.DataAnnotations;

namespace SecretMsgApi.Models
{
    public class User
    {
        public int Id { get; set; }

        [EmailAddress, MaxLength(255)]
        public string? Email { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string? Username { get; set; }

        [StringLength(50, MinimumLength = 1)]
        public string? Name { get; set; }

        [StringLength(255, MinimumLength = 8)]
        public string? Password { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }
        public string? Image { get; set; }
        public int? Views { get; set; }
        public bool? NotAvailable { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime? CreatedAt { get; }
    }
}
