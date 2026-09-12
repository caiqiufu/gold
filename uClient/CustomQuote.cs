using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TradingAPI.MT4Server;

namespace uClient.Comm
{
    public class QuotaDisplayPanel 
    {
        public Label lblMT4Speed;
        public Label lblMT4Bid;
        public Label lblMT4BidDiff0;
        public Label lblMT4BidDiff1;
        public Label lblMT4BidDiff2;
        public Label lblMT4BidDiff3;
        public Label lblMT4BidDiff4;
        public Label lblMT4Ask;
        public Label lblMT4AskDiff0;
        public Label lblMT4AskDiff1;
        public Label lblMT4AskDiff2;
        public Label lblMT4AskDiff3;
        public Label lblMT4AskDiff4;
    }
    class CustomMT4QuotePanel
    {
        private string _symbol = "";
        private double _point = 0.0001;
        private DateTime _lastTime =DateTime.Now;
        private DateTime _time;
        private QuoteEventArgs _lastQuote = null;
        private QuoteEventArgs _quote = null;
        private List<double> _bidDiff = new List<double>() { 0, 0, 0, 0, 0 };
        private List<double> _askDiff = new List<double>() { 0, 0, 0, 0, 0 };
        private string _speed= "0ms";
        private static object obj = new object();
        private bool _bidChanged = false;
        public List<double> BidDiff { get { return _bidDiff; } }
        public List<double> AskDiff { get { return _askDiff; } }
        public QuoteEventArgs Quote { get { return _quote; } }
        public  string Speed { get { return _speed; } }
        public bool BidChanged { get { return _bidChanged; } }
        public string Symbol { get { return _symbol; } }
        public CustomMT4QuotePanel(string symbol,double point=0.1)
        {
            _symbol = symbol == "" ? "EURUSD":symbol ;
            _point = point;
        }
        public void NewQuoute(QuoteEventArgs q)
        {
            _bidChanged = false;
            if (q.Symbol == _symbol)
            {
                _time = DateTime.Now;
                if (_lastQuote == null)
                {
                    _lastQuote = q;
                    _quote = q;
                }
                else
                {
                    _quote = q;
                    _speed = Convert.ToInt32(( _time- _lastTime ).TotalMilliseconds)+"ms";
                    _lastTime = _time;
                    if (_lastQuote.Bid != _quote.Bid)
                    {
                        _bidChanged = true;
                        double x = ( _quote.Bid- _lastQuote.Bid );
                        double y = _point;
                        double result = x / y;
                        //double changeValue = System.Convert.ToInt32(result - x % y / y);
                        double changeValue = Math.Round(result,0);
                        if (changeValue != 0.0)
                        {
                            _bidDiff.Insert(0, changeValue);
                            _bidDiff.RemoveAt(5);

                            _lastQuote.Bid = _quote.Bid;                            
                        }
                    }
                    if (_lastQuote.Ask != _quote.Ask)
                    {
                        double x = ( _quote.Ask- _lastQuote.Ask );
                        double y = _point;
                        double result = x / y;
                        //double changeValue = System.Convert.ToInt32(result - x % y / y);
                        double changeValue = Math.Round(result, 0);
                        if (changeValue != 0.0)
                        {
                            _askDiff.Insert(0, changeValue);
                            _askDiff.RemoveAt(5);
                            _lastQuote.Ask = _quote.Ask;
                        }
                    }
                }
            }         
        }
        public void Clear()
        {
            _quote.Ask = 0;
            _quote.Bid = 0;
            _speed = "0ms";
            _bidDiff[0] = 0;
            _bidDiff[1] = 0;
            _bidDiff[2] = 0;
            _bidDiff[3] = 0;
            _bidDiff[4] = 0;
            _askDiff[0] = 0;
            _askDiff[1] = 0;
            _askDiff[2] = 0;
            _askDiff[3] = 0;
            _askDiff[4] = 0;
        }
    }
    public class QuotePanelData 
    {
        private string _symbol = "";
        private double _point = 0.0001;
        private DateTime _lastTime = DateTime.Now;
        private DateTime _time;

        private List<double> _bidDiff = new List<double>() { 0, 0, 0, 0, 0 };
        private List<double> _askDiff = new List<double>() { 0, 0, 0, 0, 0 };
        private string _speed = "0ms";
        private static object obj = new object();
        private bool _bidChanged = false;

