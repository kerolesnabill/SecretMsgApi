using Microsoft.Data.SqlClient;
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
    }
}
