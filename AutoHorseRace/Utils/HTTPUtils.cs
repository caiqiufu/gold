using Newtonsoft.Json;
using NLog;
using System.Text;
using System.Web;

namespace AutoHorseRace.Utils
{
    public class HTTPUtils
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        // 🔧 关键修复：HttpClient 必须全局只创建一次、长期复用，绝不能每次请求 new 一个再 Dispose。
        // 原因：每次新建都要重新走一次TCP三次握手，Dispose 后连接进入 TIME_WAIT 不会立刻释放，
        // 高频调用下会持续产生几百毫秒的额外延迟（这正好解释了日志里"客户端总耗时"比
        // "后端服务耗时"平白多出的280~630ms），长时间运行还有本地端口耗尽的风险。
        private static readonly HttpClient _sharedClient = new HttpClient();

        /// <summary>
        /// 测试登录接口，并记录服务端响应耗时
        /// </summary>
        /// <returns></returns>
        public static string TestLogin()
        {
            string url = "http://localhost:8000/api/v1/auth/login";
            var payloadObj = new
            {
                username = "vcd666888",
                password = "aabb1122.m",
                pin = "1111"
            };

            string payload = JsonConvert.SerializeObject(payloadObj);

            // 🌟 调用带 out 参数的重载方法，获取服务端的 X-Process-Time
            string response = SendSyncRequest(url, "POST", null, null, payload, out string serverProcessTime);

            _logger.Info($"[TestLogin] 请求完成 | 服务端耗时: {serverProcessTime} | 响应结果: {response}");
            return response;
        }

        /// <summary>
        /// 发送同步 HTTP/HTTPS 请求（兼容老版本的重载方法）
        /// </summary>
        public static string SendSyncRequest(string url, string method,
                                             (string Key, string Value)[]? headers = null,
                                             (string Key, string Value)[]? parameters = null,
                                             string? payload = null)
        {
            // 内部自动调用带 out 参数的方法，忽略耗时输出
            return SendSyncRequest(url, method, headers, parameters, payload, out _);
        }

        /// <summary>
        /// 发送同步 HTTP/HTTPS 请求（完整核心实现）
        /// </summary>
        /// <param name="url">请求的 URL</param>
        /// <param name="method">HTTP 方法（GET, POST, PUT, DELETE 等）</param>
        /// <param name="headers">请求头（key-value 格式）</param>
        /// <param name="parameters">URL请求参数（key-value 格式）</param>
        /// <param name="payload">请求体（用于 POST/PUT 请求）</param>
        /// <param name="serverProcessTime">输出参数：获取服务端返回的 X-Process-Time 耗时</param>
        /// <returns>响应结果字符串</returns>
        public static string SendSyncRequest(string url, string method,
                                             (string Key, string Value)[]? headers,
                                             (string Key, string Value)[]? parameters,
                                             string? payload,
                                             out string serverProcessTime)
        {
            serverProcessTime = string.Empty;
            try
            {
                // 初始化 UriBuilder
                UriBuilder uriBuilder = new UriBuilder(url);

                // 使用 HttpUtility.ParseQueryString 添加参数
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        query[parameter.Key] = parameter.Value;
                    }
                }
                // 将查询字符串附加到 URL 中
                uriBuilder.Query = query.ToString();
                string finalUrl = uriBuilder.ToString();

                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = new HttpMethod(method),
                    RequestUri = new Uri(finalUrl)
                };

                // 🔧 Header 不能再挂在 client.DefaultRequestHeaders 上了——client 现在是全局
                // 共享的单例，多线程并发下往它的 DefaultRequestHeaders 里 Add，会导致所有请求
                // 互相污染（甚至重复 Add 同名 Header 直接抛异常）。改成挂在每次新建的这个
                // HttpRequestMessage.Headers 上，天然线程安全，互不影响。
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // 设置 Payload（适用于 POST/PUT 请求）
                if (!string.IsNullOrEmpty(payload) &&
                    (method.ToUpper() == "POST" || method.ToUpper() == "PUT"))
                {
                    request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                }

                // 同步发送请求并获取响应（复用共享的 _sharedClient，不再每次新建）
                HttpResponseMessage response = _sharedClient.SendAsync(request).GetAwaiter().GetResult();

                // 🌟 获取服务端返回的 X-Process-Time 响应头
                if (response.Headers.TryGetValues("X-Process-Time", out var values))
                {
                    serverProcessTime = values.FirstOrDefault() ?? string.Empty;
                }

                response.EnsureSuccessStatusCode(); // 确保返回成功状态码

                // 读取响应内容
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.Error($"[HTTP Error] 请求失败: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }
}