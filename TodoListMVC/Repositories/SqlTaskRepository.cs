using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TodoListMVC.Models;

namespace TodoListMVC.Repositories
{
    public class SqlTaskRepository : ITaskRepository
    {
        //Thêm 2 trường kết nối và giao dịch mà bên UnitOfWork truyền vào
        private readonly SqlConnection _connection;

        private readonly SqlTransaction _transaction;

        //Repository sẽ được tạo bằng cách nhận 1 kết nối và 1 giao dịch từ UnitOfWork
        public SqlTaskRepository(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        // Hàm tạo SqlComment dùng chung
        private SqlCommand CreateCommand()
        {
            var cmd = _connection.CreateCommand();
            cmd.Transaction = _transaction;
            return cmd;
        }

        public IEnumerable<TaskModel> GetTasks()
        {
            //Tạo danh sách rỗng để chứa các task
            List<TaskModel> taskList = new List<TaskModel>();

            //Câu lệnh SQL để lấy tất cả các task
            string sqlQuery = "SELECT Id, Title, IsCompleted, CreatedAt, UpdatedAt FROM Tasks ORDER BY Id DESC";

            using (SqlCommand cmd = CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                //Thực thi câu lệnh và lấy bộ đọc (Reader) kết quả
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    //reader.Read() sẽ đọng từng dòng(row)
                    //Nếu còn dòng nó sẽ trả về true và đi tiếp
                    while (reader.Read())
                    {
                        //Tạo 1 đối tượng TaskModel mới
                        TaskModel task = new TaskModel();

                        //Nhét (populate) dữ liệu từ cột vào đối tượng
                        // reader["TenCot"]
                        task.Id = Convert.ToInt32(reader["Id"]);
                        task.Title = reader["Title"].ToString();
                        task.IsCompleted = Convert.ToBoolean(reader["IsCompleted"]);
                        task.CreatedAt = Convert.ToDateTime(reader["CreatedAt"]);
                        task.UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]);
                        //Thêm đối tượng task vào danh sách
                        taskList.Add(task);
                    }
                    //reader sẽ tự đóng nhờ using
                }
                return taskList;
            }
        }

        //Kết thúc khối ADO.NET

        public TaskModel GetTask(int id)
        {
            TaskModel task = null;
            string sqlQuery = "SELECT Id, Title, IsCompleted, CreatedAt, UpdatedAt FROM Tasks WHERE Id = @Id";
            using (SqlCommand cmd = CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                cmd.Parameters.AddWithValue("@Id", id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        task = new TaskModel();
                        task.Id = Convert.ToInt32(reader["Id"]);
                        task.Title = reader["Title"].ToString();
                        task.IsCompleted = Convert.ToBoolean(reader["IsCompleted"]);
                        task.CreatedAt = Convert.ToDateTime(reader["CreatedAt"]);
                        task.UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]);
                    }
                }
            }
            return task;
        }

        public TaskModel PostTask(TaskModel task)
        {
            //Chuẩn bị câu lệnh SQL Insert với tham số @Title
            string sqlQuery = "INSERT INTO Tasks (Title, IsCompleted) VALUES (@Title, @IsCompleted); SELECT SCOPE_IDENTITY();";

            //Tạo câu lệnh
            using (SqlCommand cmd = CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                //Thêm giá trị cho tham số @Title
                //Đây là cách chống lỗi bảo mật (sql injection)
                cmd.Parameters.AddWithValue("@Title", task.Title);
                cmd.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);

                int newId = Convert.ToInt32(cmd.ExecuteScalar());
                task.Id = newId;
            }
            return task;
        }

        //Update Post
        public void PutTask(int id, TaskModel task)
        {
            string sqlQuery = "UPDATE Tasks SET Title = @Title, IsCompleted = @IsCompleted WHERE Id = @Id";

            using (SqlCommand cmd = CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                cmd.Parameters.AddWithValue("@Id", task.Id);
                cmd.Parameters.AddWithValue("@Title", task.Title);
                cmd.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);

                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteTask(int id)
        {
            string sqlQuery = "DELETE FROM Tasks WHERE Id = @Id";

            using (SqlCommand cmd = CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                //thêm giá trị cho tham số @Id (Chống SQL Injection)
                cmd.Parameters.AddWithValue("@Id", id);

                //thực thi
                cmd.ExecuteNonQuery();
            }
        }
    }
}