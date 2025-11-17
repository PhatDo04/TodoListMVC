using System.ComponentModel.DataAnnotations;

namespace TodoListMVC.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email là b?t bu?c")]
        [EmailAddress(ErrorMessage = "Email không h?p l?")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password là b?t bu?c")]
        [MinLength(6, ErrorMessage = "Password ph?i có ít nh?t6 ký t?")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "M?t kh?u xác nh?n không kh?p")]
        public string ConfirmPassword { get; set; }
    }
}
