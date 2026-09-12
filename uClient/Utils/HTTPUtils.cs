using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Tea;

namespace uClient.Comm
{
    public class HTTPUtils
    {
        static CookieContainer cookieContainer = new CookieContainer(); // 用于管理Session

        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 万州数据获取
        /// </summary>
        /// <param name="category"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static string SendWZRequest(String category, String sort)
        {
            string url = "http://alb-az3c15s6s51xo8gjbc.cn-hongkong.alb.aliyuncs.com:52400/trade/longShortPositions";
            (string Key, string Value)[] headers = new[] {("Cookie","acw_tc=e5652e12b35795e93bf8b14c90deafadbc7fe8d539eff9486598b28153e1564f"),
                ("Cache-Control","no-cache"),
                ("host","alb-az3c15s6s51xo8gjbc.cn-hongkong.alb.aliyuncs.com:52400") };
            var payloadObj = new
            {
                category = category,
                symbol = sort,
                now_bid = ""
            };

            string payload = JsonConvert.SerializeObject(payloadObj);
            return SendSyncRequest(url, "POST", headers, null, payload);
        }

        /// <summary>
        /// 获取金荣数据
        /// </summary>
        /// <param name="category"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static string SendJRRequest(String category, String sort)
        {
            TimestampAndKeyGenerator generator = new TimestampAndKeyGenerator();
            string[] result = generator.GenerateTimestampAndKey("index/jrdkcc").Split(',');
            var url = new StringBuilder();
            url.AppendFormat("http://120.77.237.33:23001/?r=index/jrdkcc&v=v1&key={0}&timestamp={1}&access_token=&device=android&cv=79&lan=chinese&userid=&category={2}&sort={3}",
                result[1], result[0], category, sort);
            return SendSyncRequest(url.ToString(), "POST");
        }

        /// <summary>
        /// 获取DataCenter数据
        /// </summary>
        /// <param name="category"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static string SendDataCenterRequest(String category, String sort)
        {
            TimestampAndKeyGenerator generator = new TimestampAndKeyGenerator();
            string[] result = generator.GenerateTimestampAndKey("index/jrdkcc").Split(',');
            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string url = "https://datacenter-api.jin10.com/sentiment/list";
            (string Key, string Value)[] parameters = new[] { (category, sort) };
            (string Key, string Value)[] headers = new[] { ("authority", "datacenter-api.jin10.com"),
                ("method","GET"),("path","/sentiment/list?_=" + timestamp),("origin","https://datacenter.jin10.com"),("x-app-id","rU6QIu7JHe2gOUeR"),
                ("x-version","1.0.0")  };

            return SendSyncRequest(url.ToString(), "GET", headers, parameters);
        }
        /// <summary>
        /// 获取Dukascopy情绪指数数据
        /// </summary>
        /// <returns></returns>
        public static string SendDukascopyRequest()
        {
            string url = "https://freeserv.dukascopy.com/2.0/api/?group=quotes&method=realtimeSentimentIndex&enabled=true&key=bsq3l3p5lc8w4s0c&type=ccy&jsonp=_callbacks____0mfb8u36w";
            return SendSyncRequest(url.ToString(), "GET");
        }

        /// <summary>
        /// 发送同步 HTTP/HTTPS 请求。
        /// </summary>
        /// <param name="url">请求的 URL</param>
        /// <param name="method">HTTP 方法（GET, POST, PUT, DELETE 等）</param>
        /// <param name="headers">请求头（key-value 格式）</param>
        /// <param name="parameters">URL请求参数（key-value 格式）</param>
        /// <param name="payload">请求体（用于 POST/PUT 请求）</param>
        /// <returns>响应结果字符串</returns>
        public static string SendSyncRequest(string url, string method,
                                             (string Key, string Value)[] headers = null,
                                             (string Key, string Value)[] parameters = null,
                                             string payload = null)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 设置 Header
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }
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

                    // 设置 Payload（适用于 POST/PUT 请求）
                    if (!string.IsNullOrEmpty(payload) &&
                        (method.ToUpper() == "POST" || method.ToUpper() == "PUT"))
                    {
                        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                    }

                    // 同步发送请求并获取响应
                    HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode(); // 确保返回成功状态码

