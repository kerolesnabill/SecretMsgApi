﻿using System.ComponentModel.DataAnnotations;

namespace SecretMsgApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }
        public int? Views { get; set; }
        public bool? Available { get; set; }
        public bool? ShowLastSeen { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
