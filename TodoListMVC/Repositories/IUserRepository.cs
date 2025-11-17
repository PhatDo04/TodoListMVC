using TodoListMVC.Models;
namespace TodoListMVC.Repositories
{
    public interface IUserRepository
    {
        UserModel GetByEmail(string email);
        UserModel Create(UserModel user);
    }
}
