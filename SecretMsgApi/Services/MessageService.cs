using Microsoft.Data.SqlClient;
using SecretMsgApi.Models;
using System.Data;

namespace SecretMsgApi.Services
{
    public static class MessageService
    {
        private static readonly IConfiguration _configuration =
            new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static readonly string _constr = _configuration.GetSection("constr").Value!;

        public static string? AddMessage(int userId, string body)
        {
            if (!UserService.HasUser(userId))
                return "There is no user with this Id.";

            using (var connection = new SqlConnection(_constr))
            {
                SqlCommand command = new SqlCommand("InsertMessage", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("UserId", userId);
                command.Parameters.AddWithValue("Body", body);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch { return "Error while adding the message."; }
                finally { connection.Close(); }
            }

            return null;
        }

        public static (string? Error, List<Message>? Messages) GetMessages(int userId)
        {
            if (!UserService.HasUser(userId))
                return ("There is no user with this Id.", null);

            var messages = new List<Message>();

            using(var connection = new SqlConnection(_constr))
            {
                string sql = "SELECT * FROM Messages WHERE UserId = @UserId";
                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("UserId", userId);

                try
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                        if (reader.Read())
                            foreach (var message in reader)
                                messages.Add(new Message
                                {
                                    Id = (int)reader["MessageId"],
                                    UserId = userId,
                                    Body = reader["Body"].ToString()!,
                                    CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()!)
                                });
                }
                catch { return ("Error while get messages.", null); }
                finally { connection.Close(); }
            }

            return (null, messages);
        }

        public static (string? Error, Message? Message) GetMessage(int userId, int messageId)
        {
            Message message = null;

            using (var connection = new SqlConnection(_constr))
            {
                string sql = "SELECT * FROM Messages WHERE MessageId = @MessageId";
                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("MessageId", messageId);

                try
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                        if (reader.Read())
                            message = new Message
                            {
                                Id = (int)reader["MessageId"],
                                UserId = (int)reader["UserId"],
                                Body = reader["Body"].ToString()!,
                                CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()!)
                            };
                        else
                            return ("Thers is no message with this Id.", null);
                }
                catch { return ("Error while get messages.", null); }
                finally { connection.Close(); }
            }

            if(message.UserId != userId)
                return ("Thers is no message with this Id.", null);

            return (null, message);
        }

        public static string? DeleteMessage(int userId, int messageId)
        {
            var (error, message) = GetMessage(userId, messageId);
            if (message is null) return error;

            if (message.UserId != userId)
                return "There is no message with this Id.";

            using (var connection = new SqlConnection(_constr))
            {
                string sql = "DELETE Messages FROM Messages WHERE MessageId = @MessageId";
                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("MessageId", messageId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch { return "Error while deleting the message"; }
                finally { connection.Close(); }
            }

            return null;
        }
    }
}
