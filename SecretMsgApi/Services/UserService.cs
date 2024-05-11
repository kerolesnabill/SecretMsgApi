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

        public static (string? Error, string? Token) Register(string name, string email, string password)
        {
            if (HasUser(email))
                return ("This email is already registered", null);

            password = BCryptNet.HashPassword(password);
            int? userId = null;

            email = email.ToLower();
            using (var connection = new SqlConnection(_constr))
            {
                SqlCommand command = new SqlCommand("InsertUser", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("Name", name);
                command.Parameters.AddWithValue("Email", email);
                command.Parameters.AddWithValue("Password", password);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                        if (reader.Read()) userId = (int) reader["UserId"];
                        else throw new Exception();
                }
                catch (Exception) { return ("Failed to register", null); }
                finally { connection.Close(); }
            }

            string? token = null;
            if(userId != null)
                token = JwtService.GenerateToken(userId.ToString()!);

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
                string sql = "SELECT * FROM Users WHERE Email = @Email";
                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("Email", email);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
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
                string sql = "SELECT * From Users WHERE UserId = @UserId";
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
                            Username = reader["Username"].ToString()!,
                            Name = reader["Name"].ToString()!,
                            Bio = reader["Bio"].ToString()!,
                            Image = reader["Image"].ToString()!,
                            Views = (int)reader["Views"],
                            Available = (bool)reader["Available"],
                            ShowLastSeen = (bool)reader["ShowLastSeen"],
                            LastSeen = (DateTime)reader["LastSeen"],
                            CreatedAt = (DateTime)reader["LastSeen"],
                        };
                    }
                }
                connection.Close();
            }

            return user;
        }

        public static User? GetUserByUsername(string username)
        {
            User? user = null;

            using(var connection = new SqlConnection(_constr))
            {
                string sql = "SELECT * FROM Users WHERE Username = @Username";
                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("Username", username);

                try
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                        user = new User {
                            Id = (int) reader["UserId"],
                            Username = reader["Username"].ToString(),
                            Name = reader["Name"].ToString(),
                            Bio = reader["Bio"].ToString(),
                            Image = reader["Image"].ToString(),
                            Available = (bool)reader["Available"],
                            ShowLastSeen = (bool)reader["ShowLastSeen"],
                            LastSeen = (DateTime)reader["LastSeen"],
                        };
                }
                catch { return null; }
                finally { connection.Close(); }
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

        public static (string? Error, string? Message) UpdateUser(int userId, UpdateUserModel user)
        {
            if (!HasUser(userId))
                return ("There is no user with this Id", null);

            using(var connection = new SqlConnection(_constr))
            {
                SqlCommand sqlCommand = new SqlCommand("UpdateUser", connection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("UserId", userId);
                sqlCommand.Parameters.AddWithValue("Name", user.Name);
                sqlCommand.Parameters.AddWithValue("Bio", user.Bio);
                sqlCommand.Parameters.AddWithValue("Image", user.Image);
                sqlCommand.Parameters.AddWithValue("Available", user.Available);
                sqlCommand.Parameters.AddWithValue("ShowLastSeen", user.ShowLastSeen);

                connection.Open();
                try { sqlCommand.ExecuteNonQuery(); }
                catch { return ("Error while updating user.", null); }
                finally { connection.Close(); }
            }

            user.Name = user.Name is null ? null! : "name, ";
            user.Bio = user.Bio is null ? null : "bio, ";
            user.Image = user.Image is null ? null : "image, ";
            string? Available = user.Available is null ? null : " available state, ";
            string? ShowLastSeen = user.ShowLastSeen is null ? null : " show last seen state ";

            string message = $"User's {user.Name}{user.Bio}{user.Image}{Available}{ShowLastSeen}was updated.";

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

            newEmail = newEmail.ToLower();
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
                newUsername = newUsername.ToLower();
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
    
        public static string? UpdateLastSeen(int userId)
        {
            if (!HasUser(userId))
                return "There is no user with this Id.";

            using(var connection = new SqlConnection(_constr))
            {
                SqlCommand command = new SqlCommand("UpdateLastSeen", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("UserId", userId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch { return "Error while updating last seen."; }
                finally { connection.Close(); }
            }

            return null;
        }

        public static string? UpdateViews(int userId)
        {
            if (!HasUser(userId))
                return "There is no user with this Id.";

            using (var connection = new SqlConnection(_constr))
            {
                SqlCommand command = new SqlCommand("UpdateViews", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("UserId", userId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch { return "Error while updating views count."; }
                finally { connection.Close(); }
            }

            return null;
        }

        public static string? DeleteUser(int userId)
        {
            if (!HasUser(userId))
                return "There is no user with this Id.";

            using(var connection = new SqlConnection(_constr))
            {
                SqlCommand command = new SqlCommand("DeleteUser", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("UserId", userId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch { return "Error while deleting the user."; }
                finally { connection.Close(); }
            }

            return null;
        }
    }

}
