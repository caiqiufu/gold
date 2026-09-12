using System;
using System.Linq;
using System.Windows.Forms;
using NLog;

namespace uClient.Comm
{
    public class TradeUtils
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// 获取最新合约列表
        /// </summary>
        /// <param name="_Broker"></param>
        /// <param name="Symbol"></param>
        /// <returns></returns>
        public static string[] GetLastSymbolList(Comm.Broker _Broker,string Symbol)
        {
            if (!_Broker.Symbol.Contains(Symbol))
            {
                string[] SymbolList = new string[_Broker.Symbol.Length + 1];
                for (int i = 0; i < _Broker.Symbol.Length; i++)
                {
                    SymbolList[i] = _Broker.Symbol[i];
                }
                SymbolList[_Broker.Symbol.Length] = Symbol;
                return SymbolList;
            }
            else
            {
                return _Broker.Symbol;
            }
        }
        /// <summary>
        /// 判断是否锁单价
        /// </summary>
        /// <param name="AutoLockPoint"></param>
        /// <param name="CurrentPrice"></param>
        /// <param name="AutoLockPrice"></param>
        /// <param name="Point"></param>
        /// <param name="TradeSide"></param>
        /// <returns></returns>
        public static bool GetAutoLockPrice(double AutoLockPoint,double CurrentPrice,double AutoLockPrice,double Point, string TradeSide)
        {
            double pp = 0.1;
            bool autoFlag = false;
            switch (TradeSide) 
            {
                case "BUY":
                    if (CurrentPrice<= AutoLockPrice - Point * pp + AutoLockPoint * pp)
                    {
                        autoFlag =  true;
                    }
                    break;
                case "SELL":
                    if (CurrentPrice >= AutoLockPrice + Point * pp - AutoLockPoint * pp)
                    {
                        autoFlag = true;
                    }
                    break;
                case "CLOSE_BUY":
                    if (CurrentPrice >= AutoLockPrice + Point * pp - AutoLockPoint * pp)
                    {
                        autoFlag = true;
                    }
                    break;
                case "CLOSE_SELL":
                    if (CurrentPrice <= AutoLockPrice - Point * pp + AutoLockPoint * pp)
                    {
                        autoFlag = true;
                    }
                    break;
            }
            return autoFlag;
        }
        /// <summary>
        /// 判断开仓时间是否超过设置时间
        /// </summary>
        /// <param name="CloseTimeDuration"></param>
        /// <param name="OpenTime"></param>
        /// <param name="ServerTime"></param>
        /// <returns></returns>
        public static bool IsCloseOrderTime(int CloseTimeDuration,DateTime ServerTime, DateTime OpenTime)
        {
            if (CloseTimeDuration <= 0)
            {
                return true;
            }
            else 
            {
                TimeSpan interval = ServerTime - OpenTime;
                return interval.TotalMinutes >= CloseTimeDuration;
            }
        }
        /// <summary>
        /// 设置自动交易
        /// </summary>
        /// <param name="TimeDuration"></param>
        /// <param name="gvPositions"></param>
        /// <param name="chkBuyOpen"></param>
        /// <param name="chkSellOpen"></param>
        /// <param name="chkBuyClose"></param>
        /// <param name="chkSellClose"></param>
        public static void SetAutoTradeParameters(int CloseTimeDuration,bool OverOpenTimeDuration, DataGridView gvPositions,CheckBox chkBuyOpen, CheckBox chkSellOpen, CheckBox chkBuyClose, CheckBox chkSellClose)
        {
            if (CloseTimeDuration>0)
            {
                if (gvPositions.Rows.Count == 0 && OverOpenTimeDuration)
                {
                    if (!chkBuyOpen.Checked)
                    {
                        chkBuyOpen.Checked = true;
                        chkBuyClose.Checked = false;
                    }
                    if (!chkSellOpen.Checked)
                    {
                        chkSellOpen.Checked = true;
                        chkSellClose.Checked = false;
                    }
                }
                else if (gvPositions.Rows.Count > 0)
                {
                    DataGridViewRow row = gvPositions.Rows[0];
                    string OrderOpenTimeDuration = row.Cells[8].Value.ToString();
                    string[] times = OrderOpenTimeDuration.Split(':');
                    TimeSpan ts = new TimeSpan(0, Convert.ToInt32(times[0]), Convert.ToInt32(times[1]), Convert.ToInt32(times[2]), 0);
                    if (chkBuyOpen.Checked)
                    {
                        chkBuyOpen.Checked = false;
                    }
                    if (chkSellOpen.Checked)
                    {
                        chkSellOpen.Checked = false;
                    }
                    if (ts.TotalMinutes >= CloseTimeDuration)
                    {
                        if (!chkBuyClose.Checked)
                        {
                            chkBuyClose.Checked = true;
                        }
                        if (!chkSellClose.Checked)
                        {
                            chkSellClose.Checked = true;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 平仓限制时间
        /// </summary>
        /// <param name="AutoCloseTimeDuration"></param>
        /// <returns></returns>
        public static int GetCloseTimeDuration(string AutoCloseTimeDuration)
        {
            if (!AutoCloseTimeDuration.Equals("无限制")&&!string.IsNullOrEmpty(AutoCloseTimeDuration))
            {
                return Convert.ToInt32(AutoCloseTimeDuration.Substring(0, AutoCloseTimeDuration.Length - 4));
            }
            else 
            {
                return -1;
            }
        }
        /// <summary>
        /// 是否在交易时间内，07:30 - 04:30 避开夏令时间和冬令时间
        /// </summary>
        /// <returns></returns>
        public static bool isTradeTime()
        {
            string _strWorkingDayStart = "07:30";
            string _strWorkingDayEnd = "04:30";
            TimeSpan dspWorkingDayStart = DateTime.Parse(_strWorkingDayStart).TimeOfDay;
            TimeSpan dspWorkingDayEnd = DateTime.Parse(_strWorkingDayEnd).TimeOfDay;
            TimeSpan currentTime =  DateTime.Now.TimeOfDay;
            if (currentTime >= dspWorkingDayStart || currentTime <= dspWorkingDayEnd)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
    }
}
