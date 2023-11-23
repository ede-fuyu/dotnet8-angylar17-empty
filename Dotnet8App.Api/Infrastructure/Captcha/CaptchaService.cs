using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

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
        /// 繪製干擾線
        /// </summary>
        /// <param name="random"></param>
        /// <param name="bitmap"></param>
        /// <param name="graphics"></param>
        /// <param name="lineCount"></param>
        /// <param name="pointCount"></param>
        public static void Disturb(Random random, Bitmap bitmap, Graphics graphics, int lineCount, int pointCount)
        {
            var colors = new List<Color>
                {
                    Color.AliceBlue,
                    Color.Azure,
                    Color.CadetBlue,
                    Color.Beige,
                    Color.Chartreuse
                };

            //干擾線
            for (var i = 0; i < lineCount; i++)
            {
                var x1 = random.Next(bitmap.Width);
                var x2 = random.Next(bitmap.Width);
                var y1 = random.Next(bitmap.Height);
                var y2 = random.Next(bitmap.Height);

                //Pen 類 定義用於繪製直線和曲線的物件。
                var pen = new Pen(colors[random.Next(0, colors.Count - 1)]);

                graphics.DrawLine(pen, x1, y1, x2, y2);
            }

            //干擾點
            for (var i = 0; i < pointCount; i++)
            {
                var x = random.Next(bitmap.Width);
                var y = random.Next(bitmap.Height);
                bitmap.SetPixel(x, y, Color.FromArgb(random.Next()));
            }
        }

        /// <summary>
        /// 生成驗證碼圖片
        /// </summary>
        /// <param name="charType"></param>
        /// <param name="charCount"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public CaptchaInfo Create(int charType, int charCount = 4, int width = 85, int height = 40)
        {
            var characters = new List<char>();

            // 0:英數 1:英 2:數
            //去掉0、o、O

            if (charType != 1)
                for (var c = '0'; c <= '9'; c++)
                {
                    if (c == '0') continue;

                    characters.Add(c);
                }

            if (charType != 2)
            {
                for (var c = 'a'; c < 'z'; c++)
                {
                    if (c == 'o') continue;

                    characters.Add(c);
                }

                for (var c = 'A'; c < 'Z'; c++)
                {
                    if (c == 'O') continue;

                    characters.Add(c);
                }
            }

            var chars = new char[charCount];
            var len = characters.Count;
            var random = new Random();
            for (var i = 0; i < chars.Length; i++)
            {
                var r = random.Next(len);
                chars[i] = characters[r];
            }

            var captcha = string.Join(string.Empty, chars);

            var fontNames = new List<string>
            {
                "Helvetica", "Arial", "Lucida Family", "Verdana", "Tahoma", "Trebuchet MS", "Georgia", "Times"
            };

            //Bitmap 類 封裝 GDI+ 包含圖形圖像和其屬性的圖元資料的點陣圖。 一個 Bitmap 是用來處理圖像圖元資料所定義的物件。
            //Bitmap 類 繼承自 抽象基類 Image 類
            using var bitmap = new Bitmap(width, height);
            //Graphics 類 封裝一個 GDI+ 繪圖圖面。
            using var graphics = Graphics.FromImage(bitmap);
            //填充背景色 白色
            graphics.Clear(Color.White);

            //繪製干擾線和干擾點
            Disturb(random, bitmap, graphics, width / 2, height);

            //添加灰色邊框
            var pen = new Pen(Color.Silver);
            graphics.DrawRectangle(pen, 0, 0, width - 1, height - 1);

            var x = 1;
            const int y = 5;

            var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var color = Color.FromArgb(random.Next(100, 122), random.Next(100, 122), random.Next(100, 122));

            foreach (var c in chars)
            {
                //隨機選擇字元 字體樣式和大小
                var fontName = fontNames[random.Next(0, fontNames.Count - 1)];
                var font = new Font(fontName, random.Next(15, 20));
                //淡化字元顏色
                using var brush = new LinearGradientBrush(rectangle, color, color, 90f, true);
                brush.SetSigmaBellShape(0.5f);
                graphics.DrawString(c.ToString(), font, brush, x + random.Next(-2, 2), y + random.Next(-5, 5));
                x += width / charCount;
            }

            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Jpeg);
            var model = new CaptchaInfo()
            {
                Image = memoryStream.ToArray(),
                ContentType = ContentType,
                Captcha = captcha
            };

            session.SetString(this.Captcha, model.Captcha);

            return model;
        }

        /// <summary>
        /// 比對驗證碼
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="captcha"></param>
        /// <returns></returns>
        public bool Verify(string answer)
        {
            var captcha = session.GetString(this.Captcha);

            string logInfo = $"session: {session.Id}, captcha: {captcha}, answer: {answer}";
            logger.LogInformation(logInfo);

            return string.Equals(answer, captcha, StringComparison.CurrentCultureIgnoreCase);
        }
    }

    public interface ICaptchaService
    {
        /// <summary>
        /// 生成驗證碼圖片
        /// </summary>
        /// <param name="charType">0:英數 1:英 2:數</param>
        /// <param name="charCount">圖片中出現字元數</param>
        /// <param name="width">圖片寬度</param>
        /// <param name="height">圖片高度</param>
        /// <returns></returns>
        CaptchaInfo Create(int charType, int charCount = 4, int width = 85, int height = 40);

        /// <summary>
        /// 比對驗證碼
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="captcha"></param>
        /// <returns></returns>
        bool Verify(string answer);
    }
}