                    // 读取响应内容
                    return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }
        /**
        * 使用阿里云发送短信
        */
        public static string sendSMSByAliyun(string[] noumbers, string content)
        {
            string accessKeyId = "";
            string accessKeySecret = "";
            AlibabaCloud.SDK.Dysmsapi20170525.Client client = CreateClient(accessKeyId, accessKeySecret);
            AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest sendSmsRequest = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest
            {
                PhoneNumbers = string.Join(",", noumbers),
                SignName = "龙知易",
                TemplateCode = "SMS_275075690",//TPL_09041  LJZY_0001
                TemplateParam = content

                //SignName = "阿里云短信测试",
                //TemplateCode = "SMS_154950909",
                //PhoneNumbers = "15994725242",
                //TemplateParam = "{\"code\":\"1234\"}",
            };
            AlibabaCloud.TeaUtil.Models.RuntimeOptions runtime = new AlibabaCloud.TeaUtil.Models.RuntimeOptions();
            try
            {
                // 复制代码运行请自行打印 API 的返回值
                client.SendSmsWithOptions(sendSmsRequest, runtime);
            }
            catch (TeaException error)
            {
                // 如有需要，请打印 error
                AlibabaCloud.TeaUtil.Common.AssertAsString(error.Message);
            }
            catch (Exception _error)
            {
                TeaException error = new TeaException(new Dictionary<string, object>
                {
                    { "message", _error.Message }
                });
                // 如有需要，请打印 error
                AlibabaCloud.TeaUtil.Common.AssertAsString(error.Message);
            }

            return sendSmsRequest.ToString();
        }

        /**
         * 使用AK&SK初始化账号Client
         * @param accessKeyId
         * @param accessKeySecret
         * @return Client
         * @throws Exception
         */
        public static AlibabaCloud.SDK.Dysmsapi20170525.Client CreateClient(string accessKeyId, string accessKeySecret)
        {
            //AccessKeyId: LTAI4GAU6dX9U3ZfncJvK3fV
            // AccessKeySecret:2ySJ8WYdkk45VogXQSTrRixxjl8hs8
            AlibabaCloud.OpenApiClient.Models.Config config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                // 必填，您的 AccessKey ID
                AccessKeyId = accessKeyId,
                // 必填，您的 AccessKey Secret
                AccessKeySecret = accessKeySecret,
            };
            // 访问的域名
            config.Endpoint = "dysmsapi.aliyuncs.com";
            return new AlibabaCloud.SDK.Dysmsapi20170525.Client(config);
        }

        /// <summary>
        /// 获取Token
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string HorseGetToken(string userCode, string password)
        {
            string url = "https://api.ma288.com/members/oauth/token";
            string payload = "grant_type=password&username=" + userCode + "&password=" + password + "&d=0.9503883153227932";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("Authorization", "Basic bWEyODhyYWRpbzptYTI4OG1hMjg4bWEyODg=");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36";

            // 绑定 CookieContainer
            request.CookieContainer = cookieContainer;

            // 发送数据
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(payload);
            }

            // 获取响应
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string result = reader.ReadToEnd();
                JObject json = JObject.Parse(result);

                // 提取 Access Token
                string accessToken = json["access_token"]?.ToString();

                // 存入 CookieContainer
                if (!string.IsNullOrEmpty(accessToken))
                {
                    Uri uri = new Uri("https://www.ma288.com");
                    cookieContainer.Add(uri, new Cookie("auth_token", accessToken));
                }

                return accessToken;
            }
        }
        /// <summary>
        /// 获取参数raceId,currentRaceDate
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static string[] GetFirstPage(string accessToken)
        {
            string url = "https://www.ma288.com/has/zh_TW/odds/oddsTrendAction_getPersonalPage.do?raceNo=0&displayType=5";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36";
            request.Referer = "https://www.ma288.com/has/zh_TW/home/index.do";
            request.CookieContainer = cookieContainer; // 维持Session

            // 确保 Token 被传递
            request.Headers.Add("Cookie", $"locale=zh_hk; auth_token={accessToken};");

            // 获取响应
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string html = reader.ReadToEnd();

                // 解析 Race ID 和 Race Date
                string raceId = Regex.Match(html, @"var\s+raceId\s*=\s*(\d+)")?.Groups[1].Value;
                string currentRaceDate = Regex.Match(html, @"var\s+currentRaceDate\s*=\s*'(\d{2}/\d{2}/\d{4})'")?.Groups[1].Value;

                return new string[] { raceId, currentRaceDate };
            }
        }
        /// <summary>
        /// 获取最新下注数据
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="raceId"></param>
        /// <param name="currentRaceDate"></param>
        /// <returns>
        /// ||280K|7|W|22:25|Wed Mar 05 22:25:30 GMT+08:00 2025
        /// </returns>
        public static List<List<string>> GetFirstData(string accessToken, string raceId, string currentRaceDate)
        {
            string url = "https://www.ma288.com/has/zh_TW/odds/oddsTrendAction_getAllTicketsHTML.do";
            string payload = $"noOfRow=15&raceId={raceId}&oddsType=WIN,PLA,QIN,QPL,DBL,TRI,LDBL-2,FCT&setting=sort1&decorator=blank&confirm=true";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookieContainer;
            request.Headers.Add("Cookie", $"locale=zh_hk; auth_token={accessToken};");

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(payload);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string html = reader.ReadToEnd();
                    return ExtractBetDataList(html);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
            return null;
        }
        /// <summary>
        /// 返回数据中,0为最新数据
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<List<string>> ExtractBetDataList(string html)
        {
            if (!string.IsNullOrEmpty(html))
            {
                html = html.Replace("&nbsp;", "");
                //Console.WriteLine(html);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                // 逆序遍历 class="nt" 的表格
                var tables = doc.DocumentNode.SelectNodes("//table[contains(@class, 'nt')]")?.Reverse();
                if (tables != null)
                {
                    List<List<string>> allRowsData = new List<List<string>>();
                    foreach (var table in tables)
                    {
                        // 遍历所有 <tr>
                        var rows = table.SelectNodes(".//tr");
                        if (rows == null) continue;
                        //第一条是最新数据
                        rows.Reverse();

                        foreach (var row in rows)
                        {
                            List<string> rowData = new List<string>();

                            // 解析普通的 <td>
                            foreach (var td in row.SelectNodes(".//td") ?? new HtmlNodeCollection(null))
                            {
                                string text = td.InnerText.Trim();
                                rowData.Add(WebUtility.HtmlDecode(text));
                            }

                            // 解析被注释的 <td>
                            string rowHtml = row.InnerHtml;
                            Regex regex = new Regex(@"<!--\s*(td\s+[^>]*>.*?<\/td)\s*-->", RegexOptions.Singleline);
                            MatchCollection matches = regex.Matches(rowHtml);

                            foreach (Match match in matches)
                            {
                                string tdHtml = "<" + match.Groups[1].Value + ">";
                                HtmlDocument tempDoc = new HtmlDocument();
                                tempDoc.LoadHtml(tdHtml);
                                var tdNode = tempDoc.DocumentNode.SelectSingleNode("//td");
                                if (tdNode != null)
                                {
                                    string text = tdNode.InnerText.Trim();
                                    rowData.Add(WebUtility.HtmlDecode(text));
                                }
                            }
                            //||280K|7|W|22:25|Wed Mar 05 22:25:30 GMT+08:00 2025
                            allRowsData.Add(rowData);
                        }
                    }
                    return allRowsData;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定文件的测试数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static List<List<string>> GetSimulateDataList(string fileName)
        {
            string baseDirectory = "C:\\eadata\\TestData\\horsedata";
            string filePath = Path.Combine(baseDirectory, fileName);
            //读取文件的所有行，并跳过前7行
            string html = string.Join(Environment.NewLine, File.ReadLines(filePath).Skip(7));
            List<List<string>> dataList = ExtractBetDataList(html);
            return dataList;
            // 保存到 CSV 文件
            //string outputCsvPath = @"C:\Users\caiqi\Desktop\datas\output.txt";     // 结果保存为 CSV 文件
            //File.WriteAllLines(outputCsvPath, extractedData.Select(row => string.Join(",", row)));
            //Console.WriteLine("数据已成功提取并保存到：" + outputCsvPath);
        }
        // 清理字符串：去掉 &nbsp; 和多余空格
        public static string CleanText(string input)
        {
            return Regex.Replace(input, @"\s*&nbsp;\s*", "").Trim();
        }
    }
}
