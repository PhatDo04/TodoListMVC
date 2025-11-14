using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TodoListMVC.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt{ get; set; }
    }
}