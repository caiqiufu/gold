using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetMQ;
using NetMQ.Sockets;
using System.IO;
using Newtonsoft.Json;
using NLog;
using uClient.Comm;
using System.Threading;
using System.Threading.Tasks;
using M4.Common.Enums;
using TradingAPI.MT4Server;
using Order = TradingAPI.MT4Server.Order;
using mtapi.mt5;

namespace uClient.Broker
{
    public partial class TradeForm : Form
    {
        //MT4 平台 begin
        public MT4 _MT4;
        //MT4 平台 end

        //MF4 平台 beign
        public MF4 _MF4;
        //MF4 平台 end

        //V4 平台 beign
        public V4 _V4;
        //V4 平台 end

        //MT5 平台 begin
        public MT5 _MT5;
        //MT5 平台 end

        /// <summary>
        /// 数据源
        /// </summary>
        public CustomQuotePanel _DSQuote = new CustomQuotePanel("Gold", 0.1);
        /// <summary>
        /// 公共参数
        /// </summary>
        public Config _Config;
        public MyConfig _MyConfig;
        public StrategyConfig _StrategyConfig;
        public Comm.Platform _Platform;
        public string _TradeSymbol = "";
        public TradeParametre _TradePara;
        public Comm.Broker _Broker;
        public Account _Account;
        /// <summary>
        /// 配置文件是否加载完成
        /// </summary>
        public bool _IsLoadConfigCompleted = false;
        /// <summary>
        /// 订单发送时间
        /// </summary>
        public DateTime _SendTime = DateTime.Now;
        /// <summary>
        /// 订单接收时间
        /// </summary>
        //public DateTime _ReceiveTime;
        /// <summary>
        /// 系统配置文件
        /// </summary>
        public string _ConfigFile = "\\Config.json";
        /// <summary>
        /// 操作员配置文件
        /// </summary>
        public string _MyConfigFile = "\\MyConfig.json";
        /// <summary>
        /// 自动交易策略文件
        /// </summary>
        public string _StrategyFile = "\\Strategy.json";

        public NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 数据源消息订阅
        /// </summary>
        public SubscriberSocket _Subscriber = new SubscriberSocket();
        /// <summary>
        /// 主窗口发送消息
        /// </summary>
        public PublisherSocket _MasterPublisher = new PublisherSocket();
        /// <summary>
        /// 锁窗口接收消息
        /// </summary>
        public SubscriberSocket _LockSubscriber = new SubscriberSocket();
        /// <summary>
        /// 主窗口参数
        /// </summary>
        public MainFormPara _MainFormPara;
        /// <summary>
        /// 报价显示面板
        /// </summary>
        public QuotaDisplayPanel quotaDisplayPanel = new QuotaDisplayPanel();
        /// <summary>
        /// 账户显示面板
        /// </summary>
        public AccountDisplay accountDisplay = new AccountDisplay();
        /// <summary>
        /// 日志
        /// </summary>
        public Log _Log;
        public TradeForm(MainFormPara mainFormPara)
        {
            //默认值在Program.cs 文件中设置
            if (mainFormPara == null)
            {
                mainFormPara = new MainFormPara();
                mainFormPara.PlatformCode = "MT4";
                mainFormPara.BrokerCode = "";
                mainFormPara.BrokerName = "";
                mainFormPara.FormType = "MT4";
                mainFormPara.FormNo = "Trade1";
            }
            _MainFormPara = mainFormPara;            
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            InitObject();
        }

        public TradeForm()
        {
            MainFormPara mainFormPara = new MainFormPara();
            mainFormPara.PlatformCode = "MT4";
            mainFormPara.BrokerCode = "";
            mainFormPara.BrokerName = "";
            mainFormPara.FormType = "MT4";
            mainFormPara.FormNo = "Trade1";
            _MainFormPara = mainFormPara;
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            InitObject();
        }
        public void ChangePlatform(string platformCode)
        {
            if (string.Equals(platformCode, "MF4"))
            {
                _MainFormPara.PlatformCode = "MF4";
                _MainFormPara.BrokerCode = "SUI";
                _MainFormPara.BrokerName = "实德环球";
                _MainFormPara.FormType = "MF4";
                //_MainFormPara.FormNo = "";
            }
            else
            {
                _MainFormPara.PlatformCode = "MT4";
                _MainFormPara.BrokerCode = "";
                _MainFormPara.BrokerName = "";
                _MainFormPara.FormType = "MT4";
                //_MainFormPara.FormNo = "";
            }
            if (string.Equals(_Config.clientType,"EA")) 
            {
                //该段逻辑是从数据库trade_auto表读取托管交易配置信息
                IDictionary<string, string> eaInfo = DBHelper.getAutoInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo);
                if (eaInfo != null)
                {
                    _Config.PublishAddressEA = "tcp://" + _Config.SysConfig[eaInfo["ea_type"]] + ":5553";
                    string brokerCode = eaInfo["broker_code"];
                    if (string.Equals(brokerCode, "SUI"))
                    {
                        _MainFormPara.PlatformCode = "MF4";
                        _MainFormPara.FormType = "MF4";
                    }
                    else
                    {
                        _MainFormPara.PlatformCode = "MT4";
                        _MainFormPara.FormType = "MT4";
                    }
                }
                LoadEAType();
            }
            
            this.Text = _MainFormPara.FormNo;
            LoadConfig();
            
            _IsLoadConfigCompleted = true;
            GeneratePlatformObj();
            IniUI();
        }

        /// <summary>
        /// 实例化对象，该方法只能调用一次
        /// </summary>
        public void InitObject()
        {
            //Log 实例只初始化一次
            _Log = new Log(lstTradeRecord, lstLog,_MainFormPara.FormNo);
        }
        /// <summary>
        /// 初始化UI参数
        /// </summary>
        public void IniUI()
        {
            _logger.Info("开始初始化UI");
            //交易商
            comboBox_Broker.Items.Clear();
            foreach (Comm.Broker bro in _Platform.Broker)
            {
                comboBox_Broker.Items.Add(bro.BrokerName);
            }
            comboBox_Broker.SelectedItem = _Broker.BrokerName;
            //账户类型
            comboBox_AccType.Items.Clear();
            comboBox_AccType.Items.Add("Demo");
            comboBox_AccType.Items.Add("Live");
            //交易品类
            if (_Account != null && _Broker.Symbol != null && _Broker.Symbol.Length > 0)
            {
                comboBox_AccType.SelectedItem = _Account.Type;
                comboBox_Symbol.Items.Clear();
                foreach (string ss in _Broker.Symbol)
                {
                    comboBox_Symbol.Items.Add(ss);
                }
                comboBox_Symbol.SelectedItem = _TradeSymbol;
            }
            //用户账号密码
            textBox_UserCode.Text = _Account.UserCode;
            textBox_Password.Text = _Account.Password;

            //初始化订单列表
            IniPositionGrid();
            //停止按钮颜色改变事件
            if (_Button_ChangeColor_Timer_TradePara != null)
            {
                _Button_ChangeColor_Timer_TradePara.Stop();
                _Button_ChangeColor_Timer_TradePara.Enabled = false;
                button_TradePara_Save.BackColor = Color.White;
            }          
        }
        public string rootPath = "C:\\iAutoTrade";

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void LoadConfigFile() {
            string newMyConfigFile = rootPath + _MyConfigFile;
            string newConfigFile = rootPath + _ConfigFile;
            string newStrategyFile = rootPath + _StrategyFile;
            _logger.Info("开始加载初始化文件");
            if (File.Exists(newMyConfigFile))
            {
                _MyConfig = JsonConvert.DeserializeObject<MyConfig>(File.ReadAllText(newMyConfigFile));
            }
            else
            {
                _logger.Fatal("没有配置文件，系统初始化异常");
            }

            if (File.Exists(newConfigFile))
            {
                _Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(newConfigFile));
            }
            else
            {
                _logger.Fatal("没有初始化配置文件，系统初始化异常");
            }

