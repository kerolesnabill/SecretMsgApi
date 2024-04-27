using Microsoft.Data.SqlClient;
using SecretMsg.Services;
using SecretMsgApi.Models;
using System.Data;
using BCryptNet = BCrypt.Net.BCrypt;

namespace SecretMsgApi.Services
{
    public static class UserService
    {
        private static readonly IConfiguration _configuration = 
            new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static readonly string _constr = _configuration.GetSection("constr").Value!;

        public static (string? Error, string? Token) Register(User user)
        {
            if (GetUser(user.Email) != null)
                return ("This email is already registered", null);

            user.Password = BCryptNet.HashPassword(user.Password);

            using (var connection = new SqlConnection(_constr))
            {
                SqlCommand command = new SqlCommand("InsertUser", connection);
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter EmailParameter = new SqlParameter
                {
                    ParameterName = "Email",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = user.Email,
                };
                SqlParameter NameParameter = new SqlParameter
                {
                    ParameterName = "Name",
                    SqlDbType = SqlDbType.NVarChar,
                    Direction = ParameterDirection.Input,
                    Value = user.Name,
                };
                SqlParameter PasswordParameter = new SqlParameter
                {
                    ParameterName = "Password",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = user.Password,
                };
                command.Parameters.Add(NameParameter);
                command.Parameters.Add(EmailParameter);
                command.Parameters.Add(PasswordParameter);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                        if (reader.Read())
                            user.Id = (int) reader["UserId"];
                        else
                            throw new Exception();
                }
                catch (Exception) { return ("Failed to register", null); }
                finally { connection.Close(); }
            }

            string? token = null;
            if(user.Id != null)
                token = JwtService.GenerateToken(user.Id.ToString());

            return (null, token);
        }

        public static User? GetUser(string email)
        {
            User? user = null;

            using(var connection = new SqlConnection(_constr))
            {
                SqlCommand sqlCommand = new SqlCommand("GetUserByEmail", connection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "Email",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = email
                };
                sqlCommand.Parameters.Add(parameter);

                connection.Open();
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            Id = (int)reader["UserId"],
                            Email = reader["Email"].ToString()!,
                        };
                    }
                }
                connection.Close();
            }

            return user;
        }
    }

}
