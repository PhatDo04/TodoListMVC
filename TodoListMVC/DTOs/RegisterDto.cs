using System.ComponentModel.DataAnnotations;

namespace TodoListMVC.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password là bắt buộc")]
        [MinLength(6, ErrorMessage = "Password ph?i có ít nhất 6 ký tự")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhậnn không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
