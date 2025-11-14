using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoListMVC.Models;
namespace TodoListMVC.Repositories
{
    public interface IUserRepository
    {
        UserModel GetByEmail(string email);
    }
}