        public List<double> BidDiff { get { return _bidDiff; } }
        public List<double> AskDiff { get { return _askDiff; } }

        public string Speed { get { return _speed; } }
        public bool BidChanged { get { return _bidChanged; } }
        public string Symbol { get { return _symbol; } }
        public DateTime LastTime { get { return _lastTime; } }
        public DateTime Time { get { return _time; } }
        public QuotePanelData(string symbol, double point = 0.1)
        {
            if (point==0)
            {
                point = 0.1;
            }
            _symbol = symbol == "" ? "XAUUSD" : symbol;
            _point = point;
        }
        // 定义一个用于同步的私有对象
        private readonly object _bidDiffLock = new object();
        public void NewQuoute(string symbol,decimal newBidPrice,decimal oldBidPrice,decimal newAskPrice,decimal oldAskPrice)
        {
            _bidChanged = false;
            if (symbol == _symbol)
            {
                _time = DateTime.Now;
                _speed = Convert.ToInt64((_time - _lastTime).TotalMilliseconds) + "ms";
                _lastTime = _time;
                if (oldBidPrice != newBidPrice)
                {
                    _bidChanged = true;
                    double x = (double)(newBidPrice - oldBidPrice);
                    double y = _point;
                    double result = x / y;
                    //double changeValue = System.Convert.ToInt32(result - x % y / y);
                    double changeValue = Math.Round(result, 0);
                    if (changeValue != 0.0)
                    {
                        _bidDiff.Insert(0, changeValue);
                        // 在操作列表的地方使用锁：
                        lock (_bidDiffLock)
                        {
                            if (_bidDiff.Count > 5)
                            {
                                // 假设您确实想移除索引 5 处的元素
                                _bidDiff.RemoveAt(5);
                            }
                        }
                        //oldBidPrice = newBidPrice;
                    }
                }
                if (oldAskPrice != newAskPrice)
                {
                    _bidChanged = true;
                    double x = (double)(newAskPrice - oldAskPrice);
                    double y = _point;
                    double result = x / y;
                    //double changeValue = System.Convert.ToInt32(result - x % y / y);
                    double changeValue = Math.Round(result, 0);
                    if (changeValue != 0.0)
                    {
                        _askDiff.Insert(0, changeValue);
                        // 在操作列表的地方使用锁：
                        lock (_bidDiffLock)
                        {
                            if (_askDiff.Count > 5)
                            {
                                // 假设您确实想移除索引 5 处的元素
                                _askDiff.RemoveAt(5);
                            }
                        }
                        //oldAskPrice = newAskPrice;
                    }
                }
            }
        }
        public void Clear()
        {
            //_quote.AskPrice = 0;
            //_quote.BidPrice = 0;
            _speed = "0ms";
            _bidDiff[0] = 0;
            _bidDiff[1] = 0;
            _bidDiff[2] = 0;
            _bidDiff[3] = 0;
            _bidDiff[4] = 0;
            _askDiff[0] = 0;
            _askDiff[1] = 0;
            _askDiff[2] = 0;
            _askDiff[3] = 0;
            _askDiff[4] = 0;
        }
    }
    class CustomV4QuotePanel
    {
        private string _symbol = "";
        private double _point = 0.0001;
        private DateTime _lastTime = DateTime.Now;
        private DateTime _time;
        private Quote _lastQuote = null;
        private Quote _quote = null;
        private List<double> _bidDiff = new List<double>() { 0, 0, 0, 0, 0 };
        private List<double> _askDiff = new List<double>() { 0, 0, 0, 0, 0 };
        private string _speed = "0ms";
        private static object obj = new object();
        private bool _bidChanged = false;

