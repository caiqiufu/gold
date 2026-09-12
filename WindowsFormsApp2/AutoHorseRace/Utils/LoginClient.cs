using System.Net;

namespace AutoHorseRace.Utils
{
    /// <summary>
    /// 负责处理 CTB988 系统的网络会话管理、WAF 防御绕过及自动化登录逻辑。
    /// </summary>
    public class LoginClient
    {
        private readonly HttpClient _client;
        private readonly CookieContainer _cookies = new CookieContainer();

        /// <summary>
        /// 初始化 HttpClient，配置持久化 Cookie 容器及标准浏览器 User-Agent。
        /// </summary>
        public LoginClient()
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = _cookies,
                AllowAutoRedirect = true
            };
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
        }

        /// <summary>
        /// 获取真实登录页面的 HTML 内容，包含 Incapsula WAF 防护机制的自动处理。
        /// </summary>
        /// <returns>登录页的 HTML 源码</returns>
        public async Task<string> GetLoginPageAsync()
        {
            // 对应 Python 的 get_real_login_page
            // 注意：此处需要处理 Incapsula 重定向 (L2) 和 像素点 (L3) 的逻辑
            var response = await _client.GetAsync("https://www.ctb988.com/_index_ctb.jsp");
            return await response.Content.ReadAsStringAsync();
        }
    }
}
