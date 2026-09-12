using NLog;
using System;
using System.Windows.Forms;

namespace uClient.Comm
{
    public class Log
    {
        private NLog.Logger _logger = LogManager.GetCurrentClassLogger();
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
        public string _TradeWinName { set; get; }
        /// <summary>
        ///  账户信息
        /// </summary>
        public Comm.Account _Account { set; get; }

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
        public Log(ListBox lstTradeRecord, ListBox lstLog,string tradeWinName)
        {
            this._LstTradeRecord = lstTradeRecord;
            this._LstLog = lstLog;
            this._TradeWinName = tradeWinName;
        }
        /// <summary>
        /// 交易日志
        /// </summary>
        /// <param name="lstTradeRecord"></param>
        /// <param name="_Account"></param>
        /// <param name="msg"></param>
        public void LogTradeRecord(Comm.Account account, string msg)
        {
            _LstTradeRecord.TopIndex = _LstTradeRecord.Items.Count - 1;
            _LstTradeRecord.Items.Add(msg);
            Utils.AddTradeMsg(msg, account.UserCode);
            if (msg.Contains("[接收]"))
            {
                Utils.AddTradeMsg("", account.UserCode);
            }
            _logger.Info(msg);
            if (_LstTradeRecord.Items.Count > 50)
            {
                _LstTradeRecord.Items.RemoveAt(_LstTradeRecord.Items.Count - 1);
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
                Utils.AddTradeMsg(msgt, accountCode);
                //每个平仓指令后增加一个空行,方便查看
                if (msg.Contains("[接收]"))
                {
                    Utils.AddTradeMsg("", "999999");
                }
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
                _LstTradeRecord.Invoke(new Action<string, string, string, string, string>(LogTradeRecord), new object[] { path, fileDate, type , header, msg });
            }
            else
            {
                string msgt = DateTime.Now.ToString("HH:mm:ss") + " " + msg;
                _LstTradeRecord.TopIndex = _LstTradeRecord.Items.Count - 1;
                _LstTradeRecord.Items.Add(msgt);
                Utils.AddTradeMsg(path, fileDate, type, header, msgt);
                if (msg.Contains("[接收]"))
                {
                    Utils.AddTradeMsg("", "999999");
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
                if (Utils.RunningModeIsDebug)
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
