using System;
using System.Data.SqlClient;
using TodoListMVC.Models;

namespace TodoListMVC.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public UserRepository(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public UserModel GetByEmail(string email)
        {
            string query = "SELECT Id, Email, PasswordHash, CreatedAt FROM Users WHERE Email = @Email";

            using (SqlCommand cmd = new SqlCommand(query, _connection, _transaction))
            {
                cmd.Parameters.AddWithValue("@Email", email);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new UserModel
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Email = reader["Email"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        };
                    }
                }
            }

            return null; // User không t?n t?i
        }

        public UserModel Create(UserModel user)
        {
            const string sql = @"INSERT INTO Users (Email, PasswordHash, CreatedAt)
                                VALUES (@Email, @PasswordHash, @CreatedAt);
                                SELECT SCOPE_IDENTITY();";

            using (var cmd = new SqlCommand(sql, _connection, _transaction))
            {
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

                var newIdObj = cmd.ExecuteScalar();
                user.Id = Convert.ToInt32(newIdObj);
                return user;
            }
        }
    }
}