            if (File.Exists(newStrategyFile))
            {
                _StrategyConfig = JsonConvert.DeserializeObject<StrategyConfig>(File.ReadAllText(newStrategyFile));
            }
            else
            {
                _logger.Fatal("没有策略配置文件，系统初始化异常");
            }
        }

        /// <summary>
        /// 加载配置参数
        /// </summary>
        public void LoadConfig()
        {
            if (_Config != null)
            {
                _Config.PlatformCode = _MainFormPara.PlatformCode;
                if (string.Equals(_Config.clientType, "EA"))
                {
                    //替换成数据库中配置的平台信息
                    Comm.Broker[] brokerList = DBHelper.getBrokerList();
                    if (brokerList != null && brokerList.Length > 0)
                    {
                        Platform mf4 = new Platform();
                        mf4.PlatformNo = 1;
                        mf4.PlatformCode = "MF4";
                        mf4.PlatformName = "MF4";
                        Platform mt4 = new Platform();
                        mt4.PlatformNo = 2;
                        mt4.PlatformCode = "MT4";
                        mt4.PlatformName = "MT4";

                        Platform mt5 = new Platform();
                        mt5.PlatformNo = 4;
                        mt5.PlatformCode = "MT5";
                        mt5.PlatformName = "MT5";


                        foreach (uClient.Comm.Broker bb in brokerList)
                        {
                            if (bb.PlatformNo == 2)
                            {
                                mt4.BrokerList.Add(bb.BrokerCode, bb);
                                mt4.DefaultBrokerCode = bb.BrokerCode;
                            }
                            if (bb.PlatformNo == 4)
                            {
                                mt5.BrokerList.Add(bb.BrokerCode, bb);
                                mt5.DefaultBrokerCode = bb.BrokerCode;
                            }
                            if (bb.PlatformNo == 1)
                            {
                                mf4.BrokerList.Add(bb.BrokerCode, bb);
                                mf4.DefaultBrokerCode = bb.BrokerCode;
                            }
                        }
                        if (mf4.BrokerList != null && mf4.BrokerList.Count > 0)
                        {
                            int i = 0;
                            foreach (var key in mf4.BrokerList.Keys)
                            {
                                mf4.Broker.Add(mf4.BrokerList[key]);
                            }
                        }
                        if (mt4.BrokerList != null && mt4.BrokerList.Count > 0)
                        {
                            foreach (var key in mt4.BrokerList.Keys)
                            {
                                mt4.Broker.Add(mt4.BrokerList[key]);
                            }
                        }
                        if (mt5.BrokerList != null && mt5.BrokerList.Count > 0)
                        {
                            foreach (var key in mt5.BrokerList.Keys)
                            {
                                mt5.Broker.Add(mt5.BrokerList[key]);
                            }
                        }
                        _Config.Platform[0] = mf4;
                        _Config.Platform[1] = mt4;
                        _Config.Platform[2] = mt5;
                    }
                }  
                

                if (_Config.Platform.Count > 0)
                {
                    foreach (Platform platform in _Config.Platform)
                    {
                        _Config.PlatformList.Remove(platform.PlatformCode);
                        _Config.PlatformList.Add(platform.PlatformCode, platform);
                        if (platform.Broker != null && platform.Broker.Count > 0)
                        {
                            foreach (Comm.Broker broker in platform.Broker)
                            {
                                if (_MyConfig != null && _MyConfig.Account.Length > 0)
                                {
                                    foreach (Account account in _MyConfig.Account)
                                    {
                                        if (string.Equals(broker.BrokerCode, account.BrokerCode))
                                        {
                                            if (string.Equals(account.Type, "Demo"))
                                            {
                                                broker.DemoAccount = account;
                                            }
                                            else
                                            {
                                                broker.LiveAccount = account;
                                            }
                                            if (account.TradePara != null)
                                            {
                                                broker.TradePara = account.TradePara;
                                            }
                                            else
                                            {
                                                account.TradePara = broker.TradePara;
                                            }
                                        }
                                    }
                                }
                                platform.BrokerList.Remove(broker.BrokerCode);
                                platform.BrokerList.Add(broker.BrokerCode, broker);
                            }
                        }
                        if (String.Equals(_MainFormPara.PlatformCode, platform.PlatformCode))
                        {
                            _Platform = platform;
                        }
                    }
                    if (_Platform.BrokerList.Keys.Contains(_Platform.DefaultBrokerCode))
                    {
                        string defaultBrokerCode = string.IsNullOrEmpty(_MainFormPara.BrokerCode) ? _Platform.DefaultBrokerCode : _MainFormPara.BrokerCode;
                        _Broker = _Platform.BrokerList[defaultBrokerCode];
                        if (string.Equals(_Config.clientType, "EA"))
                        {
                            //_Log.LogInfo("查询EA账户参数:IP="+ Utils.GetLocalIP()+ ",FormNo="+ _MainFormPara.FormNo);
                            //替换成数据库中配置的交易账户信息
                            Comm.Broker queryBroker = DBHelper.getBrokerInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo);
                            if (queryBroker != null)
                            {
                                _Broker = queryBroker;
                                //_Log.LogInfo("EA账户信息:"+ _Broker.toString());
                            }
                        }                           
                        if (_Broker != null)
                        {
                            _Account = _Broker.DefaultAccountType.Equals("Demo") ? _Broker.DemoAccount : _Broker.LiveAccount;
                            _TradeSymbol = _Broker.DefaultSymbol;
                            if (_Broker.TradePara == null)
                            {
                                _Broker.TradePara = _Platform.TradePara;
                            }
                            _TradePara = _Account.TradePara == null ? _Broker.TradePara : _Account.TradePara;
                            //初始化参数赋值,对于新增加的参数，如果没有赋初始化值，可能会报错
                            _TradePara.IncreaseLots = _TradePara.IncreaseLots == 0 ? (decimal)0.1 : _TradePara.IncreaseLots;

                            /////////////////////////////////////////////////////////
                            _Broker.TradePara = _TradePara;
                        }
                        else
                        {
                            _logger.Fatal("代理商[" + defaultBrokerCode + "]信息不存在，请检查配置文件");
                        }
                    }
                }
                //获取数据库配置数据
                _Config.SysConfig = DBHelper.getSysConfig();
            }
            else
            {
                _logger.Fatal("没有初始化配置文件，系统初始化异常");
            }
        }
        /// <summary>
        /// 加载eaType地址信息,默认使用EA主数据服务器地址,平台配置参数等
        /// </summary>
        public void LoadEAType() {
            _Config.PlatformCode = _MainFormPara.PlatformCode;

            IDictionary<string, string> eaInfo = DBHelper.getAutoInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo);
            if (eaInfo != null)
            {
                //tcp://127.0.0.1:5553
                _Config.PublishAddressEA = "tcp://"+ _Config.SysConfig[eaInfo["ea_type"]] + ":5553" ;
                _Config.DSType = "EA";
                _Config.clientType = "EA";
            }           
        }
        /// <summary>
        /// 配置参数加载完成后，或者切换平台、切换账户后，初始化参数
        /// </summary>
        public void InitialPara()
        {
            InitQuoteDisplay();
        }
        public void InitQuoteDisplay()
        {
            quotaDisplayPanel.lblMT4Speed = lblMT4Speed;
            quotaDisplayPanel.lblMT4Bid = lblMT4Bid;
            quotaDisplayPanel.lblMT4BidDiff0 = lblMT4BidDiff0;
            quotaDisplayPanel.lblMT4BidDiff1 = lblMT4BidDiff1;
            quotaDisplayPanel.lblMT4BidDiff2 = lblMT4BidDiff2;
            quotaDisplayPanel.lblMT4BidDiff3 = lblMT4BidDiff3;
            quotaDisplayPanel.lblMT4BidDiff4 = lblMT4BidDiff4;

            quotaDisplayPanel.lblMT4Ask = lblMT4Ask;
            quotaDisplayPanel.lblMT4AskDiff0 = lblMT4AskDiff0;
            quotaDisplayPanel.lblMT4AskDiff1 = lblMT4AskDiff1;
            quotaDisplayPanel.lblMT4AskDiff2 = lblMT4AskDiff2;
            quotaDisplayPanel.lblMT4AskDiff3 = lblMT4AskDiff3;
            quotaDisplayPanel.lblMT4AskDiff4 = lblMT4AskDiff4;

            accountDisplay.lblBlance = lblBlance;
            accountDisplay.lblEquity = lblEquity;
            accountDisplay.lblMargin = lblMargin;
            accountDisplay.lblFreeMargin = lblFreeMargin;
        }
        /// <summary>
        /// 初始化平台对象
        /// </summary>
        public virtual void GeneratePlatformObj()
        {

        }
        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void SaveConfig()
        {
            //DirectoryInfo pathInfo = new DirectoryInfo(Application.StartupPath);
            //string path = pathInfo.Parent.FullName + "\\iAutoTrade\\";
            string newMyConfigFile = rootPath + _MyConfigFile;
            string newConfigFile = rootPath + _ConfigFile;
            try
            {
                if (_Config != null)
                {
                    _Account.BrokerCode = _Broker.BrokerCode;
                    _Account.UserCode = textBox_UserCode.Text;
                    _Account.Password = textBox_Password.Text;
                    _Account.Type = comboBox_AccType.Text;
                    bool existFlag = false;
                    for (int i = 0; i < _MyConfig.Account.Length; i++)
                    {
                        Account acc = _MyConfig.Account[i];
                        if (string.Equals(acc.BrokerCode, _Account.BrokerCode) && string.Equals(acc.Type, _Account.Type))
                        {
                            //acc = _Account;
                            existFlag = true;
                            break;
                        }
                    }
                    if (!existFlag)
                    {
                        uClient.Comm.Account[] NewAccount = new uClient.Comm.Account[_MyConfig.Account.Length + 1];
                        _MyConfig.Account.CopyTo(NewAccount, 0);
                        NewAccount[_MyConfig.Account.Length] = _Account;
                        _MyConfig.Account = NewAccount;
                    }
                    /////////////////////////////////////////////
                    _Platform.DefaultBrokerCode = _Broker.BrokerCode;

                    //交易商配置信息覆盖平台配置信息
                    _Broker.DefaultSymbol = comboBox_Symbol.Text;
                    _Broker.DefaultAccountType = comboBox_AccType.Text;
                    _Broker.TradePara = _TradePara;
                    _Account.TradePara = _TradePara;
                    if (string.Equals(_Account.Type, "Demo"))
                    {
                        _Broker.DemoAccount = _Account;
                    }
                    else
                    {
                        _Broker.LiveAccount = _Account;
                    }
                    _Platform.TradePara = _Broker.TradePara;
                    File.WriteAllText(newConfigFile, JsonConvert.SerializeObject(_Config));
                    File.WriteAllText(newMyConfigFile, JsonConvert.SerializeObject(_MyConfig));
                    InitClient();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                _Log.LogInfo(ex.Message + ex.Source);
            }
        }
        /// <summary>
        /// 保存配置文件后赋值到对象变量
        /// </summary>
        public void InitClient()
        {
            switch (_Platform.PlatformNo)
            {
                case (int)Comm.Enum.Platform.MF4:
                    _MF4._DSQuote = _DSQuote;
                    _MF4._Config = _Config;
                    _MF4._MyConfig = _MyConfig;
                    _MF4._Broker = _Broker;
                    _MF4._TradeSymbol = _TradeSymbol;
                    break;
                case (int)Comm.Enum.Platform.MT4:
                    _MT4._DSQuote = _DSQuote;
                    _MT4._Config = _Config;
                    _MT4._MyConfig = _MyConfig;
                    _MT4._Broker = _Broker;
                    _MT4._TradeSymbol = _TradeSymbol;
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    _MT5._DSQuote = _DSQuote;
                    _MT5._Config = _Config;
                    _MT5._MyConfig = _MyConfig;
                    _MT5._Broker = _Broker;
                    _MT5._TradeSymbol = _TradeSymbol;
                    break;
                case (int)Comm.Enum.Platform.V4:
                    _V4._DSQuote = _DSQuote;
                    _V4._Config = _Config;
                    _V4._MyConfig = _MyConfig;
                    _V4._Broker = _Broker;
                    _V4._TradeSymbol = _TradeSymbol;
                    break;
            }
        }
        /// <summary>
        /// 是否连接
        /// </summary>
        /// <returns></returns>
        public bool isConnect()
        {
            switch (_Platform.PlatformNo)
            {
                case (int)Comm.Enum.Platform.MF4:
                    if (_MF4.isConnect())
                    {
                        return true;
                    }
                    break;
                case (int)Comm.Enum.Platform.MT4:
                    if (_MT4.isConnect())
                    {
                        return true;
                    }
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    if (_MT5.isConnect())
                    {
                        return true;
                    }
                    break;
                case (int)Comm.Enum.Platform.V4:
                    if (_V4.isConnect())
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnLogin_Click(object sender, EventArgs e)
        {
            AsynExec(this.btnLogin, () => {
                if (btnLogin.Text.Equals("平台连接"))
                {
                    switch (_Platform.PlatformNo)
                    {
                        case (int)Comm.Enum.Platform.MF4:
                            _MF4.Connect();
                            _MF4._IsManullyDisconnect = false;
                            break;
                        case (int)Comm.Enum.Platform.MT4:
                            _MT4.Connect();
                            _MT4._IsManullyDisconnect = false;
                            break;
                        case (int)Comm.Enum.Platform.MT5:
                            _MT5.Connect();
                            _MT5._IsManullyDisconnect = false;
                            break;
                        case (int)Comm.Enum.Platform.V4:
                            _V4.Connect();
                            _V4._IsManullyDisconnect = false;
                            break;
                    }
                    if (isConnect())
                    {
                        AfterConnectCompleted();
                    }
                }
                else if (btnLogin.Text.Equals("平台断开"))
                {
                    _Log.LogInfo("断开连接");
                    switch (_Platform.PlatformNo)
                    {
                        case (int)Comm.Enum.Platform.MF4:
                            _MF4.DisConnect();
                            _MF4._IsManullyDisconnect = true;
                            _MF4._BgwBroberQuote.CancelAsync();
                            _MF4._BgwOrderUpdate.CancelAsync();
                            break;
                        case (int)Comm.Enum.Platform.MT4:
                            _MT4.DisConnect();
                            _MT4._IsManullyDisconnect = true;                          
                            break;
                        case (int)Comm.Enum.Platform.MT5:
                            _MT5.DisConnect();
                            _MT5._IsManullyDisconnect = true;
                            break;
                        case (int)Comm.Enum.Platform.V4:
                            _V4.DisConnect();
                            _V4._IsManullyDisconnect = true;
                            _V4._BgwBroberQuote.CancelAsync();
                            _V4._BgwOrderUpdate.CancelAsync();
                            break;
                    }
                    uClient.Comm.Utils.ClearAccountCaptial(accountDisplay);
                    UpdateConnectStatus(false);
                    PlatformConnectCheckFlag = true;
                    StopAutoTrade();
                    CancelCheck();
                }
            });
        }

        /// <summary>
        /// 取消平台连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnLogout_Click(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// 连接数据源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnDSLogin_Click(object sender, EventArgs e)
        {
            DSConnection("DS");
        }
        /// <summary>
        /// 数据源断开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnDSLogout_Click(object sender, EventArgs e)
        {
            DSDisconnect("DS");
        }


        /// <summary>
        /// 手动平仓
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnClose_Click(object sender, EventArgs e)
        {
            StopAutoTrade();
            if (isConnect())
            {
                CancelCheck();
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
                //平仓所有选中的单
                for (int i = 0; i < gvPositions.RowCount; i++)
                {
                    DataGridViewRow row = gvPositions.Rows[i];
                    if ((bool)row.Cells[0].Value == true)
                    {
                        _IsAutoOperationFlag = false;
                        double timezone = _Account.TradePara.Timezone;
                        switch (_Platform.PlatformNo)
                        {
                            case (int)Comm.Enum.Platform.MF4:
                                try
                                {
                                    M4.Common.Classes.Position position = _MF4.GetOrder(row.Cells[1].Value.ToString());
                                    //TimeSpan ts = DateTime.Now - position.CreationTime.AddHours(timezone);
                                    if (!TradeUtils.IsCloseOrderTime(TradeUtils.GetCloseTimeDuration(_TradePara.AutoCloseTimeDuration), DateTime.Now, position.CreationTime.AddHours(timezone)))
                                    {
                                        DialogResult dr = MessageBox.Show("开仓时间未超过[" + _TradePara.AutoCloseTimeDuration + "]，是否需要平仓？", "平仓提示", MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                                        if (dr == DialogResult.Yes)
                                        {
                                            _MF4.CloseOrder(position, _IsAutoOperationFlag, out _SendTime);
                                        }
                                    }
                                    else
                                    {
                                        _MF4.CloseOrder(position, _IsAutoOperationFlag, out _SendTime);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex.Message + "  " + ex.StackTrace);
                                    _Log.LogInfo(ex.Message);
                                }
                                break;
                            case (int)Comm.Enum.Platform.MT4:
                                try
                                {
                                    TradingAPI.MT4Server.Order order = _MT4.GetOrder(row.Cells[1].Value.ToString());
                                    if (!TradeUtils.IsCloseOrderTime(TradeUtils.GetCloseTimeDuration(_TradePara.AutoCloseTimeDuration), DateTime.Now, order.OpenTime.AddHours(timezone)))
                                    {
                                        DialogResult dr = MessageBox.Show("开仓时间未超过[" + _TradePara.AutoCloseTimeDuration + "]，是否需要平仓？", "平仓提示", MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                                        if (dr == DialogResult.Yes)
                                        {
                                            _MT4.CloseOrder(order, _IsAutoOperationFlag, out _SendTime);
                                        }
                                    }
                                    else
                                    {
                                        _MT4.CloseOrder(order, _IsAutoOperationFlag, out _SendTime);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex.Message + "  " + ex.StackTrace);
                                    _Log.LogInfo(ex.Message);
                                }
                                break;

                            case (int)Comm.Enum.Platform.MT5:
                                try
                                {
                                    mtapi.mt5.Order order = _MT5.GetOrder(row.Cells[1].Value.ToString());
                                    if (!TradeUtils.IsCloseOrderTime(TradeUtils.GetCloseTimeDuration(_TradePara.AutoCloseTimeDuration), DateTime.Now, order.OpenTime.AddHours(timezone)))
                                    {
                                        DialogResult dr = MessageBox.Show("开仓时间未超过[" + _TradePara.AutoCloseTimeDuration + "]，是否需要平仓？", "平仓提示", MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                                        if (dr == DialogResult.Yes)
                                        {
                                            _MT5.CloseOrder(order, _IsAutoOperationFlag, out _SendTime);
                                        }
                                    }
                                    else
                                    {
                                        _MT5.CloseOrder(order, _IsAutoOperationFlag, out _SendTime);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex.Message + "  " + ex.StackTrace);
                                    _Log.LogInfo(ex.Message);
                                }
                                break;
                            case (int)Comm.Enum.Platform.V4:
                                try
                                {
                                    uClient.Comm.Position order = _V4.GetOrder(row.Cells[1].Value.ToString());
                                    if (!TradeUtils.IsCloseOrderTime(TradeUtils.GetCloseTimeDuration(_TradePara.AutoCloseTimeDuration), DateTime.Now, order.CreationTime.AddHours(timezone)))
                                    {
                                        DialogResult dr = MessageBox.Show("开仓时间未超过[" + _TradePara.AutoCloseTimeDuration + "]，是否需要平仓？", "平仓提示", MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                                        if (dr == DialogResult.Yes)
                                        {
                                            _V4.CloseOrder(order, _IsAutoOperationFlag, out _SendTime);
                                        }
                                    }
                                    else
                                    {
                                        _V4.CloseOrder(order, _IsAutoOperationFlag, out _SendTime);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex.Message + "  " + ex.StackTrace);
                                    _Log.LogInfo(ex.Message);
                                }
                                break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 启动自动交易
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnStart_Click(object sender, EventArgs e)
        {
            if (isConnect() && _IsEAConnectFlag)
            {

                StartAutoTrade();
                //启动自动交易成功
                DBHelper.updateAutoInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo, _Account.UserCode, "Y");
            }
            else
            {
                _Log.LogInfo("先连接数据源和交易商，再启动自动交易");
            }
        }
        /// <summary>
        /// 启动自动交易
        /// </summary>
        public void StartAutoTrade()
        {
            _Log.LogInfo("自动交易启用");
            btnStart.Enabled = false;
            btnStart.BackColor = Color.Green;
            btnStop.Enabled = true;
            lblTradeStatus.Text = "自动交易已启动";
            lblTradeStatus.BackColor = Color.Green;
            _EnableAutoTrade = true;
        }
        /// <summary>
        /// 停止自动交易
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnStop_Click(object sender, EventArgs e)
        {
            StopAutoTrade();
            //停止自动交易成功
            DBHelper.updateAutoInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo, _Account.UserCode, "N");
        }
        public void StopAutoTrade()
        {
            _EnableAutoTrade = false;
            _Log.LogInfo("自动交易停止");
            lblTradeStatus.Text = "自动交易已停止";
            CancelCheck();
            //AutoLockBind(false);
            btnStart.Enabled = true;
            btnStart.BackColor = Color.Red;
            btnStop.Enabled = false;
            lblTradeStatus.BackColor = Color.Red;
        }



        /// <summary>
        /// 改变合约
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void cmbSymbol_SelectedValueChanged(object sender, EventArgs e)
        {
            _TradeSymbol = comboBox_Symbol.SelectedItem.ToString();
            InitialPara();
            GeneratePlatformObj();
            //重新订阅行情
            switch (_Platform.PlatformNo)
            {
                case (int)Comm.Enum.Platform.MF4:
                    if (_MF4.isConnect())
                    {
                        _MF4.SymbolSubscription(_TradeSymbol);
                    }
                    break;
                case (int)Comm.Enum.Platform.MT4:
                    if (_MT4.isConnect())
                    {
                        _MT4.SymbolSubscription(_TradeSymbol);
                    }
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    if (_MT5.isConnect())
                    {
                        _MT5.SymbolSubscription(_TradeSymbol);
                    }
                    break;
                case (int)Comm.Enum.Platform.V4:
                    if (_V4.isConnect())
                    {
                        _V4.SymbolSubscription(_TradeSymbol);
                    }
                    break;
            }
        }
        /// <summary>
        /// 连接成功后
        /// </summary>
        public void AfterConnectCompleted()
        {
            FindForm().Text = _MainFormPara.FormNo + "-" + _Broker.BrokerName;
            //2.更新连接状态
            UpdateConnectStatus(true);
            PlatformConnectCheckFlag = false;
        }

        /// <summary>
        /// 数据源报价更新
        /// </summary>
        public void UpdateDSQuote()
        {
            lblDSSpeed.Text = _DSQuote.Speed.ToString();
            lblDSBid.Text = _DSQuote.Bid.ToString("f2");
            lblDSBidDiff0.Text = _DSQuote.BidDiff.ElementAt(0).ToString();
            lblDSBidDiff1.Text = _DSQuote.BidDiff.ElementAt(1).ToString();
            lblDSBidDiff2.Text = _DSQuote.BidDiff.ElementAt(2).ToString();
            lblDSBidDiff3.Text = _DSQuote.BidDiff.ElementAt(3).ToString();
            lblDSBidDiff4.Text = _DSQuote.BidDiff.ElementAt(4).ToString();

            lblDSAsk.Text = _DSQuote.Ask.ToString("f2");
            lblDSAskDiff0.Text = _DSQuote.AskDiff.ElementAt(0).ToString();
            lblDSAskDiff1.Text = _DSQuote.AskDiff.ElementAt(1).ToString();
            lblDSAskDiff2.Text = _DSQuote.AskDiff.ElementAt(2).ToString();
            lblDSAskDiff3.Text = _DSQuote.AskDiff.ElementAt(3).ToString();
            lblDSAskDiff4.Text = _DSQuote.AskDiff.ElementAt(4).ToString();
        }

        /// <summary>
        /// 更新帐户连接状态
        /// </summary>
        /// <param name="Status">true= 连接，false=断开</param>
        public void UpdateConnectStatus(bool Status)
        {
            if (lblConnectStatus.InvokeRequired)
            {
                lblConnectStatus.Invoke(new Action<bool>(UpdateConnectStatus), new object[] { Status });
            }
            else
            {
                lblConnectStatus.Text = Status ? "已连接" : "断开";
                if (Status)
                {
                    lblConnectStatus.BackColor = Color.Green;
                    //连接成功后直接启动锁单消息绑定，其他地方不再进行锁单消息解绑和绑定操作
                    AutoLockBind(true);
                    //登陆成功
                    DBHelper.updateLoginInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo,_Account.UserCode,"Y");
                }
                else
                {
                    StopAutoTrade();
                    CancelCheck();
                    AutoLockBind(false);
                    //登出成功
                    DBHelper.updateLoginInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo, _Account.UserCode, "N");
                    lblConnectStatus.BackColor = Color.Red;
                    uClient.Comm.Utils.ClearBrokerDiff(quotaDisplayPanel);
                }
                btnLogin.Text = !Status ? "平台连接" : "平台断开";
                btnLogin.BackColor = !Status ? Color.Red : Color.Green;
                comboBox_Broker.Enabled = !Status;
                comboBox_AccType.Enabled = !Status;
                comboBox_Symbol.Enabled = !Status;
                textBox_UserCode.Enabled = !Status;
                textBox_Password.Enabled = !Status;

            }
        }
        bool _DataGridViewInitCompleteFlag = false;
        /// <summary>
        /// 初始化订单列表
        /// </summary>
        public void IniPositionGrid()
        {
            gvPositions.Columns.Clear();
            gvPositions.AutoGenerateColumns = false;
            gvPositions.AllowUserToAddRows = false;
            gvPositions.AutoSize = true;
            gvPositions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gvPositions.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 10, FontStyle.Regular);
            gvPositions.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gvPositions.DefaultCellStyle.Font = new Font("微软雅黑", 10, FontStyle.Regular);
            gvPositions.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

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
            column6.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            gvPositions.Columns.Add(column6);

            DataGridViewColumn column7 = new DataGridViewTextBoxColumn();
            column7.HeaderText = "盈亏";
            column7.DataPropertyName = "PL";
            column7.Name = "盈亏";
            column7.ReadOnly = true;
            column7.Width = 60;
            column7.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column7.SortMode = DataGridViewColumnSortMode.NotSortable;
            column7.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
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
            _DataGridViewInitCompleteFlag = true;
        }
        /// <summary>
        /// 自动交易是否启动，自动交易和是否自动选中组合使用
        /// </summary>
        public bool _EnableAutoTrade = false;
        /// <summary>
        /// 该订单是否为自动交易订单
        /// </summary>
        public bool _IsAutoOperationFlag = false;
        //订单是否已执行标记
        public bool executedFlag = false;
        //跳次分析执行标记
        public bool isExecuting = false;
        //交易订单是否执行完成，包括发送和接收订单消息
        public bool isOrderCompleted = true;
        /// <summary>
        /// 订单创建类型,H:主动创建的订单，C:锁仓创建的订单
        /// </summary>
        public string _OrderCreateType = "";
        /// <summary>
        /// 该订单是否为只平订单，如果是只平订单，该窗口仍然作为锁单窗口，不能作为开单窗口
        /// true: H端停止自动交易,C端启动自动交易
        /// false:H端启动自动交易,C端停止自动交易
        /// </summary>
        public bool _OnlyCloseOrder = false;
        /// <summary>
        /// 交易跳次计时器，跳次相隔5s
        /// </summary>
        public DateTime priceCount = DateTime.Now;

        /// <summary>
        /// 策略数据分析是否运行
        /// </summary>
        public bool _EAAnalysisRunning = false;
        /// <summary>
        /// 数据源行情获取并计算是否自动交易
        /// </summary>
        public void MarketQuote()
        {
            _EAAnalysisRunning = true;
            string PreReceiveMsg = "";
            while (true)
            {
                string results = _Subscriber.ReceiveFrameString();
                //_Log.LogInfo("接收消息："+ results);
                if (results.Equals(PreReceiveMsg) || string.IsNullOrEmpty(results))
                {
                    continue;
                }
                else
                {
                    PreReceiveMsg = results;
                    if (string.Equals(labelConnectType.Text, "T4") || string.Equals(labelConnectType.Text, "OEC"))
                    {
                        if (results.StartsWith("Gold"))
                        {
                            //EA1(results);
                        }
                    } 
                }
                
            }
        }

        /// <summary>
        /// EA2的公共交易参数，作为全局变量保存
        /// </summary>
        string _EA2_TradeRecord = "";
        /// <summary>
        /// 对手反向交易策略
        /// </summary>
        public async void EA2(string results)
        {
            _Log.LogInfo(results);
            string[] split = results.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //参数说明：XAUUSD:2004.79:BUY:1:50:50 HH:mm:ss orderTypeDetail:openPrice:closePrice:profit
            string opt = split[0];
            //string ut = split[1];
            if (split.Length >= 3)
            {
                _EA2_TradeRecord = split[2];
            }
            if (!string.IsNullOrEmpty(opt))
            {
                int tradeType = -1;
                string optt = opt.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[2];
                switch (optt)
                {
                    case "BUY":
                        tradeType = 1;
                        break;
                    case "SELL":
                        tradeType = 2;
                        break;
                    case "CLOSE_BUY":
                        tradeType = 3;
                        break;
                    case "CLOSE_SELL":
                        tradeType = 4;
                        break;
                    case "IN_BUY":
                        tradeType = 1;
                        break;
                    case "IN_SELL":
                        tradeType = 2;
                        break;
                }
                if (_EnableAutoTrade && !isExecuting)
                {
                    isExecuting = true;
                    //跳次分析完成，执行订单逻辑
                    //上次订单已执行完成（订单返回更新状态后才算订单执行完成）
                    //if (isOrderCompleted && tradeType != -1 && (DateTime.Now- priceCount).TotalSeconds >= 5 && TradeUtils.isTradeTime())
                    if (isOrderCompleted && tradeType != -1)
                    {
                        //执行自动交易订单时，暂时取消自动交易功能，待订单完成后再回复自动交易                       
                        isOrderCompleted = false;
                        _IsAutoOperationFlag = true;
                        //EA2 策略下所有交易端都是C端,如果开单失败需要重试
                        _OrderCreateType = "H";
                        _Log.LogInfo("执行自动订单开始");
                        executedFlag = false;

                        _Log.LogInfo("启动锁单,延时处理中");
                        if (_MyConfig.SpecialNumber != null && _MyConfig.SpecialNumber.Length > 0)
                        {
                            // 检查数组中是否包含特定的数据
                            string searchTerm = _Account.UserCode;
                            bool contains = Array.Exists(_MyConfig.SpecialNumber, element => element == searchTerm);
                            //该账号不包含在特殊账号中,执行延迟
                            if (!contains)
                            {
                                await DelayExecution(); // 调用延迟执行的函数
                            }
                        }
                        else
                        {
                            //没有配置特殊账号,全部账号都执行延迟
                            await DelayExecution(); // 调用延迟执行的函数
                        }

                        switch (_Platform.PlatformNo)
                        {
                            case (int)Comm.Enum.Platform.MF4:
                                _MF4._OrderCreateType = _OrderCreateType;
                                _MF4._EA2_TradeRecord = _EA2_TradeRecord;
                                _MF4._CurrentPrice = _StrategyConfig.CurrentPrice;
                                //在执行自动交易情况,无论是否已有单,都进行开单操作
                                executedFlag = _MF4.AutoTradeTransaction3(tradeType, _IsAutoOperationFlag, out _SendTime);
                                _StrategyConfig.CurrentPrice = _MF4._CurrentPrice;
                                break;
                            case (int)Comm.Enum.Platform.MT4:
                                _MT4._OrderCreateType = _OrderCreateType;
                                _MT4._EA2_TradeRecord = _EA2_TradeRecord;
                                _MT4._CurrentPrice = _StrategyConfig.CurrentPrice;
                                executedFlag = _MT4.AutoTradeTransaction3(tradeType, _IsAutoOperationFlag, out _SendTime);
                                _StrategyConfig.CurrentPrice = _MT4._CurrentPrice;
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                _MT5._OrderCreateType = _OrderCreateType;
                                _MT5._EA2_TradeRecord = _EA2_TradeRecord;
                                _MT5._CurrentPrice = _StrategyConfig.CurrentPrice;
                                executedFlag = _MT5.AutoTradeTransaction3(tradeType, _IsAutoOperationFlag, out _SendTime);
                                _StrategyConfig.CurrentPrice = _MT5._CurrentPrice;
                                break;
                            case (int)Comm.Enum.Platform.V4:
                                _V4._OrderCreateType = _OrderCreateType;
                                _V4._EA2_TradeRecord = _EA2_TradeRecord;
                                executedFlag = _V4.AutoTradeTransaction2(tradeType, _IsAutoOperationFlag, out _SendTime);
                                break;
                        }
                        //发送订单完成后，停止自动交易，待订单更新后修改自动交易标识
                        if (executedFlag)
                        {
                            _Log.LogInfo("执行自动订单完成");
                        }
                        else
                        {
                            isOrderCompleted = true;
                            _Log.LogInfo("无自动订单生成");
                        }
                    }
                    //CancelCheckForSelected();
                    isExecuting = false;
                }
                else
                {
                    _Log.LogInfo("未启动自动交易");
                }
            }
        }

        /// <summary>
        /// 延迟等待时间,1-5s之间
        /// </summary>
        /// <returns></returns>
        private async Task DelayExecution()
        {
            //生成随机数时每次循环生成的数是一样的,循环中计算时间太快
            //利用Guid.NewGuid().GetHashCode()返回的哈希代码得到种子,得到不重复的值
            //Random random = new Random((int)DateTime.Now.Ticks);
            Random random = new Random(Guid.NewGuid().GetHashCode());
            //Random random = new Random();
            int randomNumber = random.Next(3,10);
            _Log.LogInfo("等待["+ randomNumber + "]秒");
            await Task.Delay(TimeSpan.FromSeconds(randomNumber)); // 设置延迟
            _Log.LogInfo("延迟执行完成");
        }

        /// <summary>
        /// 实时监测数据源变化，根据给定规则启动自动交易
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void bgwMarketQuote_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!_EAAnalysisRunning) 
            {
                MarketQuote();
            }           
        }

        /// <summary>
        /// 更新跳次小黑板数据
        /// </summary>
        public void OutputDiffList()
        {
            if (_DSQuote.BidDiff[0] >= (double)_TradePara.BuyOpen || _DSQuote.BidDiff[0] <= (double)(-_TradePara.SellOpen) || _DSQuote.BidDiff[0] <= (double)(-_TradePara.BuyClose) || _DSQuote.BidDiff[0] >= (double)_TradePara.SellClose)
            {
                double Diff = _DSQuote.BidDiff[0];
                if (System.Math.Abs(Diff) >= 3)
                {
                    string BidDiffMsg = DateTime.Now.ToString("HH:mm:ss") + "  +" + _DSQuote.BidDiff[0];
                    if (Diff < 0)
                    {
                        BidDiffMsg = DateTime.Now.ToString("HH:mm:ss") + "  " + _DSQuote.BidDiff[0];
                    }
                    WriteBiddViewList(BidDiffMsg);
                }
            }
        }
        /// <summary>
        /// 取消自动交易选中框
        /// </summary>
        public void CancelCheck()
        {

        }
        /// <summary>
        /// 保存平台配置信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void button1_Click(object sender, EventArgs e)
        {
            SaveConfig();
            if (!string.Equals(_MainFormPara.PlatformCode, _Config.PlatformCode))
            {
                ChangePlatform(_Config.PlatformCode);
            }
            _Log.LogInfo("信息保存成功");
        }
        public void comboBox_Broker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_IsLoadConfigCompleted)
            {
                string BrokerName = comboBox_Broker.SelectedItem.ToString();
                uClient.Comm.Broker bb = _Platform.GetBrokerByName(BrokerName);
                _Platform.DefaultBrokerCode = bb.BrokerCode;
                if (!string.Equals(_Broker.BrokerCode,bb.BrokerCode))
                {
                    _Broker = bb;
                    _Account = _Broker.DefaultAccountType == "Demo" ? _Broker.DemoAccount : _Broker.LiveAccount;
                    _TradeSymbol = _Broker.DefaultSymbol;
                }               
                InitialPara();
                if (_Broker != null && _Account != null)
                {
                    if (_Account.TradePara != null)
                    {
                        _Broker.TradePara = _Account.TradePara;
                    }
                    else
                    {
                        _Account.TradePara = _Broker.TradePara;
                    }
                    if (_Broker.TradePara == null)
                    {
                        _Broker.TradePara = _Platform.TradePara;
                        _Account.TradePara = _Broker.TradePara;
                    }
                    _TradePara = _Account.TradePara;
                    comboBox_AccType.SelectedItem = _Account.Type;
                    comboBox_Symbol.Items.Clear();
                    if (_Account != null && _Broker.Symbol != null && _Broker.Symbol.Length > 0)
                    {
                        comboBox_AccType.SelectedItem = _Account.Type;
                        foreach (string ss in _Broker.Symbol)
                        {
                            comboBox_Symbol.Items.Add(ss);
                        }
                        comboBox_Symbol.SelectedItem = _TradeSymbol;
                    }
                    textBox_UserCode.Text = _Account.UserCode;
                    textBox_Password.Text = _Account.Password;

                }

                //停止按钮颜色改变事件
                if (_Button_ChangeColor_Timer_TradePara != null)
                {
                    _Button_ChangeColor_Timer_TradePara.Stop();
                    _Button_ChangeColor_Timer_TradePara.Enabled = false;
                    button_TradePara_Save.BackColor = Color.White;
                }
            }
        }
        /// <summary>
        /// 保存配置信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void button_Save_Click(object sender, EventArgs e)
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
            _Log.LogInfo("信息保存成功");
        }
        public void comboBox_AccType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_IsLoadConfigCompleted)
            {
                string type = comboBox_AccType.SelectedItem.ToString();
                _Broker.DefaultAccountType = type;
                _Account = _Broker.DefaultAccountType == "Demo" ? _Broker.DemoAccount : _Broker.LiveAccount;
                if (_Account == null)
                {
                    _Account = new Account();
                    _Account.UserCode = "";
                    _Account.Password = "";
                    _Account.Type = _Broker.DefaultAccountType;
                    _Account.BrokerCode = _Broker.BrokerCode;
                    if (type == "Demo")
                    {
                        _Broker.DemoAccount = _Account;
                    }
                    else
                    {
                        _Broker.LiveAccount = _Account;
                    }
                }
                InitialPara();
                GeneratePlatformObj();
                textBox_UserCode.Text = _Account.UserCode;
                textBox_Password.Text = _Account.Password;
            }
        }
        public void button_TradePara_Save_Click(object sender, EventArgs e)
        {
            if (_Button_ChangeColor_Timer_TradePara != null)
            {
                _Button_ChangeColor_Timer_TradePara.Stop();
                _Button_ChangeColor_Timer_TradePara.Enabled = false;
                button_TradePara_Save.BackColor = (Color.White);
            }
            SaveConfig();
            _Log.LogInfo("信息保存成功");
        }
        public void button_TradePara_Cancel_Click(object sender, EventArgs e)
        {
            if (_Button_ChangeColor_Timer_TradePara != null)
            {
                _Button_ChangeColor_Timer_TradePara.Stop();
                _Button_ChangeColor_Timer_TradePara.Enabled = false;
                button_TradePara_Save.BackColor = (Color.White);
            }
            _Log.LogInfo("取消交易参数修改");
        }

        public System.Timers.Timer _Button_ChangeColor_Timer_TradePara = null;
        public void timer_Tick_Color_TradePara()
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
                button_TradePara_Save.BackColor = (Color.White);
            }
        }
        public void timer_Tick_TradePara(object sender, EventArgs e)
        {
            _Button_ChangeColor_Timer_TradePara.Stop();
            if (button_TradePara_Save.BackColor == Color.White)
            {
                button_TradePara_Save.BackColor = Color.Red;
            }
            else
            {
                button_TradePara_Save.BackColor = (Color.White);
            }
            _Button_ChangeColor_Timer_TradePara.Start();
        }

        public void nudSellOpen_ValueChanged(object sender, EventArgs e)
        {
            timer_Tick_Color_TradePara();
        }

        public void nudSellLots_ValueChanged(object sender, EventArgs e)
        {
            timer_Tick_Color_TradePara();
        }
        public void nudBuyLots_ValueChanged(object sender, EventArgs e)
        {
            timer_Tick_Color_TradePara();
        }
        public void nudBuyOpen_ValueChanged(object sender, EventArgs e)
        {
            timer_Tick_Color_TradePara();
        }

        public void nudSellClose_ValueChanged(object sender, EventArgs e)
        {
            timer_Tick_Color_TradePara();
        }

        public void nudBuyClose_ValueChanged(object sender, EventArgs e)
        {
            timer_Tick_Color_TradePara();
        }
        public void textBox_UserCode_TextChanged(object sender, EventArgs e)
        {
            if (_Account != null)
            {
                _Account.UserCode = textBox_UserCode.Text;
            }
        }

        public void textBox_Password_TextChanged(object sender, EventArgs e)
        {
            if (_Account != null)
            {
                _Account.Password = textBox_Password.Text;
            }
        }
        public void button_Open_Trade_Click(object sender, EventArgs e)
        {
            string path = uClient.Comm.Utils.GenerateDirectoryPath("TradeInfo");
            System.DateTime currentTime = System.DateTime.Now;
            string fileName = currentTime.ToString("yyyy-MM-dd") + "_" + _Account.UserCode;
            string destFile = System.IO.Path.Combine(path, fileName + ".txt");
            if (File.Exists(destFile))
            {
                System.Diagnostics.Process.Start(destFile);
            }
        }
        /// <summary>
        /// 清空交易信息窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void button_Clear_Click(object sender, EventArgs e)
        {
            lstTradeRecord.Items.Clear();
        }
        /// <summary>
        /// 跳次小黑板写入信息
        /// </summary>
        /// <param name="msg"></param>
        public void WriteBiddViewList(string msg)
        {
            //让数据向下移动，上方会一直显示最新数据
            //listBox1.Items.Add(0, msg);
            //让数据向上移动，下方会一直显示最新数据
            listBox1.Items.Add(msg);
            listBox1.TopIndex = listBox1.Items.Count - 1;
            if (listBox1.Items.Count > 50)
            {
                listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
            }
            _Log.LogInfo("有效跳次:" + msg);

        }
        /// <summary>
        /// 跳次小黑板颜色修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                string Diff = listBox1.Items[e.Index].ToString();
                //_logger.Debug("跳次=" + Diff);
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
        /// <summary>
        /// 清空跳次小黑板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void listBox1_DoubleClick(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }
        public void gvPositions_CellValueChanged(object sender, DataGridViewCellEventArgs e)
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
        public void gvPositions_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
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
        /// <summary>
        /// 保存配置信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void button2_Click_1(object sender, EventArgs e)
        {
            SaveConfig();
            _Log.LogInfo("信息保存成功");
        }
        /// <summary>
        /// 自动绑定锁单消息
        /// </summary>
        public void AutoLockBind(bool isActive)
        {
            if (isActive)
            {
                string port = uClient.Comm.Utils.GetPort(_MainFormPara.FormNo);
                string address = string.Format("tcp://{0}:{1}", uClient.Comm.Utils.GetLocalIP(), port);
                try
                {
                    _MasterPublisher.Unbind(address);
                    _Log.LogInfo("取消锁单消息绑定，地址:" + address);
                    _MasterPublisher.Bind(address);
                    _Log.LogInfo("锁单消息发送绑定成功，窗口：" + _MainFormPara.FormNo + ",地址:" + address);
                }
                catch (Exception e2)
                {
                    _Log.LogInfo("锁单消息发送取消[" + address + "]异常:" + e2.Message);
                    _MasterPublisher.Bind(address);
                    _Log.LogInfo("锁单消息发送绑定成功，窗口：" + _MainFormPara.FormNo + ",地址:" + address);
                }
            }
            else
            {
                string port = uClient.Comm.Utils.GetPort(_MainFormPara.FormNo);
                string address = string.Format("tcp://{0}:{1}", uClient.Comm.Utils.GetLocalIP(), port);
                try
                {
                    _MasterPublisher.Unbind(address);
                    _Log.LogInfo("取消锁单消息绑定，地址:" + address);
                }
                catch (Exception e)
                {
                    _Log.LogInfo("取消绑定[" + address + "]异常:" + e.Message);
                }
            }
        }
        /// <summary>
        /// 关闭当前窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void frmTrade_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveConfig();
            //取消自动锁单绑定
            //AutoLockBind(false);
        }
        /// <summary>
        /// 定时刷新账号、订单信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timerAccountPosionRefresh_Tick(object sender, EventArgs e)
        {
            if (_Config != null)
            {
                switch (_Platform.PlatformNo)
                {
                    case (int)Comm.Enum.Platform.MF4:
                        if (_MF4.isConnect())
                        {
                            if (_DataGridViewInitCompleteFlag)
                            {
                                _MF4.UpdateAccountCaptial();
                                _MF4.UpdatePositionGrid();
                            }
                        }
                        if (!_MF4.isConnect())
                        {
                            if (string.Equals(lblConnectStatus.Text, "已连接"))
                            {
                                UpdateConnectStatus(false);
                            }
                        }
                        break;
                    case (int)Comm.Enum.Platform.MT4:
                        if (_MT4.isConnect())
                        {
                            if (_DataGridViewInitCompleteFlag)
                            {
                                _MT4.UpdateAccountCaptial();
                                _MT4.UpdatePositionGrid();
                            }
                        }
                        if (!_MT4.isConnect())
                        {
                            if (string.Equals(lblConnectStatus.Text, "已连接"))
                            {
                                UpdateConnectStatus(false);
                            }
                        }
                        break;
                    case (int)Comm.Enum.Platform.MT5:
                        if (_MT5.isConnect())
                        {
                            if (_DataGridViewInitCompleteFlag)
                            {
                                _MT5.UpdateAccountCaptial();
                                _MT5.UpdatePositionGrid();
                            }
                        }
                        if (!_MT5.isConnect())
                        {
                            if (string.Equals(lblConnectStatus.Text, "已连接"))
                            {
                                UpdateConnectStatus(false);
                            }
                        }
                        break;
                    case (int)Comm.Enum.Platform.V4:
                        if (_V4.isConnect())
                        {
                            if (_DataGridViewInitCompleteFlag)
                            {
                                //Task.Run(() => {
                                //    _V4.TradingData1();
                                //});
                                Task.Run(() => {
                                    _V4.UpdateAccountCaptial();
                                    _V4.UpdatePositionGrid();
                                });
                            }
                        }
                        if (!_V4.isConnect())
                        {
                            if (string.Equals(lblConnectStatus.Text, "已连接"))
                            {
                                UpdateConnectStatus(false);
                            }
                        }
                        break;
                }
            }
        }
        /// <summary>
        /// 订单状态更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void bgwOrderUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            _Log.LogInfo("更新订单状态:IsAuto:" + _IsAutoOperationFlag + ",OrderType:" + _OrderCreateType + ",OnlyClose:" + _OnlyCloseOrder);
            switch (_Platform.PlatformNo)
            {
                case (int)Comm.Enum.Platform.MF4:
                    if (_MF4._SystemMessage != null)
                    {
                        _MF4.OrderUpdate(_IsAutoOperationFlag, _SendTime,"");
                        Dictionary<string, object> dic = uClient.Comm.Utils.GetSystemMessageReceived(_MF4._SystemMessage);
                        string orderType = _MF4._SystemMessage.Title;
                        string tradeSide = dic["TradeSide"].ToString();
                        if (_IsAutoOperationFlag)
                        {
                            //开仓被接纳
                            if (Comm.Enum.MarketOrderStatus.Accepted.Contains(orderType))
                            {
                                string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}", Comm.Enum.Buy.Contains(tradeSide) ? "SELL" : "BUY", dic["Lots"], dic["Price"],
                                         Comm.Enum.Buy.Contains(tradeSide) ? _TradePara.BuyOpen : _TradePara.SellOpen, _TradePara.OnlyCloseFlag ? "OCO" : "CO");
                                if (string.Equals(_MF4._OrderCreateType, "H"))
                                {
                                    _MasterPublisher.SendFrame(sendMessage);
                                    _Log.LogInfo("发送自动锁单消息:" + sendMessage);
                                }
                                StartAutoTrade();
                            }
                            //平仓被接纳
                            else if (Comm.Enum.LiquidationOrderStatus.Accepted.Contains(orderType))
                            {
                                M4.Common.Classes.Position positionmf = _MF4.GetClosedPosition(dic["OpenPositionReference"].ToString());
                                string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}", positionmf.BuySell == TradeSide.Buy ? "SELL_CLOSE" : "BUY_CLOSE", positionmf.Lot, positionmf.OpenPrice,
                                          positionmf.BuySell == TradeSide.Buy ? _TradePara.BuyOpen : _TradePara.SellOpen, _TradePara.OnlyCloseFlag ? "OCO" : "CO");                         
                                if (positionmf != null)
                                {
                                    if (string.Equals(_OrderCreateType, "H"))
                                    {
                                        _MasterPublisher.SendFrame(sendMessage);
                                        _Log.LogInfo("发送自动锁单消息:" + sendMessage);
                                        //主窗口正常情况都是需要启动开单，需要判断是否是只平不开
                                        if (_TradePara.OnlyCloseFlag)
                                        {
                                            StopAutoTrade();
                                        }
                                        else
                                        {
                                            StartAutoTrade();
                                        }
                                    }
                                    if (string.Equals(_OrderCreateType, "C"))
                                    {
                                        StartAutoTrade();
                                    }
                                    _MF4.ClosedPosition.Remove(positionmf);
                                }
                            }
                            else if (Comm.Enum.MarketOrderStatus.Rejected.Contains(orderType))
                            {
                                //市价单被拒绝，即开仓被拒绝
                                if (string.Equals(_OrderCreateType, "H"))
                                {
                                    if (_TradePara.OnlyCloseFlag)
                                    {
                                        StopAutoTrade();
                                    }
                                    else
                                    {
                                        StartAutoTrade();
                                    }
                                }
                                if (string.Equals(_OrderCreateType, "C"))
                                {
                                    StartAutoTrade();
                                }
                            }
                            else if (Comm.Enum.LiquidationOrderStatus.Rejected.Contains(orderType))
                            {
                                //平仓被拒绝
                                if (string.Equals(_OrderCreateType, "H"))
                                {
                                    if (_TradePara.OnlyCloseFlag)
                                    {
                                        StopAutoTrade();
                                    }
                                    else
                                    {
                                        StartAutoTrade();
                                    }
                                }
                                if (string.Equals(_OrderCreateType, "C"))
                                {
                                    StartAutoTrade();
                                }
                            }
                            else
                            {
                                _Log.LogInfo("[异常参数][_MF4._SystemMessage.Title=" + orderType + "]");
                                StopAutoTrade();
                            }
                        }
                    }
                    _MF4._SystemMessage = null;
                    break;
                case (int)Comm.Enum.Platform.MT4:
                    if (_MT4._OrderProgressEventArgs.Exception == null)
                    {
                        _MT4.OrderUpdate(_IsAutoOperationFlag, _SendTime,"");
                        Order order = _MT4._OrderProgressEventArgs.Order;
                        switch (_MT4._OrderProgressEventArgs.Type)
                        {
                            case TradingAPI.MT4Server.ProgressType.Opened:
                                //发送锁单消息
                                //参数说明M|BUY/SELL/SELL_CLOSE/BUY_CLOSE|Lot|Price|Point
                                if (_IsAutoOperationFlag)
                                {
                                    string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}", order.Type == Op.Buy ? "SELL" : "BUY", order.Lots, order.OpenPrice,
                                        order.Type == Op.Buy ? _TradePara.BuyOpen : _TradePara.SellOpen, _TradePara.OnlyCloseFlag ? "OCO" : "CO");
                                    if (string.Equals(_OrderCreateType, "H"))
                                    {
                                        _MasterPublisher.SendFrame(sendMessage);
                                        _Log.LogInfo("发送自动锁单消息:" + sendMessage);
                                    }
                                    StartAutoTrade();
                                }
                                break;
                            case TradingAPI.MT4Server.ProgressType.Closed:
                                //发送锁单消息
                                if (_IsAutoOperationFlag)
                                {
                                    string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}", order.Type == Op.Buy ? "SELL_CLOSE" : "BUY_CLOSE", order.Lots, order.OpenPrice,
                                        order.Type == Op.Buy ? _TradePara.BuyClose : _TradePara.SellClose, _TradePara.OnlyCloseFlag ? "OCO" : "CO");
                                    if (string.Equals(_OrderCreateType, "H"))
                                    {
                                        _MasterPublisher.SendFrame(sendMessage);
                                        _Log.LogInfo("发送自动锁单消息:" + sendMessage);
                                        if (_TradePara.OnlyCloseFlag)
                                        {
                                            StopAutoTrade();
                                        }
                                        else
                                        {
                                            StartAutoTrade();
                                        }
                                    }
                                    if (string.Equals(_OrderCreateType, "C"))
                                    {
                                        StartAutoTrade();
                                    }
                                }
                                break;
                            default:
                                _Log.LogInfo("[异常参数][_OrderProgressEventArgs.Type=" + _MT4._OrderProgressEventArgs.Type + "]");
                                StopAutoTrade();
                                break;
                        }
                    }
                    break;

                case (int)Comm.Enum.Platform.MT5:
                    if (_MT5._OrderUpdateEventArgs != null)
                    {
                        _MT5.OrderUpdate(_IsAutoOperationFlag, _SendTime, "");
                        mtapi.mt5.Order order = _MT5._OrderUpdateEventArgs.Order;
                        switch (_MT5._OrderUpdateEventArgs.Type)
                        {
                            case UpdateType.MarketOpen:
                                //发送锁单消息
                                //参数说明M|BUY/SELL/SELL_CLOSE/BUY_CLOSE|Lot|Price|Point
                                if (_IsAutoOperationFlag)
                                {
                                    string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}|{5}", order.OrderType == mtapi.mt5.OrderType.Buy ? "SELL" : "BUY", order.Lots, order.OpenPrice,
                                        order.OrderType == mtapi.mt5.OrderType.Buy ? _TradePara.BuyOpen : _TradePara.SellOpen, _TradePara.OnlyCloseFlag ? "OCO" : "CO", order.Ticket);
                                    if (string.Equals(_OrderCreateType, "H"))
                                    {
                                        _MasterPublisher.SendFrame(sendMessage);
                                        _Log.LogInfo("发送自动锁单消息:" + sendMessage);
                                    }
                                    StartAutoTrade();
                                }
                                break;
                            case UpdateType.MarketClose:
                                //发送锁单消息
                                if (_IsAutoOperationFlag)
                                {
                                    string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}|{5}", order.OrderType == mtapi.mt5.OrderType.Buy ? "SELL_CLOSE" : "BUY_CLOSE", order.Lots, order.OpenPrice,
                                        order.OrderType == mtapi.mt5.OrderType.Buy ? _TradePara.BuyClose : _TradePara.SellClose, _TradePara.OnlyCloseFlag ? "OCO" : "CO", order.Ticket);
                                    if (string.Equals(_OrderCreateType, "H"))
                                    {
                                        _MasterPublisher.SendFrame(sendMessage);
                                        _Log.LogInfo("发送自动锁单消息:" + sendMessage);
                                        if (_TradePara.OnlyCloseFlag)
                                        {
                                            StopAutoTrade();
                                        }
                                        else
                                        {
                                            StartAutoTrade();
                                        }
                                    }
                                    if (string.Equals(_OrderCreateType, "C"))
                                    {
                                        //C端不再判断[OnlyCloseFlag]配置
                                        if (_OnlyCloseOrder)
                                        {
                                            StartAutoTrade();
                                        }
                                        else
                                        {
                                            StopAutoTrade();
                                        }
                                    }
                                }
                                break;
                            default:
                                _Log.LogInfo("[异常参数][_OrderUpdateEventArgs.Type=" + _MT5._OrderUpdateEventArgs.Type + "]");
                                StopAutoTrade();
                                break;
                        }
                    }
                    break;

                case (int)Comm.Enum.Platform.V4:
                    if (_V4._OrderProgressEvent != null)
                    {
                        _V4.OrderUpdate(_IsAutoOperationFlag, _SendTime,"");
                        Position positionv4 = _V4._OrderProgressEvent.postion;
                        switch (_V4._OrderProgressEvent.Type)
                        {
                            case Comm.ProgressType.Opened:
                                //发送锁单消息
                                //参数说明M|BUY/SELL/SELL_CLOSE_/BUY_CLOSE|Lot|Price|Point
                                if (_IsAutoOperationFlag)
                                {
                                    string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}", Comm.Enum.Buy.Contains(positionv4.BuySell) ? "SELL" : "BUY", positionv4.Lot, positionv4.OpenPrice,
                                         Comm.Enum.Buy.Contains(positionv4.BuySell) ? _TradePara.BuyOpen : _TradePara.SellOpen, _TradePara.OnlyCloseFlag ? "OCO" : "CO");
                                    if (string.Equals(_OrderCreateType, "H"))
                                    {
                                        _MasterPublisher.SendFrame(sendMessage);
                                        _Log.LogInfo("发送自动锁单消息:" + sendMessage);
                                    }
                                    StartAutoTrade();
                                }
                                break;
                            case Comm.ProgressType.Closed:
                                //发送锁单消息
                                if (_IsAutoOperationFlag)
                                {
                                    string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}", Comm.Enum.Buy.Contains(positionv4.BuySell) ? "SELL_CLOSE" : "BUY_CLOSE", positionv4.Lot, positionv4.OpenPrice,
                                        Comm.Enum.Buy.Contains(positionv4.BuySell) ? _TradePara.BuyClose : _TradePara.SellClose, _TradePara.OnlyCloseFlag ? "OCO" : "CO");
                                    if (string.Equals(_OrderCreateType, "H"))
                                    {
                                        Thread.Sleep(1000);
                                        _MasterPublisher.SendFrame(sendMessage);
                                        _Log.LogInfo("发送自动锁单消息:" + sendMessage);
                                        if (_TradePara.OnlyCloseFlag)
                                        {
                                            StopAutoTrade();
                                        }
                                        else
                                        {
                                            StartAutoTrade();
                                        }
                                    }
                                    if (string.Equals(_V4._OrderCreateType, "C"))
                                    {
                                        StartAutoTrade();
                                    }
                                }
                                break;
                            default:
                                _Log.LogInfo("[异常参数][_OrderProgressEventArgs.Type=" + _MT4._OrderProgressEventArgs.Type + "]");
                                StopAutoTrade();
                                break;
                        }
                    }
                    //_V4.UpdateAccountCaptial();
                    //_V4.UpdatePositionGrid();
                    break;
            }
            //只要执行了订单信息程序，就认为该订单已完成
            isOrderCompleted = true;
            _Log.LogInfo("执行订单结束");
        }
        /// <summary>
        /// 平台商报价刷新
        /// </summary>
        public void BroberQuote()
        {
            switch (_Platform.PlatformNo)
            {
                case (int)Comm.Enum.Platform.MF4:
                    if (_MF4._QuoteUpdatedEventArgs.Quote.Symbol == _TradeSymbol)
                    {
                        _MF4.NewQuote();
                        _MF4.UpdateQuote();
                    }
                    break;
                case (int)Comm.Enum.Platform.MT4:
                    if (_MT4._QuoteEventArgs.Symbol == _TradeSymbol)
                    {
                        _MT4.NewQuote();
                        _MT4.UpdateQuote();
                    }
                    string symbols4 = _MT4._QuoteEventArgs.Symbol;
                    double prices4 = _MT4._QuoteEventArgs.Bid;
                    if (string.Equals(symbols4, _MT4._GOLD))
                    {
                        _MT4._GOLD_BID_PRICE = prices4;
                    }
                    if (string.Equals(symbols4, _MT4._SILVER))
                    {
                        _MT4._SILVER_BID_PRICE = prices4;
                    }
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    if (_MT5._QuoteEventArgs.Symbol == _TradeSymbol)
                    {
                        _MT5.NewQuote();
                        _MT5.UpdateQuote();
                    }
                    string symbols5 = _MT5._QuoteEventArgs.Symbol;
                    double prices5 = _MT5._QuoteEventArgs.Bid;
                    if (string.Equals(symbols5, _MT5._GOLD))
                    {
                        _MT5._GOLD_BID_PRICE = prices5;
                    }
                    if (string.Equals(symbols5, _MT5._SILVER))
                    {
                        _MT5._SILVER_BID_PRICE = prices5;
                    }
                    break;
                case (int)Comm.Enum.Platform.V4:
                    if (_V4._Quote != null && _V4._Quote.Symbol == _TradeSymbol)
                    {
                        _V4.NewQuote();
                        _V4.UpdateQuote();
                    }
                    break;
            }
        }
        /// <summary>
        /// 平台商行情变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="doWorkEventArgs"></param>
        public void bgwBroberQuote_DoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            BroberQuote();
        }
        /// <summary>
        /// 自动锁仓
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void bgwAutoLock_DoWork(object sender, DoWorkEventArgs e)
        {
            string PreReceiveMsg = "";
            DateTime ts = DateTime.Now;
            //接收锁单信息
            while (true)
            {
                //DateTime tsc = DateTime.Now;
                //if ((tsc - ts).TotalMinutes > 30)
                //{
                //    ts = DateTime.Now;
                //    _Log.LogInfo("自动锁单功能运行正常...");
                //}
                string automsg = _LockSubscriber.ReceiveFrameString();
                if (!string.IsNullOrEmpty(automsg))
                {
                    if (automsg.Equals(PreReceiveMsg))
                    {
                        //Thread.Sleep(10);
                        continue;
                    }
                    else
                    {
                        PreReceiveMsg = automsg;
                        isOrderCompleted = false;
                        _OrderCreateType = "C";
                        _IsAutoOperationFlag = true;
                        _OnlyCloseOrder = false;
                        CancelCheck();
                    }
                    _Log.LogInfo("接收自动锁单消息:" + PreReceiveMsg);
                    string[] splitmsg = PreReceiveMsg.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    //参数说明M|BUY/SELL/SELL_CLOSE/BUY_CLOSE|Lot|Price|Point|OnlyCloseFlag
                    //只有在锁单价格范围内才能进行锁单操作
                    //20210503:锁单逻辑：超过锁单时间后，按照设置的点数和手数锁单
                    string AutoLockTradeSide = splitmsg[1];
                    //该订单是否为只平不开订单
                    //true: H端停止自动交易,C端启动自动交易
                    //false:H端启动自动交易,C端停止自动交易
                    //_OnlyCloseOrder = string.Equals(splitmsg[5], "OCO");
                    //EA 策略中C端自动开平仓
                    _OnlyCloseOrder = true;
                    //decimal AutoLockLot = Convert.ToDecimal(splitmsg[2]);
                    //double AutoLockPrice = Convert.ToDouble(splitmsg[3]);
                    //double AutoLockPoint = Convert.ToDouble(splitmsg[4]);                   
                    //_Log.LogInfo("OrderCreateType:" + _OrderCreateType);
                    executedFlag = AutoLockProcess(AutoLockTradeSide);
                    //发送订单完成后，停止自动交易，待订单更新后修改自动交易标识
                    if (!executedFlag)
                    {
                        isOrderCompleted = true;
                        _Log.LogInfo("执行自动订单结束");
                    }
                }
                Thread.Sleep(10);
            }
        }
        /// <summary>
        /// 自动锁单，开仓前不会检查是否有订单
        /// </summary>
        /// <param name="AutoLockTradeSide"></param>
        /// <returns></returns>
        public bool AutoLockProcess(string AutoLockTradeSide)
        {
            bool mexecutedFlag = false;
            _Log.LogInfo("启动锁单,设置时长:[" + _TradePara.AutoLockTimeDuration + "ms]点数:[" + _TradePara.AutoLockPoint + "]");
            DateTime currentTime = DateTime.Now;
            int timelogcount = 0;
            while (true)
            {
                //_Log.LogInfo((double)_Config.TradePara.AutoLockPoint+";"+ price+";"+ Convert.ToDouble(splitmsg[3])+";"+ Convert.ToDouble(splitmsg[4])+";"+ splitmsg[1]);
                //bool IsAutoLock = TradeUtils.GetAutoLockPrice((double)_Config.TradePara.AutoLockPoint, CurrentPrice, AutoLockPrice, AutoLockPoint, AutoLockTradeSide);
                double delayTime = (DateTime.Now - currentTime).TotalMilliseconds;
                if (delayTime >= (double)_TradePara.AutoLockTimeDuration || _TradePara.AutoLockTimeDuration==0)
                {
                    if ("BUY".Equals(AutoLockTradeSide) && (_DSQuote.BidDiff[0] >= (double)_TradePara.AutoLockPoint))
                    {
                        _SendTime = DateTime.Now;
                        switch (_Platform.PlatformNo)
                        {
                            case (int)Comm.Enum.Platform.MF4:
                                _MF4._OrderCreateType = _OrderCreateType;
                                mexecutedFlag = _MF4.AutoTradeTransaction2(1, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT4:
                                _MT4._OrderCreateType = _OrderCreateType;
                                mexecutedFlag = _MT4.AutoTradeTransaction2(1, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                _MT5._OrderCreateType = _OrderCreateType;
                                mexecutedFlag = _MT5.AutoTradeTransaction2(1, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.V4:
                                _V4._OrderCreateType = _OrderCreateType;
                                mexecutedFlag = _V4.AutoTradeTransaction2(1, true, out _SendTime);
                                break;
                        }
                        _Log.LogInfo("自动锁单[BUY]手数:[" + _TradePara.BuyLots + "]跳次:[" + _DSQuote.BidDiff[0] + "]延时:[" + Convert.ToInt32(delayTime) + "ms]");
                        break;
                    }
                    else if ("SELL".Equals(AutoLockTradeSide) && (_DSQuote.BidDiff[0] <= (double)-_TradePara.AutoLockPoint))
                    {
                        _SendTime = DateTime.Now;
                        switch (_Platform.PlatformNo)
                        {
                            case (int)Comm.Enum.Platform.MF4:
                                _MF4._OrderCreateType = _OrderCreateType;
                                mexecutedFlag = _MF4.AutoTradeTransaction2(2, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT4:
                                _MT4._OrderCreateType = _OrderCreateType;
                                mexecutedFlag = _MT4.AutoTradeTransaction2(2, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                _MT5._OrderCreateType = _OrderCreateType;
                                mexecutedFlag = _MT5.AutoTradeTransaction2(2, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.V4:
                                _V4._OrderCreateType = _OrderCreateType;
                                mexecutedFlag = _V4.AutoTradeTransaction2(2, true, out _SendTime);
                                break;
                        }
                        if (mexecutedFlag)
                        {
                            _Log.LogInfo("自动锁单[SELL]手数:[" + _TradePara.SellLots + "]跳次:[" + _DSQuote.BidDiff[0] + "]延时:[" + Convert.ToInt32(delayTime) + "ms]");
                        }
                        break;
                    }
                    else if ("BUY_CLOSE".Equals(AutoLockTradeSide) && _DSQuote.BidDiff[0] <= (double)-_TradePara.AutoLockPoint)
                    {
                        _SendTime = DateTime.Now;
                        switch (_Platform.PlatformNo)
                        {
                            case (int)Comm.Enum.Platform.MF4:
                                mexecutedFlag = _MF4.AutoTradeTransaction2(3, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT4:
                                mexecutedFlag = _MT4.AutoTradeTransaction2(3, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                mexecutedFlag = _MT5.AutoTradeTransaction2(3, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.V4:
                                mexecutedFlag = _V4.AutoTradeTransaction2(3, true, out _SendTime);
                                break;
                        }
                        if (mexecutedFlag)
                        {
                            _Log.LogInfo("自动锁单[Close][" + ("BUY_CLOSE".Equals(AutoLockTradeSide) ? -_DSQuote.BidDiff[0] : _DSQuote.BidDiff[0]) + "]延时:[" + Convert.ToInt32(delayTime) + "ms]");
                        }
                        break;
                    }
                    else if ("SELL_CLOSE".Equals(AutoLockTradeSide) && _DSQuote.BidDiff[0] >= (double)_TradePara.AutoLockPoint)
                    {
                        _SendTime = DateTime.Now;
                        switch (_Platform.PlatformNo)
                        {
                            case (int)Comm.Enum.Platform.MF4:
                                mexecutedFlag = _MF4.AutoTradeTransaction2(4, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT4:
                                mexecutedFlag = _MT4.AutoTradeTransaction2(4, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                mexecutedFlag = _MT5.AutoTradeTransaction2(4, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.V4:
                                mexecutedFlag = _V4.AutoTradeTransaction2(4, true, out _SendTime);
                                break;
                        }
                        if (mexecutedFlag)
                        {
                            _Log.LogInfo("自动锁单[Close][" + ("SELL_CLOSE".Equals(AutoLockTradeSide) ? -_DSQuote.BidDiff[0] : _DSQuote.BidDiff[0]) + "]延时:[" + Convert.ToInt32(delayTime) + "ms]");
                        }
                        break;
                    }
                }
                if (((int)(delayTime / 1000)) > timelogcount)
                {
                    timelogcount++;
                    _Log.LogInfo("已延时[" + (int)delayTime + "]ms");
                }
                Thread.Sleep(3);
            }
            return mexecutedFlag;
        }
        /// <summary>
        /// 关闭窗体的时候将对RunWorkerCompleted事件的注册接触掉
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void frmTrade_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.bgwMarketQuote.IsBusy)
            {
                this.bgwMarketQuote.CancelAsync();
            }
            switch (_Platform.PlatformNo)
            {
                case (int)Comm.Enum.Platform.MF4:
                    if (_MF4 != null && _MF4.isConnect())
                    {
                        _MF4.DisConnect();
                    }

                    break;
                case (int)Comm.Enum.Platform.MT4:
                    if (_MT4 != null && _MT4.isConnect())
                    {
                        _MT4.DisConnect();
                    }
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    if (_MT5 != null && _MT5.isConnect())
                    {
                        _MT5.DisConnect();
                    }
                    break;
                case (int)Comm.Enum.Platform.V4:
                    if (_V4 != null && _V4.isConnect())
                    {
                        _V4.DisConnect();
                    }
                    break;
            }
            SaveConfig();
            DBHelper.updateAutoInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo, _Account.UserCode, "N");
            DBHelper.updateLoginInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo, _Account.UserCode, "N");
        }
        public void buttonDSConnect_Click(object sender, EventArgs e)
        {
            AsynExec(this.buttonDSConnect, () => {
                if (buttonDSConnect.Text.Equals("DS连接"))
                {
                    DSConnection("DS");
                }
                else if (buttonDSConnect.Text.Equals("DS断开"))
                {
                    DSDisconnect("DS");
                }
            });
        }
        /// <summary>
        /// 数据源连接标志
        /// </summary>
        public bool _IsDSConnectFlag = false;
        /// <summary>
        /// EA连接标识
        /// </summary>
        public bool _IsEAConnectFlag = false;
        /// <summary>
        /// 连接数据源
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public void DSConnection(string type)
        {
            string dsAddress;
            //连接之前先断连,避免出现端口被占用情况
            if (string.Equals("EA", type))
            {             
                dsAddress = _Config.PublishAddressEA;
                DSDisconnect("EA");
            }
            else
            {
                dsAddress = _Config.DSType == "T4" ? _Config.PublishAddressT4 : _Config.PublishAddressOEC;
                DSDisconnect("DS");
            }
            if (dsAddress.Contains("127.0.0.1"))
            {
                dsAddress = dsAddress.Replace("127.0.0.1", uClient.Comm.Utils.GetLocalIP());
            }
            _Log.LogInfo(string.Format("连接到{0}服务器：{1}", type, dsAddress));
            try
            {
                if (dsAddress == null || dsAddress == "")
                {
                    MessageBox.Show(string.Format("{0}服务器地址未配置", type));
                    return;
                }                              
                _Subscriber.Options.TcpKeepalive = true;
                _Subscriber.Options.TcpKeepaliveIdle = new TimeSpan(5, 0, 0);
                _Subscriber.Options.TcpKeepaliveInterval = new TimeSpan(0, 0, 1);
                _Subscriber.Connect(dsAddress);
                _Subscriber.Subscribe("");
                if (string.Equals("DS", type))
                {
                    labelConnectType.Text = _Config.DSType;
                    buttonDSConnect.Text = "DS断开";
                    buttonDSConnect.BackColor = Color.Green;
                    _IsDSConnectFlag = true;
                }
                else
                {
                    labelConnectType.Text = "EA";
                    buttonEAConnect.Text = "EA断开";
                    buttonEAConnect.BackColor = Color.Green;
                    _IsEAConnectFlag = true;
                }
                labelConnectStatus.BackColor = Color.Green;
                labelConnectStatus.Text = "已连接";
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                _Log.LogInfo(string.Format("连接到{0}服务器时错误：{1}", _Config.DSType, ex.Message));
                labelConnectStatus.BackColor = Color.Red;
                labelConnectStatus.Text = "连接失败";
            }
        }
        public void DSDisconnect(string type)
        {
            bgwMarketQuote.CancelAsync();
            string dsAddress;
            if (string.Equals("DS", type))
            {
                dsAddress = _Config.DSType == "T4" ? _Config.PublishAddressT4 : _Config.PublishAddressOEC;
            }
            else
            {                
                dsAddress = _Config.PublishAddressEA;
            }
            if (dsAddress.Contains("127.0.0.1"))
            {
                dsAddress = dsAddress.Replace("127.0.0.1", uClient.Comm.Utils.GetLocalIP());
            }
            _Log.LogInfo(string.Format("断开{0}服务器：{1}", type, dsAddress));
            if (string.Equals("DS", type))
            {
                buttonDSConnect.Text = "DS连接";
                buttonDSConnect.BackColor = Color.Red;
                _IsDSConnectFlag = false;
            }
            else
            {
                buttonEAConnect.Text = "EA连接";
                buttonEAConnect.BackColor = Color.Red;
                _IsEAConnectFlag = false;
            }
            try
            {
                _Subscriber.Unbind(dsAddress);
                _Log.LogInfo("取消连接");
            }
            catch (Exception e)
            {
                _Log.LogInfo("取消连接," + e.Message);
            }
            //_Subscriber.Unbind(dsAddress);
            labelConnectStatus.BackColor = Color.Red;
            labelConnectStatus.Text = "断开";
            ClearDSDiff();
            StopAutoTrade();
        }
        public void ClearDSDiff()
        {
            lblDSSpeed.Text = "0ms";
            lblDSBid.Text = "0";
            lblDSBidDiff0.Text = "0";
            lblDSBidDiff1.Text = "0";
            lblDSBidDiff2.Text = "0";
            lblDSBidDiff3.Text = "0";
            lblDSBidDiff4.Text = "0";
            lblDSAsk.Text = "0";
            lblDSAskDiff0.Text = "0";
            lblDSAskDiff1.Text = "0";
            lblDSAskDiff2.Text = "0";
            lblDSAskDiff3.Text = "0";
            lblDSAskDiff4.Text = "0";
        }
        public void TradeForm_Load(object sender, EventArgs e)
        {
            _Log.LogInfo("MainFormPara:" + _MainFormPara.toString());
            this.Text = _MainFormPara.FormNo;
            LoadConfigFile();

            ///初始化数据库连接信息
            ///"PublishAddressEA":"tcp://47.242.149.181:5553"
            //string[] split = _Config.PublishAddressEA.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            //DBUtils.connAddress = split[1].Substring(2);
            if (!string.IsNullOrEmpty(_Config.EAInfoAddress))
            {
                DBUtils.connEAInfoAddress = _Config.EAInfoAddress;
                DBUtils.createEaConn(_Config.EAInfoAddress);               
            }
            _Log.LogInfo("EAInfo 数据库连接地址:" + DBUtils.eaConnStr);

            LoadEAType();

            LoadConfig();

            _IsLoadConfigCompleted = true;
            InitialPara();
            GeneratePlatformObj();
            IniUI();

            if (bgwMarketQuote.IsBusy != true)
            {
                bgwMarketQuote.RunWorkerAsync();
            }
        }


        /// <summary>
        /// 点击按钮后异步响应
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected Task AsynExec(Control control, Action action)
        {
            control.Enabled = false;
            return Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    control.Invoke(new Action(delegate ()
                    {
                        MessageBox.Show(ex.ToString());
                    }));
                }
                finally
                {
                    control.Invoke(new Action(delegate ()
                    {
                        control.Enabled = true;
                    }));
                }
            });
        }
        /// <summary>
        /// 界面渲染
        /// control.invoke(参数delegate)方法:在拥有此控件的基础窗口句柄的线程上执行指定的委托。
        /// control.begininvoke(参数delegate)方法:在创建控件的基础句柄所在线程上异步执行指定委托。
        /// 如果你的后台线程在更新一个UI控件的状态后不需要等待，而是要继续往下处理，那么你就应该使用BeginInvoke来进行异步处理。
        /// 如果你的后台线程需要操作UI控件，并且需要等到该操作执行完毕才能继续执行，那么你就应该使用Invoke。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        protected void RenderUI(Control control, Action action)
        {
            if (control.IsHandleCreated)
            {
                control.Invoke(new Action(delegate ()
                {
                    action();
                }));
            }
        }
        //如果你的后台线程在更新一个UI控件的状态后不需要等待，而是要继续往下处理，那么你就应该使用BeginInvoke来进行异步处理。
        protected void AsynRenderUI(Control control, Action action)
        {
            if (control.IsHandleCreated)
            {
                control.BeginInvoke(new Action(delegate ()
                {
                    action();
                }));
            }

        }
        /// <summary>
        /// 开始一个延时任务
        /// </summary>
        /// <param name="DelayTime">延时时长（秒）</param>
        /// <param name="taskEndAction">延时时间完毕之后执行的委托（会跳转回UI线程）</param>
        /// <param name="control">UI线程的控件</param>
        public void StartDelayTask(int DelayTime, Action taskEndAction, Control control)
        {
            if (control == null)
            {
                return;
            }

            Task task = new Task(() => {
                try
                {
                    Thread.Sleep(DelayTime * 1000);

                    //返回UI线程
                    control.Invoke(new Action(() =>
                    {
                        taskEndAction();
                    }));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });

            task.Start();
        }


        public void timeHeartPeer_Tick(object sender, EventArgs e)
        {
            //HH:mm:ss
            string status = labelConnectStatus.Text;
            if (!string.IsNullOrEmpty(status) && !string.Equals(labelConnectStatus.Text, "断开") && !string.Equals(labelConnectStatus.Text, "未连接"))
            {
                if (status.IndexOf(":") > 0)
                {
                    DateTime dt1 = Convert.ToDateTime(status);
                    dt1 = dt1.AddMinutes(3);
                    DateTime dt2 = DateTime.Now;
                    if (DateTime.Compare(dt1, dt2) < 0)
                    {
                        DSDisconnect("EA");
                        _Log.LogInfo("策略自动断开连接");
                        Task.Run(() =>
                        {
                            EmailHelper.SendEmail(_MyConfig.NotifyEmail, "[龙知易][策略断连][" + _Broker.BrokerName + "][" + _Account.UserCode + "][策略]自动断开连接,请及时处理", "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][" + _Account.UserCode + "][策略]自动断开连接,请及时处理");
                        });
                    }
                }
            }
        }

        public void label3_Click(object sender, EventArgs e)
        {

        }

       
       

       
       
        

        
        
        public bool isTradingDataExecuting = false;
        public void timerV4AccountPositionRefresh_Tick(object sender, EventArgs e)
        {
            if (_V4 != null && _V4.isConnect() && _DataGridViewInitCompleteFlag && !isTradingDataExecuting)
            {
                isTradingDataExecuting = true;
                Task.Run(() => {
                    _V4._TradingData = _V4.GetOpenedOrders();
                    isTradingDataExecuting = false;
                });
            }
        }
        /// <summary>
        /// 平台是否连接标识
        /// </summary>
        public bool PlatformConnectCheckFlag = false;
        /// <summary>
        /// 数据源连接正常，平台连接不正常时，自动刷新平台的连接状态并发送断链通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timerCheckPlatformConnectStatus_Tick(object sender, EventArgs e)
        {
            //加载完成后
            if (_IsLoadConfigCompleted)
            {
                bool isTradeTime = uClient.Comm.Utils.checkIsTradeTime(_Config.SysConfig["TradeNotradeDuration"], DateTime.Now.ToString(), _Config.SysConfig["TradeTime"]);
                //_Log.LogInfo("平台拉起判断交易时间参数:[TradeNotradeDuration=" + _Config.SysConfig["TradeNotradeDuration"] + "][TradeTime=" + _Config.SysConfig["TradeTime"] + "][isTradeTime="+ isTradeTime + "]");
                //在EA策略执行时间内,MT4平台才启动自动拉起
                if (isTradeTime && _Platform.PlatformNo == 2)
                {
                    //如果是自动端口连接,系统自动拉起,只针对MT平台
                    if ((!_MT4.isConnect() || string.Equals(lblMT4Bid.Text, "0")) && _ConnectRetryCount <= 99)
                    {
                        Task.Run(() =>
                        {
                            _Log.LogInfo("MT4平台自动断开连接,由系统自动拉起开始");
                            _Log.LogInfo("第[" + _ConnectRetryCount + "]拉起MT4平台");
                            _ConnectRetryCount++;
                            //先断开连接,再重新连接
                            _Log.LogInfo("断开连接");
                            switch (_Platform.PlatformNo)
                            {
                                case (int)Comm.Enum.Platform.MF4:
                                    _MF4.DisConnect();
                                    _MF4._BgwBroberQuote.CancelAsync();
                                    _MF4._BgwOrderUpdate.CancelAsync();
                                    break;
                                case (int)Comm.Enum.Platform.MT4:
                                    _MT4._IsManullyDisconnect = true;
                                    _MT4.DisConnect();
                                    break;
                                case (int)Comm.Enum.Platform.MT5:
                                    _MT5._IsManullyDisconnect = true;
                                    _MT5.DisConnect();
                                    break;
                                case (int)Comm.Enum.Platform.V4:
                                    _V4.DisConnect();
                                    _V4._BgwBroberQuote.CancelAsync();
                                    _V4._BgwOrderUpdate.CancelAsync();
                                    break;
                            }
                            uClient.Comm.Utils.ClearAccountCaptial(accountDisplay);
                            UpdateConnectStatus(false);
                            PlatformConnectCheckFlag = true;
                            StopAutoTrade();
                            CancelCheck();

                            //连接平台
                            switch (_Platform.PlatformNo)
                            {
                                case (int)Comm.Enum.Platform.MF4:
                                    _MF4.Connect();
                                    break;
                                case (int)Comm.Enum.Platform.MT4:
                                    _MT4.Connect();
                                    break;
                                case (int)Comm.Enum.Platform.MT5:
                                    _MT5.Connect();
                                    break;
                                case (int)Comm.Enum.Platform.V4:
                                    _V4.Connect();
                                    break;
                            }
                            if (isConnect())
                            {
                                _ConnectRetryCount = 0;
                                AfterConnectCompleted();
                            }
                            _Log.LogInfo("MT4平台自动断开连接,由系统自动拉起完成");
                        });
                    }
                }
            }
        }
        public void checkBoxNotify_CheckedChanged(object sender, EventArgs e)
        {
            if (_TradePara != null)
            {
                //_TradePara.NotifyFlag = checkBoxNotify.Checked;
                _Log.LogInfo(_TradePara.NotifyFlag ? "发送通知" : "取消通知");
            }
        }
       
        
        
        
       

        private void buttonEAConnect_Click(object sender, EventArgs e)
        {
            AsynExec(this.buttonEAConnect, () => {
                if (buttonEAConnect.Text.Equals("EA连接"))
                {
                    DSConnection("EA");
                }
                else if (buttonEAConnect.Text.Equals("EA断开"))
                {
                    DSDisconnect("EA");
                }
            });
        }

       
        /// <summary>
        /// 连接拉起重试次数,最大不超过3次
        /// </summary>
        private int _ConnectRetryCount = 0;
        /// <summary>
        /// 刷新托管数据,包括策略,手数,自动交易,如果是断连自动拉起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerRefreshAutoTrade_Tick(object sender, EventArgs e)
        {           
            //加载完成后
            if (_IsLoadConfigCompleted) 
            {             
                bool isTradeTime = uClient.Comm.Utils.checkIsTradeTime(_Config.SysConfig["TradeNotradeDuration"], DateTime.Now.ToString(), _Config.SysConfig["TradeTime"]);
                //_Log.LogInfo("刷新托管判断交易时间参数:[notradeDuration=" + _Config.SysConfig["notradeDuration"] + "][TradeTime=" + _Config.SysConfig["TradeTime"] + "][isTradeTime=" + isTradeTime + "]");
                //在EA策略执行时间内,MT4平台才启动自动拉起
                if (isTradeTime && _Platform.PlatformNo == 2) 
                {
                    //清空交易日志
                    if (lstTradeRecord.Items.Count > 5)
                    {
                        lstTradeRecord.Items.Clear();
                    }

                    if (_MainFormPara.FormNo.Contains("Trade"))
                    {
                        //更新手数
                        string valumn = DBHelper.getAutoValumn(_Account.UserCode);
                        if (valumn != null && valumn != "")
                        {
                            decimal dv = Convert.ToDecimal(valumn);
                            if (dv != _TradePara.BuyLots && dv != _TradePara.SellLots)
                            {
                                //自动交易参数
                                _TradePara.BuyLots = dv;
                                _TradePara.SellLots = dv;
                                nudSellLots.Value = _TradePara.SellLots;
                                //停止按钮颜色改变事件
                                if (_Button_ChangeColor_Timer_TradePara != null)
                                {
                                    _Button_ChangeColor_Timer_TradePara.Stop();
                                    _Button_ChangeColor_Timer_TradePara.Enabled = false;
                                    button_TradePara_Save.BackColor = Color.White;
                                }
                                //_Log.LogInfo("更新托管交易手数完成");
                            }
                        }
                        //更新自动交易标识
                        string autoTradeFlag = DBHelper.getAutoTradeFlag(_Account.UserCode);
                        if (autoTradeFlag != null && autoTradeFlag != "")
                        {
                            if (string.Equals(autoTradeFlag, "Y"))
                            {
                                _TradePara.AutoTrade = true;
                            }
                            else
                            {
                                _TradePara.AutoTrade = false;
                            }
                            //_Log.LogInfo("更新托管自动交易标识完成");
                        }
                        //更新EA策略
                        IDictionary<string, string> eaInfo = DBHelper.getAutoInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo);
                        if (eaInfo != null)
                        {
                            string nEATypeAddress = "tcp://" + _Config.SysConfig[eaInfo["ea_type"]] + ":5553";

                            if (!string.Equals(nEATypeAddress, _Config.PublishAddressEA))
                            {
                                DSDisconnect("EA");
                                _Config.PublishAddressEA = nEATypeAddress;
                                DSConnection("EA");
                                StartAutoTrade();
                            }
                        }
                        //更新余额
                        switch (_Platform.PlatformNo)
                        {
                            case (int)Comm.Enum.Platform.MF4:
                                if (_MF4.isConnect())
                                {
                                    _MF4.UpdateAccountBalance();
                                }
                                break;
                            case (int)Comm.Enum.Platform.MT4:
                                if (_MT4.isConnect())
                                {
                                    _MT4.UpdateAccountBalance();
                                }
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                if (_MT5.isConnect())
                                {
                                    _MT5.UpdateAccountBalance();
                                }
                                break;
                            case (int)Comm.Enum.Platform.V4:
                                break;
                        }
                        //连接EA数据源
                        if (!_IsEAConnectFlag)
                        {
                            DSConnection("EA");
                        }
                        //启动自动交易
                        if (!_EnableAutoTrade)
                        {
                            //启动自动交易
                            if (isConnect() && _IsEAConnectFlag)
                            {
                                StartAutoTrade();
                                //启动自动交易成功
                                DBHelper.updateAutoInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo, _Account.UserCode, "Y");
                            }
                        }
                    } 
                }              
            }            
        }
        
    }
}
