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
        //
        // 🆕 V20260923_HTTP_TIMEOUT：给 HttpClient 设置一个全局默认超时时间。
        // 之前完全没设置，HttpClient.Timeout 默认是 100 秒——意味着一旦后端卡住、网络中断
        // 或对端只建立了连接却不返回任何数据（既不是连接失败、也不是正常响应，是"悬着不动"），
        // SendAsync(...).GetAwaiter().GetResult() 会同步阻塞调用它的那个线程长达 100 秒才超时。
        // 如果调用方恰好是在 UI 线程上（或持有某把被 UI 线程依赖的锁的后台线程上）同步调用到这里，
        // 效果就是整个界面"卡死"近 2 分钟——这和之前排查到的好几处"程序运行中会卡死"症状是
        // 同一类根因（同步阻塞 I/O 没有超时兜底）。
        //
        // 这里参照 EAForm.cs 里"后端最短 timeout 为 3 秒，前端请求必须大于 10 秒"的既有约定，
        // 给一个比后端处理时间宽松、但远小于默认 100 秒的全局默认值（20 秒）：既能覆盖正常的
        // 网络抖动/后端偶尔处理稍慢，又能保证一旦请求真的卡住，最多 20 秒后会自动抛出
        // TaskCanceledException 结束阻塞，而不是无限期（或近 2 分钟）挂起调用线程。
        //
        // 注意：HttpClient.Timeout 只能在这个实例发出第一次请求之前设置一次，后续再修改会直接
        // 抛 InvalidOperationException（这也是 HttpClient 必须是单例、不能每次请求都 new 的另一
        // 个原因）。如果某次具体调用需要比这个全局默认值更短/更长的超时，不要在这里改全局默认
        // 值，改用下面 SendSyncRequest 新增的 timeoutSeconds 参数按次覆盖（通过 CancellationToken
        // 实现，不受"只能设置一次"的限制）。
        private static readonly HttpClient _sharedClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

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
        /// <param name="timeoutSeconds">
        /// 🆕 V20260923_HTTP_TIMEOUT：本次请求单独覆盖的超时秒数；不传则使用
        /// _sharedClient.Timeout 的全局默认值（20 秒，见上方字段声明处注释）。
        /// 追加在参数列表最后、给了默认值 null，是为了不影响任何已有调用点的positional调用方式。
        /// </param>
        public static string SendSyncRequest(string url, string method,
                                             (string Key, string Value)[]? headers = null,
                                             (string Key, string Value)[]? parameters = null,
                                             string? payload = null,
                                             int? timeoutSeconds = null)
        {
            // 内部自动调用带 out 参数的方法，忽略耗时输出
            return SendSyncRequest(url, method, headers, parameters, payload, out _, timeoutSeconds);
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
        /// <param name="timeoutSeconds">
        /// 🆕 V20260923_HTTP_TIMEOUT：本次请求单独覆盖的超时秒数，不传（null）则使用
        /// _sharedClient.Timeout 的全局默认值。放在 out 参数之后是为了保持已有的 6 个位置参数
        /// （url, method, headers, parameters, payload, out serverProcessTime）完全不变，
        /// 现有按位置传参的调用点不需要任何改动就能继续编译通过。
        /// 实现上用 CancellationTokenSource 而不是修改 _sharedClient.Timeout——后者是单例的
        /// 全局设置，且只能在发出第一次请求前设置一次，之后再改会直接抛异常，没法做到"这一次
        /// 请求单独用不同的超时"。
        /// </param>
        /// <returns>响应结果字符串</returns>
        public static string SendSyncRequest(string url, string method,
                                             (string Key, string Value)[]? headers,
                                             (string Key, string Value)[]? parameters,
                                             string? payload,
                                             out string serverProcessTime,
                                             int? timeoutSeconds = null)
        {
            serverProcessTime = string.Empty;
            // 🆕 timeoutSeconds 指定时才额外创建一个按超时自动取消的 CancellationTokenSource；
            // 不指定就传 CancellationToken.None，完全依赖 _sharedClient.Timeout 的全局默认值，
            // 行为和改动前一致。using 保证无论成功/异常/超时，这个 CTS 都会被正确释放。
            using var cts = timeoutSeconds.HasValue
                ? new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds.Value))
                : null;
            CancellationToken cancellationToken = cts?.Token ?? CancellationToken.None;
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

                // 同步发送请求并获取响应（复用共享的 _sharedClient，不再每次新建）。
                // 🆕 带上 cancellationToken：超时或调用方主动取消时，SendAsync 会尽快中止这次
                // 请求并抛出 TaskCanceledException/OperationCanceledException，而不是无限期挂起。
                HttpResponseMessage response = _sharedClient.SendAsync(request, cancellationToken).GetAwaiter().GetResult();

                // 🌟 获取服务端返回的 X-Process-Time 响应头
                if (response.Headers.TryGetValues("X-Process-Time", out var values))
                {
                    serverProcessTime = values.FirstOrDefault() ?? string.Empty;
                }

                response.EnsureSuccessStatusCode(); // 确保返回成功状态码

                // 读取响应内容
                return response.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                // 🆕 V20260923_HTTP_TIMEOUT：TaskCanceledException 派生自 OperationCanceledException，
                // HttpClient.Timeout 到期、或本次调用传入的 timeoutSeconds 到期，都会走到这里。
                // 单独捕获并打印一条明确的"请求超时"日志，跟下面通用 catch 里笼统的
                // "请求失败: ex.Message"（对超时来说往往只会打印"The operation was canceled."这种
                // 看不出原因的信息）区分开，方便运维一眼看出是超时而不是连接失败/服务端报错。
                int effectiveTimeout = timeoutSeconds ?? (int)_sharedClient.Timeout.TotalSeconds;
                string timeoutMsg = $"[HTTP Timeout] 请求超时（超过 {effectiveTimeout} 秒未收到响应）: {method} {url}";
                _logger.Error(timeoutMsg);
                return $"Error: {timeoutMsg}";
            }
            catch (Exception ex)
            {
                _logger.Error($"[HTTP Error] 请求失败: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }
}
