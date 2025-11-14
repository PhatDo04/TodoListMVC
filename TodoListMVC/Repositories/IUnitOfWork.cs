using System;

namespace TodoListMVC.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        //Cửa (property) để truy cập nhân viên Task
        ITaskRepository Tasks { get; }

        //Cửa (property) để truy cập nhân viên User
        IUserRepository Users { get; }

        //Nút để hoàn tất giao dịch (lưu thay đổi)
        // Chạy Transaction.Commit()
        //Trả về số dòng bị ảnh hưởng
        int SaveChanges();
    }
}