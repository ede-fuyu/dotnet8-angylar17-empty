using System.ComponentModel.DataAnnotations;

namespace Dotnet8App.Api.Infrastructure
{
    public class LoginDto
    {
        public LoginDto()
        {
            UserName = string.Empty;
            Password = string.Empty;
            Captcha = string.Empty;
        }

        [Required(ErrorMessage = "Address is required")]
        public string UserName { get; init; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; init; }

        [Required(ErrorMessage = "Captcha is required")]
        public string Captcha { get; init; }
    }
}
