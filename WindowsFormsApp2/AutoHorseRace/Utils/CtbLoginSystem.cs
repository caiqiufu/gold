using Microsoft.Playwright;

namespace AutoHorseRace.Utils
{
    public class CtbLoginSystem
    {
        private const string BaseUrl = "https://www.ctb988.com";

        public async Task<string> LoginAsync(string username, string password, string pin)
        {
            // 1. 启动 Playwright 并配置为真实浏览器模式
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions { UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36" });
            var page = await context.NewPageAsync();

            // 2. 访问首页并自动处理 Incapsula WAF 校验 (Playwright 会自动加载 JS/图片)
            await page.GotoAsync($"{BaseUrl}/_index_ctb.jsp");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle); // 等待所有 WAF 指纹注入完成

            // 3. 获取 valid token
            var validToken = await page.Locator("input[name='valid']").GetAttributeAsync("value");

            // 4. 获取并识别验证码
            var captchaElement = page.Locator("img[src*='img.jpg']");
            var captchaBytes = await captchaElement.ScreenshotAsync();
            string captcha = RecognizeCaptcha(captchaBytes);

            // 5. 调用你的 SHA1 加密算法 (需自行实现 CryptoHelper.EncryptPassword)
            string encPass = CryptoHelper.EncryptPassword(password, username, validToken, captcha);

            // 6. 执行登录提交 (模拟 URL 跳转)
            await page.GotoAsync($"{BaseUrl}/login?uid={username}&pass={encPass}&code={captcha}");

            // 7. 处理 PIN 码页面
            // 等待页面加载完成并获取 r1, r2 (通过执行 JS 直接从页面变量读取)
            await page.WaitForSelectorAsync("body");
            string r1 = await page.EvaluateAsync<string>("r1");
            string r2 = await page.EvaluateAsync<string>("r2");

            // 8. 计算 PIN 密码并提交
            string encPin = CryptoHelper.EncryptPin(username, pin, r1, r2);
            await page.EvaluateAsync($"fetch('/verifypin', {{ method: 'POST', body: 'code={encPin}&fpVisitorId=NO_VISITOR_ID' }})");

            // 9. 确认进入主页
            await page.WaitForURLAsync("**/playerhk.jsp");
            return "登录成功";
        }

        private string RecognizeCaptcha(byte[] imgBytes)
        {
            // 1. 手动创建引擎实例
            var ocr = new PaddleOCRSharp.PaddleOCREngine();

            try
            {
                // 2. 调用 DetectText 进行识别
                // 该方法通常返回一个包含识别结果的对象，通过 .Text 获取字符串
                var result = ocr.DetectText(imgBytes);

                if (result != null && !string.IsNullOrEmpty(result.Text))
                {
                    // 3. 过滤非数字字符
                    return new string(result.Text.Where(char.IsDigit).ToArray());
                }
                return string.Empty;
            }
            finally
            {
                // 4. 手动释放资源
                ocr.Dispose();
            }
        }
    }
}
