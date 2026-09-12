using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Media;
using TradingAPI.MT4Server;
using NetMQ;
using NetMQ.Sockets;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace xClient
{
    public partial class frmTrade : Form
    {
        //MT4 平台 begin
        QuoteClient _MT4qc;
        OrderClient _MT4oc;
        Dictionary<string, SymbolInfo> _MT4dicSymbolInfos;
        Order[] _MT4orders;
        //MT4 平台 end
        //MF4 平台 beign

        //MF4 平台 end
        CustomMT4QuotePanel _mt4Quote ;
        CustomOECQuotePanel _oecQutote = new CustomOECQuotePanel("Gold", 0.1);

        bool EnableAutoTrade = false;
        string  TradeSymbol = "";

        Config _config;
        StrategyConfig _StrategyConfig;
        private Broker _Broker;
        private Account _Account;
        private string _BrokerName = "";
        private string _AccountType = "";
        private string _UserCode = "";
        private string _Password = "";
        private string _Symbol = "";
        private string _IP = "";
        private string _Port = "";
        private string _DSType = "";
        private string _PlatForm = "MT4";

        //trade para
        private int _Slippage;

        //string _wavPath = "../wav";
        //private string _configFile = System.Windows.Forms.Application.StartupPath + "\\Config.json";
        private string _configFile = System.Windows.Forms.Application.StartupPath + "\\MyConfig.json";
        private string _MyStrategyFile = System.Windows.Forms.Application.StartupPath + "\\MyStrategy.json";
        private NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        SubscriberSocket  _subscriber = new SubscriberSocket();

        private static object obj = new object();

        public frmTrade()
        {
            InitializeComponent(); 
        }

        private void frmTrade_Load(object sender, EventArgs e)
        {
            try
            {
                LoadConfig();
                IniUI();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                //_logger.Fatal(ex.Message +"  " + ex.StackTrace);
                MessageBox.Show(ex.Message + "  " + ex.StackTrace);
            }
        }

        private void frmTrade_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveConfig();
        }


        private void  IniUI()
        {
            IniParameterControl();
            IniPositionGrid();
        }
        private void IniParameterControl()
        {
            nudSellLots.DecimalPlaces = 2;
            nudSellLots.Increment = 0.01M;

            nudBuyLots.DecimalPlaces = 2;
            nudBuyLots.Increment = 0.01M;
        }

        private bool isLoadConfigCompleted = false;
        private void LoadConfig()
        {
            if (File.Exists(_configFile))  // 判断是否已有相同文件 
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(_configFile));
                if (_config != null)
                {
                    if (_config.Broker != null && _config.Broker.Length > 0)
                    {
                        foreach (Broker bro in _config.Broker)
                        {
                            comboBox_Broker.Items.Add(bro.BrokerName);
                        }
                    }
                    comboBox_AccType.Items.Add("Demo");
                    comboBox_AccType.Items.Add("Live");
                    _Broker = GetBroker(_config.DefaultBroker);
                    if (_Broker != null)
                    {
                        _Account = _Broker.DefaultAccount == "Demo" ? _Broker.DemoAccount : _Broker.LiveAccount;
                        InitialPara();
                        comboBox_Broker.SelectedItem = _BrokerName;
                        comboBox_AccType.SelectedItem = _Account.Type;
                        SetValues();
                        SetBrokerPara();
                    }
                    _DSType = _config.DSType;
                    _PlatForm = _config.PlatForm;
                    comboBox_PlatForm.SelectedItem = _PlatForm;
                    SetDSPara();
                    if (_Button_ChangeColor_Timer_TradePara != null)
                    {
                        _Button_ChangeColor_Timer_TradePara.Stop();
                        _Button_ChangeColor_Timer_TradePara.Enabled = false;
                        button_TradePara_Save.BackColor = (Color.Transparent);
                    }
                    nudSellOpen.Value = _config.TradePara.SellOpen;
                    nudSellClose.Value = _config.TradePara.SellClose;
                    nudSellLots.Value = _config.TradePara.SellLots;
                    nudBuyOpen.Value = _config.TradePara.BuyOpen;
                    nudBuyClose.Value = _config.TradePara.BuyClose;
                    nudBuyLots.Value = _config.TradePara.BuyLots;
                    _Slippage = Convert.ToInt32(_config.TradePara.Slippage);
                    comboBox_Slippage.SelectedItem = Convert.ToString(_Slippage);
                    if (_Button_ChangeColor_Timer_TradePara != null)
                    {
                        _Button_ChangeColor_Timer_TradePara.Stop();
                        _Button_ChangeColor_Timer_TradePara.Enabled = false;
                        button_TradePara_Save.BackColor = (Color.Transparent);
                    }
                    isLoadConfigCompleted = true;
                }
            }
            else
            {
                _logger.Fatal("没有初始化配置文件，系统初始化异常");
            }
            if (File.Exists(_MyStrategyFile))
            {
                _StrategyConfig = JsonConvert.DeserializeObject<StrategyConfig>(File.ReadAllText(_MyStrategyFile));
                _StrategyConfig.Symbols[0].TradePara = _config.TradePara;
            }
            else
            {
                _logger.Fatal("没有策略配置文件，系统初始化异常");
            }
        }
        private void SetDSPara()
        {
            comboBox1.SelectedItem = _DSType;
            if ("T4".Equals(_DSType))
            {
                txtPublishAddress.Text = _config.PublishAddressT4;
            }
            if ("OEC".Equals(_DSType))
            {
                txtPublishAddress.Text = _config.PublishAddressOEC;
            }
        }
        private void ClearInitialPara()
        {
            comboBox_Broker.Items.Clear();
            comboBox_Broker.Text = "";
            comboBox_AccType.Items.Clear();
            comboBox_AccType.Text = "";
            comboBox_Symbol.Items.Clear();
            comboBox_Symbol.Text = "";
        }
        private void InitialPara()
        {
            _BrokerName = _Broker.BrokerName;
            _AccountType = _Account.Type;
            _Symbol = _Broker.DefaultSymbol;
            _UserCode = _Account.UserCode;
            _Password = _Account.Password;
            _IP = _Account.IP;
            _Port = _Account.Port;
            TradeSymbol = _Symbol;
        }
        /**
         * 从配置文件中获取平台商信息
         */
        private Broker GetBroker(string BrokerName)
        {
            Broker[] BroList = _config.Broker;
            if (BroList != null && BroList.Length > 0)
            {
                foreach (Broker bro in BroList)
                {
                    if (bro.BrokerName.Equals(BrokerName))
                    {
                        return bro;
                    }
                }
            }
            return null;
        }
        private Account GetAccount(string BrokerName, string TypeName)
        {
            Broker Bro = GetBroker(BrokerName);
            if (Bro!=null)
            {
                if (TypeName.Equals("Demo"))
                {
                    return Bro.DemoAccount;
                }
                if (TypeName.Equals("Live"))
                {
                    return Bro.LiveAccount;
                }
            }
            return null;
        }
        private string[] GetLastSymbolList(string Symbol)
        {
            if (!_Broker.LiveAccount.Symbol.Contains(Symbol))
            {
                string[] SymbolList = new string[_Broker.LiveAccount.Symbol.Length + 1];
                for (int i = 0; i < _Broker.LiveAccount.Symbol.Length; i++)
                {
                    SymbolList[i] = _Broker.LiveAccount.Symbol[i];
                }
                SymbolList[_Broker.LiveAccount.Symbol.Length] = Symbol;
                return SymbolList;
            }
            else 
            {
                return _Broker.LiveAccount.Symbol;
            }
        }
        private void SetValues()
        {
            textBox_UserCode.Text = _UserCode;
            textBox_Password.Text = _Password;
            comboBox_Symbol.Items.Clear();
            if (_Account.Symbol != null && _Account.Symbol.Length > 0)
            {
                foreach (string ss in _Account.Symbol)
                {
                    comboBox_Symbol.Items.Add(ss);
                }
                comboBox_Symbol.SelectedItem = _Symbol;
            }
        }
        private void SaveConfig()
        {
            try
            {
                _Account.UserCode = textBox_UserCode.Text;
                _Account.Password = textBox_Password.Text;
                _Broker.DefaultSymbol = _Symbol;
                string[] SymbolList = GetLastSymbolList(_Symbol);
                _Broker.DemoAccount.Symbol = SymbolList;
                _Broker.LiveAccount.Symbol = SymbolList;
                _Broker.DefaultAccount = _AccountType;
                /////////////////////////////////////////////
                _config.DSType = _DSType;
                _config.PlatForm = _PlatForm;
                if ("T4".Equals(_DSType))
                {
                    _config.PublishAddressT4 = txtPublishAddress.Text;
                }
                if ("OEC".Equals(_DSType))
                {
                    _config.PublishAddressOEC = txtPublishAddress.Text;
                }
                _config.TradePara.SellOpen = nudSellOpen.Value;
                _config.TradePara.SellClose = nudSellClose.Value;
                _config.TradePara.SellLots = nudSellLots.Value;
                _config.TradePara.BuyOpen = nudBuyOpen.Value;
                _config.TradePara.BuyClose = nudBuyClose.Value;
                _config.TradePara.BuyLots = nudBuyLots.Value;
                _config.TradePara.Slippage = _Slippage;
                File.WriteAllText(_configFile, JsonConvert.SerializeObject(_config));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                Log(ex.Message + ex.Source);
            }
        }
        private void AddConfig()
        {
            bool isNewBroker = false;
            for (int i = 0; i < _config.Broker.Length; i++)
            {
                if (txtBrokeName.Text.Equals(_config.Broker[i].BrokerName))
                {
                    isNewBroker = false;
                    _config.Broker[i].DemoAccount.IP = textBox_Demo_IP.Text;
                    _config.Broker[i].DemoAccount.Port = textBox_Demo_Port.Text;
                    string[] SymbolList = GetLastSymbolList(textBox_Symbol.Text);
                    _config.Broker[i].DemoAccount.Symbol = SymbolList;
                    _config.Broker[i].LiveAccount.IP = textBox_Live_IP.Text;
                    _config.Broker[i].LiveAccount.Port = textBox_Live_Port.Text;
                    _config.Broker[i].LiveAccount.Symbol = SymbolList;
                    _config.Broker[i].DefaultSymbol = textBox_Symbol.Text;
                    break;
                }
                else 
                {
                    isNewBroker = true;
                }
            }
            try
            {
                if (isNewBroker)
                {
                    Broker NewBroker = new Broker();
                    NewBroker.BrokerName = txtBrokeName.Text;
                    NewBroker.DefaultAccount = "Demo";
                    NewBroker.DefaultSymbol = textBox_Symbol.Text;
                    Account NewDemoAccount = new Account();
                    NewDemoAccount.Type = "Demo";
                    NewDemoAccount.IP = textBox_Demo_IP.Text;
                    NewDemoAccount.Port = textBox_Demo_Port.Text;
                    NewDemoAccount.UserCode = "";
                    NewDemoAccount.Password = "";
                    string[] SymbolList = GetLastSymbolList(textBox_Symbol.Text);
                    NewDemoAccount.Symbol = SymbolList;
                    NewBroker.DemoAccount = NewDemoAccount;
                    Account NewLiveAccount = new Account();
                    NewLiveAccount.Type = "Live";
                    NewLiveAccount.IP = textBox_Live_IP.Text;
                    NewLiveAccount.Port = textBox_Live_Port.Text;
                    NewLiveAccount.UserCode = "";
                    NewLiveAccount.Password = "";
                    NewLiveAccount.Symbol = SymbolList;
                    NewBroker.LiveAccount = NewLiveAccount;
                    Broker[] NewBrokerList = new Broker[_config.Broker.Length + 1];
                    for (int i = 0; i < _config.Broker.Length; i++)
                    {
                        NewBrokerList[i] = _config.Broker[i];
                    }
                    NewBrokerList[_config.Broker.Length] = NewBroker;
                    _config.Broker = NewBrokerList;
                }
                
                File.WriteAllText(_configFile, JsonConvert.SerializeObject(_config));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                Log(ex.Message + ex.Source);
            }
        }

        #region ＵＩ交互
        private void btnLogin_Click(object sender, EventArgs e)
        {
            if ("MT4".Equals(_PlatForm))
            {
                MT4Connect();
            }
            else if ("mF4".Equals(_PlatForm))
            {
                MF4Connect();
            }
            else
            {
                _logger.Error("["+_PlatForm+"]平台不存在");
                Log("[" + _PlatForm + "]平台不存在");
            }
        }

        private void MF4Connect()
        {
        }
        private void MT4Connect()
        {
            try
            {
                if (_MT4qc != null && _MT4qc.Connected)
                {
                    _MT4qc.Disconnect();
                }
                _mt4Quote = new CustomMT4QuotePanel(comboBox_Symbol.Text, 0.1);

                if (_UserCode == null || _UserCode == "")
                {
                    MessageBox.Show("账号不能为空");
                    return;
                }
                if (_Password == null || _Password == "")
                {
                    MessageBox.Show("密码不能为空");
                    return;
                }
                if (_IP == null || _IP == "")
                {
                    MessageBox.Show("IP不能为空");
                    return;
                }
                if (_Port == null || _Port == "")
                {
                    MessageBox.Show("Port不能为空");
                    return;
                }
                //_qc = new QuoteClient(Convert.ToUInt32(_UserCode), _Password,_IP, Convert.ToInt32(_Port) );
                //_qc.LoginIdWebServerUrl = "http://49.234.110.126:6030/loginid";
                _MT4qc = new QuoteClient(Convert.ToInt32(_UserCode), _Password, _IP, Convert.ToInt32(_Port));
                //_qc.LoginIdPath = "http://49.234.110.126:6030/loginid";
                //_qc.LoginIdPath = "http://3.140.55.139:9900/loginid";
                //_qc.LoginIdPath = "http://192.168.79.39:9900/loginid";
                //_qc.LoginIdPath = "http://3.20.212.33:9900/loginid";
                Log("Connecting to server ...");
                _MT4qc.OnConnect += MT4Qc_OnConnect;
                _MT4qc.OnDisconnect += MT4Qc_OnDisconnect;
                _MT4qc.OnQuote += MT4Qc_OnQuote;
                _MT4qc.OnOrderUpdate += MT4Qc_OnOrderUpdate;
                _MT4qc.CalculateTradeProps = true;
                _MT4qc.ConnectAsync();
                _MT4oc = new OrderClient(_MT4qc);
                _MT4oc.OnOrderProgress += MT4Oc_OnOrderProgress;
                this.FindForm().Text = "【" + comboBox_Broker.Text + "】 交易窗口";
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                Log(ex.Message);
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            lblMT4Bid.Text = "0";
            lblMT4BidDiff0.Text = "0";
            lblMT4BidDiff1.Text = "0";
            lblMT4BidDiff2.Text = "0";
            lblMT4BidDiff3.Text = "0";
            lblMT4BidDiff4.Text = "0";
            lblMT4Ask.Text = "0";
            lblMT4AskDiff0.Text = "0";
            lblMT4AskDiff1.Text = "0";
            lblMT4AskDiff2.Text = "0";
            lblMT4AskDiff3.Text = "0";
            lblMT4AskDiff4.Text = "0";
            this.FindForm().Text = "交易窗口";
            if ("MT4".Equals(_PlatForm))
            {
                MT4Logout();
            }
            else if ("mF4".Equals(_PlatForm))
            {
                MF4Logout();
            }
            else
            {
                _logger.Error("[" + _PlatForm + "]平台不存在");
                Log("[" + _PlatForm + "]平台不存在");
            }
        }
        private void MT4Logout()
        {
            _MT4qc.Disconnect();
        }
        private void MF4Logout()
        {
        }
        private void btnOECLogin_Click(object sender, EventArgs e)
        {

            Log(string.Format("连接到{0}服务器：{1}",_DSType,txtPublishAddress.Text));
            try
            {
                if (txtPublishAddress.Text==null|| txtPublishAddress.Text =="")
                {
                    MessageBox.Show(string.Format("{0}服务器地址未配置", _DSType));
                    return;
                }
                _subscriber.Options.TcpKeepalive = true;
                _subscriber.Options.TcpKeepaliveIdle = new TimeSpan(5, 0, 0);
                _subscriber.Options.TcpKeepaliveInterval = new TimeSpan(0, 0, 1);
                _subscriber.Connect(txtPublishAddress.Text);

                _subscriber.Subscribe("");
                if (bgwMarketQuote.IsBusy != true)
                {
                    // Start the asynchronous operation.
                    bgwMarketQuote.RunWorkerAsync();
                }
                btnOECLogin.Enabled = false;
                btnOECLogout.Enabled = true;
                comboBox1.Enabled = false;
                txtPublishAddress.Enabled = false;
                label_Price_Connect.BackColor = Color.Green;
                label5.Text = _DSType;
                label_Price_Connect.Text = "已连接";
            }
            catch (Exception ex )
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                Log(string.Format("连接到{0}服务器时错误：{1}",_DSType,ex.Message));
                label_Price_Connect.BackColor = Color.Red;
                label_Price_Connect.Text = "连接失败";
            } 
        }
        private void btnOECLogout_Click(object sender, EventArgs e)
        {
            Log(string.Format("断开{0}服务器：{1}",_DSType, txtPublishAddress.Text));
            btnOECLogin.Enabled = true;
            btnOECLogout.Enabled = false;
            comboBox1.Enabled = true;
            txtPublishAddress.Enabled = true;
            _subscriber.Unbind(txtPublishAddress.Text);
            label_Price_Connect.BackColor = Color.Red;
            label_Price_Connect.Text = "断开";
            lblOECBid.Text = "0";
            lblOECBidDiff0.Text = "0";
            lblOECBidDiff1.Text = "0";
            lblOECBidDiff2.Text = "0";
            lblOECBidDiff3.Text = "0";
            lblOECBidDiff4.Text = "0";
            lblOECAsk.Text = "0";
            lblOECAskDiff0.Text = "0";
            lblOECAskDiff1.Text = "0";
            lblOECAskDiff2.Text = "0";
            lblOECAskDiff3.Text = "0";
            lblOECAskDiff4.Text = "0";
        }
        private void btnBuy_Click(object sender, EventArgs e)
        {
            if ("MT4".Equals(_PlatForm))
            {
                MT4Buy();
            }
            else if ("mF4".Equals(_PlatForm))
            {
                MF4Buy();
            }
            else
            {
                _logger.Error("[" + _PlatForm + "]平台不存在");
                Log("[" + _PlatForm + "]平台不存在");
            }
        }
        private void MF4Buy()
        { 
        
        }
        private void MT4Buy()
        {
            if (comboBox_Symbol.Text == "")
            {
                MessageBox.Show("合约不能为空");
                return;
            }
            if (_MT4qc == null || !_MT4qc.Connected && _MT4oc == null)
            {
                MessageBox.Show("平台商没有连接或未初始化");
                return;
            }
            try
            {
                double lots = (double)nudBuyLots.Value;
                var quote = _MT4qc.GetQuote(TradeSymbol);
                double price = quote.Ask;
                SendTime = DateTime.Now;
                MyLogTradeRecord(string.Format("[" + SendTime.ToString("HH:mm:ss.fff") + "]" + "[手动买入][发送]报价:{0} 手数:{1} 平台价:{2}", price, lots, price));
                int tempID = _MT4oc.OrderSendAsync(comboBox_Symbol.Text, Op.Buy, lots, price, _Slippage, 0, 0, "", 0, new DateTime());
                IsAutoOperationFlag = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                Log(ex.Message);
            }
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            if ("MT4".Equals(_PlatForm))
            {
                MT4Sell();
            }
            else if ("mF4".Equals(_PlatForm))
            {
                MF4Sell();
            }
            else
            {
                _logger.Error("[" + _PlatForm + "]平台不存在");
                Log("[" + _PlatForm + "]平台不存在");
            }
        }
        private void MF4Sell()
        { 
        
        }
        private void MT4Sell()
        {
            if (comboBox_Symbol.Text == "")
            {
                MessageBox.Show("合约不能为空");
                return;
            }
            if (_MT4qc == null || !_MT4qc.Connected && _MT4oc == null)
            {
                MessageBox.Show("平台商没有连接或未初始化");
                return;
            }
            try
            {
                double lots = (double)nudSellLots.Value;
                var quote = _MT4qc.GetQuote(TradeSymbol);
                double price = quote.Bid;
                SendTime = DateTime.Now;
                MyLogTradeRecord(string.Format("[" + SendTime.ToString("HH:mm:ss.fff") + "]" + "[手动卖空][发送]报价:{0} 手数:{1} 平台价:{2}", price, lots, price));
                int tempID = _MT4oc.OrderSendAsync(comboBox_Symbol.Text, Op.Sell, lots, price, _Slippage, 0, 0, "", 0, new DateTime());
                IsAutoOperationFlag = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                Log(ex.Message);
            }
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            if ("MT4".Equals(_PlatForm))
            {
                MT4Close();
            }
            else if ("mF4".Equals(_PlatForm))
            {
                MF4Close();
            }
            else
            {
                _logger.Error("[" + _PlatForm + "]平台不存在");
                Log("[" + _PlatForm + "]平台不存在");
            }
        }
        private void MF4Close()
        { 
        }
        private void MT4Close()
        {
            if (gvPositions.Rows.Count == 1)
            {
                gvPositions.Rows[0].Selected = true;
                gvPositions.Rows[0].Cells[0].Value = true;
            }
            if (gvPositions.SelectedRows.Count != 1)
            {
                MessageBox.Show("选择一个要平仓订单");
                return;
            }
            if (_MT4qc == null || !_MT4qc.Connected && _MT4oc == null)
            {
                MessageBox.Show("平台商没有连接或未初始化");
                return;
            }
            try
            {
                for (int i = 0; i < gvPositions.SelectedRows.Count; i++)
                {
                    DataGridViewRow row = gvPositions.SelectedRows[i];
                    int ticket = Convert.ToInt32(row.Cells[1].Value);
                    string symbol = row.Cells[2].Value.ToString();
                    string type = row.Cells[3].Value.ToString();
                    double lots = Convert.ToDouble(row.Cells[4].Value);
                    var quote = _MT4qc.GetQuote(symbol);
                    double price = quote.Bid;
                    SendTime = DateTime.Now;
                    if (type.Equals("Buy"))
                    {
                        MyLogTradeRecord(string.Format("[" + SendTime.ToString("HH:mm:ss.fff") + "]" + "[手动买平][发送]报价:{0} 手数:{1} 平台价:{2}", price, lots, price));
                    }
                    else
                    {
                        MyLogTradeRecord(string.Format("[" + SendTime.ToString("HH:mm:ss.fff") + "]" + "[手动卖平][发送]报价:{0} 手数:{1} 平台价:{2}", price, lots, price));
                    }
                    var ret = _MT4oc.OrderCloseAsync(symbol, ticket, lots, price, _Slippage);
                }
                IsAutoOperationFlag = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                Log(ex.Message);
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            Log("自动交易启用....");
            lblTradeStatus.Text = "自动交易已启动";
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            lblTradeStatus.BackColor = Color.Green;
            EnableAutoTrade = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Log("自动交易停止");
            lblTradeStatus.Text = "自动交易已停止";
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            lblTradeStatus.BackColor = Color.Red;
            EnableAutoTrade = false;
        }

        private void chkSellOpen_CheckedChanged(object sender, EventArgs e)
        {
            Log((chkSellOpen.Checked ? "" : "不") + "允许开仓(卖空单)");
        }

        private void chkBuyOpen_CheckedChanged(object sender, EventArgs e)
        {
            Log((chkBuyOpen.Checked ? "" : "不") + "允许开仓(买多单)");
        }

        private void chkSellClose_CheckedChanged(object sender, EventArgs e)
        {
            Log((chkSellClose.Checked ? "" : "不") + "允许平仓（卖空单）");
        }

        private void chkBuyClose_CheckedChanged(object sender, EventArgs e)
        {
            Log((chkBuyClose.Checked ? "" : "不") + "允许平仓（买多单）");
        }

        private void cmbSymbol_SelectedValueChanged(object sender, EventArgs e)
        {
            _Symbol = comboBox_Symbol.SelectedItem.ToString();
            TradeSymbol = _Symbol;
            textBox_Symbol.Text = _Symbol;
            if ("MT4".Equals(_PlatForm))
            {
                MT4Symbol_SelectedValueChanged();
            }
            else if ("mF4".Equals(_PlatForm))
            {
                MF4Symbol_SelectedValueChanged();
            }
            else
            {
                _logger.Error("[" + _PlatForm + "]平台不存在");
                Log("[" + _PlatForm + "]平台不存在");
            }

        }
        private void MF4Symbol_SelectedValueChanged()
        { 
        }
        private void MT4Symbol_SelectedValueChanged()
        {
            try
            {
                if (_MT4qc != null && _MT4qc.Connected)
                {
                    _mt4Quote = new CustomMT4QuotePanel(comboBox_Symbol.Text, _MT4dicSymbolInfos[comboBox_Symbol.Text].Point);
                    if (!_MT4qc.IsSubscribed(comboBox_Symbol.Text))
                    {
                        _MT4qc.Subscribe(comboBox_Symbol.Text);
                        Log("订阅MT4平台行情" + comboBox_Symbol.Text);
                    }
                }
                else
                {
                    Log("MT4服务器未连接，不能订阅MT4平台行情" + comboBox_Symbol.Text);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                Log("品种切换时错误：" + ex.Message);
            }
        }
        private void btnRefreshPositionGrid_Click(object sender, EventArgs e)
        {
            if ("MT4".Equals(_PlatForm))
            {
                MT4RefreshPositionGrid();
            }
            else if ("mF4".Equals(_PlatForm))
            {
                MF4RefreshPositionGrid();
            }
            else
            {
                _logger.Error("[" + _PlatForm + "]平台不存在");
                Log("[" + _PlatForm + "]平台不存在");
            }
        }
        private void MF4RefreshPositionGrid()
        { 

        }
        private void MT4RefreshPositionGrid()
        {
            if (_MT4qc != null && _MT4qc.Connected)
            {
                MT4GetPosition();
                MT4MyUpdatePositionGrid();
            }
            else
            {
                gvPositions.Rows.Clear();
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if ("MT4".Equals(_PlatForm))
            {
                MT4Timer1Tick();
            }
            else if ("mF4".Equals(_PlatForm))
            {
                MF4Timer1Tick();
            }
            else
            {
                _logger.Error("[" + _PlatForm + "]平台不存在");
                Log("[" + _PlatForm + "]平台不存在");
            }
        }
        private void MF4Timer1Tick()
        { 
        }
        private void MT4Timer1Tick()
        {
            if (_MT4qc != null && _MT4qc.Connected)
            {
                MT4GetPosition();
                MT4MyUpdatePositionGrid();
            }
            else
            {
                gvPositions.Rows.Clear();
            }
        }
        #endregion
        #region API Event
        private void MF4Qc_OnDisconnect(object sender, DisconnectEventArgs args)
        { }
        private void MT4Qc_OnDisconnect(object sender, DisconnectEventArgs args)
        {
            SoundPlayer player = new System.Media.SoundPlayer("wav\\disconnect.wav");
            player.Play();
            Log("OnDisconnect");
            MT4UpdateConnectStatus(false);
        }
        private void MF4Qc_OnConnect(object sender, ConnectEventArgs args)
        { 
        }
        private void MT4Qc_OnConnect(object sender, ConnectEventArgs args)
        {
            if (args.Exception != null)
            {
                Log("OnConnect Exception"+args.Exception.Message);
                return;
            }
            Log("OnConnect");
            Log(string.Format("平台商：{0} 服务器地址:{1} 端口:{2} 交易品种:{3}", _BrokerName, _IP, _Port, _Symbol));
            SoundPlayer player = new System.Media.SoundPlayer("wav\\connect.wav");
            //简单播放一遍
            player.Play();
            //循环播放
            //player.PlayLooping();
            //另起线程播放
            //player.PlaySync();
            MT4GetSymbolInfo();
            if (!_MT4qc.IsSubscribed(TradeSymbol))
            {
                Log("订阅MT4平台行情" + TradeSymbol);
                _MT4qc.Subscribe(TradeSymbol);
            }
            //2.更新连接状态
            MT4UpdateConnectStatus(true);
            //3.更新账户资金
            MT4UpdateAccountCaptial();
            //4.更新帐户持仓
            MT4GetPosition();
            MT4MyUpdatePositionGrid();

        }
        private void MF4GetSymbolInfo()
        { 
        }
        private void MT4GetSymbolInfo()
        {
            _MT4dicSymbolInfos = new Dictionary<string, SymbolInfo>();

            foreach (var symbol in _MT4qc.Symbols)
            {
                if (_Account.Symbol != null && _Account.Symbol.Length > 0)
                {
                    Boolean IsExistSymbol = false;
                    foreach (string mySymbol in _Account.Symbol)
                    {
                        if (symbol.Equals(mySymbol))
                        {
                            IsExistSymbol = true;
                            try
                            {
                                var info = _MT4qc.GetSymbolInfo(symbol);
                                _MT4dicSymbolInfos.Add(symbol, info);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex.Message + "  " + ex.StackTrace);
                                Log(ex.Source + "  " + ex.Message);
                            }
                        }
                    }
                    if (!IsExistSymbol)
                    {
                        var info = _MT4qc.GetSymbolInfo(symbol);
                        _MT4dicSymbolInfos.Add(symbol, info);
                    }
                }
                else
                {
                    _Symbol = symbol;
                    TradeSymbol = symbol;
                    try
                    {
                        var info = _MT4qc.GetSymbolInfo(symbol);
                        _MT4dicSymbolInfos.Add(symbol, info);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message + "  " + ex.StackTrace);
                        Log(ex.Source + "  " + ex.Message);
                    }
                }
            }
        }
        private bool IsAutoOperationFlag = true;
        /**
         * TPT_Accepted, //接受
          * TPT_InProcess, //处理中
          * TPT_Opened, //开仓
          * TPT_Closed, //平仓
          * TPT_Modified, //修改
          * TPT_PendingDeleted, //删除挂单
          * TPT_ClosedBy, //对冲平仓
          * TPT_MultipleClosedBy,//同一证券多订单对冲平仓
          * TPT_Timeout, //超时
          * TPT_Price, //获取价格
          * TPT_Cancel, //取消
          * TPT_UNKNOW,
         */
        DateTime SendTime;
        DateTime ReceiveTime;
        /// <summary>
        /// 报单处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MF4Oc_OnOrderProgress(object sender, OrderProgressEventArgs args)
        { 
        }
        private void MT4Oc_OnOrderProgress(object sender, OrderProgressEventArgs args)
        {
            SoundPlayer player;
            switch (args.Type)
            {
                case ProgressType.Accepted:
                    break;
                case ProgressType.InProcess:
                    break;
                case ProgressType.Opened:
                    ReceiveTime = DateTime.Now;
                    long OpenDiffTime = Convert.ToInt32((ReceiveTime -SendTime).TotalMilliseconds);
                    Order o_open = args.Order;
                    _MT4orders = new Order[1] { o_open };
                    if (o_open.Type == Op.Buy)
                    {
                        string msgFormt1 = "[" + ReceiveTime.ToString("HH:mm:ss.fff") + "]" + "[自动买多][接收]成交价:{0} 手数:{1} 成交时间:{2} 耗时:{3}ms";
                        if (!IsAutoOperationFlag)
                        {
                            msgFormt1 = "[" + ReceiveTime.ToString("HH:mm:ss.fff") + "]" + "[手动买多][接收]成交价:{0} 手数:{1} 成交时间:{2} 耗时:{3}ms";
                        }
                        MyLogTradeRecord(string.Format(msgFormt1, o_open.OpenPrice, o_open.Lots, o_open.OpenTime.ToString("HH:mm:ss"), OpenDiffTime));
                    };
                    if (o_open.Type == Op.Sell)
                    {
                        string msgFormt2 = "[" + ReceiveTime.ToString("HH:mm:ss.fff") + "]" + "[自动卖空][接收]成交价:{0} 手数:{1} 成交时间:{2} 耗时:{3}ms";
                        if (!IsAutoOperationFlag)
                        {
                            msgFormt2 = "[" + ReceiveTime.ToString("HH:mm:ss.fff") + "]" + "[手动卖空][接收]成交价:{0} 手数:{1} 成交时间:{2} 耗时:{3}ms";
                        }
                        MyLogTradeRecord(string.Format(msgFormt2, o_open.OpenPrice, o_open.Lots, o_open.OpenTime.ToString("HH:mm:ss"), OpenDiffTime));
                    }
                    player = new System.Media.SoundPlayer("wav\\ok.wav");
                    player.Play();
                    break;
                case ProgressType.Closed:
                    ReceiveTime = DateTime.Now;
                    long CloseDiffTime = Convert.ToInt32((ReceiveTime - SendTime).TotalMilliseconds);
                    Order o_close = args.Order;
                    _MT4orders = null;
                    if (o_close.Type == Op.Buy)
                    {
                        string msgFormt3 = "[" + ReceiveTime.ToString("HH:mm:ss.fff") + "]" + "[自动买平][接收]成交价:{0} 手数:{1} 成交时间:{2} 耗时:{3}ms";
                        if (!IsAutoOperationFlag)
                        {
                            msgFormt3 = "[" + ReceiveTime.ToString("HH:mm:ss.fff") + "]" + "[手动买平][接收]成交价:{0} 手数:{1} 成交时间:{2} 耗时:{3}ms";
                        }
                            MyLogTradeRecord(string.Format(msgFormt3, o_close.ClosePrice, o_close.Lots, o_close.CloseTime.ToString("HH:mm:ss"), CloseDiffTime));
                    };
                    if (o_close.Type == Op.Sell)
                    {
                        string msgFormt4 = "[" + ReceiveTime.ToString("HH:mm:ss.fff") + "]" + "[自动卖平][接收]成交价:{0} 手数:{1} 成交时间:{2} 耗时:{3}ms";
                        if (!IsAutoOperationFlag)
                        {
                            msgFormt4 = "[" + ReceiveTime.ToString("HH:mm:ss.fff") + "]" + "[手动卖平][接收]成交价:{0} 手数:{1} 成交时间:{2} 耗时:{3}ms";
                        }
                        MyLogTradeRecord(string.Format(msgFormt4, o_close.ClosePrice, o_close.Lots, o_close.CloseTime.ToString("HH:mm:ss"), CloseDiffTime));
                    }
                    player = new System.Media.SoundPlayer("wav\\ok.wav");
                    player.Play();
                    break;
                case ProgressType.Modified:
                    break;
                case ProgressType.PendingDeleted:
                    break;
                case ProgressType.ClosedBy:
                    break;
                case ProgressType.MultipleClosedBy:
                    break;
                case ProgressType.Price:
                    break;
                case ProgressType.Rejected:
                    Order o_reject = args.Order;
                    string msgFormt5 = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" +  "[拒绝][{0}]";
                    MyLogTradeRecord(string.Format(msgFormt5,args.ToString()));
                    Log(string.Format("[委托={0}][Rejected][{1}]", args.TempID, args.ToString()));
                    player = new System.Media.SoundPlayer("wav\\alert.wav");
                    player.Play();
                    break;
                case ProgressType.Timeout:
                    Log(string.Format("[委托={0}][Timeout][{1}]", args.TempID, args.ToString()));
                   player = new System.Media.SoundPlayer("wav\\alert.wav");
                    player.Play();
                    break;
              
                case ProgressType.Exception:
                    Log(string.Format("[委托={0}][Exception][{1}]", args.TempID, args.ToString()));
                    player = new System.Media.SoundPlayer("wav\\alert.wav");
                    player.Play();
                    break;
                default:
                    player = new System.Media.SoundPlayer("wav\\ok.wav");
                    player.Play();
                    Log(string.Format("[委托={0}][{1}][{2}]", args.TempID,  args.Type,args.ToString()));
                    break;
            }
        }
        private void MF4Qc_OnOrderUpdate(object sender, OrderUpdateEventArgs update)
        { 
        }
        private void MT4Qc_OnOrderUpdate(object sender, OrderUpdateEventArgs update)
        {
            switch (update.Action)
            {
                case UpdateAction.PositionOpen:
                    Log("\r\n");
                    Log(string.Format("[{0}][{1}][开仓][成交][{2}]手数:{3}", update.Order.Symbol, update.Order.Type, update.Order.OpenPrice, update.Order.Lots));
                    break;
                case UpdateAction.PositionClose:
                    Log("\r\n");
                    Log(string.Format("[{0}][{1}][平仓][成交][{2}]手数:{3}盈亏：{4}", update.Order.Symbol, update.Order.Type, update.Order.ClosePrice, update.Order.Lots, update.Order.Profit));
                    break;
                case UpdateAction.PositionModify:
                    break;
                case UpdateAction.PendingOpen:
                    break;
                case UpdateAction.PendingClose:
                    break;
                case UpdateAction.PendingModify:
                    break;
                case UpdateAction.PendingFill:
                    break;
                case UpdateAction.Balance:

                    break;
                case UpdateAction.Credit:
                    break;
                default:
                    break;
            }
            MT4GetPosition();
            MT4MyUpdatePositionGrid();
            MT4UpdateAccountCaptial();
        }
        private void MT4Qc_OnQuote(object sender, QuoteEventArgs args)
        {
           
            if (args.Symbol == _mt4Quote.Symbol)
            {
                _mt4Quote.NewQuoute(args);
                UpdateMT4Quote();
            }
           
        }
        #endregion
        #region UI
        private void UpdateMT4Quote()
        {
            if (lblMT4Speed.InvokeRequired)
            {
                lblMT4Speed.Invoke(new Action(UpdateMT4Quote));
            }
            else
            {
                lblMT4Speed.Text = _mt4Quote.Speed.ToString();
                lblMT4Bid.Text = _mt4Quote.Quote.Bid.ToString("f2");
                lblMT4BidDiff0.Text = _mt4Quote.BidDiff.ElementAt(0).ToString();
                lblMT4BidDiff1.Text = _mt4Quote.BidDiff.ElementAt(1).ToString();
                lblMT4BidDiff2.Text = _mt4Quote.BidDiff.ElementAt(2).ToString();
                lblMT4BidDiff3.Text = _mt4Quote.BidDiff.ElementAt(3).ToString();
                lblMT4BidDiff4.Text = _mt4Quote.BidDiff.ElementAt(4).ToString();

                lblMT4Ask.Text = _mt4Quote.Quote.Ask.ToString("f2");
                lblMT4AskDiff0.Text = _mt4Quote.AskDiff.ElementAt(0).ToString();
                lblMT4AskDiff1.Text = _mt4Quote.AskDiff.ElementAt(1).ToString();
                lblMT4AskDiff2.Text = _mt4Quote.AskDiff.ElementAt(2).ToString();
                lblMT4AskDiff3.Text = _mt4Quote.AskDiff.ElementAt(3).ToString();
                lblMT4AskDiff4.Text = _mt4Quote.AskDiff.ElementAt(4).ToString();               
            }
        }
        private void UpdateDSQuote()
        {
            if (lblOECSpeed.InvokeRequired)
            {
                lblOECSpeed.Invoke(new Action(UpdateDSQuote));
            }
            else
            {
                lblOECSpeed.Text = _oecQutote.Speed.ToString();
                lblOECBid.Text = _oecQutote.Bid.ToString("f2");
                lblOECBidDiff0.Text = _oecQutote.BidDiff.ElementAt(0).ToString();
                lblOECBidDiff1.Text = _oecQutote.BidDiff.ElementAt(1).ToString();
                lblOECBidDiff2.Text = _oecQutote.BidDiff.ElementAt(2).ToString();
                lblOECBidDiff3.Text = _oecQutote.BidDiff.ElementAt(3).ToString();
                lblOECBidDiff4.Text = _oecQutote.BidDiff.ElementAt(4).ToString();

                lblOECAsk.Text = _oecQutote.Ask.ToString("f2");
                lblOECAskDiff0.Text = _oecQutote.AskDiff.ElementAt(0).ToString();
                lblOECAskDiff1.Text = _oecQutote.AskDiff.ElementAt(1).ToString();
                lblOECAskDiff2.Text = _oecQutote.AskDiff.ElementAt(2).ToString();
                lblOECAskDiff3.Text = _oecQutote.AskDiff.ElementAt(3).ToString();
                lblOECAskDiff4.Text = _oecQutote.AskDiff.ElementAt(4).ToString();
            }
        }

        /// <summary>
        /// 更新帐户资金信息
        /// </summary>
        private void MF4UpdateAccountCaptial()
        { 
        }
        private void MT4UpdateAccountCaptial()
        {
            if(_MT4qc!=null && _MT4qc.Connected)
            {
                if (lblBlance.InvokeRequired)
                {
                    lblBlance.Invoke(new Action(MT4UpdateAccountCaptial));
                }
                else
                {
                    lblBlance.Text = _MT4qc.AccountBalance.ToString();
                    lblEquity.Text = _MT4qc.AccountEquity.ToString();
                    lblMargin.Text = _MT4qc.AccountMargin.ToString();
                    lblFreeMargin.Text = _MT4qc.AccountFreeMargin.ToString();
                }
            }
        }

        /// <summary>
        /// 更新帐户连接状态
        /// </summary>
        /// <param name="Status">true= 连接，false=断开</param>
        private void MF4UpdateConnectStatus(bool Status)
        { 
        }
        private void  MT4UpdateConnectStatus(bool Status)
        {
            if (lblConnectStatus.InvokeRequired)
            {
                lblConnectStatus.Invoke(new Action<bool>(MT4UpdateConnectStatus), new object[] { Status });

            }
            else
            {
                lblConnectStatus.Text = Status ? "已连接" : "断开";
                if (Status)
                {
                    lblConnectStatus.BackColor = Color.Green;
                }
                else
                {
                    lblConnectStatus.BackColor = Color.Red;
                }
                btnLogin.Enabled = !Status;
                btnLogout.Enabled = Status;
            }
        }
        private void Log(string msg)
        {
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action<string>(Log), new object[] { msg });
            }
            else
            {
                lstLog.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss.fff") + " " + msg);
                if (lstLog.Items.Count>50)
                {
                    lstLog.Items.RemoveAt(lstLog.Items.Count-1);
                }
            }
            _logger.Info(msg);
        }
        private void LogTradeRecord(string msg)
        {
            if (lstTradeRecord.InvokeRequired)
            {
                lstTradeRecord.Invoke(new Action<string>(LogTradeRecord), new object[] { msg });
            }
            else
            {
                lstTradeRecord.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss.fff") + " " + msg);
            }
            _logger.Info(msg);
        }
        private void MyLogTradeRecord(string msg)
        {
            if (lstTradeRecord.InvokeRequired)
            {
                lstTradeRecord.Invoke(new Action<string>(MyLogTradeRecord), new object[] { msg });
            }
            else
            {
                lstTradeRecord.TopIndex = lstTradeRecord.Items.Count - 1;
                lstTradeRecord.Items.Add(msg);
                Utils.AddTradeMsg(msg, _UserCode);
                if (msg.Contains("[接收]"))
                {
                    Utils.AddTradeMsg("", _UserCode);
                }
            }
            if (lstTradeRecord.Items.Count>50) 
            {
                lstTradeRecord.Items.RemoveAt(lstTradeRecord.Items.Count - 1);
            }
            _logger.Info(msg);
        }

        #region Position
        private void MF4GetPosition()
        { 
        }
        private void MT4GetPosition()
        {
            try
            {
               if(_MT4qc!=null && _MT4qc.Connected)
                {
                    _MT4orders = _MT4qc.GetOpenedOrders();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                Log(ex.Message);
            }            
        }
        private void MF4MyUpdatePositionGrid()
        { 
        }
        private void MT4MyUpdatePositionGrid()
        {
            if (gvPositions.InvokeRequired)
            {
                gvPositions.Invoke(new Action(MT4MyUpdatePositionGrid));
            }
            else
            {
                if (_MT4orders != null)
                {
                    foreach (var o in _MT4orders)
                    {
                        double points = 0.0;
                        if (_mt4Quote.Symbol == o.Symbol && _mt4Quote.Quote != null)
                        {
                            points = o.Type == Op.Sell || o.Type == Op.SellLimit ? o.OpenPrice - _mt4Quote.Quote.Ask : _mt4Quote.Quote.Bid - o.OpenPrice;
                        }
                        bool IsExistOrder = false;
                        for (int i = 0; i < gvPositions.Rows.Count; i++)
                        {
                            if (o.Ticket.ToString().Equals(gvPositions.Rows[i].Cells[1].Value.ToString()))
                            {
                                IsExistOrder = true;
                                gvPositions.Rows[i].Cells[6].Value = points.ToString("f2");
                                gvPositions.Rows[i].Cells[7].Value = o.Profit;
                                TimeSpan ts = _MT4qc.ServerTime - o.OpenTime;
                                string h = ts.Hours.ToString().PadLeft(2, '0');
                                string m = ts.Minutes.ToString().PadLeft(2, '0');
                                string s = ts.Seconds.ToString().PadLeft(2, '0');
                                gvPositions.Rows[i].Cells[8].Value = h + ":" + m + ":" + s;
                            }
                        }
                        if (!IsExistOrder)
                        {
                            TimeSpan ts = _MT4qc.ServerTime - o.OpenTime;
                            string h = ts.Hours.ToString().PadLeft(2, '0');
                            string m = ts.Minutes.ToString().PadLeft(2, '0');
                            string s = ts.Seconds.ToString().PadLeft(2, '0');
                            gvPositions.Rows.Add(false, o.Ticket, o.Symbol, o.Type == Op.Sell?"卖出":"买入", o.Lots, o.OpenPrice, points.ToString("f2"), o.Profit, h + ":" + m + ":" + s);
                        }
                    }
                    for (int j = 0; j < gvPositions.Rows.Count; j++)
                    {
                        bool IsDeleteOrder = true;
                        foreach (var o1 in _MT4orders)
                        {
                            if (o1.Ticket.ToString().Equals(gvPositions.Rows[j].Cells[1].Value.ToString()))
                            {
                                IsDeleteOrder = false;
                            }
                        }
                        if (IsDeleteOrder)
                        {
                            gvPositions.Rows.RemoveAt(gvPositions.Rows[j].Index);
                        }
                    }
                }
                else
                {
                    gvPositions.Rows.Clear();
                }
                if (gvPositions.Rows.Count == 1)
                {
                    gvPositions.Rows[0].Selected = true;
                    gvPositions.Rows[0].Cells[0].Value = true;
                    //DateTime ot = Convert.ToDateTime(gvPositions.Rows[0].Cells[9].Value);
                    //TimeSpan ts = _qc.ServerTime - ot;
                    //string h = ts.Hours.ToString().PadLeft(2, '0');
                    //string m = ts.Minutes.ToString().PadLeft(2, '0');
                    //string s = ts.Seconds.ToString().PadLeft(2, '0');
                    //string td = h + ":" + m + ":" + s;
                    //gvPositions.Rows[0].Cells[8].Value = td;
                }
                else
                {
                    /**
                    for (int k = 0; k < gvPositions.Rows.Count; k++)
                    {
                        if (((bool)gvPositions.Rows[k].Cells[0].Value) == true)
                        {
                            gvPositions.Rows[k].Selected = true;
                        }
                        else 
                        {
                            gvPositions.Rows[k].Selected = false;
                        }
                    }*/
                }
            }

        }
        private void IniPositionGrid()
        {
            gvPositions.AutoGenerateColumns = false;
            gvPositions.AllowUserToAddRows = false;
            gvPositions.AutoSize = true;
            gvPositions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            DataGridViewColumn column0 = new DataGridViewCheckBoxColumn();
            column0.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column0.SortMode = DataGridViewColumnSortMode.NotSortable;
            column0.HeaderText = "";
            column0.ReadOnly = false;
            column0.Width = 20;
            gvPositions.Columns.Add(column0);

            DataGridViewColumn column1 = new DataGridViewTextBoxColumn();
            column1.HeaderText = "订单号";
            column1.DataPropertyName = "Ticket";
            column1.Name = "订单号";
            column1.ReadOnly = true;
            column1.Width = 70;
            column1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column1.SortMode = DataGridViewColumnSortMode.NotSortable;
            gvPositions.Columns.Add(column1);
            
            DataGridViewColumn column2 = new DataGridViewTextBoxColumn();
            column2.HeaderText = "品种";
            column2.DataPropertyName = "symbol";
            column2.Name = "品种";
            column2.ReadOnly = true;
            column2.Width = 48;
            column2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column2.SortMode = DataGridViewColumnSortMode.NotSortable;
            gvPositions.Columns.Add(column2);

            DataGridViewColumn column3 = new DataGridViewTextBoxColumn();
            column3.HeaderText = "类型";
            column3.DataPropertyName = "Dir";
            column3.Name = "类型";
            column3.ReadOnly = true;
            column3.Width = 45;
            column3.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column3.SortMode = DataGridViewColumnSortMode.NotSortable;
            gvPositions.Columns.Add(column3);

            DataGridViewColumn column4 = new DataGridViewTextBoxColumn();
            column4.HeaderText = "手数";
            column4.DataPropertyName = "Lots";
            column4.Name = "手数";
            column4.ReadOnly = true;
            column4.Width = 45;
            column4.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column4.SortMode = DataGridViewColumnSortMode.NotSortable;
            gvPositions.Columns.Add(column4);

            DataGridViewColumn column5 = new DataGridViewTextBoxColumn();
            column5.HeaderText = "开仓价";
            column5.DataPropertyName = "OpenPrice";
            column5.Name = "开仓价";
            column5.ReadOnly = true;
            column5.Width = 70;
            column5.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column5.SortMode = DataGridViewColumnSortMode.NotSortable;
            gvPositions.Columns.Add(column5);

            DataGridViewColumn column6 = new DataGridViewTextBoxColumn();
            column6.HeaderText = "点数";
            column6.DataPropertyName = "Points";
            column6.Name = "点数";
            column6.ReadOnly = true;
            column6.Width = 60;
            column6.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column6.SortMode = DataGridViewColumnSortMode.NotSortable;
            gvPositions.Columns.Add(column6);

            DataGridViewColumn column7 = new DataGridViewTextBoxColumn();
            column7.HeaderText = "盈亏";
            column7.DataPropertyName = "PL";
            column7.Name = "盈亏";
            column7.ReadOnly = true;
            column7.Width = 60;
            column7.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column7.SortMode = DataGridViewColumnSortMode.NotSortable;
            gvPositions.Columns.Add(column7);

            DataGridViewColumn column8 = new DataGridViewTextBoxColumn();
            column8.HeaderText = "持仓时间";
            column8.DataPropertyName = "Period";
            column8.Name = "持仓时间";
            column8.ReadOnly = true;
            column8.Width = 90;
            column8.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column8.SortMode = DataGridViewColumnSortMode.NotSortable;
            gvPositions.Columns.Add(column8);
            /**
            DataGridViewColumn column9 = new DataGridViewTextBoxColumn();
            column9.HeaderText = "开仓时间";
            column9.DataPropertyName = "OpenTime";
            column9.Name = "开仓时间";
            column9.ReadOnly = true;
            column9.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column9.SortMode = DataGridViewColumnSortMode.NotSortable;
            gvPositions.Columns.Add(column9);
            */

        }
        #endregion



        #endregion
        #region OEC 
        private void bgwMarketQuote_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if( bgwMarketQuote.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    string results = _subscriber.ReceiveFrameString();
                    string[] split = results.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    //参数说明：Gold 1893.4 1893.6 145158
                    //_logger.Info("OnMarketQuote :" + split[0] + " " + split[1] + " " + split[2] + " " + split[3]);
                    /**
                     * caiqiufu modified by 20201010
                     * _oecQutote.BidDiff[0]  获取上次bid值
                     * _orders[] 判断是否有订单,不用再次从平台获取
                     */
                    _oecQutote.MyNewQuoute(split[0], double.Parse(split[1]), double.Parse(split[2]));
                    if (EnableAutoTrade)
                    {
                        MT4MyAutoTradeTransaction();
                    }
                    UpdateOECQuote();
                    outputDiffList(double.Parse(split[1]), double.Parse(split[2]));
                }
            }           
        }

        private void MyOECConnection() 
        {
            Log("连接到OEC服务器：" + txtPublishAddress.Text);
            try
            {
                if (txtPublishAddress.Text == null || txtPublishAddress.Text == "")
                {
                    MessageBox.Show("OEC服务器地址未配置");
                    return;
                }
                _subscriber.Options.TcpKeepalive = true;
                _subscriber.Options.TcpKeepaliveIdle = new TimeSpan(5, 0, 0); 
                _subscriber.Options.TcpKeepaliveInterval = new TimeSpan(0, 0, 1);
                _subscriber.Connect(txtPublishAddress.Text);

                _subscriber.Subscribe("");
                if (bgwMarketQuote.IsBusy != true)
                {
                    // Start the asynchronous operation.
                    bgwMarketQuote.RunWorkerAsync();
                }
                btnOECLogin.Enabled = false;
                btnOECLogout.Enabled = true;
                label_Price_Connect.BackColor = Color.Green;
                label_Price_Connect.Text = "已连接";
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                Log("连接到OEC服务器时，错误：" + ex.Message);
                label_Price_Connect.BackColor = Color.Red;
                label_Price_Connect.Text = "连接失败";
            }
        }


        #endregion
        //Boolean BuyFlag = false;
        //Boolean SellFlag = false;
        /**
         * modified by caiqiufu 20201010
         * 交易 
         */
        private void MF4MyAutoTradeTransaction()
        { 
        }
        private void MT4MyAutoTradeTransaction() 
        {

            //自动买多开仓
            if (chkBuyOpen.Checked && (_oecQutote.BidDiff[0] >= (double)_config.TradePara.BuyOpen))
            {
                MT4OpenBuyOrder();
                IsAutoOperationFlag = true;
                CancelCheck();
            }
            //自动卖空开仓
            if (chkSellOpen.Checked && (_oecQutote.BidDiff[0] <= (double)(-_config.TradePara.SellOpen)))
            {
                MT4OpenSellOrder();
                IsAutoOperationFlag = true;
                CancelCheck();
            }

            //自动买多平仓
            if (chkBuyClose.Checked && (_oecQutote.BidDiff[0] <= (double)(-_config.TradePara.BuyClose)))
            {
                if (_MT4orders != null && _MT4orders.Length > 0)
                {
                    MT4CloseBuyOrder(_MT4orders[0]);
                    IsAutoOperationFlag = true;
                    _MT4orders = null;
                    CancelCheck();
                }
            }

            //自动卖空平仓
            if (chkSellClose.Checked && (_oecQutote.BidDiff[0] >= (double)(_config.TradePara.SellClose)))
            {
                if (_MT4orders != null && _MT4orders.Length > 0)
                {
                    MT4CloseSellOrder(_MT4orders[0]);
                    IsAutoOperationFlag = true;
                    _MT4orders = null;
                    CancelCheck();
                }
            }
        }
        private double _lastBid = 0;
        private void outputDiffList(double cBid, double cAsk)
        {
            if ((_lastBid!=cBid)&&(_oecQutote.BidDiff[0] >= (double)_config.TradePara.BuyOpen|| _oecQutote.BidDiff[0] <= (double)(-_config.TradePara.SellOpen) || _oecQutote.BidDiff[0] <= (double)(-_config.TradePara.BuyClose) || _oecQutote.BidDiff[0] >= (double)(_config.TradePara.SellClose)))
            {
                double Diff = _oecQutote.BidDiff[0];
                string BidDiffMsg = DateTime.Now.ToString("HH:mm:ss") + "  +" + _oecQutote.BidDiff[0];
                if (Diff < 0)
                {
                    BidDiffMsg = DateTime.Now.ToString("HH:mm:ss") + "  " + _oecQutote.BidDiff[0];
                }
                writeBiddViewList(BidDiffMsg);
            }
            _lastBid = cBid;
        }

        /**
         * 买多开仓
         */
        private void MF4OpenBuyOrder()
        { 
        }
        private void MT4OpenBuyOrder()
        {
            try
            {
                double lots = (double)_config.TradePara.BuyLots;
                var quote = _MT4qc.GetQuote(TradeSymbol);
                double price = quote.Ask;
                SendTime = DateTime.Now;
                MyLogTradeRecord(string.Format("["+ SendTime.ToString("HH:mm:ss.fff")+"]"+"[自动买多][发送]报价:{0} 手数:{1} 平台价:{2} 跳次:{3}", price, lots, price, _oecQutote.BidDiff[0]));
                int tempID = _MT4oc.OrderSendAsync(TradeSymbol, Op.Buy, lots, price, _Slippage, 0, 0, "", 0, new DateTime());
            }
            catch (Exception ex)
            {

                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[自动买多][发送]异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                MyLogTradeRecord(msg);
                Log(ex.Message);
            }
        }
        /**
         * 买多平仓
         */
        private void MF4CloseBuyOrder(Order o)
        { 
        }
        private void MT4CloseBuyOrder(Order o)
        {
            try
            {
                int ticket = o.Ticket;
                string symbol = o.Symbol;
                string type = o.Type.ToString();
                double lots = o.Lots;
                double price = _mt4Quote.Quote.Bid;
                SendTime = DateTime.Now;
                MyLogTradeRecord(string.Format("[" + SendTime.ToString("HH:mm:ss.fff") + "]" + "[自动买平][发送]报价:{0} 手数:{1} 平台价:{2} 跳次:{3}", price, lots, price, _oecQutote.BidDiff[0]));
                int tempID = _MT4oc.OrderCloseAsync(symbol, ticket, lots, price, _Slippage);
            }
            catch (Exception ex)
            {
                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[自动买平][发送]异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                MyLogTradeRecord(msg);
                Log(ex.Message);
            }
        }
        /**
         * 卖空开仓
         */
        private void MF4OpenSellOrder()
        { 
        }
        private void MT4OpenSellOrder()
        {
            try
            {
                double lots = (double)_config.TradePara.SellLots;
                var quote = _MT4qc.GetQuote(TradeSymbol);
                double price = quote.Bid;
                SendTime = DateTime.Now;
                MyLogTradeRecord(string.Format("[" + SendTime.ToString("HH:mm:ss.fff") + "]" + "[自动卖空][发送]报价:{0} 手数:{1} 平台价:{2} 跳次:{3}", price, lots, price,_oecQutote.BidDiff[0]));
                int tempID = _MT4oc.OrderSendAsync(TradeSymbol, Op.Sell, lots, price, _Slippage, 0, 0, "", 0, new DateTime());
            }
            catch (Exception ex)
            {
                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[自动卖空][发送]异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                MyLogTradeRecord(msg);
                Log(ex.Message);
            }
        }
        /**
         * 卖空平仓
         */
        private void MF4CloseSellOrder(Order o)
        { }
        private void MT4CloseSellOrder(Order o)
        {
            try
            {
                int ticket = o.Ticket;
                string symbol = o.Symbol;
                string type = o.Type.ToString();
                double lots = o.Lots;
                double price = _mt4Quote.Quote.Bid;
                SendTime = DateTime.Now;
                MyLogTradeRecord(string.Format("[" + SendTime.ToString("HH:mm:ss.fff") + "]" + "[自动卖平][发送] 报价:{0} 手数:{1} 平台价:{2} 跳次:{3}", price, lots, price, _oecQutote.BidDiff[0]));
                int tempID = _MT4oc.OrderCloseAsync(symbol, ticket, lots, price, _Slippage);
            }
            catch (Exception ex)
            {
                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[自动卖平][发送] 异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                MyLogTradeRecord(msg);
                Log(ex.Message);
            }
        }
        private void MF4CalcNeedOpenOrder()
        { }
        private void MT4CalcNeedOpenOrder()
        {
            if (chkBuyOpen.Checked)
            {
               if (_oecQutote.BidDiff[0]>= (double)_config.TradePara.BuyOpen)
                {
                    LogTradeRecord($"允许[开仓（买多单）]:OEC市场{TradeSymbol}.BidDiff {_oecQutote.BidDiff[0]}>=设置{nudBuyOpen.Value} 手数={_config.TradePara.BuyOpen}");
                    
                    CancelCheck();
                    try
                    {
                        var quote = _MT4qc.GetQuote(TradeSymbol);
                        double lots = (double)_config.TradePara.BuyLots;
                        int tempID = _MT4oc.OrderSendAsync(TradeSymbol, Op.Buy, lots, quote.Ask, _Slippage, 0, 0, "", 0, new DateTime());
                        LogTradeRecord(string.Format("[自动][开仓][委托={0}][{1}-{2}-{3}-{4}]", tempID, tempID, TradeSymbol, "Buy", lots));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message + "  " + ex.StackTrace);
                        Log(ex.Message);
                    }
                }
            }
           
            if (chkSellOpen.Checked)
            {

                if (_oecQutote.BidDiff[0] <= (double)(-_config.TradePara.SellOpen))
                {
                    LogTradeRecord($"允许[开仓（卖空单）]:OEC市场{TradeSymbol}.BidDiff {_oecQutote.BidDiff[0]}<=设置-{nudSellOpen.Value} 手数={_config.TradePara.SellOpen}");

                    CancelCheck();
                    try
                    {
                        var quote = _MT4qc.GetQuote(TradeSymbol);
                        double lots = (double)_config.TradePara.SellLots;
                        int tempID = _MT4oc.OrderSendAsync(TradeSymbol, Op.Sell, lots, quote.Bid, _Slippage, 0, 0, "", 0, new DateTime());
                        LogTradeRecord(string.Format("[自动][开仓][委托={0}][{1}-{2}-{3}-{4}]", tempID, tempID, TradeSymbol, "Sell", lots));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message + "  " + ex.StackTrace);
                        Log(ex.Message);
                    }
                }

            }

        }
        private void MF4CalcNeedCloseOrder(Order o)
        { }
        private void MT4CalcNeedCloseOrder( Order o)
        {

            bool needClose = false;
            if (chkBuyClose.Checked)
            {
                if(o.Type== Op.Buy|| o.Type == Op.BuyLimit)
                {

                    if (_oecQutote.BidDiff[0] <= (double)(-_config.TradePara.BuyClose))
                    {
                        LogTradeRecord($"允许[平仓（买多单）]:OEC市场{TradeSymbol}.BidDiff {_oecQutote.BidDiff[0]}<=设置-{_config.TradePara.BuyClose}");
                        CancelCheck();
                        needClose = true;
                    }

                }

            }
           

            if (chkSellClose.Checked)
            {
                if (o.Type == Op.Sell || o.Type == Op.SellLimit)
                {
                    if (_oecQutote.BidDiff[0] >=(double)_config.TradePara.SellClose)
                    {
                        LogTradeRecord($"允许[平仓（卖空单）]:OEC市场{TradeSymbol}.BidDiff{_oecQutote.BidDiff[0]}>=设置{_config.TradePara.SellClose}");
                        CancelCheck();
                        needClose = true;
                    }
                }
            }

            if (needClose)
            {
                try
                {
                    int ticket = o.Ticket;
                    string symbol = o.Symbol;
                    string type = o.Type.ToString();
                    double lots =o.Lots;
                    var quote = _MT4qc.GetQuote(symbol);
                    double price = quote.Bid;//**************************  
                    var ret = _MT4oc.OrderCloseAsync(symbol, ticket, lots, price, _Slippage);
                    LogTradeRecord(string.Format("[自动][平仓][委托={0}][{1}-{2}-{3}-{4}]", ret, ticket, symbol, type, lots));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message + "  " + ex.StackTrace);
                    Log(ex.Message);
                }
            }
           
        }

        private void CancelCheck()
        {
            if (chkBuyOpen.InvokeRequired)
            {
                chkBuyOpen.Invoke(new Action(CancelCheck), new object[] { });
            }
            else
            {
                chkBuyOpen.Checked = false;
                chkSellOpen.Checked = false;
                chkSellClose.Checked = false;
                chkBuyClose.Checked = false;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (txtBrokeName.Text == null || txtBrokeName.Text == "")
            {
                MessageBox.Show("平台商不能为空");
                return;
            }
            if (!((textBox_Live_IP.Text != "" && textBox_Live_Port.Text != "") || (textBox_Demo_IP.Text != "" && textBox_Demo_Port.Text != "")))
            {
                MessageBox.Show("至少需要填写模拟或实盘服务器地址和端口");
                return;
            }
            AddConfig();
            ClearInitialPara();
            LoadConfig();
            Log("平台信息保存成功");
        }
        private void comboBox_Broker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isLoadConfigCompleted)
            {
                string BrokerName = comboBox_Broker.SelectedItem.ToString();
                _config.DefaultBroker = BrokerName;
                _Broker = GetBroker(BrokerName);
                _Account = GetAccount(BrokerName, _Broker.DefaultAccount);
                if (_Broker != null && _Account != null)
                {
                    InitialPara();
                    comboBox_AccType.SelectedItem = _AccountType;
                    SetValues();
                    SetBrokerPara();
                }
            }
        }
        private void SetBrokerPara()
        {
            txtBrokeName.Text = _Broker.BrokerName;
            textBox_Symbol.Text = _Symbol;
            textBox_Demo_IP.Text = _Broker.DemoAccount.IP;
            textBox_Demo_Port.Text = _Broker.DemoAccount.Port;
            textBox_Live_IP.Text = _Broker.LiveAccount.IP;
            textBox_Live_Port.Text = _Broker.LiveAccount.Port;
        }

        private void button_Save_Click(object sender, EventArgs e)
        {
            if (textBox_UserCode.Text == null || textBox_UserCode.Text == "")
            {
                MessageBox.Show("账号不能为空");
                return;
            }
            if (textBox_Password.Text == null || textBox_Password.Text == "")
            {
                MessageBox.Show("密码不能为空");
                return;
            }
            SaveConfig();
            Log("账户信息保存成功");
        }
        private void comboBox_AccType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isLoadConfigCompleted)
            {
                _AccountType = comboBox_AccType.SelectedItem.ToString();
                _Account = GetAccount(_BrokerName, _AccountType);
                if (_Broker != null && _Account != null)
                {
                    InitialPara();
                    SetValues();
                }
            }
        }
        private void button_TradePara_Save_Click(object sender, EventArgs e)
        {
            if (_Button_ChangeColor_Timer_TradePara != null)
            {
                _Button_ChangeColor_Timer_TradePara.Stop();
                _Button_ChangeColor_Timer_TradePara.Enabled = false;
                button_TradePara_Save.BackColor = (Color.Transparent);
            }
            _config.TradePara.SellOpen = nudSellOpen.Value;
            _config.TradePara.SellClose = nudSellClose.Value;
            _config.TradePara.SellLots = nudSellLots.Value;
            _config.TradePara.BuyOpen = nudBuyOpen.Value;
            _config.TradePara.BuyClose = nudBuyClose.Value;
            _config.TradePara.BuyLots = nudBuyLots.Value;

            File.WriteAllText(_configFile, JsonConvert.SerializeObject(_config));
        }
        private void button_TradePara_Cancel_Click(object sender, EventArgs e)
        {
            nudSellOpen.Value = _config.TradePara.SellOpen;
            nudSellClose.Value = _config.TradePara.SellClose;
            nudSellLots.Value = _config.TradePara.SellLots;
            nudBuyOpen.Value = _config.TradePara.BuyOpen;
            nudBuyClose.Value = _config.TradePara.BuyClose;
            nudBuyLots.Value = _config.TradePara.BuyLots;
            if (_Button_ChangeColor_Timer_TradePara != null)
            {
                _Button_ChangeColor_Timer_TradePara.Stop();
                _Button_ChangeColor_Timer_TradePara.Enabled = false;
                button_TradePara_Save.BackColor = (Color.Transparent);
            }
        }

        private System.Timers.Timer _Button_ChangeColor_Timer_TradePara = null;
        private void timer_Tick_Color_TradePara()
        {
            if (_Button_ChangeColor_Timer_TradePara == null)
            {
                _Button_ChangeColor_Timer_TradePara = new System.Timers.Timer(500);
                _Button_ChangeColor_Timer_TradePara.Enabled = false;
                _Button_ChangeColor_Timer_TradePara.Elapsed += new System.Timers.ElapsedEventHandler(timer_Tick_TradePara);
            }
            else 
            {
                _Button_ChangeColor_Timer_TradePara.Start();
                _Button_ChangeColor_Timer_TradePara.Enabled = true;
                button_TradePara_Save.BackColor = (Color.Transparent);
            }
        }
        void timer_Tick_TradePara(object sender, EventArgs e)
        {
            _Button_ChangeColor_Timer_TradePara.Stop();
            if (button_TradePara_Save.BackColor == Color.Transparent)
            {
                button_TradePara_Save.BackColor = Color.Red;
            }
            else
            {
                button_TradePara_Save.BackColor = (Color.Transparent);
            }
            _Button_ChangeColor_Timer_TradePara.Start();
        }

        private void nudSellOpen_ValueChanged(object sender, EventArgs e)
        {
            timer_Tick_Color_TradePara();
        }

        private void nudSellLots_ValueChanged(object sender, EventArgs e)
        {
            timer_Tick_Color_TradePara();
        }

        private void nudBuyOpen_ValueChanged(object sender, EventArgs e)
        {
            timer_Tick_Color_TradePara();
        }

        private void nudSellClose_ValueChanged(object sender, EventArgs e)
        {
            timer_Tick_Color_TradePara();
        }

        private void nudBuyClose_ValueChanged(object sender, EventArgs e)
        {
            timer_Tick_Color_TradePara();
        }
        private void textBox_UserCode_TextChanged(object sender, EventArgs e)
        {
            _UserCode = textBox_UserCode.Text;
        }

        private void textBox_Password_TextChanged(object sender, EventArgs e)
        {
            _Password = textBox_Password.Text;
        }

        private void button_OEC_Save_Click(object sender, EventArgs e)
        {
            if ("T4".Equals(_DSType))
            {
                _config.PublishAddressT4 = txtPublishAddress.Text;
            }
            if ("OEC".Equals(_DSType))
            {
                _config.PublishAddressOEC = txtPublishAddress.Text;
            }
            SaveConfig();
            Log("数据源信息保存成功");
        }

        private void button_Open_Trade_Click(object sender, EventArgs e)
        {
            string path = Utils.GenerateDirectoryPath("TradeInfo");
            System.DateTime currentTime = System.DateTime.Now;
            string fileName = currentTime.ToString("yyyy-MM-dd") + "_" + _UserCode;
            string destFile = System.IO.Path.Combine(path, fileName + ".txt");
            if (File.Exists(destFile))
            {
                System.Diagnostics.Process.Start(destFile);
            }
        }

        private void button_Clear_Click(object sender, EventArgs e)
        {
            lstTradeRecord.Items.Clear();
        }
        private void writeBiddViewList(string msg)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new Action<string>(writeBiddViewList), new object[] { msg });

            }
            else
            {
                //让数据向下移动，上方会一直显示最新数据
                //listBox1.Items.Add(0, msg);
                //让数据向上移动，下方会一直显示最新数据
                listBox1.Items.Add(msg);
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
            if (listBox1.Items.Count > 50)
            {
                listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
            }

        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                string Diff = listBox1.Items[e.Index].ToString();
                _logger.Debug("跳次="+ Diff);
                if (Diff.Contains("+"))
                {
                    e.Graphics.DrawString(Diff, e.Font, Brushes.Red, e.Bounds, StringFormat.GenericDefault);
                }
                else
                {
                    e.Graphics.DrawString(Diff, e.Font, Brushes.Green, e.Bounds, StringFormat.GenericDefault);
                }
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }
        private void button_Broker_Delete_Click(object sender, EventArgs e)
        {
            if (_Broker != null && _config.Broker.Length > 1)
            {
                Broker[] NewBroker = new Broker[_config.Broker.Length - 1];
                int j = 0;
                for (int i = 0; i < _config.Broker.Length; i++)
                {
                    if (!txtBrokeName.Text.Equals(_config.Broker[i].BrokerName))
                    {
                        NewBroker[j] = _config.Broker[i];
                        j++;
                    }
                }
                if (_BrokerName.Equals(txtBrokeName.Text) && _config.Broker.Length > 0)
                {
                    _config.DefaultBroker = _config.Broker[0].BrokerName;
                }
                _config.Broker = NewBroker;
                File.WriteAllText(_configFile, JsonConvert.SerializeObject(_config));
                ClearInitialPara();
                LoadConfig();
                Log("平台信息删除成功");
            }
            else 
            {
                Log("最后一条平台信息不能删除");
            }
        }

        private void gvPositions_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            bool checkBoxValue = (bool)gvPositions.Rows[e.RowIndex].Cells[0].Value;
            if (checkBoxValue)
            {
                //gvPositions.Rows[e.RowIndex].Selected = true;
            }
            else 
            {
                //gvPositions.Rows[e.RowIndex].Selected = false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
             _DSType = comboBox1.SelectedItem.ToString();
            SetDSPara();
        }
        private void gvPositions_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                string OptType = gvPositions.Rows[e.RowIndex].Cells[3].Value.ToString();
                if ("买入".Equals(OptType))
                {
                    gvPositions.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.Red;
                    gvPositions.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
                }
                if ("卖出".Equals(OptType))
                {
                    gvPositions.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.Green;
                    gvPositions.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Green;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmTrade form2 = new frmTrade();
            form2.ShowDialog(this);//这里一定要用ShowDialog，否则画面程序依旧会结束。
        }
        private void AutoTrade_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoTrade.Checked)
            {
                Log("启动全自动交易...");
            }
            else
            {
                Log("关闭全自动交易...");
            }
        }

        private void comboBox_Slippage_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Slippage = Convert.ToInt32(comboBox_Slippage.SelectedItem);
        }

        private void comboBox_PlatForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            _PlatForm = comboBox_PlatForm.SelectedItem.ToString();
        }
    }
}
