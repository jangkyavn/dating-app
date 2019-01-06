using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tài khoản.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(12, MinimumLength = 6, ErrorMessage = "Bạn phải nhập mật khẩu từ 6 đến 12 ký tự.")]
        public string Password { get; set; }
    }
}
