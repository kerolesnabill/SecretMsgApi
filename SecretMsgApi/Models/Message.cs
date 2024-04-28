namespace SecretMsgApi.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
