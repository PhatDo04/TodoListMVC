using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TodoListMVC.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly SqlConnection _connection;
        private SqlTransaction _transaction;
        private bool _disposed = false;
        public ITaskRepository Tasks { get; private set; }

        public UnitOfWork()
        {
            //Lấy chuỗi kết nối từ Web.config
            string connectionString = System.Configuration.ConfigurationManager
                .ConnectionStrings["TodoListDBConnection"].ConnectionString;
            //Tạo, mở kết nối 
            _connection = new SqlConnection(connectionString);
            _connection.Open();

            //Bắt đầu giao dịch
            _transaction = _connection.BeginTransaction();

            //Sếp tạo nhân viên TaskRepository, truyền vào kết nối và giao dịch
            Tasks = new SqlTaskRepository(_connection, _transaction);
        }

        //Nút hoàn tất giao dịch
        public int SaveChanges()
        {
            if (_transaction == null)
            {
                //ném ngoại lệ nếu transaction bị đóng hoặc hủy
                //InvalidOperationException: giao dịch không hợp lệ (đóng/hủy)
                throw new InvalidOperationException("Transaction đã bị đóng hoặc bị hủy");
            }

            try
            {
                _transaction.Commit(); //Chạy lệnh Commit
                return 1;
            }
            catch (Exception e)
            {
                _transaction.Rollback(); //Hủy giao dịch nếu có lỗi
                throw e;
            }
            finally
            {
                if (_transaction != null)
                {
                    //Dọn dẹp giao dịch
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }
        //hàm dọn dẹp (tuân theo mẫu IDisposable)
        public void Dispose()
        {
            //Gọi hàm dọn dẹp có tham số
            Dispose(true);
            //Ngăn trình thu gom rác gọi hàm hủy (destructor)
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            //Kiểm tra đã dọn dẹp chưa nếu rồi thì thoát
            if (_disposed) return;
            if (disposing)
            {
                // "Dọn dẹp" giao dịch (nếu nó vẫn còn)
                // Toán tử điều kiện null: nếu _null khác null thì gọi hàm Rollback
                _transaction?.Rollback(); // (SỬA) Rollback nếu chưa Save
                _transaction?.Dispose();

                // "Dọn dẹp" kết nối
                _connection?.Dispose(); 
            }
            //Đánh dấu đã dọn
            _disposed = true;
        }
    }
}