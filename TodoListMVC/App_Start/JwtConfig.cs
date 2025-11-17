using System;

namespace TodoListMVC.App_Start
{
    public class JwtConfig
    {
        public static string Issuer = "TodoListMVC";

        public static string Audience = "todolist";

        public static string Secret = "slkajdflkjl12kj3l13908a0s9cdaolidkaldje212_l23n1l";

        public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);
    }
}