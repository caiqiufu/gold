using M4.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace uClient.Comm
{
    public class Utils
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 日志信息的根目录
        /// </summary>
        public static string _LogInfoRootPath = "C:\\LogInfo";
        /// <summary>
        /// 交易日志
        /// </summary>
        public static string _TradeInfo = "TradeInfo";
        /// <summary>
        /// 运行Debug日志
        /// </summary>
        public static string _Logs = "Logs";
        /// <summary>
        /// 运行信息
        /// </summary>
        public static string _Info = "Info";
        //public static StreamWriter sw = null;
        //public static FileStream fs;
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        /// <summary>
        /// 写入文件,如果目录不存在则创建目录
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="msg"></param>
        public static void AddMsgToTXT(string path, string fileName, string msg)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string destFile = System.IO.Path.Combine(path, fileName);
            if (!File.Exists(destFile))
            {
                //报错xxxxxxxx...because it is being used by another process.创建完后关闭文件即可。
                File.Create(destFile).Close();
            }
            //FileStream fs = new FileStream(destFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
            _lock.EnterUpgradeableReadLock();
            try
            {
                _lock.EnterWriteLock();
                try
                {
                    FileStream fs = File.OpenWrite(destFile);
                    try
                    {
                        // 将待写的入数据从字符串转换为字节数组
                        Encoding encoder = Encoding.UTF8;
                        byte[] bytes = encoder.GetBytes(msg + "\r\n");
                        //设定书写的开始位置为文件的末尾  
                        fs.Position = fs.Length;
                        //将待写入内容追加到文件末尾  
                        fs.Write(bytes, 0, bytes.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message + "  " + ex.StackTrace);
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
            //sw.WriteLine(msg);
            //sw.Flush();
        }
        /// <summary>
        /// 写入文件，有文件头
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="header"></param>
        /// <param name="msg"></param>
        public static void AddMsgToTXT(string path, string fileName, string header, string msg)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string destFile = System.IO.Path.Combine(path, fileName);
            bool isExist = true;
            if (!File.Exists(destFile))
            {
                //报错xxxxxxxx...because it is being used by another process.创建完后关闭文件即可。
                File.Create(destFile).Close();
                isExist = false;
            }
            //FileStream fs = new FileStream(destFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
            _lock.EnterUpgradeableReadLock();
            try
            {
                _lock.EnterWriteLock();
                try
                {
                    FileStream fs = File.OpenWrite(destFile);
                    try
                    {
                        // 将待写的入数据从字符串转换为字节数组
                        Encoding encoder = Encoding.UTF8;
                        if (!isExist && !string.IsNullOrEmpty(header))
                        {
                            byte[] byteHeader = encoder.GetBytes(header + "\r\n");
                            //设定书写的开始位置为文件的末尾  
                            fs.Position = fs.Length;
                            //将待写入内容追加到文件末尾  
                            fs.Write(byteHeader, 0, byteHeader.Length);
                        }
                        byte[] bytes = encoder.GetBytes(msg + "\r\n");
                        //设定书写的开始位置为文件的末尾  
                        fs.Position = fs.Length;
                        //将待写入内容追加到文件末尾  
                        fs.Write(bytes, 0, bytes.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message + "  " + ex.StackTrace);
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
            //sw.WriteLine(msg);
            //sw.Flush();
        }
        /// <summary>
        /// 写入文件,数据是List
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="header"></param>
        /// <param name="msgs"></param>
        public static void AddMsgToTXT(string path, string fileName, string header, string[] msgs)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string destFile = System.IO.Path.Combine(path, fileName);
            bool isExist = true;
            if (!File.Exists(destFile))
            {
                //报错xxxxxxxx...because it is being used by another process.创建完后关闭文件即可。
                File.Create(destFile).Close();
                isExist = false;
            }
            //FileStream fs = new FileStream(destFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
            _lock.EnterUpgradeableReadLock();
            try
            {
                _lock.EnterWriteLock();
                try
                {
                    FileStream fs = File.OpenWrite(destFile);
                    try
                    {
                        // 将待写的入数据从字符串转换为字节数组
                        Encoding encoder = Encoding.UTF8;
                        if (!isExist && !string.IsNullOrEmpty(header))
                        {
                            byte[] byteHeader = encoder.GetBytes(header + "\r\n");
                            //设定书写的开始位置为文件的末尾  
                            fs.Position = fs.Length;
                            //将待写入内容追加到文件末尾  
                            fs.Write(byteHeader, 0, byteHeader.Length);
                        }
                        foreach (string msg in msgs)
                        {
                            byte[] bytes = encoder.GetBytes(msg + "\r\n");
                            //设定书写的开始位置为文件的末尾  
                            fs.Position = fs.Length;
                            //将待写入内容追加到文件末尾  
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message + "  " + ex.StackTrace);
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
            //sw.WriteLine(msg);
            //sw.Flush();
        }
        public static string GenerateDirectoryPath(string baseDirectory)
        {
            System.DateTime currentTime = System.DateTime.Now;
            string directory = currentTime.ToString("yyyyMMdd");
            //string path = AppDomain.CurrentDomain.BaseDirectory + "\\" + baseDirectory + "\\" + directory;
            //string path = Application.StartupPath + "\\" + baseDirectory + "\\" + directory;
            //string rootPath = "C:\\eadata";
            string path = _LogInfoRootPath + "\\" + baseDirectory + "\\" + directory;
            return path;
        }
        /// <summary>
        /// 写入信息到交易日志
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="accountCode">账户</param>
        public static void AddTradeMsg(string msg, string accountCode)
        {
            string path = GenerateDirectoryPath("TradeInfo");
            System.DateTime currentTime = System.DateTime.Now;
            string fileName = currentTime.ToString("yyyy-MM-dd") + "_" + accountCode;
            AddMsgToTXT(path, fileName + ".txt", msg);
        }
        /// <summary>
        /// 日志文件
        /// </summary>
        /// <param name="path">如果未指定目录,默认为TradeInfo</param>
        /// <param name="fileDate">如果未知道日期,默认为当前日期</param>
        /// <param name="type">文件类型,必填值</param>
        /// <param name="header">文件头,可以为空</param>
        /// <param name="msg">文件消息</param>
        public static void AddTradeMsg(string path, string fileDate, string type, string header, string msg)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GenerateDirectoryPath("TradeInfo");
            }
            if (string.IsNullOrEmpty(fileDate))
            {
                System.DateTime currentTime = System.DateTime.Now;
                fileDate = currentTime.ToString("yyyy-MM-dd");
            }
            string fileName = fileDate + "_" + type;
            if (!string.IsNullOrEmpty(msg))
            {
                AddMsgToTXT(path, fileName + ".txt", header, msg);
            }
        }
        /// <summary>
        /// 日志文件
        /// </summary>
        /// <param name="path">如果未指定目录,默认为TradeInfo</param>
        /// <param name="fileDate">如果未知道日期,默认为当前日期</param>
        /// <param name="type">文件类型,必填值</param>
        /// <param name="header">文件头,可以为空</param>
        /// <param name="msgList">文件消息</param>
        public static void AddTradeMsg(string path, string fileDate, string type, string header, string[] msgList)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GenerateDirectoryPath("TradeInfo");
            }
            if (string.IsNullOrEmpty(fileDate))
            {
                System.DateTime currentTime = System.DateTime.Now;
                fileDate = currentTime.ToString("yyyy-MM-dd");
            }
            string fileName = fileDate + "_" + type;
            if (msgList != null && msgList.Length > 0)
            {
                AddMsgToTXT(path, fileName + ".txt", header, msgList);
            }
        }

        public static List<string> ReadFile(string filePath)
        {
            //string fpath = Application.StartupPath + "\\" + filePath;
            String line;
            List<string> data = new List<string>();
            try
            {
                StreamReader sr = new StreamReader(filePath);
                line = sr.ReadLine();
                while (line != null)
                {
                    //write the line to console window
                    //Console.WriteLine(line);
                    data.Add(line);
                    //Read the next line
                    line = sr.ReadLine();
                }
                //close the file
                sr.Close();
                //Console.ReadLine();
                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            return null;
        }
        public static string GetLocalIP()
        {
            string name = System.Net.Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            foreach (IPAddress ipa in ipadrlist)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipa.ToString();
                }
            }
            return "";
        }
        public static Dictionary<string, object> GetSystemMessageReceived(SystemMessage message)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("Message", message.Message);
            dic.Add("MessageType", message.MessageType.ToString());
            dic.Add("MessageTime", message.MessageTime);
            dic.Add("OrderStatus", message.Title);
            foreach (var item in message.MessageItems)
            {
                if (Enum.Contract.Contains(item.Key))
                {
                    dic.Add("Contract", item.Value);
                }
                else if (Enum.OrderType.Contains(item.Key))
                {
                    dic.Add("OrderType", item.Value);
                }
                else if (Enum.TradeSide.Contains(item.Key))
                {
                    dic.Add("TradeSide", item.Value);
                }
                else if (Enum.LiquidationType.Contains(item.Key))
                {
                    dic.Add("LiquidationType", item.Value);
                }
                else if (Enum.Lots.Contains(item.Key))
                {
                    dic.Add("Lots", item.Value);
                }
                else if (Enum.Price.Contains(item.Key))
                {
                    dic.Add("Price", item.Value);
                }
                else if (Enum.ServerPositionRef.Contains(item.Key))
                {
                    dic.Add("ServerPositionRef", item.Value);
                }
                else if (Enum.OpenPositionReference.Contains(item.Key))
                {
                    dic.Add("OpenPositionReference", item.Value);
                }
                else if (Enum.Reason.Contains(item.Key))
                {
                    dic.Add("Reason", item.Value);
                }
                else if (Enum.ExecutionTime.Contains(item.Key))
                {
                    dic.Add("ExecutionTime", item.Value);
                }
                else if (Enum.Profit.Contains(item.Key))
                {
                    dic.Add("Profit", item.Value);
                }
                else
                {
                    dic.Add(item.Key, item.Value);
                }
            }
            return dic;
            //市价单被接纳 | 合約:本地伦敦金 | 订单类型:市价单 | 卖出/买进:买进 | 手数:0.1 | 执行价格:1847.41 | 止盈价格:- | 止损价格:- | 编号:161639
            //市价单被拒绝 [合約, 本地伦敦金] [订单类型, 市价单] [卖出/买进, 买进] [手数, 0.1] [止盈价格, -] [止损价格, -] [原因, 价格已变动]
            //平仓被接纳 [合約, 本地伦敦金] [平仓类型, 市价平仓] [卖出/买进, 卖出] [手数, 0.1] [执行价格, 1860.88] [开仓编号, 160679] [编号, 160738] [盈亏, -89.70] [平仓时间, 2021-01-12 19:33] [备注, -]
            //平仓被拒绝 [合約, 本地伦敦金] [平仓类型, 市价平仓] [卖出/买进, 卖出] [手数, 0.1] [指 令价格, 1862.06] [开仓编号, 160114] [原因, 价格已变动]
            //[1/23/2021 12:30:45 AM] [Information] Liquidation Accepted [Contract, LLGUSD] [Liquidation Type, Market Liquidation] [Sell/Buy, Sell] [Lots, 0.3] [Execution Price, 1853.33] [Open Position Reference, 175236] [Ref., 175238] [Profit/Loss, 84.24] [Liquidation Time, 2021-01-23 00:30] [Remark, -]
        }
        /// <summary>
        /// 交易日志
        /// </summary>
        /// <param name="lstTradeRecord"></param>
        /// <param name="_Account"></param>
        /// <param name="msg"></param>
        public static void LogTradeRecord(ListBox lstTradeRecord, Comm.Account _Account, string msg)
        {
            if (lstTradeRecord.InvokeRequired)
            {
                lstTradeRecord.Invoke(new Action<ListBox, Comm.Account, string>(LogTradeRecord), new object[] { lstTradeRecord, _Account, msg });
            }
            else
            {
                lstTradeRecord.TopIndex = lstTradeRecord.Items.Count - 1;
                lstTradeRecord.Items.Add(msg);
                Utils.AddTradeMsg(msg, _Account.UserCode);
                if (msg.Contains("[接收]"))
                {
                    Utils.AddTradeMsg("", _Account.UserCode);
                }
            }
            if (lstTradeRecord.Items.Count > 50)
            {
                lstTradeRecord.Items.RemoveAt(lstTradeRecord.Items.Count - 1);
            }
            _logger.Info(msg);
        }
        /// <summary>
        /// 系统日志
        /// </summary>
        /// <param name="msg"></param>
        public static void Log(ListBox lstLog, string msg)
        {
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action<ListBox, string>(Log), new object[] { lstLog, msg });
            }
            else
            {
                lstLog.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss.fff") + " " + msg);
                if (lstLog.Items.Count > 50)
                {
                    lstLog.Items.RemoveAt(lstLog.Items.Count - 1);
                }
                _logger.Info(msg);
            }
        }

        /// <summary>
        /// 判断是锁窗口
        /// </summary>
        /// <param name="formType"></param>
        /// <returns></returns>
        public static bool IsLForm(string formType)
        {
            return "L1,L2,L3,L4,L5,L6,L7,L8,L9".Contains(formType);
        }
        /// <summary>
        /// 判断是主窗口
        /// </summary>
        /// <param name="formType"></param>
        /// <returns></returns>
        public static bool IsMForm(string formType)
        {
            return "M1,M2,M3,M4,M5,M6,M7,M8,M9".Contains(formType);
        }
        /// <summary>
        /// 根据窗口类型获取绑定端口
        /// </summary>
        /// <param name="formType"></param>
        /// <returns></returns>
        public static string GetPort(string formNo)
        {
            string port = "5501";
            switch (formNo)
            {
                //价格数据源
                case "PS":
                    port = "5558";
                    break;
                case "MF4Trade1":
                    port = "5501";
                    break;
                case "MF4Trade2":
                    port = "5502";
                    break;
                case "MF4Trade3":
                    port = "5503";
                    break;
                case "MF4Trade4":
                    port = "5504";
                    break;
                case "MF4Trade5":
                    port = "5505";
                    break;
                case "MF4Trade6":
                    port = "5506";
                    break;
                case "MT4Trade1":
                    port = "5511";
                    break;
                case "MT4Trade2":
                    port = "5512";
                    break;
                case "MT4Trade3":
                    port = "5513";
                    break;
                case "MT4Trade4":
                    port = "5514";
                    break;
                case "MT4Trade5":
                    port = "5515";
                    break;
                case "MT4Trade6":
                    port = "5516";
                    break;
                case "MT5Trade1":
                    port = "5521";
                    break;
                case "MT5Trade2":
                    port = "5522";
                    break;
                case "MT5Trade3":
                    port = "5523";
                    break;
                case "MT5Trade4":
                    port = "5524";
                    break;
                case "MT5Trade5":
                    port = "5525";
                    break;
                case "MT5Trade6":
                    port = "5526";
                    break;
                case "EACTrade1":
                    port = "5531";
                    break;
                case "EACTrade2":
                    port = "5532";
                    break;
                case "EACTrade3":
                    port = "5533";
                    break;
                case "EACTrade4":
                    port = "5534";
                    break;
                case "EACTrade5":
                    port = "5535";
                    break;
                case "EACTrade6":
                    port = "5536";
                    break;
                case "EACTrade11":
                    port = "5541";
                    break;
                case "EACTrade12":
                    port = "5542";
                    break;
                case "EACTrade13":
                    port = "5543";
                    break;
                case "EACTrade14":
                    port = "5544";
                    break;
                case "EACTrade15":
                    port = "5545";
                    break;
                case "EACTrade16":
                    port = "5546";
                    break;
                case "EACTrade21":
                    port = "5551";
                    break;
                case "EACTrade22":
                    port = "5552";
                    break;
                case "EACTrade23":
                    port = "5553";
                    break;
                case "EACTrade24":
                    port = "5554";
                    break;
                case "EACTrade25":
                    port = "5555";
                    break;
                case "EACTrade26":
                    port = "5556";
                    break;
                case "Trade1":
                    port = "5561";
                    break;
                case "Trade2":
                    port = "5562";
                    break;
                case "Trade3":
                    port = "5563";
                    break;
                case "Trade4":
                    port = "5564";
                    break;
                case "Trade5":
                    port = "5565";
                    break;
                case "Trade6":
                    port = "5566";
                    break;
                case "Trade7":
                    port = "5571";
                    break;
                case "Trade8":
                    port = "5572";
                    break;
                case "Trade9":
                    port = "5573";
                    break;
                case "Trade10":
                    port = "5574";
                    break;
                case "Trade11":
                    port = "5575";
                    break;
                case "Trade12":
                    port = "5576";
                    break;
                case "Trade13":
                    port = "5581";
                    break;
                case "Trade14":
                    port = "5582";
                    break;
                case "Trade15":
                    port = "5583";
                    break;
                case "Trade16":
                    port = "5584";
                    break;
                case "Trade17":
                    port = "5585";
                    break;
                case "Trade18":
                    port = "5586";
                    break;
            }
            return port;
        }
        //公用方法
        public static void ClearBrokerDiff(QuotaDisplayPanel quotaDisplayPanel)
        {
            quotaDisplayPanel.lblMT4Speed.Text = "0ms";
            quotaDisplayPanel.lblMT4Bid.Text = "0";
            quotaDisplayPanel.lblMT4BidDiff0.Text = "0";
            quotaDisplayPanel.lblMT4BidDiff1.Text = "0";
            quotaDisplayPanel.lblMT4BidDiff2.Text = "0";
            quotaDisplayPanel.lblMT4BidDiff3.Text = "0";
            quotaDisplayPanel.lblMT4BidDiff4.Text = "0";
            quotaDisplayPanel.lblMT4Ask.Text = "0";
            quotaDisplayPanel.lblMT4AskDiff0.Text = "0";
            quotaDisplayPanel.lblMT4AskDiff1.Text = "0";
            quotaDisplayPanel.lblMT4AskDiff2.Text = "0";
            quotaDisplayPanel.lblMT4AskDiff3.Text = "0";
            quotaDisplayPanel.lblMT4AskDiff4.Text = "0";
        }
        public static void UpdateQuoteDisplay(QuotaDisplayPanel quotaDisplayPanel, QuotePanelData quoteData, decimal bid, decimal ask)
        {
            quotaDisplayPanel.lblMT4Speed.Text = quoteData.Speed.ToString();
            quotaDisplayPanel.lblMT4Bid.Text = bid.ToString("f2");
            quotaDisplayPanel.lblMT4BidDiff0.Text = quoteData.BidDiff[0].ToString();
            quotaDisplayPanel.lblMT4BidDiff1.Text = quoteData.BidDiff[1].ToString();
            quotaDisplayPanel.lblMT4BidDiff2.Text = quoteData.BidDiff[2].ToString();
            quotaDisplayPanel.lblMT4BidDiff3.Text = quoteData.BidDiff[3].ToString();
            quotaDisplayPanel.lblMT4BidDiff4.Text = quoteData.BidDiff[4].ToString();

            quotaDisplayPanel.lblMT4Ask.Text = ask.ToString("f2");
            quotaDisplayPanel.lblMT4AskDiff0.Text = quoteData.AskDiff[0].ToString();
            quotaDisplayPanel.lblMT4AskDiff1.Text = quoteData.AskDiff[1].ToString();
            quotaDisplayPanel.lblMT4AskDiff2.Text = quoteData.AskDiff[2].ToString();
            quotaDisplayPanel.lblMT4AskDiff3.Text = quoteData.AskDiff[3].ToString();
            quotaDisplayPanel.lblMT4AskDiff4.Text = quoteData.AskDiff[4].ToString();
        }
        public static void ClearAccountCaptial(AccountDisplay _AccountDisplay)
        {
            _AccountDisplay.lblBlance.Text = "8888.88";
            _AccountDisplay.lblEquity.Text = "8888.88";
            _AccountDisplay.lblMargin.Text = "8888.88";
            _AccountDisplay.lblFreeMargin.Text = "8888.88";
        }
        public static bool PortInUse(int port)
        {
            bool inUse = false;
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }

        /// <summary>
        /// 深拷贝（通过反射）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepCopyByReflect<T>(T obj)
        {
            //如果是字符串或值类型则直接返回
            if (obj == null || obj is string || obj.GetType().IsValueType) return obj;
            object retval = Activator.CreateInstance(obj.GetType());
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (FieldInfo field in fields)
            {
                try
                {
                    field.SetValue(retval, DeepCopyByReflect(field.GetValue(obj)));
                }
                catch { }
            }
            return (T)retval;
        }

        /// <summary>
        /// Clones the specified list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="List">The list.</param>
        /// <returns>List{``0}.</returns>
        public static List<T> Clone<T>(object List)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, List);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as List<T>;
            }
        }
        /// <summary>
        /// 删除文件夹下面的所有文件和文件夹
        /// </summary>
        /// <param name="fileDir"></param>
        /// <param name="IsDeleteDir"></param>
        public static void DeleteDirectory(string fileDir, bool IsDeleteDir)
        {
            DirectoryInfo dir = new DirectoryInfo(fileDir);
            if (dir.Exists)
            {
                Directory.Delete(fileDir, true);
                if (!IsDeleteDir)
                {
                    Directory.CreateDirectory(fileDir);
                }
            }
        }
        /// <summary>
        /// 判断是否是debug模式
        /// </summary>
        public static bool RunningModeIsDebug
        {
            get
            {
                var assebly = Assembly.GetEntryAssembly();
                if (assebly == null)
                {
                    assebly = new StackTrace().GetFrames().Last().GetMethod().Module.Assembly;
                }

                var debugableAttribute = assebly.GetCustomAttribute<DebuggableAttribute>();
                var isdebug = debugableAttribute.DebuggingFlags.HasFlag(DebuggableAttribute.DebuggingModes.EnableEditAndContinue);

                return isdebug;
            }
        }
        /// <summary>
        /// 转换Dictionary为String
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static string ConvertDictionaryToString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            var sb = new StringBuilder();

            foreach (var pair in dictionary)
            {
                sb.Append(pair.Key.ToString());
                sb.Append(" = ");
                sb.Append(pair.Value.ToString());
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 修改时间段字符串的“开始时间” (横杠前的部分)
        /// </summary>
        /// <param name="duration">原始字符串 (如 "04:45-07:15")</param>
        /// <param name="minutes">偏移分钟数 (正数往后挪，负数往前挪)</param>
        /// <returns>修改后的完整字符串</returns>
        public static string UpdateStartTime(string duration, int minutes)
        {
            return ModifySpecificPart(duration, minutes, true);
        }

        /// <summary>
        /// 修改时间段字符串的“结束时间” (横杠后的部分)
        /// </summary>
        /// <param name="duration">原始字符串 (如 "04:45-07:15")</param>
        /// <param name="minutes">偏移分钟数 (正数往后挪，负数往前挪)</param>
        /// <returns>修改后的完整字符串</returns>
        public static string UpdateEndTime(string duration, int minutes)
        {
            return ModifySpecificPart(duration, minutes, false);
        }

        /// <summary>
        /// 核心私有处理逻辑
        /// </summary>
        private static string ModifySpecificPart(string duration, int minutes, bool isStartPart)
        {
            if (string.IsNullOrWhiteSpace(duration) || !duration.Contains("-"))
                return duration;

            string[] parts = duration.Split('-');
            if (parts.Length != 2) return duration;

            string targetStr = isStartPart ? parts[0] : parts[1];

            if (TimeSpan.TryParse(targetStr, out TimeSpan time))
            {
                // 计算新时间并处理 24 小时循环
                TimeSpan newTime = time.Add(TimeSpan.FromMinutes(minutes));

                if (newTime.Ticks < 0)
                    newTime = newTime.Add(TimeSpan.FromHours(24));
                else if (newTime.TotalHours >= 24)
                    newTime = newTime.Subtract(TimeSpan.FromHours(24));

                string newTimeStr = newTime.ToString(@"hh\:mm");

                return isStartPart ? $"{newTimeStr}-{parts[1]}" : $"{parts[0]}-{newTimeStr}";
            }

            return duration;
        }

        /// <summary>
        /// 判断当前时间是否为非交易时间,在交易时间内返回true, 非交易时间还回false
        /// 如果noTradeDuration时间为空,默认值为04:45-06:05(夏令时间)
        /// </summary>
        /// <param name="noTradeDuration">禁止交易时间段，格式: "21:00-06:00,12:00-13:00"</param>
        /// <param name="currentTime">当前时间，格式 yyyy-MM-dd HH:mm:ss</param>
        /// <param name="tradeTime">交易时间,DST 夏令时间,ST 冬令时间</param>
        /// <returns>true = 可交易, false = 不可交易</returns>
        public static bool checkIsTradeTime(string noTradeDuration, string currentTime)
        {
            //Console.WriteLine("currentTime:"+ currentTime);
            if (string.IsNullOrEmpty(currentTime))
            {
                return false; // 不允许交易
            }
            if (string.IsNullOrEmpty(noTradeDuration))
            {
                noTradeDuration = "04:45-07:15";
            }
            DateTime now = DateTime.ParseExact(currentTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            if (IsWeekendBlocked(now, noTradeDuration))
            {
                return false; // 不允许交易
            }
            if (!string.IsNullOrEmpty(noTradeDuration))
            {
                // 规则2：检查配置的禁止交易时段
                foreach (var duration in noTradeDuration.Split(','))
                {
                    var times = duration.Split('-');
                    if (times.Length != 2) continue;

                    TimeSpan start = TimeSpan.Parse(times[0]);
                    TimeSpan end = TimeSpan.Parse(times[1]);
                    TimeSpan current = now.TimeOfDay;

                    if (start < end)
                    {
                        // 正常区间，例如 12:00-13:00
                        if (current >= start && current < end)
                            // 不允许交易
                            return false;
                    }
                    else
                    {
                        // 跨天区间，例如 21:00-06:00 
                        if (current >= start || current < end)
                            // 不允许交易
                            return false;
                    }
                }
            }
            return true; // 允许交易
        }

        /// <summary>
        /// 判断最近3分钟内是否已经获取到K线数据
        /// </summary>
        /// <returns></returns>
        public static bool checkKLineCollectionStatus()
        {
            List<Candle> M1Datas = DBHelper.GetRecentCandles("XAUUSD", "ONE_MIN", 3, DBUtils.GetOffsetTime(-3));
            if (M1Datas == null || M1Datas.Count == 0)
            {
                return false;
            }

            var firstOpen = M1Datas[0].Open;

            // 只要有一个不相同就返回 true
            return M1Datas.Any(c => c.Open != firstOpen);
        }
        /// <summary>
        /// 判断报价获取是否正常
        /// </summary>
        /// <returns></returns>
        public static bool checkQuotaCollectionStatus()
        {
            List<IDictionary<string, string>> datas = DBHelper.GetQuotaStatus("dukascopy", 3, DBUtils.GetOffsetTime(-6));

            // 数据不足
            if (datas == null || datas.Count < 3)
                return false;

            try
            {
                double p1 = Convert.ToDouble(datas[0]["price"]);
                double p2 = Convert.ToDouble(datas[1]["price"]);
                double p3 = Convert.ToDouble(datas[2]["price"]);

                // 误差范围（根据交易品种调整）
                double tolerance = 0.001;   // XAUUSD 建议 0.001
                                            // double tolerance = 0.0001; // 外汇建议

                // 如果价格几乎没变化 → 认为行情冻结
                if (Math.Abs(p1 - p2) < tolerance &&
                    Math.Abs(p2 - p3) < tolerance)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 禁止交易时间:周六5点之后,星期天,周一7点之前
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        private static bool IsWeekendBlocked(DateTime now, string tradeTime)
        {
            // 解析时间段 04:45-06:15
            var parts = tradeTime.Split('-');
            TimeSpan start = TimeSpan.Parse(parts[0]);
            TimeSpan end = TimeSpan.Parse(parts[1]);

            // 星期六：超过 start 禁止
            if (now.DayOfWeek == DayOfWeek.Saturday && now.TimeOfDay >= start)
            {
                return true;
            }

            // 星期天：全天禁止
            if (now.DayOfWeek == DayOfWeek.Sunday)
            {
                return true;
            }

            // 星期一：早于 end 禁止
            if (now.DayOfWeek == DayOfWeek.Monday && now.TimeOfDay < end)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// 判断是否是周六交易结束时间
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="tradeTime"></param>
        /// <returns></returns>
        public static bool checkIsSaturdayEndTime(string currentTime, string tradeTime)
        {
            DateTime now = DateTime.ParseExact(currentTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            return IsWeekendBlocked(now, tradeTime);
        }
        /// <summary>
        /// 删除已存在的文件
        /// </summary>
        /// <param name="filePath"></param>
        public static void deleteFile(string path, string fileName)
        {
            string destFile = System.IO.Path.Combine(path, fileName);
            if (File.Exists(destFile))
            {
                File.Delete(destFile);
            }
        }
        /// <summary>
        /// 查询交易日内的订单数量
        /// </summary>
        /// <param name="noTradeDuration"></param>
        /// <param name="currentDateTime"></param>
        /// <returns></returns>
        public static int orderCountForDuration(string eaType, string noTradeDuration, string currentDateTime)
        {
            //当前时间
            DateTime currentDate = DateTime.Parse(currentDateTime);
            // 当天凌晨5点
            DateTime startTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 5, 0, 0);
            // 如果当前时间早于当天5点，则回到前一天5点
            if (currentDate < startTime)
            {
                startTime = startTime.AddDays(-1);
            }
            // 结束时间 = 开始时间 + 1天
            DateTime endTime = startTime.AddDays(1);
            string startDate = startTime.ToString("yyyy-MM-dd HH:mm:ss");
            string endDate = endTime.ToString("yyyy-MM-dd HH:mm:ss");

            int orderCount = DBHelper.getOrderCountForDuration(eaType, startDate, endDate);
            return orderCount;
        }
        /// <summary>
        /// 查询交易日内的订单数量
        /// </summary>
        /// <param name="testBatchNo"></param>
        /// <param name="selectedStrategy"></param>
        /// <param name="eaType"></param>
        /// <param name="tradeTime"></param>
        /// <param name="noTradeDuration"></param>
        /// <param name="currentDateTime"></param>
        /// <returns></returns>
        public static int orderCountForDurationForTest(string testBatchNo, string selectedStrategy, string eaType, string noTradeDuration, string currentDateTime)
        {
            //当前时间
            DateTime currentDate = DateTime.Parse(currentDateTime);
            // 当天凌晨5点
            DateTime startTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 5, 0, 0);
            // 如果当前时间早于当天5点，则回到前一天5点
            if (currentDate < startTime)
            {
                startTime = startTime.AddDays(-1);
            }
            // 结束时间 = 开始时间 + 1天
            DateTime endTime = startTime.AddDays(1);
            string startDate = startTime.ToString("yyyy-MM-dd HH:mm:ss");
            string endDate = endTime.ToString("yyyy-MM-dd HH:mm:ss");

            int orderCount = DBHelper.getOrderCountForDurationForTest(testBatchNo, selectedStrategy, eaType, startDate, endDate);
            return orderCount;
        }

        /// <summary>
        /// 本周内是否连续亏损
        /// </summary>
        /// <param name="eaType"></param>
        /// <param name="lossCount"></param>
        /// <returns></returns>
        public static bool orderCountForContinuousLossesWeekly(string eaType, string currentDateTimeStr, int lossCount)
        {
            return DBHelper.getOrderCountForContinuousLossesWeekly(eaType, currentDateTimeStr, lossCount);
        }
        /// <summary>
        /// 判断本周总亏损单数是否已经超过给定值
        /// </summary>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="lossCount"></param>
        /// <returns></returns>
        public static bool getOrderCountForTotalLossesWeekly(string eaType, string currentDateTimeStr, int lossCount)
        {
            return DBHelper.getOrderCountForTotalLossesWeekly(eaType, currentDateTimeStr, lossCount);
        }
        /// <summary>
        /// 本周内连续亏损
        /// </summary>
        /// <param name="testBatchNo"></param>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="lossCount"></param>
        /// <returns></returns>
        public static bool orderCountForContinuousLossesWeeklyForTest(string testBatchNo, string selectedStrategy, string eaType, string currentDateTimeStr, int lossCount)
        {
            return DBHelper.getOrderCountForContinuousLossesWeeklyForTest(testBatchNo, selectedStrategy, eaType, currentDateTimeStr, lossCount);
        }
        /// <summary>
        /// 判断本周总亏损单数是否已经超过给定值
        /// </summary>
        /// <param name="testBatchNo"></param>
        /// <param name="selectedStrategy"></param>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="lossCount"></param>
        /// <returns></returns>
        public static bool getOrderCountForTotalLossesWeeklyForTest(string testBatchNo, string selectedStrategy, string eaType, string currentDateTimeStr, int lossCount)
        {
            return DBHelper.getOrderCountForTotalLossesWeeklyForTest(testBatchNo, selectedStrategy, eaType, currentDateTimeStr, lossCount);
        }

        /// <summary>
        /// 本周盈亏值
        /// </summary>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="sumLossPoint"></param>
        /// <returns></returns>
        public static double getOrderCountForSumLossesWeekly(string eaType, string currentDateTimeStr)
        {
            return DBHelper.getOrderCountForSumLossesWeekly(eaType, currentDateTimeStr);
        }

        /// <summary>
        /// 本周盈亏值
        /// </summary>
        /// <param name="testBatchNo"></param>
        /// <param name="selectedStrategy"></param>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="sumLossPoint"></param>
        /// <returns></returns>
        public static double getOrderCountForSumLossesWeeklyForTest(string testBatchNo, string selectedStrategy, string eaType, string currentDateTimeStr)
        {
            return DBHelper.getOrderCountForSumLossesWeeklyForTest(testBatchNo, selectedStrategy, eaType, currentDateTimeStr);
        }
        /// <summary>
        /// 当天内连续亏损
        /// </summary>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="lossCount"></param>
        /// <returns></returns>
        public static bool orderCountForContinuousLossesDaily(string eaType, string currentDateTimeStr, int lossCount)
        {
            return DBHelper.getOrderCountForContinuousLossesDaily(eaType, currentDateTimeStr, lossCount);
        }
        /// <summary>
        /// 回测当天内连续亏损
        /// </summary>
        /// <param name="testBatchNo"></param>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="lossCount"></param>
        /// <returns></returns>
        public static bool orderCountForContinuousLossesDailyForTest(string testBatchNo, string selectedStrategy, string eaType, string currentDateTimeStr, int lossCount)
        {
            return DBHelper.getOrderCountForContinuousLossesDailyForTest(testBatchNo, selectedStrategy, eaType, currentDateTimeStr, lossCount);
        }

        /// <summary>
        /// 生成蜡烛图数据
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="intervalMinutes"></param>
        /// <returns></returns>
        public static List<Candle> AggregatePriceData(List<PricePoint> raw, int intervalMinutes)
        {
            var grouped = raw
                .GroupBy(p => new DateTime(p.Time.Year, p.Time.Month, p.Time.Day, p.Time.Hour, p.Time.Minute / intervalMinutes * intervalMinutes, 0))
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<Candle>();
            var minTime = grouped.Keys.Min();
            var maxTime = grouped.Keys.Max();

            for (var time = minTime; time <= maxTime; time = time.AddMinutes(intervalMinutes))
            {
                if (grouped.ContainsKey(time))
                {
                    var group = grouped[time];
                    var open = group.First().Price;
                    var close = group.Last().Price;
                    var high = group.Max(p => p.Price);
                    var low = group.Min(p => p.Price);
                    result.Add(new Candle { Time = time, Open = open, High = high, Low = low, Close = close });
                }
                else if (result.Count > 0)
                {
                    var lastClose = result.Last().Close;
                    result.Add(new Candle { Time = time, Open = lastClose, High = lastClose, Low = lastClose, Close = lastClose });
                }
            }

            return result;
        }

    }
}