        public List<double> BidDiff { get { return _bidDiff; } }
        public List<double> AskDiff { get { return _askDiff; } }
        public Quote Quote { get { return _quote; } }
        public string Speed { get { return _speed; } }
        public bool BidChanged { get { return _bidChanged; } }
        public string Symbol { get { return _symbol; } }
        public CustomV4QuotePanel(string symbol, double point = 0.1)
        {
            _symbol = symbol == "" ? "EURUSD" : symbol;
            _point = point;
        }
        public void NewQuoute(Quote q)
        {
            _bidChanged = false;
            _time = DateTime.Now;
            if (_lastQuote == null)
            {
                _lastQuote = q;
                _quote = q;
            }
            else
            {
                _quote = q;
                _speed = Convert.ToInt32((_time - _lastTime).TotalMilliseconds) + "ms";
                _lastTime = _time;
                if (_lastQuote.Bid != _quote.Bid)
                {
                    _bidChanged = true;
                    double x = (double)(_quote.Bid - _lastQuote.Bid);
                    double y = _point;
                    double result = x / y;
                    //double changeValue = System.Convert.ToInt32(result - x % y / y);
                    double changeValue = Math.Round(result, 0);
                    if (changeValue != 0.0)
                    {
                        _bidDiff.Insert(0, changeValue);
                        if (_bidDiff.Count > 5)
                        {
                            _bidDiff.RemoveAt(5);
                        }                       
                        _lastQuote.Bid = _quote.Bid;
                    }
                }
                if (_lastQuote.Ask != _quote.Ask)
                {
                    double x = (double)(_quote.Ask - _lastQuote.Ask);
                    double y = _point;
                    double result = x / y;
                    //double changeValue = System.Convert.ToInt32(result - x % y / y);
                    double changeValue = Math.Round(result, 0);
                    if (changeValue != 0.0)
                    {
                        _askDiff.Insert(0, changeValue);
                        if (_askDiff.Count > 5)
                        {
                            _askDiff.RemoveAt(5);
                        }                        
                        _lastQuote.Ask = _quote.Ask;
                    }
                }
            }
        }
        public void Clear()
        {
            _quote.Ask = 0;
            _quote.Bid = 0;
            _speed = "0ms";
            _bidDiff[0] = 0;
            _bidDiff[1] = 0;
            _bidDiff[2] = 0;
            _bidDiff[3] = 0;
            _bidDiff[4] = 0;
            _askDiff[0] = 0;
            _askDiff[1] = 0;
            _askDiff[2] = 0;
            _askDiff[3] = 0;
            _askDiff[4] = 0;
        }
    }
    /// <summary>
    /// 数据源跳价
    /// </summary>
    public class CustomQuotePanel
    {
        private string _symbol = "";
        private double _point = 0.1;

        private DateTime _lastTime = DateTime.Now;
        private double   _lastBid = 0;
        private double   _lastAsk = 0;
        private bool _valuehanged = false;


        private DateTime _curtime;
        private double _curBid=0;
        private double _curAsk = 0;

        /// <summary>
        /// 专门针对EventTrade策略使用的跳次数据列表
        /// </summary>
        public List<double> BidDiffForEventTrade = new List<double>();

        private List<double> _bidDiff = new List<double>() { 0, 0, 0, 0, 0 };
        private List<double> _askDiff = new List<double>() { 0, 0, 0, 0, 0 };
        private string _speed = "0ms";

        private static object obj = new object();

        public List<double> BidDiff { get { return _bidDiff; } }
        public List<double> AskDiff { get { return _askDiff; } }
        public double Bid { get { return _curBid; } }
        public double Ask { get { return _curAsk; } }
        public string Speed { get { return _speed; } }
        public string Symbol { get { return _symbol; } }
        public double LastBid { get { return _lastBid; } }
        public double LastAsk { get { return _lastAsk; } }
        public bool ValueChanged { get { return _valuehanged; } }
        public CustomQuotePanel(string symbol, double point = 0.1)
        {
            _symbol = symbol == "" ? "EURUSD" : symbol;
            _point = point;
        }


        // 该组参数只针对Event策略
        public DateTime _EventLastTime = DateTime.Now;
        public double _EventLastBid = 0;
        public double _EventLastAsk = 0;
        public bool _EventValuehanged = false;
        public DateTime _EventCurtime;
        public double _EventCurBid = 0;
        public double _EventCurAsk = 0;
        public string _EventSpeed = "0ms";


        /// <summary>
        /// 事件策略中总时间计算
        /// </summary>
        public double EventDataTimeDuration;

