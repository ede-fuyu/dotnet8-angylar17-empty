namespace Dotnet8App.Api.Infrastructure
{
    public enum CharType
    {
        OnlyNumbers, // 只用數字
        OnlyUppercase, // 只用大寫字母
        OnlyLowercase, // 只用小寫字母   
        NoNumbers, // 不用數字(不含O、o)
        NoUppercase, // 不用大寫字母(不含0、o)
        NoLowercase, // 不用小寫字母(不含0、O)
        RandomChoice, // 只用數字, 只用大寫字母, 只用小寫字母  三種隨機擇一
        All // 全部   
    }

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
