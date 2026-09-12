using NLog;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace AutoHorseRace
{
    /// <summary>
    /// 日志处理
    /// </summary>
    public class Log
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 日志信息的根目录（程序exe运行所在目录下的LogInfo文件夹）
        /// </summary>
        public static string _LogInfoRootPath = Path.Combine(Application.StartupPath, "LogInfo");
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
        /// 交易日志记录
        /// </summary>
        public ListBox _LstTradeRecord { set; get; }
        /// <summary>
        /// 操作日志
        /// </summary>
        public ListBox _LstLog { set; get; }
        /// <summary>
        /// 日志生成窗口
        /// </summary>
        public string _TradeWinName { get; set; } = string.Empty;
        /// <summary>
        ///  账户信息
        /// </summary>
        public Account _Account { get; set; } = null!; // null-forgiving if you guarantee it will be set before use

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="lstTradeRecord"></param>
        /// <param name="lstLog"></param>
        public Log(ListBox lstTradeRecord, ListBox lstLog)
        {
            this._LstTradeRecord = lstTradeRecord;
            this._LstLog = lstLog;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="lstTradeRecord"></param>
        /// <param name="lstLog"></param>
        /// <param name="tradeWinName"></param>
        public Log(ListBox lstTradeRecord, ListBox lstLog, string tradeWinName)
        {
            this._LstTradeRecord = lstTradeRecord;
            this._LstLog = lstLog;
            this._TradeWinName = tradeWinName;
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
        /// 生成日志目录
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <returns></returns>
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
        /// 交易日志,自动增加时间戳
        /// </summary>
        /// <param name="accountCode">账户</param>
        /// <param name="msg">交易信息</param>
        public void LogTradeRecord(string accountCode, string msg)
        {
            if (_LstTradeRecord.InvokeRequired)
            {
                _LstTradeRecord.Invoke(new Action<string, string>(LogTradeRecord), new object[] { accountCode, msg });
            }
            else
            {
                if (string.IsNullOrEmpty(accountCode))
                {
                    accountCode = "999999";
                }
                string msgt = DateTime.Now.ToString("HH:mm:ss") + " " + msg;
                _LstTradeRecord.TopIndex = _LstTradeRecord.Items.Count - 1;
                _LstTradeRecord.Items.Add(msgt);
                AddTradeMsg(msgt, accountCode);
                _logger.Info(msgt);
                if (_LstTradeRecord.Items.Count > 50)
                {
                    _LstTradeRecord.Items.RemoveAt(_LstTradeRecord.Items.Count - 1);
                }
            }
        }
        /// <summary>
        /// 交易文件放到指定目录,自动增加时间戳
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="header"></param>
        /// <param name="msg"></param>
        public void LogTradeRecord(string path, string fileDate, string type, string header, string msg)
        {
            if (_LstTradeRecord.InvokeRequired)
            {
                _LstTradeRecord.Invoke(new Action<string, string, string, string, string>(LogTradeRecord), new object[] { path, fileDate, type, header, msg });
            }
            else
            {
                string msgt = DateTime.Now.ToString("HH:mm:ss") + " " + msg;
                _LstTradeRecord.TopIndex = _LstTradeRecord.Items.Count - 1;
                _LstTradeRecord.Items.Add(msgt);
                AddTradeMsg(path, fileDate, type, header, msgt);
                if (msg.Contains("[接收]"))
                {
                    AddTradeMsg("", "999999");
                }
                _logger.Info(msgt);
                if (_LstTradeRecord.Items.Count > 50)
                {
                    _LstTradeRecord.Items.RemoveAt(_LstTradeRecord.Items.Count - 1);
                }
            }
        }
        /// <summary>
        /// 操作日志
        /// </summary>
        /// <param name="msg"></param>
        public void LogInfo(string msg)
        {
            if (_LstLog.InvokeRequired)
            {
                _LstLog.Invoke(new Action<string>(LogInfo), new object[] { msg });
            }
            else
            {
                if (RunningModeIsDebug)
                {
                    _LstLog.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss.fff") + " " + msg);
                    if (_LstLog.Items.Count > 50 && _LstLog.Items[_LstLog.Items.Count - 1] != null)
                    {
                        _LstLog.Items.RemoveAt(_LstLog.Items.Count - 1);
                    }
                    if (!string.IsNullOrEmpty(_TradeWinName) && _Account != null)
                    {
                        _logger.Debug("[" + _TradeWinName + "][" + _Account.UserCode + "]" + msg);
                    }
                    else
                    {
                        _logger.Debug(msg);
                    }
                }
            }
        }
    }
}
