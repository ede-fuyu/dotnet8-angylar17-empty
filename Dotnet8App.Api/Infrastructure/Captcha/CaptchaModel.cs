namespace Dotnet8App.Api.Infrastructure
{
    public class CaptchaInfo
    {
        public CaptchaInfo()
        {
            Image = [];
            Captcha = string.Empty;
            ContentType = string.Empty;
        }

        public byte[] Image { get; set; }

        public string Captcha { get; set; }

        public string ContentType { get; set; }
    }
}
