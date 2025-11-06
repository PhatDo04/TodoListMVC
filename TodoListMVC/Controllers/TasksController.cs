using System;
using System.Collections.Generic;
using System.Configuration; // Đọc web.config
using System.Data.SqlClient; // Kết nối SQL Server - thư viện lõi ADO.NET\
using System.Web.Mvc;
using TodoListMVC.Models; // Sử dụng TaskModel

namespace TodoListMVC.Controllers
{
    public class TasksController : Controller
    {
        //Lấy connection string từ web.config
        private string connectionString = ConfigurationManager.ConnectionStrings["TodoListDBConnection"].ConnectionString;

        // GET: Tasks
        public ActionResult Index()
        {
            //Tạo danh sách rỗng để chứa các task
            List<TaskModel> taskList = new List<TaskModel>();

            //Câu lệnh SQL để lấy tất cả các task
            string sqlQuery = "SELECT Id, Title, IsCompleted FROM Tasks ORDER BY Id DESC";

            //Bắt đầu cú pháp ADO.NET
            //Cú pháp using là cách tốt nhất để làm việc với ADO.NET vì nó tự động giải phóng tài nguyên
            //Nó đảm bảo kết nối (conn) sẽ tự động đóng lại kể cả khi có lỗi
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    try
                    {
                        //Mở kết nối tới DB
                        conn.Open();

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

                                //Thêm đối tượng task vào danh sách
                                taskList.Add(task);
                            }
                            //reader sẽ tự đóng nhờ using
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Lỗi khi tải dữ liệu: " + e.Message);
                    }
                    // conn sẽ tự đóng nhờ using
                }
            }
            //Kết thúc khối ADO.NET

            //Gửi danh sách tassk tới View
            return View(taskList);
        }

        //Báo cho MVC biết hàm này chỉ nhận dữ liệu POST từ form
        [HttpPost]
        public ActionResult Create(string taskTitle)
        {
            //Kiểm tra xem người dùng có nhập gì không
            if (string.IsNullOrEmpty(taskTitle))
            {
                //Nếu không nhập, quay về trang Index
                return RedirectToAction("Index");
            }

            //Chuẩn bị câu lệnh SQL Insert với tham số @Title
            string sqlQuery = "INSERT INTO Tasks (Title, IsCompleted) VALUES (@Title, 0)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                //Tạo câu lệnh
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    try
                    {
                        //Thêm giá trị cho tham số @Title
                        //Đây là cách chống lỗi bảo mật (sql injection)
                        cmd.Parameters.AddWithValue("@Title", taskTitle);

                        //Mở cầu nối
                        conn.Open();

                        //Thực thi câu lệnh
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Lỗi khi thêm: " + e.Message);
                    }
                }
            }
            //Chuyển hướng người dùng về trang Index
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            string sqlQuery = "DELETE FROM Tasks WHERE Id = @Id";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    try
                    {
                        //thêm giá trị cho tham số @Id (Chống SQL Injection)
                        cmd.Parameters.AddWithValue("@Id", id);

                        //mở cầu nối
                        conn.Open();

                        //thực thi
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Lỗi khi xóa" + e.Message);
                    }
                }
            }
            return RedirectToAction("Index");
        }

        //Sửa
        [HttpGet]
        public ActionResult Edit(int id)
        {
            TaskModel task = new TaskModel();
            string sqlQuery = "Select * FROM Tasks WHERE Id = @Id";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                task.Id = Convert.ToInt32(reader["Id"]);
                                task.Title = reader["Title"].ToString();
                                task.IsCompleted = Convert.ToBoolean(reader["IsCompleted"]);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Lỗi khi lấy task để sửa: " + e.Message);
                    }
                }
            }
            return View(task);
        }

        //Update Post
        [HttpPost]
        public ActionResult Edit(TaskModel task)
        {
            string sqlQuery = "UPDATE Tasks SET Title = @Title, IsCompleted = @IsCompleted WHERE Id = @Id";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@Id", task.Id);
                        cmd.Parameters.AddWithValue("@Title", task.Title);
                        cmd.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Lỗi khi cập nhật", e.Message);
                    }
                }
            }
            return RedirectToAction("Index");
        }
    }
}