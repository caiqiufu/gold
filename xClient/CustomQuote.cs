using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingAPI.MT4Server;

namespace xClient
{
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

        public List<double> BidDiff { get { return _bidDiff; } }
        public List<double> AskDiff { get { return _askDiff; } }
        public QuoteEventArgs Quote { get { return _quote; } }
        public  string Speed { get { return _speed; } }

        public string Symbol { get { return _symbol; } }
        public CustomMT4QuotePanel(string symbol,double point=0.1)
        {
            _symbol = symbol == "" ? "EURUSD":symbol ;
            _point = point;
        }
        public void NewQuoute(QuoteEventArgs q)
        {
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
                    /*
                    //Console.WriteLine("_quote.Time=" + _quote.Time.ToString("hh:mm:ss fff") + "  _lastQuote.Time=" + _lastQuote.Time.ToString("hh:mm:ss fff"));
                    _bidDiff.Insert(0, Convert.ToInt16((_lastQuote.Bid - _quote.Bid)/_point));
                    _bidDiff.RemoveAt(5);


                    _askDiff.Insert(0, Convert.ToInt16((_lastQuote.Ask - _quote.Ask) / _point));
                    _askDiff.RemoveAt(5);*/

                }
            }
            
        }
    }

    class CustomDSQuotePanel
    {
        private string _symbol = "";
        private double _point = 0.1;

        private DateTime _lastTime = DateTime.Now;
        private double   _lastBid = 0;
        private double   _lastAsk = 0;


        private DateTime _curtime;
        private double _curBid=0;
        private double _curAsk = 0;
       
      
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
        public CustomDSQuotePanel(string symbol, double point = 0.1)
        {
            _symbol = symbol == "" ? "EURUSD" : symbol;
            _point = point;
        }
        public void NewQuoute(string symbol,double bid ,double  ask)
        {
            if (symbol == _symbol)
            {
                _curBid = bid;
                _curAsk = ask;
                _curtime = DateTime.Now;
                _speed = Convert.ToInt32((_curtime - _lastTime).TotalMilliseconds) + "ms";
                _lastTime = _curtime;

                if (_lastBid == 0)
                {
                    _lastTime = DateTime.Now;
                    _lastBid = bid;
                    _lastAsk = ask;
                }
                else
                {                 
                    if (_lastBid != bid)
                    {
                        double x = ( bid- _lastBid );
                        double y = _point;
                        double result = x / y;
                        //double changeValue = System.Convert.ToInt32(result - x % y / y);
                        double changeValue = Math.Round(result, 0);
                        if ( changeValue != 0.0)
                        {
                            _bidDiff.Insert(0, changeValue);
                            _bidDiff.RemoveAt(5);
                            _lastBid = bid;
                        }
                       
                    }


                    if (_lastAsk != ask)
                    {
                        double x = (ask- _lastAsk );
                        double y = _point;
                        double result = x / y;
                        //double changeValue = System.Convert.ToInt32(result - x % y / y);
                        double changeValue = Math.Round(result, 0);
                        if (changeValue !=0.0)
                        {
                            _askDiff.Insert(0, changeValue);
                            _askDiff.RemoveAt(5);
                            _lastAsk = ask;
                        }
                    }
                } 
            }
        }
        public void MyNewQuoute(string symbol, double bid, double ask)
        {

            _curBid = bid;
            _curAsk = ask;
            _curtime = DateTime.Now;
            _speed = Convert.ToInt32((_curtime - _lastTime).TotalMilliseconds) + "ms";
            _lastTime = _curtime;

            if (_lastBid != bid)
            {
                double x = (bid - _lastBid);
                double y = _point;
                double result = x / y;
                //double changeValue = System.Convert.ToInt32(result - x % y / y);
                double changeValue = Math.Round(result, 0);
                if (changeValue != 0.0)
                {
                    _bidDiff.Insert(0, changeValue);
                    _bidDiff.RemoveAt(5);
                    _lastBid = bid;

                    _askDiff.Insert(0, changeValue);
                    _askDiff.RemoveAt(5);
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
    }
}
