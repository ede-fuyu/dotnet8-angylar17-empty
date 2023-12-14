using SkiaSharp;

namespace Dotnet8App.Api.Infrastructure
{
    public class CaptchaService : ICaptchaService
    {
        private readonly ISession session;
        private readonly ILogger logger;

        private readonly string Captcha = "Captcha";
        private const string ContentType = "image/jpeg";

        public CaptchaService(IHttpContextAccessor httpContextAccessor, ILogger<CaptchaService> ilogger)
        {
            if (httpContextAccessor.HttpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor));
            }

            session = httpContextAccessor.HttpContext.Session;
            logger = ilogger;
        }

        /// <summary>
        /// 生成隨機驗證碼字串
        /// </summary>
        /// <param name="charType"></param>
        /// <param name="charCount"></param>
        /// <returns></returns>
        private string GenerateCaptchaCode(CharType charType, int charCount)
        {
            var characters = string.Empty;
            var random = new Random();

            if (charType == CharType.RandomChoice)
            {
                var choices = new[] { CharType.OnlyNumbers, CharType.OnlyUppercase, CharType.OnlyLowercase };
                charType = choices[random.Next(choices.Length)];
            }

            // 加入數字
            if (!new[] { CharType.OnlyUppercase, CharType.OnlyLowercase, CharType.NoNumbers }.Contains(charType))
            {
                var numbers = Enumerable.Range('0', '9' - '0' + 1).Select(n => (char)n);

                // 不是只用數字 去除 0  
                if (charType != CharType.OnlyNumbers)
                {
                    numbers = numbers.Where(n => n != '0');
                }

                characters = new string(numbers.ToArray());
            }

            // 加入大寫字母
            if (!new[] { CharType.OnlyNumbers, CharType.OnlyLowercase, CharType.NoUppercase }.Contains(charType))
            {
                var uppercase = Enumerable.Range('A', 'Z' - 'A' + 1).Select(n => (char)n);

                // 不是只用大寫字母 去除 O
                if (charType != CharType.OnlyUppercase)
                {
                    uppercase = uppercase.Where(n => n != 'O');
                }

                characters += new string(uppercase.ToArray());
            }

            // 加入小寫字母
            if (!new[] { CharType.OnlyNumbers, CharType.OnlyUppercase, CharType.NoLowercase }.Contains(charType))
            {
                var lowercase = Enumerable.Range('a', 'z' - 'a' + 1).Select(n => (char)n);

                // 不是只用小寫字母 去除 o
                if (charType != CharType.OnlyLowercase)
                {
                    lowercase = lowercase.Where(n => n != 'o');
                }

                characters += new string(lowercase.ToArray());
            }

            var captcha = new string(Enumerable.Repeat(characters, charCount).Select(s => s[random.Next(s.Length)]).ToArray());

            session.SetString(this.Captcha, captcha);

            return captcha;
        }

        private static byte[] GenerateCaptchaImage(string captcha, int width, int height)
        {
            var random = new Random();

            var fontNames = new List<string>
            {
                "Helvetica", "Arial", "Lucida Family", "Verdana", "Tahoma", "Trebuchet MS", "Georgia", "Times"
            };

            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            // Draw captcha text
            foreach (var (ch, i) in captcha.Select((value, index) => (value, index)))
            {
                var fontName = fontNames[random.Next(0, fontNames.Count - 1)];
                var font = SKTypeface.FromFamilyName(fontName);
                var paint = new SKPaint
                {
                    Typeface = font,
                    TextSize = random.Next(20, 25),
                    Color = new SKColor((byte)random.Next(0, 80), (byte)random.Next(0, 80), (byte)random.Next(0, 80)),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2
                };

                var textWidth = paint.MeasureText(ch.ToString());
                var x = (width / captcha.Length) * i + (width / captcha.Length - textWidth) / 2;
                var y = height / 2 + (paint.TextSize / 2);

                canvas.DrawText(ch.ToString(), x, y, paint);
            }

            // Draw disturbance lines
            var lineCount = width / 4;
            var pointCount = height;
            var colors = new SKColor[]
            {
                SKColors.AliceBlue,
                SKColors.Azure,
                SKColors.CadetBlue,
                SKColors.Beige,
                SKColors.Chartreuse
            };

            // Disturbance lines
            Enumerable.Range(0, lineCount).ToList().ForEach(_ =>
            {
                var x1 = random.Next(bitmap.Width);
                var x2 = random.Next(bitmap.Width);
                var y1 = random.Next(bitmap.Height);
                var y2 = random.Next(bitmap.Height);

                var paint = new SKPaint
                {
                    Color = colors[random.Next(0, colors.Length - 1)],
                    StrokeWidth = 1,
                    IsAntialias = true
                };

                canvas.DrawLine(x1, y1, x2, y2, paint);
            });

            // Disturbance points
            Enumerable.Range(0, pointCount).ToList().ForEach(_ =>
            {
                var x = random.Next(bitmap.Width);
                var y = random.Next(bitmap.Height);
                var color = new SKColor((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));

                bitmap.SetPixel(x, y, color);
            });

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);

            return data.ToArray();
        }

        /// <summary>
        /// 生成驗證碼圖片
        /// </summary>
        /// <param name="charType"></param>
        /// <param name="charCount"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public CaptchaInfo Generate(CharType charType = CharType.RandomChoice, int charCount = 4, int width = 100, int height = 40)
        {
            var captcha = this.GenerateCaptchaCode(charType, charCount);
            var captchaImage = GenerateCaptchaImage(captcha, width, height);

            var model = new CaptchaInfo()
            {
                Image = captchaImage,
                ContentType = ContentType,
                Captcha = captcha
            };

            return model;
        }

        /// <summary>
        /// 比對驗證碼
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="sensitive">是否區分大小寫</param>
        /// <returns></returns>
        public bool Verify(string answer, bool sensitive = false)
        {
            var captcha = session.GetString(this.Captcha);
            var sessionId = session.Id;

            logger.LogInformation("session: {sessionId}, captcha: {captcha}, answer: {answer}", sessionId, captcha, answer);

            if (sensitive || string.IsNullOrEmpty(captcha))
            {
                return string.Equals(answer, captcha, StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                return string.Equals(answer.ToUpper(), captcha.ToUpper(), StringComparison.CurrentCultureIgnoreCase);
            }
        }
    }

    public interface ICaptchaService
    {
        /// <summary>
        /// 生成驗證碼圖片
        /// </summary>
        /// <param name="charType">
        /// 預設: 只用數字, 只用大寫字母, 只用小寫字母 三種隨機擇一
        /// </param>
        /// <param name="charCount">圖片中出現字元數</param>
        /// <param name="width">圖片寬度</param>
        /// <param name="height">圖片高度</param>
        /// <returns></returns>
        CaptchaInfo Generate(CharType charType = CharType.RandomChoice, int charCount = 4, int width = 100, int height = 40);

        /// <summary>
        /// 比對驗證碼
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="sensitive">是否區分大小寫</param>
        /// <returns></returns>
        bool Verify(string answer, bool sensitive = false);
    }
}