        /// <summary>
        /// 把跳次放入数组中,根据给定的时间区间删除最后一个跳次
        /// </summary>
        /// <param name="bid">价格</param>
        /// <param name="dataTimeDuration">判断跳次时间区间</param>
        /// <param name="dataPriceDiffSize">跳次大小</param>
        /// <param name="dataPriceDiffTotalCount">判断跳次总数</param>
        public void NewQuoteForEventTrade(double bid,int dataTimeDuration, int dataPriceDiffSize,int dataPriceDiffTotalCount)
        {
            _EventValuehanged = false;
            _EventCurBid = bid;
            _EventCurtime = DateTime.Now;
            // 计算两次报价间的时间差
            double speedM = (_EventCurtime - _EventLastTime).TotalMilliseconds;
            EventDataTimeDuration += speedM;
            _EventSpeed = speedM + "ms";
            _EventLastTime = _EventCurtime;

            // 初始化上一次报价
            if (_EventLastBid == 0)
            {
                _EventLastBid = bid;
                return;
            }

            // 计算价格变化，乘10并取整
            int bidChangeValue = (int)Math.Round((bid - _EventLastBid) * 10, 0, MidpointRounding.AwayFromZero);            
            if (bidChangeValue != 0)
            {
                // 如果变化量超过阈值，就加入队列
                if (Math.Abs(bidChangeValue) >= dataPriceDiffSize)
                {
                    _EventValuehanged = true;
                    //Console.WriteLine($"bidChangeValue={bidChangeValue},bid={bid},_EventLastBid={_EventLastBid}");
                    BidDiffForEventTrade.Insert(0, bidChangeValue);
                    // 保持队列长度不超过最大变化次数
                    if (BidDiffForEventTrade.Count > dataPriceDiffTotalCount)
                    {
                        BidDiffForEventTrade.RemoveAt(BidDiffForEventTrade.Count - 1);
                    }                   
                }               
            }
            _EventLastBid = bid;
            // 超过时间窗口，删除最旧的变化数据
            if (EventDataTimeDuration > dataTimeDuration * 1000)
            {
                if (BidDiffForEventTrade.Count > 0)
                {
                    BidDiffForEventTrade.RemoveAt(BidDiffForEventTrade.Count - 1);
                }                    
                EventDataTimeDuration = 0;
            }
        }

        public void NewQuoute(string symbol, double bid, double ask)
        {
            _valuehanged = false;
            _curBid = bid;
            _curAsk = ask;
            _curtime = DateTime.Now;
            _speed = Convert.ToInt32((_curtime - _lastTime).TotalMilliseconds) + "ms";
            _lastTime = _curtime;
            if (_lastBid==0)
            {
                _lastBid = bid;
            }
            if (_lastAsk == 0)
            {
                _lastAsk = ask;
            }
            //if (_lastBid != bid||_lastAsk != ask)
            if (_lastBid != bid)
             {
                _valuehanged = true;
                //double x = (bid - _lastBid);
                //double y = _point;
                //double result = x / y;
                //double bidChangeValue = Math.Round((bid - _lastBid)/ _point, 0);
                int bidChangeValue = (int)Math.Round((bid - _lastBid) / _point, 0, MidpointRounding.AwayFromZero);
                if (bidChangeValue != 0.0)
                {
                    _bidDiff.Insert(0, bidChangeValue);
                    if (_bidDiff.Count > 5)
                    {
                        _bidDiff.RemoveAt(5);
                    }                   
                    _lastBid = bid;
                }
                double askChangeValue = Math.Round((ask - _lastAsk) / _point, 0);
                if (askChangeValue != 0.0)
                {
                    _askDiff.Insert(0, askChangeValue);
                    if (_askDiff.Count > 5) 
                    { 
                        _askDiff.RemoveAt(5);
                    }
                    _lastAsk = ask;
                }
            }
        }
        public double GetChangeValue(double currPrice,double lastPrice)
        {
            double x = (currPrice - lastPrice);
            double y = _point;
            double result = x / y;
            double changeValue = Math.Round(result, 0);
            return changeValue;
        }
        public void Clear()
        {
            _curBid = 0;
            _curAsk = 0;
            _speed = "0ms";
            _bidDiff[0] = 0;
            _bidDiff[1] = 0;
            _bidDiff[2] = 0;
            _bidDiff[3] = 0;
            _bidDiff[4] = 0;
            _askDiff[0] = 0;
            _askDiff[1] = 0;
            _askDiff[2] = 0;
            _askDiff[3] = 0;
            _askDiff[4] = 0;
        }
    }
}
