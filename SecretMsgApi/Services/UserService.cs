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

        public static string? Login(string email, string password)
        {
            User? user = GetUser(email);
            if (user is null) return null;

            bool isVaild = BCryptNet.Verify(password, user.Password);
            if (!isVaild) return null;

            return JwtService.GenerateToken(user.Id.ToString());
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
                            Password = reader["Password"].ToString()!,
                        };
                    }
                }
                connection.Close();
            }

            return user;
        }

        public static User? GetUser(int id)
        {
            User? user = null;

            using (var connection = new SqlConnection(_constr))
            {
                string sql = "SELECT UserId, Email, Password From Users WHERE UserId = @UserId";
                SqlCommand sqlCommand = new SqlCommand(sql, connection);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Parameters.AddWithValue("UserId", id);

                connection.Open();
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            Id = (int)reader["UserId"],
                            Email = reader["Email"].ToString()!,
                            Password = reader["Password"].ToString()!,
                        };
                    }
                }
                connection.Close();
            }

            return user;
        }

        public static bool HasUser(int userId)
        {
            using (var connection = new SqlConnection(_constr))
            {
                string sql = "SELECT UserId From Users WHERE UserId = @Id";
                SqlCommand sqlCommand = new SqlCommand(sql, connection); ;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Parameters.AddWithValue("Id", userId);

                connection.Open();
                try
                {
                    if (sqlCommand.ExecuteReader().Read())
                        return true;
                }
                catch { return false; }
                finally { connection.Close(); }
            }
            return false;
        }

        public static bool HasUser(string email)
        {
            using (var connection = new SqlConnection(_constr))
            {
                string sql = "SELECT UserId From Users WHERE Email = @Email";
                SqlCommand sqlCommand = new SqlCommand(sql, connection); ;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Parameters.AddWithValue("Email", email);

                connection.Open();
                try
                {
                    if (sqlCommand.ExecuteReader().Read())
                        return true;
                }
                catch { return false; }
                finally { connection.Close(); }
            }
            return false;
        }

        public static (string? Error, string? Message) UpdateUser(User user)
        {
            if (!HasUser(user.Id))
                return ("There is no user with this Id", null);

            using(var connection = new SqlConnection(_constr))
            {
                SqlCommand sqlCommand = new SqlCommand("UpdateUser", connection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("UserId", user.Id);
                sqlCommand.Parameters.AddWithValue("Name", user.Name);
                sqlCommand.Parameters.AddWithValue("Bio", user.Bio);
                sqlCommand.Parameters.AddWithValue("Image", user.Image);
                sqlCommand.Parameters.AddWithValue("NotAvailable", user.NotAvailable);

                connection.Open();
                try { sqlCommand.ExecuteNonQuery(); }
                catch { return ("Error while updating user.", null); }
                finally { connection.Close(); }
            }

            user.Name = user.Name is null ? null! : "name, ";
            user.Bio = user.Bio is null ? null : "bio, ";
            user.Image = user.Image is null ? null : "image, ";
            string? notAvailable = user.NotAvailable is null ? null : "not available state ";

            string message = $"User's {user.Name}{user.Bio}{user.Image}{notAvailable}was updated.";

            return (null, message);
        }

        public static string? UpdateUserEmail(int userId, string newEmail, string password)
        {
            User? user = GetUser(userId);
            if (user == null)
                return "There is no user with this Id.";

            if (HasUser(newEmail))
                return "This email is already used.";

            bool isValid = BCryptNet.Verify(password, user.Password);
            if (!isValid)
                return "Invalid password.";

            using (var connection = new SqlConnection(_constr))
            {
                string sql = "UPDATE Users SET Email = @Email WHERE UserId = @UserId";
                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("Email", newEmail);
                command.Parameters.AddWithValue("UserId", userId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch { return "Error while changing the email."; }
                finally { connection.Close(); }
            }

            return null;
        }

        public static string? UpdateUserPassword(int userId, string currentPassword, string newPassword)
        {
            User? user = GetUser(userId);
            if (user is null)
                return "There is no user with this Id.";

            bool isValid = BCryptNet.Verify(currentPassword, user.Password);
            if (!isValid) return "Invalid password.";

            newPassword = BCryptNet.HashPassword(newPassword);

            using(var connection = new SqlConnection(_constr))
            {
                string sql = "UPDATE Users SET Password = @Password WHERE UserId = @UserId";
                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("Password", newPassword);
                command.Parameters.AddWithValue("UserId", userId);

                try
                {
                    connection.Open(); 
                    command.ExecuteNonQuery();
                } catch { return "Error while changing the password"; }
                finally { connection.Close(); }
            }

            return null;
        }
        
        public static string? UpdateUsername(int userId, string newUsername)
        {
            if (!HasUser(userId))
                return "There is no user with this Id.";

            using (var connection = new SqlConnection(_constr))
            {
                string sql = "SELECT Username FROM Users WHERE Username = @Username";
                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("Username", newUsername);

                try
                {
                    connection.Open();
                    if (command.ExecuteReader().Read())
                        return "Username is already used.";
                }
                catch { return "Error while changing username."; }
                finally { connection.Close(); }
            }

            using (var connection = new SqlConnection(_constr))
            {
                string sql = "UPDATE Users SET Username = @Username WHERE UserId = @UserId";
                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("Username", newUsername);
                command.Parameters.AddWithValue("UserId", userId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch { return "Error while changing username."; }
                finally { connection.Close(); }
            }

            return null;
        }
    
    }

}
