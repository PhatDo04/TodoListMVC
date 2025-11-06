using System.Collections.Generic;
using TodoListMVC.Models;

namespace TodoListMVC.Repositories
{
    public interface ITaskRepository
    {
        //Trả về danh sách tất cả task
        IEnumerable<TaskModel> GetTasks();

        //Trả về một task theo Id
        TaskModel GetTask(int id);

        //Thêm một task mới, trả về task vừa thêm (có Id)
        TaskModel PostTask(TaskModel task);

        //Cập nhật một task
        void PutTask(int id, TaskModel task);

        //Xóa một task theo Id
        void DeleteTask(int id);
    }
}