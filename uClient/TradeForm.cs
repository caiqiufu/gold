using M4.Common.Enums;
using mtapi.mt5;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TradingAPI.MT4Server;
using uClient.Comm;
using Order = TradingAPI.MT4Server.Order;

namespace uClient
{
    public partial class TradeForm : Form
    {
        //MT4 平台 begin
        public MT4 _MT4;

        //MT5 平台 begin
        public MT5 _MT5;
        //MT4 平台 end

        //MF4 平台 beign
        public MF4 _MF4;
        //MF4 平台 end

        //V4 平台 beign
        public V4 _V4;
        //V4 平台 end

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
        public SubscriberSocket _Subscriber;
        /// <summary>
        /// 数据源订阅线程生命周期控制。NetMQ socket 的创建、连接、接收和释放必须在同一线程。
        /// </summary>
        private readonly object _dsLifecycleLock = new object();
        private readonly ManualResetEventSlim _dsWorkerStopped = new ManualResetEventSlim(true);
        private volatile bool _dsStopRequested = false;
        private string _dsEndpoint = string.Empty;
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
            if (mainFormPara == null)
            {
                mainFormPara = new MainFormPara();
                mainFormPara.PlatformCode = "MT4";
                mainFormPara.BrokerCode = "";
                mainFormPara.BrokerName = "";
                mainFormPara.FormType = "";
                mainFormPara.FormNo = "";
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
            mainFormPara.FormType = "";
            mainFormPara.FormNo = "";
            _MainFormPara = mainFormPara;
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            InitObject();
        }
        /// <summary>
        /// 实例化对象，该方法只能调用一次
        /// </summary>
        public void InitObject()
        {
            //Log 实例只初始化一次
            _Log = new Log(lstTradeRecord, lstLog, _MainFormPara.FormNo);
            _Log._TradeWinName = _MainFormPara.FormName;
        }
        /// <summary>
        /// 初始化UI参数
        /// </summary>
        public void IniUI()
        {
            _logger.Info("开始初始化UI");
            //初始平台设置
            comboBox_Slippage.SelectedItem = Convert.ToString(_TradePara.Slippage);
            comboBoxCloseTimeDuration.Text = _TradePara.AutoCloseTimeDuration;
            checkBoxAutoChecked.Checked = _TradePara.AutoChecked;
            checkBoxNotify.Checked = _TradePara.NotifyFlag;
            checkboxTimezone.Text = _TradePara.Timezone.ToString();
            if (string.IsNullOrEmpty(_TradePara.EventConfig))
            {
                _TradePara.EventConfig = "3,5,3,3";
            }
            textBox_eventConfig.Text = _TradePara.EventConfig;

            //初始锁单设置
            comboBoxAutoLockTime.Text = Convert.ToInt32(_TradePara.AutoLockTimeDuration).ToString();
            comboBoxLockPoint.SelectedItem = Convert.ToInt32(_TradePara.AutoLockPoint).ToString();
            checkBox_QuotaCheck.Checked = _TradePara.QuotaCheckFlag;
            checkBox_Event.Checked = _TradePara.EventFlag;
            if (!string.IsNullOrEmpty(_TradePara.AutoLockForm))
            {
                comboBox_AutoLockForm.SelectedItem = _TradePara.AutoLockForm;
                //初始化锁单不选中
                //checkBox_AutoLock.Checked = _TradePara.AutoLock;
                checkBox_AutoLock.Checked = false;
                checkBoxIndLock.Checked = false;
            }
            //交易商
            comboBox_Broker.Items.Clear();
            foreach (Comm.Broker bro in _Platform.Broker)
            {
                comboBox_Broker.Items.Add(bro.BrokerName);
            }

            //账户类型
            comboBox_AccType.Items.Clear();
            comboBox_AccType.Items.Add("Demo");
            comboBox_AccType.Items.Add("Live");
            //交易品类
            if (_Account != null && _Broker.Symbol != null && _Broker.Symbol.Length > 0)
            {

                comboBox_Symbol.Items.Clear();
                foreach (string ss in _Broker.Symbol)
                {
                    comboBox_Symbol.Items.Add(ss);
                }

            }
            //默认选择的平台
            comboBox_Broker.SelectedItem = _Broker.BrokerName;
            //默认选择的账户类型
            comboBox_AccType.SelectedItem = _Account.Type;
            //默认选择的品类
            comboBox_Symbol.SelectedItem = _TradeSymbol;

            //用户账号密码
            textBox_UserCode.Text = _Account.UserCode;
            textBox_Password.Text = _Account.Password;
            //自动交易参数
            nudSellOpen.Value = _TradePara.SellOpen;
            nudSellClose.Value = _TradePara.SellClose;
            nudSellLots.Value = _TradePara.SellLots;
            nudBuyOpen.Value = _TradePara.BuyOpen;
            nudBuyClose.Value = _TradePara.BuyClose;
            nudBuyLots.Value = _TradePara.BuyLots;
            //初始化订单列表
            IniPositionGrid();
            //停止按钮颜色改变事件
            if (_Button_ChangeColor_Timer_TradePara != null)
            {
                _Button_ChangeColor_Timer_TradePara.Stop();
                _Button_ChangeColor_Timer_TradePara.Enabled = false;
                button_TradePara_Save.BackColor = System.Drawing.Color.White;
            }
            //选择交易商后锁单设置
            comboBoxAutoLockTime.SelectedItem = _TradePara.AutoLockTimeDuration;
            comboBoxLockPoint.SelectedItem = _TradePara.AutoLockPoint;
            //comboBox_AutoLockForm.SelectedItem = _TradePara.AutoLockForm;//手工选择锁单
            //checkBox_AutoLock.Checked = _TradePara.AutoLock;//手工选择锁单
            checkBoxIndLock.Checked = _TradePara.IndLockFlag;
            checkBox_QuotaCheck.Checked = _TradePara.QuotaCheckFlag;
            checkBox_Event.Checked = _TradePara.EventFlag;

            //选择交易商后平台设置
            comboBoxCloseTimeDuration.SelectedItem = _TradePara.AutoCloseTimeDuration;
            comboBox_Slippage.SelectedItem = _TradePara.Slippage;
            checkBoxAutoChecked.Checked = _TradePara.AutoChecked;
            checkBoxNotify.Checked = _TradePara.NotifyFlag;
            checkboxTimezone.Text = _TradePara.Timezone.ToString();
            numericUpDown_QuotaCheck.Value = _TradePara.QuotaCheckValue;
        }
        /// <summary>
        /// 配置文件默认目录
        /// </summary>
        public string rootPath = "C:\\iAutoTrade";


        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void LoadConfigFile()
        {
            string newMyConfigFile = rootPath + _MyConfigFile;
            string newConfigFile = rootPath + _ConfigFile;
            string newStrategyFile = rootPath + _StrategyFile;
            _logger.Info("加载初始化文件开始");
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
            _logger.Info("加载初始化文件完成");
        }
        /// <summary>
        /// 加载配置参数
        /// </summary>
        public virtual void LoadConfig()
        {
            _logger.Info("开始加载配置参数开始");
            if (_Config != null)
            {
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
                                    foreach (Comm.Account account in _MyConfig.Account)
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
                    if (_Platform != null)
                    {
                        if (_Platform.BrokerList.Keys.Contains(_Platform.DefaultBrokerCode))
                        {
                            if (!string.IsNullOrEmpty(_MainFormPara.BrokerCode))
                            {
                                if (_Platform.BrokerList.ContainsKey(_MainFormPara.BrokerCode))
                                {
                                    _Broker = _Platform.BrokerList[_MainFormPara.BrokerCode];
                                }
                            }
                            if (_Broker == null)
                            {
                                if (_Platform.BrokerList.ContainsKey(_Platform.DefaultBrokerCode))
                                {
                                    _Broker = _Platform.BrokerList[_Platform.DefaultBrokerCode];
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
                                _Broker.TradePara = _TradePara;
                            }
                            else
                            {
                                _logger.Fatal("代理商信息不存在，请检查配置文件");
                            }
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
            //加载Account信息后初始化到Log对象
            _Log._Account = _Account;
            _logger.Info("开始加载配置参数完成");
        }
        /// <summary>
        /// 配置参数加载完成后，或者切换平台、切换账户后，初始化参数
        /// </summary>
        public void InitialPara()
        {
            InitQuoteDisplay();
            InitEventConfig();
        }
        /// <summary>
        /// 事件单说明:在行情极端变化情况,如果在给定时间内连续出现给定的跳次[textBox_eventConfig],就执行开单操作
        /// 初始化事件单数据
        /// </summary>
        public void InitEventConfig()
        {
            _logger.Info("初始化事件策略参数开始");
            //初始化事件参数
            if (_TradePara != null && !string.IsNullOrEmpty(_TradePara.EventConfig) && _TradePara.EventConfig.Contains(","))
            {
                string[] split = _TradePara.EventConfig.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 4)
                {
                    _StrategyConfig.dataTimeDuration = Convert.ToInt32(split[0]);
                    _StrategyConfig.dataPriceDiffSize = Convert.ToInt32(split[1]);
                    _StrategyConfig.dataPriceDiffTotalCount = Convert.ToInt32(split[2]);
                    _StrategyConfig.dataPricePositiveCount = Convert.ToInt32(split[3]);
                }
                else
                {
                    _StrategyConfig.dataTimeDuration = 10;
                    _StrategyConfig.dataPriceDiffSize = 3;
                    _StrategyConfig.dataPriceDiffTotalCount = 5;
                    _StrategyConfig.dataPricePositiveCount = 4;
                }
            }
            _logger.Info("初始化事件策略参数完成");
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
            _logger.Info("TradeForm初始化平台对象开始");
            switch (_Platform.PlatformNo)
            {
                case (int)Comm.Enum.Platform.MF4:
                    _MF4 = new MF4(_Config, _MyConfig, _Broker, _Account, _TradeSymbol, _Log, bgwBroberQuote, bgwOrderUpdate, gvPositions, quotaDisplayPanel, accountDisplay);
                    _MF4._QuoteData = new QuotePanelData(_TradeSymbol, 0.1);
                    _MF4._DSQuote = _DSQuote;
                    _MF4._StrategyConfig = _StrategyConfig;
                    _MF4._OrderCreateType = this._OrderCreateType;
                    break;
                case (int)Comm.Enum.Platform.MT4:
                    _MT4 = new MT4(_Config, _MyConfig, _Broker, _Account, _TradeSymbol, _Log, bgwBroberQuote, bgwOrderUpdate, gvPositions, quotaDisplayPanel, accountDisplay);
                    _MT4._QuoteData = new QuotePanelData(_TradeSymbol, 0.1);
                    _MT4._DSQuote = _DSQuote;
                    _MT4._StrategyConfig = _StrategyConfig;
                    _MT4._OrderCreateType = this._OrderCreateType;
                    if (_Broker.Symbol != null && _Broker.Symbol.Length == 2)
                    {
                        _MT4._GOLD = _Broker.Symbol[0];
                        _MT4._SILVER = _Broker.Symbol[1];
                    }
                    else
                    {
                        _MT4._GOLD = _Broker.DefaultSymbol;
                        _MT4._SILVER = "";
                    }
                    _Log.LogInfo("GOLD:" + _MT4._GOLD + ",SILVER:" + _MT4._SILVER);
                    break;
                case (int)Comm.Enum.Platform.V4:
                    _V4 = new V4(_Config, _MyConfig, _Broker, _Account, _TradeSymbol, _Log, bgwBroberQuote, bgwOrderUpdate, gvPositions, quotaDisplayPanel, accountDisplay);
                    _V4._QuoteData = new QuotePanelData(_TradeSymbol, 0.1);
                    _V4._DSQuote = _DSQuote;
                    _V4._StrategyConfig = _StrategyConfig;
                    _V4._OrderCreateType = this._OrderCreateType;
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    _MT5 = new MT5(_Config, _MyConfig, _Broker, _Account, _TradeSymbol, _Log, bgwBroberQuote, bgwOrderUpdate, gvPositions, quotaDisplayPanel, accountDisplay);
                    _MT5._QuoteData = new QuotePanelData(_TradeSymbol, 0.1);
                    _MT5._DSQuote = _DSQuote;
                    _MT5._StrategyConfig = _StrategyConfig;
                    _MT5._OrderCreateType = this._OrderCreateType;
                    if (_Broker.Symbol != null && _Broker.Symbol.Length == 2)
                    {
                        _MT5._GOLD = _Broker.Symbol[0];
                        _MT5._SILVER = _Broker.Symbol[1];
                    }
                    else
                    {
                        _MT5._GOLD = _Broker.DefaultSymbol;
                        _MT5._SILVER = "";
                    }
                    _Log.LogInfo("GOLD:" + _MT5._GOLD + ",SILVER:" + _MT5._SILVER);
                    break;
            }
            _logger.Info("TradeForm初始化平台对象完成");
        }
        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void SaveConfig()
        {
            _logger.Info("保存配置文件开始");
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
                    //交易信息设置
                    _TradePara.SellOpen = nudSellOpen.Value;
                    _TradePara.SellClose = nudSellClose.Value;
                    _TradePara.SellLots = nudSellLots.Value;
                    _TradePara.BuyOpen = nudBuyOpen.Value;
                    _TradePara.BuyClose = nudBuyClose.Value;
                    _TradePara.BuyLots = nudBuyLots.Value;
                    //交易商平台设置
                    _TradePara.Slippage = Convert.ToInt32(string.IsNullOrEmpty(comboBox_Slippage.Text) ? "1" : comboBox_Slippage.Text);
                    _TradePara.AutoCloseTimeDuration = comboBoxCloseTimeDuration.Text;
                    _TradePara.AutoChecked = checkBoxAutoChecked.Checked;
                    _TradePara.NotifyFlag = checkBoxNotify.Checked;
                    _TradePara.Timezone = double.Parse(string.IsNullOrEmpty(checkboxTimezone.Text) ? "0" : checkboxTimezone.Text);
                    _TradePara.QuotaCheckValue = numericUpDown_QuotaCheck.Value;
                    //保存该配置信息后需要调用InitEventConfig 把参数解析到变量中
                    _TradePara.EventConfig = textBox_eventConfig.Text;
                    InitEventConfig();

                    //交易商锁单设置
                    _TradePara.AutoLockTimeDuration = Convert.ToInt32(string.IsNullOrEmpty(comboBoxAutoLockTime.Text) ? "1" : comboBoxAutoLockTime.Text);
                    _TradePara.AutoLockPoint = Convert.ToInt32(string.IsNullOrEmpty(comboBoxLockPoint.Text) ? "2" : comboBoxLockPoint.Text);
                    _TradePara.AutoLock = checkBox_AutoLock.Checked;
                    _TradePara.AutoLockForm = comboBox_AutoLockForm.Text;
                    _TradePara.IndLockFlag = checkBoxIndLock.Checked;
                    _TradePara.QuotaCheckFlag = checkBox_QuotaCheck.Checked;
                    _TradePara.EventFlag = checkBox_Event.Checked;
                    _TradePara.OnlyCloseFlag = false; //默认都是开平仓
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
            _logger.Info("保存配置文件完成");
        }
        /// <summary>
        /// 保存配置文件后赋值到对象变量
        /// </summary>
        public void InitClient()
        {
            switch (_Platform.PlatformNo)
            {
                case (int)Comm.Enum.Platform.MF4:
                    if (_MF4 != null)
                    {
                        _MF4._DSQuote = _DSQuote;
                        _MF4._Config = _Config;
                        _MF4._MyConfig = _MyConfig;
                        _MF4._Broker = _Broker;
                        _MF4._Account = _Account;
                        _MF4._TradeSymbol = _TradeSymbol;
                    }
                    break;
                case (int)Comm.Enum.Platform.MT4:
                    if (_MT4 != null)
                    {
                        _MT4._DSQuote = _DSQuote;
                        _MT4._Config = _Config;
                        _MT4._MyConfig = _MyConfig;
                        _MT4._Broker = _Broker;
                        _MT4._Account = _Account;
                        _MT4._TradeSymbol = _TradeSymbol;
                    }
                    break;
                case (int)Comm.Enum.Platform.V4:
                    if (_V4 != null)
                    {
                        _V4._DSQuote = _DSQuote;
                        _V4._Config = _Config;
                        _V4._MyConfig = _MyConfig;
                        _V4._Broker = _Broker;
                        _V4._Account = _Account;
                        _V4._TradeSymbol = _TradeSymbol;
                    }
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    if (_MT5 != null)
                    {
                        _MT5._DSQuote = _DSQuote;
                        _MT5._Config = _Config;
                        _MT5._MyConfig = _MyConfig;
                        _MT5._Broker = _Broker;
                        _MT5._Account = _Account;
                        _MT5._TradeSymbol = _TradeSymbol;
                    }
                    break;
            }
        }
        /// <summary>
        /// 是否连接
        /// </summary>
        /// <returns></returns>
        public bool isConnect()
        {
            if (_Platform != null)
            {
                switch (_Platform.PlatformNo)
                {
                    case (int)Comm.Enum.Platform.MF4:
                        if (_MF4 != null && _MF4.isConnect())
                        {
                            return true;
                        }
                        break;
                    case (int)Comm.Enum.Platform.MT4:
                        if (_MT4 != null && _MT4.isConnect())
                        {
                            return true;
                        }
                        break;
                    case (int)Comm.Enum.Platform.V4:
                        if (_V4 != null && _V4.isConnect())
                        {
                            return true;
                        }
                        break;
                    case (int)Comm.Enum.Platform.MT5:
                        if (_MT5 != null && _MT5.isConnect())
                        {
                            return true;
                        }
                        break;
                }
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
            AsynExec(this.btnLogin, () =>
            {
                if (btnLogin.Text.Equals("平台连接"))
                {
                    switch (_Platform.PlatformNo)
                    {
                        case (int)Comm.Enum.Platform.MF4:
                            _MF4.Connect();
                            break;
                        case (int)Comm.Enum.Platform.MT4:
                            _MT4.Connect();
                            break;
                        case (int)Comm.Enum.Platform.V4:
                            _V4.Connect();
                            break;
                        case (int)Comm.Enum.Platform.MT5:
                            _MT5.Connect();
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
                            _MF4._BgwBroberQuote.CancelAsync();
                            _MF4._BgwOrderUpdate.CancelAsync();
                            break;
                        case (int)Comm.Enum.Platform.MT4:
                            _MT4._IsManullyDisconnect = true;
                            _MT4.DisConnect();
                            break;
                        case (int)Comm.Enum.Platform.V4:
                            _V4.DisConnect();
                            _V4._BgwBroberQuote.CancelAsync();
                            _V4._BgwOrderUpdate.CancelAsync();
                            break;
                        case (int)Comm.Enum.Platform.MT5:
                            _MT5._IsManullyDisconnect = true;
                            _MT5.DisConnect();
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
        /// 手动买多开仓
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // MT5 手动开仓：每次点击均独立提交，不使用等待成交/重复点击锁。
        // SDK 调用仍在后台执行，避免阻塞 WinForms UI。
        private async Task SubmitMt5ManualOpenAsync(bool isBuy, decimal lots)
        {
            string side = isBuy ? "BUY" : "SELL";
            try
            {
                _Log.LogInfo("[MT5][MANUAL_OPEN][SEND_BEGIN] side=" + side + " lots=" + lots);
                DateTime sendTime = await Task.Run(() =>
                {
                    DateTime time;
                    if (isBuy) _MT5.OpenBuyOrder(false, out time, lots);
                    else _MT5.OpenSellOrder(false, out time, lots);
                    return time;
                });
                _SendTime = sendTime;
                _Log.LogInfo("[MT5][MANUAL_OPEN][SEND_RETURNED] side=" + side +
                             " 等待服务器回报（非成交确认）");
            }
            catch (Exception ex)
            {
                _logger.Error("[MT5][MANUAL_OPEN][ERROR] " + ex);
                _Log.LogInfo("[MT5][MANUAL_OPEN][ERROR] side=" + side + " " + ex.Message);
            }
        }

        public async void btnBuy_Click(object sender, EventArgs e)
        {
            StopAutoTrade();
            if (!isConnect()) return;
            CancelCheck();
            _IsAutoOperationFlag = false;
            if (_Platform.PlatformNo == (int)Comm.Enum.Platform.MT5)
            {
                await SubmitMt5ManualOpenAsync(true, _TradePara.BuyLots);
                return;
            }
            AsynExec(this.btnBuy, () =>
            {
                switch (_Platform.PlatformNo)
                {
                    case (int)Comm.Enum.Platform.MF4: _MF4.OpenBuyOrder(false, out _SendTime, _TradePara.BuyLots); break;
                    case (int)Comm.Enum.Platform.MT4: _MT4.OpenBuyOrder(false, out _SendTime, _TradePara.BuyLots); break;
                    case (int)Comm.Enum.Platform.V4: _V4.OpenBuyOrder(false, out _SendTime, _TradePara.BuyLots); break;
                }
            });
        }

        public async void btnSell_Click(object sender, EventArgs e)
        {
            StopAutoTrade();
            if (!isConnect()) return;
            CancelCheck();
            _IsAutoOperationFlag = false;
            if (_Platform.PlatformNo == (int)Comm.Enum.Platform.MT5)
            {
                await SubmitMt5ManualOpenAsync(false, _TradePara.SellLots);
                return;
            }
            AsynExec(this.btnSell, () =>
            {
                switch (_Platform.PlatformNo)
                {
                    case (int)Comm.Enum.Platform.MF4: _MF4.OpenSellOrder(false, out _SendTime, _TradePara.SellLots); break;
                    case (int)Comm.Enum.Platform.MT4: _MT4.OpenSellOrder(false, out _SendTime, _TradePara.SellLots); break;
                    case (int)Comm.Enum.Platform.V4: _V4.OpenSellOrder(false, out _SendTime, _TradePara.SellLots); break;
                }
            });
        }
        /// <summary>
        /// 手动平仓
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // 手动平仓不维护 Pending Ticket 集合，也不拦截连续点击。
        private void RunOnUiThread(Action action)
        {
            if (IsDisposed || Disposing || !IsHandleCreated) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(action); }
                catch (InvalidOperationException) { }
                return;
            }
            action();
        }

        private async Task CloseMt5ManuallyAsync()
        {
            // 所有 DataGridView 读取都在 UI 线程完成；SDK 操作在后台执行。
            if (_MT5 == null || !_MT5.isConnect()) return;
            gvPositions.EndEdit();
            if (gvPositions.Rows.Count == 1)
                gvPositions.Rows[0].Cells[0].Value = true;
            var tickets = new List<string>();
            foreach (DataGridViewRow row in gvPositions.Rows)
            {
                if (row.IsNewRow) continue;
                if (!Convert.ToBoolean(row.Cells[0].Value ?? false)) continue;
                string ticket = Convert.ToString(row.Cells[1].Value);
                if (!string.IsNullOrWhiteSpace(ticket)) tickets.Add(ticket);
            }
            if (tickets.Count == 0)
            {
                MessageBox.Show(this, "请选择要平仓的订单");
                return;
            }
            string closeDuration = _TradePara.AutoCloseTimeDuration;
            double timezone = _TradePara.Timezone;
            foreach (string ticket in tickets.Distinct())
            {
                try
                {
                    _Log.LogInfo("[MT5][MANUAL_CLOSE][GET_ORDER_BEGIN] ticket=" + ticket);
                    mtapi.mt5.Order order = await Task.Run(() => _MT5.GetOrder(ticket));
                    if (order == null)
                        throw new InvalidOperationException("平台未找到订单 ticket=" + ticket);
                    _Log.LogInfo("[MT5][MANUAL_CLOSE][GET_ORDER_END] ticket=" + ticket);
                    if (!TradeUtils.IsCloseOrderTime(TradeUtils.GetCloseTimeDuration(closeDuration),
                            DateTime.Now, order.OpenTime.AddHours(timezone)))
                    {
                        DialogResult dr = MessageBox.Show(this,
                            "开仓时间未超过[" + closeDuration + "]，是否需要平仓？",
                            "平仓提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2);
                        if (dr != DialogResult.Yes) continue;
                    }
                    _Log.LogInfo("[MT5][MANUAL_CLOSE][SEND_BEGIN] ticket=" + ticket);
                    DateTime sendTime = await Task.Run(() =>
                    {
                        DateTime time;
                        _MT5.CloseOrder(order, false, out time);
                        return time;
                    });
                    _SendTime = sendTime;
                    _Log.LogInfo("[MT5][MANUAL_CLOSE][SEND_RETURNED] ticket=" + ticket +
                                 " awaiting MarketClose callback (not a fill confirmation)");
                }
                catch (Exception ex)
                {
                    _logger.Error("[MT5][MANUAL_CLOSE][ERROR] ticket=" + ticket + " " + ex);
                    _Log.LogInfo("[MT5][MANUAL_CLOSE][ERROR] ticket=" + ticket + " " + ex.Message);
                }
            }
        }

        public async void btnClose_Click(object sender, EventArgs e)
        {
            StopAutoTrade();
            if (_Platform != null && _Platform.PlatformNo == (int)Comm.Enum.Platform.MT5)
            {
                CancelCheck();
                await CloseMt5ManuallyAsync();
                return;
            }
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
                        double timezone = _TradePara.Timezone;
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
            if (isConnect() && IsDataSourceReady())
            {
                StartAutoTrade();
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
            if (InvokeRequired)
            {
                RunOnUiThread(() => StartAutoTrade());
                return;
            }
            // 所有启动入口统一检查，避免定时器或其他调用绕过按钮检查。
            if (!isConnect() || !IsDataSourceReady())
            {
                _Log.LogInfo("[AUTO_TRADE][START_BLOCKED] 交易平台或数据源尚未就绪，禁止启动自动交易");
                return;
            }

            _Log.LogInfo("自动交易启用");
            btnStart.Enabled = false;
            btnStart.BackColor = System.Drawing.Color.Green;
            btnStop.Enabled = true;
            lblTradeStatus.Text = "自动交易已启动";
            lblTradeStatus.BackColor = System.Drawing.Color.Green;
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
        }
        public void StopAutoTrade()
        {
            if (InvokeRequired)
            {
                // Stop the trading flag immediately, then update controls asynchronously.
                _EnableAutoTrade = false;
                RunOnUiThread(() => StopAutoTrade());
                return;
            }
            _Log.LogInfo("自动交易停止");
            _EnableAutoTrade = false;
            lblTradeStatus.Text = "自动交易已停止";
            CancelCheck();
            btnStart.Enabled = true;
            btnStart.BackColor = System.Drawing.Color.Red;
            btnStop.Enabled = false;
            lblTradeStatus.BackColor = System.Drawing.Color.Red;
        }
        /// <summary>
        /// 允许自动卖空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void chkSellOpen_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSellOpen != null)
            {
                _Log.LogInfo((chkSellOpen.Checked ? "" : "不") + "允许开仓(卖空单)");
            }
        }
        /// <summary>
        /// 允许自动买多
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void chkBuyOpen_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBuyOpen != null)
            {
                _Log.LogInfo((chkBuyOpen.Checked ? "" : "不") + "允许开仓(买多单)");
            }
        }
        /// <summary>
        /// 允许自动平仓
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void chkSellClose_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSellClose != null)
            {
                _Log.LogInfo((chkSellClose.Checked ? "" : "不") + "允许平仓（卖空单）");
            }
        }
        /// <summary>
        /// 允许自动平仓
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void chkBuyClose_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBuyClose != null)
            {
                _Log.LogInfo((chkBuyClose.Checked ? "" : "不") + "允许平仓（买多单）");
            }
        }
        /// <summary>
        /// 改变合约
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void cmbSymbol_SelectedValueChanged(object sender, EventArgs e)
        {
            _TradeSymbol = comboBox_Symbol.SelectedItem.ToString();
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
                case (int)Comm.Enum.Platform.V4:
                    if (_V4.isConnect())
                    {
                        _V4.SymbolSubscription(_TradeSymbol);
                    }
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    if (_MT5.isConnect())
                    {
                        _MT5.SymbolSubscription(_TradeSymbol);
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
        /// 数据源和平台商价格差异,每1分钟保存一次,数据格式:price:time
        /// price = 数据源价格-平台价格
        /// </summary>
        public Queue<string> _BrokerAndDSPriceDiffList = new Queue<string>(5);
        /// <summary>
        /// V20261027_PRICEDIFF_TIME_FORMAT：价格差异记录的时间格式（读写统一、与系统区域设置无关）。
        /// </summary>
        private const string PriceDiffTimeFormat = "yyyy/MM/dd HH:mm:ss";
        /// <summary>
        /// 兼容旧记录：原来用 now.ToString() 按系统区域设置写入，中文系统下是 "yyyy/M/d H:mm:ss"。
        /// </summary>
        private static readonly string[] PriceDiffTimeParseFormats = { "yyyy/MM/dd HH:mm:ss", "yyyy/M/d H:mm:ss" };
        /// <summary>
        /// 数据源与平台价格差异类型
        /// 0:没有价格差异
        /// 1:价格扩大,开多单
        /// -1:价格缩小,开空单
        /// </summary>
        public string _BrokerAndDSPriceDiffType = "";

        public void UpdateBrokerAndDSPriceDiff()
        {
            double dsPrice = _DSQuote.Bid;
            double brokerPrice = 0;
            switch (_Platform.PlatformNo)
            {
                case (int)Comm.Enum.Platform.MF4:
                    if (_MF4._Quote != null)
                    {
                        brokerPrice = (double)_MF4._Quote.BidPrice;
                    }
                    break;
                case (int)Comm.Enum.Platform.MT4:
                    if (_MT4._Quote != null)
                    {
                        brokerPrice = (double)_MT4._Quote.Bid;
                    }
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    if (_MT5._Quote != null)
                    {
                        brokerPrice = (double)_MT5._Quote.Bid;
                    }
                    break;
            }
            if (dsPrice > 0 && brokerPrice > 0)
            {
                double priceDiff = Math.Abs(Math.Round(dsPrice - brokerPrice, 2));
                DateTime now = DateTime.Now;
                // V20261027_PRICEDIFF_TIME_FORMAT：
                // 原来写入用 now.ToString()，按系统区域设置输出（本机为 "yyyy/M/d H:mm:ss"，
                // 小时/月/日小于 10 时只有一位），读取却用严格的 "yyyy/MM/dd HH:mm:ss"。
                // 23 点的记录碰巧是两位小时能解析，过了零点写入 "2026/10/27 0:00:37"
                // 就会抛 FormatException。现在写入固定为两位格式，读取兼容旧记录且不再抛异常。
                string record = priceDiff.ToString(CultureInfo.InvariantCulture) + ":" +
                                now.ToString(PriceDiffTimeFormat, CultureInfo.InvariantCulture);
                // 本方法由 MarketQuote 中的 Task.Run 并发调用，Queue<T> 非线程安全，需加锁。
                lock (_BrokerAndDSPriceDiffList)
                {
                    if (_BrokerAndDSPriceDiffList.Count > 0)
                    {
                        // 使用 LINQ 获取最新放入的数据
                        string lastDiff = _BrokerAndDSPriceDiffList.Last();
                        string[] parts = lastDiff.Split(new[] { ':' }, 2);
                        DateTime dateTime;
                        if (parts.Length < 2 ||
                            !DateTime.TryParseExact(parts[1], PriceDiffTimeParseFormats, CultureInfo.InvariantCulture,
                                                    DateTimeStyles.None, out dateTime))
                        {
                            // 无法解析的旧记录视为已超过间隔，直接写入新记录
                            dateTime = DateTime.MinValue;
                        }

                        TimeSpan difference = now - dateTime;
                        //超过间隔时间,保存价格差异
                        if (difference.TotalMinutes > 1)
                        {
                            _BrokerAndDSPriceDiffList.Enqueue(record);
                        }
                    }
                    else
                    {
                        _BrokerAndDSPriceDiffList.Enqueue(record);
                    }
                }
            }
        }

        /// <summary>
        /// 平台价格差异提醒
        /// </summary>
        public void BrokerPriceBlockAlarm()
        {
            double dsPrice = _DSQuote.Bid;
            double brokerPrice = 0;
            switch (_Platform.PlatformNo)
            {
                case (int)Comm.Enum.Platform.MF4:
                    if (_MF4._Quote != null)
                    {
                        brokerPrice = (double)_MF4._Quote.BidPrice;
                    }
                    break;
                case (int)Comm.Enum.Platform.MT4:
                    if (_MT4._Quote != null)
                    {
                        brokerPrice = (double)_MT4._Quote.Bid;
                    }
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    if (_MT5._Quote != null)
                    {
                        brokerPrice = (double)_MT5._Quote.Bid;
                    }
                    break;
            }
            if (dsPrice > 0 && brokerPrice > 0 && _TradePara.QuotaCheckValue > 1)
            {
                double prices = 0;
                int priceCount = 0;
                // 队列的 Count、遍历均在同一把锁内，避免与行情线程并发访问。
                lock (_BrokerAndDSPriceDiffList)
                {
                    if (_BrokerAndDSPriceDiffList.Count <= 3)
                        return;

                    foreach (string diff in _BrokerAndDSPriceDiffList)
                    {
                        string[] parts = diff.Split(new[] { ':' }, 2);
                        double parsedDiff;
                        if (parts.Length == 2 &&
                            double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out parsedDiff))
                        {
                            prices += parsedDiff;
                            priceCount++;
                        }
                    }
                }
                if (priceCount == 0)
                {
                    return;
                }
                double avgPrice = Math.Round(prices / priceCount, 2);
                double currDiff = Math.Round(dsPrice - brokerPrice, 2);
                //价格差值变大,开多单
                if (currDiff - avgPrice > (double)_TradePara.QuotaCheckValue)
                {
                    /*SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\Alarm05.wav");
                    player.PlaySync();*/
                    _BrokerAndDSPriceDiffType = "1";
                }
                //价格差值变小,开空单
                if (currDiff - avgPrice < (double)_TradePara.QuotaCheckValue)
                {
                    _BrokerAndDSPriceDiffType = "-1";
                }
                if (string.Equals(_BrokerAndDSPriceDiffType, "1") || string.Equals(_BrokerAndDSPriceDiffType, "-1"))
                {
                    string message = "价格差异:平均偏差[" + avgPrice + "]当前偏差[" + currDiff + "]设置值[" + _TradePara.QuotaCheckValue + "]";
                    _Log.LogTradeRecord("", message);
                    _Log.LogInfo(message);
                    lock (_BrokerAndDSPriceDiffList)
                    {
                        _BrokerAndDSPriceDiffList.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// 更新帐户连接状态
        /// </summary>
        /// <param name="Status">true= 连接，false=断开</param>
        public virtual void UpdateConnectStatus(bool Status)
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
                }
                else
                {
                    StopAutoTrade();
                    CancelCheck();
                    AutoLockBind(false);
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
        /// <summary>
        /// 订单列表是否加载完成
        /// </summary>
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
        /// <summary>
        /// 订单是否已执行标记
        /// </summary>
        public bool executedFlag = false;
        /// <summary>
        /// 跳次分析执行标记
        /// </summary>
        public bool isExecuting = false;
        /// <summary>
        /// V20261026_MARKETQUOTE_LOOP_GUARD：
        /// isExecuting 的"检查+置位"原子锁。MarketQuote（数据源线程）与 BroberQuote（平台行情线程）
        /// 两条线程都会走同一段跳次分析/下单逻辑，原来 `!isExecuting` 判断和 `isExecuting = true`
        /// 之间没有加锁，两条线程可能同时进入，重复下单。
        /// </summary>
        private readonly object _executingLock = new object();
        /// <summary>
        /// 事件跳次列表 _DSQuote.BidDiffForEventTrade 的读写锁。
        /// 该 List 同时被 MarketQuote（写入 + 遍历 + Clear）和 BroberQuote（遍历 + Clear）访问，
        /// 一边 foreach 一边被另一线程修改会抛 InvalidOperationException
        /// （Collection was modified），这是 MarketQuote 循环异常退出的主要原因之一。
        /// </summary>
        private readonly object _eventTradeLock = new object();

        private bool TryBeginExecuting()
        {
            lock (_executingLock)
            {
                if (isExecuting)
                {
                    return false;
                }
                isExecuting = true;
                return true;
            }
        }

        private void EndExecuting()
        {
            lock (_executingLock)
            {
                isExecuting = false;
            }
        }
        /// <summary>
        /// 交易订单是否执行完成，包括发送和接收订单消息
        /// </summary>
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
        public DateTime _PriceCount = DateTime.Now;

        /// <summary>
        /// EA2的公共交易参数，作为全局变量保存
        /// </summary>
        string _EA2_TradeRecord = "";

        /// <summary>
        /// 数据源行情获取并计算是否自动交易
        /// </summary>
        /// <summary>
        /// 数据源行情获取并计算是否自动交易
        ///
        /// V20261026_MARKETQUOTE_LOOP_GUARD：
        /// 本方法运行在 bgwMarketQuote（BackgroundWorker）线程上。原实现整个 while(true) 没有任何
        /// try/catch，循环体内任意一处抛异常（报文格式不对导致 split 越界 / double.Parse 失败、
        /// 跳次列表被另一线程并发修改、下单接口异常、跨线程操作控件异常等），异常都会直接冒出
        /// DoWork，BackgroundWorker 静默吞掉异常并结束——行情循环就此停止，界面上数据源仍显示
        /// "已连接"，也没有任何日志。且如果异常发生在 isExecuting = true 之后，isExecuting 永远
        /// 停在 true，即使重新启动循环，自动交易也再不会触发。
        ///
        /// 现在：
        ///   1. 每条行情的处理单独 try/catch，异常只记日志、丢弃该条行情，循环继续；
        ///   2. isExecuting 由 TryBeginExecuting/EndExecuting 原子置位，并在 finally 中复位；
        ///   3. 报文先校验字段数并用 TryParse 解析，坏报文直接跳过；
        ///   4. 跳次列表的写入/分析/清空统一在 _eventTradeLock 下进行；
        ///   5. 用带超时的 TryReceiveFrameString，使 CancelAsync 能真正结束循环；
        ///      只有套接字已被关闭/终止这类不可恢复异常才退出循环，并记录日志。
        /// </summary>
        public virtual void MarketQuote()
        {
            string PreReceiveMsg = "";
            string endpoint = _dsEndpoint;

            _dsWorkerStopped.Reset();
            _Log.LogInfo("[行情循环] 数据源行情循环启动，Endpoint=" + endpoint);

            try
            {
                // SubscriberSocket 的创建、Connect、Subscribe、Receive、Disconnect、Dispose
                // 全部在当前 BackgroundWorker 线程执行，避免 NetMQ socket 跨线程访问。
                _Subscriber = new SubscriberSocket();
                _Subscriber.Options.TcpKeepalive = true;
                _Subscriber.Options.TcpKeepaliveIdle = new TimeSpan(5, 0, 0);
                _Subscriber.Options.TcpKeepaliveInterval = new TimeSpan(0, 0, 1);
                _Subscriber.Connect(endpoint);
                _Subscriber.Subscribe("");

                _IsDSConnectFlag = true;
                RenderUI(buttonDSConnect, () =>
                {
                    buttonDSConnect.Text = "数据源断开";
                    buttonDSConnect.BackColor = Color.Green;
                    label_Price_Connect.BackColor = Color.Green;
                    label_DSName.Text = _Config.DSType;
                    label_Price_Connect.Text = "已连接";
                });
                _Log.LogInfo("[行情循环] 数据源连接成功: " + endpoint);

                while (true)
                {
                    if (_dsStopRequested || bgwMarketQuote.CancellationPending)
                    {
                        _Log.LogInfo("[行情循环] 收到取消请求，行情循环正常退出");
                        return;
                    }

                    string results;
                    try
                    {
                        if (!_Subscriber.TryReceiveFrameString(TimeSpan.FromMilliseconds(500), out results))
                        {
                            continue; // 超时无数据，回到循环顶部检查取消标记
                        }
                    }
                    catch (ObjectDisposedException ex)
                    {
                        _logger.Error("[行情循环] 数据源套接字已释放，行情循环退出: " + ex.Message);
                        _Log.LogInfo("[行情循环] 数据源套接字已释放，行情循环退出");
                        return;
                    }
                    catch (TerminatingException ex)
                    {
                        _logger.Error("[行情循环] NetMQ 上下文已终止，行情循环退出: " + ex.Message);
                        _Log.LogInfo("[行情循环] NetMQ 上下文已终止，行情循环退出");
                        return;
                    }
                    catch (Exception ex)
                    {
                        // 其它接收异常（例如短暂的网络问题）不退出循环，稍等后重试。
                        _logger.Error("[行情循环] 接收行情异常: " + ex.Message + "  " + ex.StackTrace);
                        Thread.Sleep(200);
                        continue;
                    }

                    if (string.IsNullOrEmpty(results) || results.Equals(PreReceiveMsg))
                    {
                        continue;
                    }
                    PreReceiveMsg = results;

                    try
                    {
                        _EA2_TradeRecord = results;
                        string[] split = results.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        //参数说明：Gold 1893.4 1893.6 145158
                        //_logger.Info("OnMarketQuote :" + split[0] + " " + split[1] + " " + split[2] + " " + split[3]);
                        //_Log.LogInfo("接收到消息时间："+DateTime.Now.ToString("HH:mm:ss.fff"));

                        // 报文校验：至少需要"品种 Bid Ask"三个字段，且价格可解析。
                        // 原来直接 split[1]/split[2] + double.Parse，坏报文会抛异常终止整个循环。
                        if (split.Length < 3 ||
                            !double.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double bid) ||
                            !double.TryParse(split[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double ask))
                        {
                            _logger.Warn("[行情循环] 忽略格式异常的行情报文: " + results);
                            continue;
                        }

                        _DSQuote.NewQuoute(split[0], bid, ask);
                        if (_TradePara.EventFlag)
                        {
                            lock (_eventTradeLock)
                            {
                                _DSQuote.NewQuoteForEventTrade(bid, _StrategyConfig.dataTimeDuration, _StrategyConfig.dataPriceDiffSize, _StrategyConfig.dataPriceDiffTotalCount);
                            }
                        }
                        //买多单，使用报价是Quote.Ask
                        //卖空单，使用报价是Quote.Bid
                        //买多平仓单，使用报价是Quote.Bid
                        //卖空平仓单，使用报价是Quote.A
                        //自动交易已启动并且上次执行已完成
                        if (_DSQuote.ValueChanged && _EnableAutoTrade &&
                            (chkBuyOpen.Checked || chkSellOpen.Checked || chkBuyClose.Checked || chkSellClose.Checked) &&
                            TryBeginExecuting())
                        {
                            try
                            {
                                int tradeType = -1;
                                //非事件策略
                                if (!_TradePara.EventFlag)
                                {
                                    //自动买多开仓
                                    if (chkBuyOpen.Checked && (_DSQuote.BidDiff[0] >= (double)_TradePara.BuyOpen))
                                    {
                                        tradeType = 1;
                                    }
                                    //自动卖空开仓
                                    if (chkSellOpen.Checked && (_DSQuote.BidDiff[0] <= (double)(-_TradePara.SellOpen)))
                                    {
                                        tradeType = 2;
                                    }
                                    //自动平仓
                                    if (chkBuyClose.Checked && _DSQuote.BidDiff[0] <= (double)-_TradePara.BuyClose)
                                    {
                                        // buy close
                                        tradeType = 3;
                                    }
                                    if (chkSellClose.Checked && _DSQuote.BidDiff[0] >= (double)_TradePara.SellClose)
                                    {
                                        //sell close
                                        tradeType = 4;
                                    }
                                }
                                //事件交易策略
                                if (_TradePara.EventFlag)
                                {
                                    lock (_eventTradeLock)
                                    {
                                        tradeType = EventAnalysis();
                                        if (tradeType > 0 && _DSQuote.BidDiffForEventTrade != null)
                                        {
                                            string PriceDiffList = string.Join(",", _DSQuote.BidDiffForEventTrade);
                                            _DSQuote.BidDiffForEventTrade.Clear();
                                            //_Log.LogInfo("事件单[tradeType=" + tradeType + "][" + PriceDiffList + "](1:BUY,2:SELL,3:CLOSE_BUY,4:CLOSE_SELL)");
                                        }
                                    }
                                }
                                //平台价格差
                                if (_TradePara.QuotaCheckFlag)
                                {
                                    if (string.Equals(_BrokerAndDSPriceDiffType, "1"))
                                    {
                                        tradeType = 1;
                                    }
                                    if (string.Equals(_BrokerAndDSPriceDiffType, "-1"))
                                    {
                                        tradeType = 2;
                                    }
                                }

                                //跳次分析完成，执行订单逻辑
                                //上次订单已执行完成（订单返回更新状态后才算订单执行完成）
                                //if (isOrderCompleted && tradeType != -1 && (DateTime.Now- priceCount).TotalSeconds >= 5 && TradeUtils.isTradeTime())
                                if (isOrderCompleted && tradeType != -1)
                                {
                                    //执行自动交易订单时，暂时取消自动交易功能，待订单完成后再回复自动交易                       
                                    isOrderCompleted = false;
                                    _IsAutoOperationFlag = true;
                                    _BrokerAndDSPriceDiffType = "0";
                                    //自动交易已启动，并且在锁仓情况下执行
                                    if (checkBox_AutoLock.Checked)
                                    {
                                        //_Log.LogInfo("执行自动订单开始");
                                        _OrderCreateType = "H";
                                        executedFlag = false;
                                        switch (_Platform.PlatformNo)
                                        {
                                            case (int)Comm.Enum.Platform.MF4:
                                                _MF4._OrderCreateType = _OrderCreateType;
                                                executedFlag = _MF4.AutoTradeTransaction2(tradeType, _IsAutoOperationFlag, out _SendTime);
                                                break;
                                            case (int)Comm.Enum.Platform.MT4:
                                                _MT4._OrderCreateType = _OrderCreateType;
                                                executedFlag = _MT4.AutoTradeTransaction2(tradeType, _IsAutoOperationFlag, out _SendTime);
                                                break;
                                            case (int)Comm.Enum.Platform.V4:
                                                _V4._OrderCreateType = _OrderCreateType;
                                                executedFlag = _V4.AutoTradeTransaction2(tradeType, _IsAutoOperationFlag, out _SendTime);
                                                break;
                                            case (int)Comm.Enum.Platform.MT5:
                                                _MT5._OrderCreateType = _OrderCreateType;
                                                executedFlag = _MT5.AutoTradeTransaction2(tradeType, _IsAutoOperationFlag, out _SendTime);
                                                break;
                                        }
                                        //发送订单完成后，停止自动交易，待订单更新后修改自动交易标识
                                        if (executedFlag)
                                        {
                                            isOrderCompleted = true;
                                            _Log.LogInfo("执行自动订单完成");
                                        }
                                        else
                                        {
                                            isOrderCompleted = true;
                                            _Log.LogInfo("无自动订单生成");
                                        }
                                    }
                                    //独立锁单,即不依赖于主窗口发送的锁单消息，只需要根据跳价自动进行锁单操作
                                    else if (checkBoxIndLock.Checked)
                                    {
                                        //_Log.LogInfo("执行独立锁单");
                                        _OrderCreateType = "C";
                                        switch (_Platform.PlatformNo)
                                        {
                                            case (int)Comm.Enum.Platform.MF4:
                                                _MF4._OrderCreateType = _OrderCreateType;
                                                break;
                                            case (int)Comm.Enum.Platform.MT4:
                                                _MT4._OrderCreateType = _OrderCreateType;
                                                break;
                                            case (int)Comm.Enum.Platform.V4:
                                                _V4._OrderCreateType = _OrderCreateType;
                                                break;
                                            case (int)Comm.Enum.Platform.MT5:
                                                _MT5._OrderCreateType = _OrderCreateType;
                                                break;
                                        }
                                        string AutoLockTradeSide = "";
                                        switch (tradeType)
                                        {
                                            case 1:
                                                AutoLockTradeSide = "SELL";
                                                break;
                                            case 2:
                                                AutoLockTradeSide = "BUY";
                                                break;
                                            case 3:
                                                //独立锁单平仓只能是单边操作，所以出现买多的时候就直接平多单，对应主窗口平空单
                                                AutoLockTradeSide = "SELL_CLOSE";
                                                break;
                                            case 4:
                                                //独立锁单平仓只能是单边操作，所以出现买空的时候就直接平空单，对应主窗口平多单
                                                AutoLockTradeSide = "BUY_CLOSE";
                                                break;
                                        }
                                        executedFlag = AutoLockProcess(AutoLockTradeSide);
                                        //发送订单完成后，停止自动交易，待订单更新后修改自动交易标识
                                        if (!executedFlag)
                                        {
                                            isOrderCompleted = true;
                                            _Log.LogInfo("无独立锁单生成");
                                        }
                                    }
                                    else
                                    {
                                        _Log.LogInfo("未选择消息锁单或独立锁单，不执行自动订单操作");
                                        isOrderCompleted = true;
                                    }
                                    //取消勾选，不用停止自动交易
                                    CancelCheck();
                                    _PriceCount = DateTime.Now;
                                }
                                // V20261022_EVENT_KEEP_CHECK：事件模式由 EventAnalysis 判断开平仓。
                                // CancelCheckForSelected 使用的是非事件模式的倒平阈值（BuyClose/SellClose），
                                // 第一个达到阈值的跳次（如 -6）就会取消全部勾选，导致后续跳次凑齐
                                // （如 [-5,-6] 满足 2,5,2,2）时勾选已被清空、总开关进不去，事件单永远无法触发。
                                // 因此事件模式下不再调用。
                                if (!_TradePara.EventFlag)
                                {
                                    CancelCheckForSelected();
                                }
                            }
                            catch (Exception ex)
                            {
                                // 下单过程异常：恢复订单完成标记，避免后续永远不再下单。
                                isOrderCompleted = true;
                                _logger.Error("[行情循环] 跳次分析/下单异常: " + ex.Message + "  " + ex.StackTrace);
                                _Log.LogInfo("[行情循环] 跳次分析/下单异常: " + ex.Message);
                            }
                            finally
                            {
                                EndExecuting();
                            }
                        }
                        if (_TradePara.EventFlag)
                        {
                            if (_DSQuote._EventValuehanged)
                            {
                                OutputDiffList();
                                Task.Run(() =>
                                {
                                    if ((_MT4 != null && _MT4.isConnect()) || (_MT5 != null && _MT5.isConnect()))
                                    {
                                        UpdateBrokerAndDSPriceDiff();
                                        BrokerPriceBlockAlarm();
                                    }
                                });
                            }
                        }
                        else
                        {
                            if (_DSQuote.ValueChanged)
                            {
                                OutputDiffList();
                                Task.Run(() =>
                                {
                                    if ((_MT4 != null && _MT4.isConnect()) || (_MT5 != null && _MT5.isConnect()))
                                    {
                                        UpdateBrokerAndDSPriceDiff();
                                        BrokerPriceBlockAlarm();
                                    }
                                });
                            }
                        }
                        UpdateDSQuote();
                    }
                    catch (Exception ex)
                    {
                        // 单条行情处理失败只丢弃该条，不能让整个行情循环退出。
                        _logger.Error("[行情循环] 处理行情异常，已跳过该条行情: [" + results + "] " + ex.Message + "  " + ex.StackTrace);
                        _Log.LogInfo("[行情循环] 处理行情异常，已跳过: " + ex.Message);
                    }
                    //Thread.Sleep(3);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("[行情循环] 数据源连接/接收线程异常: " + ex.Message + "  " + ex.StackTrace);
                try { _Log.LogInfo("[行情循环] 数据源连接/接收异常: " + ex.Message); } catch { }
            }
            finally
            {
                try
                {
                    if (_Subscriber != null)
                    {
                        try { _Subscriber.Unsubscribe(""); } catch { }
                        try { _Subscriber.Disconnect(endpoint); } catch { }
                        try { _Subscriber.Dispose(); } catch { }
                        _Subscriber = null;
                    }
                }
                finally
                {
                    _IsDSConnectFlag = false;
                    _dsWorkerStopped.Set();
                    try
                    {
                        RenderUI(buttonDSConnect, () =>
                        {
                            buttonDSConnect.Text = "数据源连接";
                            buttonDSConnect.BackColor = Color.Red;
                            label_Price_Connect.BackColor = Color.Red;
                            label_Price_Connect.Text = "断开";
                        });
                    }
                    catch { }
                    _Log.LogInfo("[行情循环] 数据源行情循环已结束");
                }
            }
        }
        /// <summary>
        /// 事件策略分析
        /// 返回TradeType
        /// </summary>
        /// <returns></returns>
        public int EventAnalysis()
        {
            int tradeType = -1;
            // V20261026_MARKETQUOTE_LOOP_GUARD：遍历快照副本，原列表可能正被另一线程修改。
            List<double> priceDiffList;
            lock (_eventTradeLock)
            {
                priceDiffList = _DSQuote.BidDiffForEventTrade == null
                    ? new List<double>()
                    : new List<double>(_DSQuote.BidDiffForEventTrade);
            }
            if (priceDiffList.Count >= _StrategyConfig.dataPriceDiffTotalCount)
            {
                int dataPricePositive = 0;
                int dataPriceNegative = 0;
                foreach (double priceDiff in priceDiffList)
                {
                    if (priceDiff > 0)
                    {
                        dataPricePositive++;
                    }
                    if (priceDiff < 0)
                    {
                        dataPriceNegative++;
                    }
                }
                //自动买多开仓
                if (chkBuyOpen.Checked && dataPricePositive >= _StrategyConfig.dataPricePositiveCount)
                {
                    tradeType = 1;
                }
                //自动卖空开仓
                if (chkSellOpen.Checked && dataPriceNegative >= _StrategyConfig.dataPricePositiveCount)
                {
                    tradeType = 2;
                }
                //自动平仓
                // V20261026_EVENT_CLOSE_DIRECTION_FIX：平仓方向与非事件模式保持一致——
                //   非事件模式：BidDiff[0] <= -BuyClose（下跌）才平多单；BidDiff[0] >= SellClose（上涨）才平空单。
                // 原代码把方向写反了：连续上涨触发平多单、连续下跌触发平空单，
                // 导致出现 +6 等上涨跳次时执行了"自动买平"。
                //买多平仓：连续下跌
                if (chkBuyClose.Checked && dataPriceNegative >= _StrategyConfig.dataPricePositiveCount)
                {
                    // buy close
                    tradeType = 3;
                }
                //卖空平仓：连续上涨
                if (chkSellClose.Checked && dataPricePositive >= _StrategyConfig.dataPricePositiveCount)
                {
                    //sell close
                    tradeType = 4;
                }

            }
            return tradeType;
        }
        /// <summary>
        /// 判断是否已经超过平仓时间,默认是平仓后2分钟才能开仓
        /// </summary>
        /// <returns></returns>
        public bool IsOverOpenOrderTime()
        {
            if (_CloseOrderTime == null)
            {
                return true;
            }
            DateTime currentTime = DateTime.Now;
            // 计算当前时间和给定时间的差距
            TimeSpan timeDifference = currentTime - _CloseOrderTime;
            int t = TradeUtils.GetCloseTimeDuration(_TradePara.AutoCloseTimeDuration);
            // 判断差距是否大于2分钟
            if (timeDifference.TotalMinutes > t)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 实时监测数据源变化，根据给定规则启动自动交易
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void bgwMarketQuote_DoWork(object sender, DoWorkEventArgs e)
        {
            MarketQuote();
        }
        /// <summary>
        /// 取消自动交易选项
        /// 为减少行情大波动平仓异常，减少损失
        /// </summary>
        public void CancelCheckForSelected()
        {
            if (chkBuyClose.Checked && _DSQuote.BidDiff[0] <= (double)-_TradePara.BuyClose ||
                chkSellClose.Checked && _DSQuote.BidDiff[0] >= (double)_TradePara.SellClose)
            {
                CancelCheck();
            }
        }
        /// <summary>
        /// 更新跳次小黑板数据
        /// </summary>
        public void OutputDiffList()
        {
            if (_TradePara.EventFlag)
            {
                string result = "";
                lock (_eventTradeLock)
                {
                    if (_DSQuote.BidDiffForEventTrade != null && _DSQuote.BidDiffForEventTrade.Count > 0)
                    {
                        result = string.Join(",", _DSQuote.BidDiffForEventTrade);
                    }
                }
                if (result.Length > 0)
                {
                    string BidDiffMsg = DateTime.Now.ToString("HH:mm:ss") + " " + result;
                    WriteBiddViewList(BidDiffMsg);
                }
            }
            else
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
        }

        /// <summary>
        /// 取消自动交易选中框
        /// </summary>
        public void CancelCheck()
        {
            chkBuyOpen.Checked = false;
            chkSellOpen.Checked = false;
            chkSellClose.Checked = false;
            chkBuyClose.Checked = false;
        }
        /// <summary>
        /// 保存平台配置信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void button1_Click(object sender, EventArgs e)
        {
            SaveConfig();
            _Log.LogInfo("信息保存成功");
        }
        public void comboBox_Broker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_IsLoadConfigCompleted)
            {
                string brokerName = comboBox_Broker.SelectedItem.ToString();
                uClient.Comm.Broker bb = _Platform.GetBrokerByName(brokerName);
                _Platform.DefaultBrokerCode = bb.BrokerCode;
                _Broker = bb;
                _Account = _Broker.DefaultAccountType == "Demo" ? _Broker.DemoAccount : _Broker.LiveAccount;
                _TradeSymbol = _Broker.DefaultSymbol;
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
                    //锁单设置
                    comboBoxAutoLockTime.SelectedItem = _TradePara.AutoLockTimeDuration;
                    comboBoxLockPoint.SelectedItem = _TradePara.AutoLockPoint;
                    checkBoxIndLock.Checked = _TradePara.IndLockFlag;
                    checkBox_QuotaCheck.Checked = _TradePara.QuotaCheckFlag;
                    //平台设置
                    comboBoxCloseTimeDuration.SelectedItem = _TradePara.AutoCloseTimeDuration;
                    comboBox_Slippage.SelectedItem = _TradePara.Slippage;
                    checkBoxAutoChecked.Checked = _TradePara.AutoChecked;
                    checkBoxNotify.Checked = _TradePara.NotifyFlag;
                    checkboxTimezone.Text = _TradePara.Timezone.ToString();
                    checkBox_Event.Checked = _TradePara.EventFlag;
                    //平台基础信息
                    textBoxBrokerCode.Text = _Broker.BrokerCode;
                    textBoxBrokerName.Text = _Broker.BrokerName;
                    if (_Broker.Symbol != null)
                    {
                        textBoxTestSymbol.Text = string.Join(",", _Broker.Symbol);
                    }
                    textBoxTestIP.Text = _Broker.DemoIP + "," + _Broker.LiveIP;
                    textBoxTestPort.Text = _Broker.DemoPort + "," + _Broker.LivePort;
                    textBoxTZ.Text = _TradePara.Timezone.ToString();
                    if (_Broker.PlatformNo == 2)
                    {
                        comboBox_PlatFormType.SelectedItem = "MT4";
                    }
                    if (_Broker.PlatformNo == 4)
                    {
                        comboBox_PlatFormType.SelectedItem = "MT5";
                    }
                    if (_Broker.PlatformNo == 0)
                    {
                        comboBox_PlatFormType.SelectedItem = "MT4";
                    }
                }
                if (_TradePara != null)
                {
                    //自动交易参数
                    nudSellOpen.Value = _TradePara.SellOpen;
                    nudSellClose.Value = _TradePara.SellClose;
                    nudSellLots.Value = _TradePara.SellLots;
                    nudBuyOpen.Value = _TradePara.BuyOpen;
                    nudBuyClose.Value = _TradePara.BuyClose;
                    nudBuyLots.Value = _TradePara.BuyLots;
                }
                //停止按钮颜色改变事件
                if (_Button_ChangeColor_Timer_TradePara != null)
                {
                    _Button_ChangeColor_Timer_TradePara.Stop();
                    _Button_ChangeColor_Timer_TradePara.Enabled = false;
                    button_TradePara_Save.BackColor = Color.White;
                }
                //加载了默认的平台信息,需要重新加载平台对象
                SaveConfig();
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
                textBox_UserCode.Text = _Account.UserCode;
                textBox_Password.Text = _Account.Password;
                //修改了账户信息,需要保存并从新加载平台数据
                SaveConfig();
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
            _Log.LogInfo("保存交易参数成功");
        }
        public void button_TradePara_Cancel_Click(object sender, EventArgs e)
        {
            nudSellOpen.Value = _TradePara.SellOpen;
            nudSellClose.Value = _TradePara.SellClose;
            nudSellLots.Value = _TradePara.SellLots;
            nudBuyOpen.Value = _TradePara.BuyOpen;
            nudBuyClose.Value = _TradePara.BuyClose;
            nudBuyLots.Value = _TradePara.BuyLots;
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
            if (e.Index < 0 || e.Index >= listBox1.Items.Count)
            {
                return;
            }
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
        /// 自动绑定锁单消息,在平台连接完成后自动启动
        /// </summary>
        public void AutoLockBind(bool isActive)
        {
            string port = uClient.Comm.Utils.GetPort(_MainFormPara.FormNo);
            string address = string.Format("tcp://{0}:{1}", uClient.Comm.Utils.GetLocalIP(), port);
            if (isActive)
            {
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
            bgwAutoLock.CancelAsync();
            bgwMarketQuote.CancelAsync();
            bgwOrderUpdate.CancelAsync();
            bgwBroberQuote.CancelAsync();
        }
        /// <summary>
        /// 定时刷新账号、订单信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timerAccountPosionRefresh_Tick(object sender, EventArgs e)
        {
            if (_Config != null && _Platform != null && _IsLoadConfigCompleted)
            {
                switch (_Platform.PlatformNo)
                {
                    case (int)Comm.Enum.Platform.MF4:
                        if (_MF4 != null && _MF4.isConnect())
                        {
                            if (_DataGridViewInitCompleteFlag)
                            {
                                _MF4.UpdateAccountCaptial();
                                _MF4.UpdatePositionGrid();
                            }
                        }
                        break;
                    case (int)Comm.Enum.Platform.MT4:
                        if (_MT4 != null && _MT4.isConnect())
                        {
                            if (_DataGridViewInitCompleteFlag)
                            {
                                _MT4.UpdateAccountCaptial();
                                _MT4.UpdatePositionGrid();
                            }
                        }
                        break;
                    case (int)Comm.Enum.Platform.V4:
                        if (_V4 != null && _V4.isConnect())
                        {
                            if (_DataGridViewInitCompleteFlag)
                            {
                                //Task.Run(() => {
                                //    _V4.TradingData1();
                                //});
                                Task.Run(() =>
                                {
                                    _V4.UpdateAccountCaptial();
                                    _V4.UpdatePositionGrid();
                                });
                            }
                        }
                        break;
                    case (int)Comm.Enum.Platform.MT5:
                        if (_MT5 != null && _MT5.isConnect())
                        {
                            if (_DataGridViewInitCompleteFlag)
                            {
                                _MT5.UpdateAccountCaptial();
                                _MT5.UpdatePositionGrid();
                            }
                        }
                        break;
                }
            }
        }
        /// <summary>
        /// 当前订单类型
        /// </summary>
        public string _CurrentOrderTradeType = "";

        /// <summary>
        /// 平仓时间,确保开平仓时间
        /// </summary>
        public DateTime _CloseOrderTime;

        /// <summary>
        /// 订单状态更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProcessMT5OrderUpdate()
        {
            if (_MT5 == null || _MT5._OrderUpdateEventArgs == null) return;
            bool isTerminal = _MT5._OrderUpdateEventArgs.Type == UpdateType.MarketOpen ||
                              _MT5._OrderUpdateEventArgs.Type == UpdateType.MarketClose;
            string orderId = GetLockOrderId(_LockReceiveMsg);
            if (isTerminal) _LockReceiveMsg = "";
            _MT5.OrderUpdate(_IsAutoOperationFlag, _SendTime, orderId);
            if (_MT5._OrderUpdateEventArgs != null)
            {
                mtapi.mt5.Order order = _MT5._OrderUpdateEventArgs.Order;
                if (order == null)
                {
                    _Log.LogInfo("[MT5][ORDER_UPDATE_NO_ORDER] type=" + _MT5._OrderUpdateEventArgs.Type);
                    return;
                }
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
                        // PendingOpen/PendingClose are nonterminal. Do not stop trading
                        // or mark the request complete merely because progress was reported.
                        _Log.LogInfo("[MT5][ORDER_INTERMEDIATE] type=" + _MT5._OrderUpdateEventArgs.Type);
                        break;
                }
            }
            if (_MT5._OrderUpdateEventArgs.Type == UpdateType.MarketOpen ||
                _MT5._OrderUpdateEventArgs.Type == UpdateType.MarketClose)
            {
                isOrderCompleted = true;
                _Log.LogInfo("[MT5][ORDER_TERMINAL] 执行订单结束");
            }
        }

        public void bgwOrderUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            // MT5 must consume every callback, including those received while the worker is busy.
            if (_Platform != null && _Platform.PlatformNo == (int)Comm.Enum.Platform.MT5)
            {
                mtapi.mt5.OrderUpdate nextUpdate;
                while (_MT5 != null && _MT5.TryDequeueOrderUpdate(out nextUpdate))
                {
                    _MT5._OrderUpdateEventArgs = nextUpdate;
                    try { ProcessMT5OrderUpdate(); }
                    catch (Exception ex)
                    {
                        _logger.Error("[MT5][ORDER_UPDATE_FAILED] " + ex);
                        _Log.LogInfo("[MT5][ORDER_UPDATE_FAILED] " + ex.Message);
                    }
                }
                return;
            }
            _CloseOrderTime = DateTime.Now;
            string orderId = GetLockOrderId(_LockReceiveMsg);
            _LockReceiveMsg = "";
            //_Log.LogInfo("更新订单状态:IsAuto:" + _IsAutoOperationFlag +",OrderType:" + _OrderCreateType +",OnlyClose:"+ _OnlyCloseOrder);
            switch (_Platform.PlatformNo)
            {
                case (int)Comm.Enum.Platform.MF4:
                    if (_MF4._SystemMessage != null)
                    {
                        _MF4.OrderUpdate(_IsAutoOperationFlag, _SendTime, orderId);
                        Dictionary<string, object> dic = uClient.Comm.Utils.GetSystemMessageReceived(_MF4._SystemMessage);
                        string orderType = _MF4._SystemMessage.Title;
                        string tradeSide = dic["TradeSide"].ToString();
                        if (_IsAutoOperationFlag)
                        {
                            //开仓被接纳
                            if (Comm.Enum.MarketOrderStatus.Accepted.Contains(orderType))
                            {
                                string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}|{5}", Comm.Enum.Buy.Contains(tradeSide) ? "SELL" : "BUY", dic["Lots"], dic["Price"],
                                         Comm.Enum.Buy.Contains(tradeSide) ? _TradePara.BuyOpen : _TradePara.SellOpen, _TradePara.OnlyCloseFlag ? "OCO" : "CO", dic["ServerPositionRef"]);
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
                                if (positionmf != null)
                                {
                                    string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}|{5}", positionmf.BuySell == TradeSide.Buy ? "SELL_CLOSE" : "BUY_CLOSE", positionmf.Lot, positionmf.OpenPrice,
                                          positionmf.BuySell == TradeSide.Buy ? _TradePara.BuyOpen : _TradePara.SellOpen, _TradePara.OnlyCloseFlag ? "OCO" : "CO", dic["ServerPositionRef"]);
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
                                        //锁单窗口，判断是否是只平不开的订单
                                        if (_OnlyCloseOrder)
                                        {
                                            StartAutoTrade();
                                        }
                                        else
                                        {
                                            StopAutoTrade();
                                        }
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
                    TradingAPI.MT4Server.OrderProgressEventArgs mt4Progress;
                    while (_MT4 != null && _MT4.TryDequeueOrderProgress(out mt4Progress))
                    {
                        _MT4._OrderProgressEventArgs = mt4Progress;
                        try
                        {
                            if (mt4Progress.Exception != null || mt4Progress.Order == null)
                            {
                                _Log.LogInfo("[MT4][ORDER_CALLBACK] " + mt4Progress.Type +
                                    " exception=" + (mt4Progress.Exception == null ? "none" : mt4Progress.Exception.Message));
                                continue;
                            }
                            _MT4.OrderUpdate(_IsAutoOperationFlag, _SendTime, orderId);
                            Order order = _MT4._OrderProgressEventArgs.Order;
                            switch (_MT4._OrderProgressEventArgs.Type)
                            {
                                case TradingAPI.MT4Server.ProgressType.Opened:
                                    //发送锁单消息
                                    //参数说明M|BUY/SELL/SELL_CLOSE/BUY_CLOSE|Lot|Price|Point
                                    if (_IsAutoOperationFlag)
                                    {
                                        string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}|{5}", order.Type == Op.Buy ? "SELL" : "BUY", order.Lots, order.OpenPrice,
                                            order.Type == Op.Buy ? _TradePara.BuyOpen : _TradePara.SellOpen, _TradePara.OnlyCloseFlag ? "OCO" : "CO", order.Ticket);
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
                                        string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}|{5}", order.Type == Op.Buy ? "SELL_CLOSE" : "BUY_CLOSE", order.Lots, order.OpenPrice,
                                            order.Type == Op.Buy ? _TradePara.BuyClose : _TradePara.SellClose, _TradePara.OnlyCloseFlag ? "OCO" : "CO", order.Ticket);
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
                                    _Log.LogInfo("[异常参数][_OrderProgressEventArgs.Type=" + _MT4._OrderProgressEventArgs.Type + "]");
                                    StopAutoTrade();
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("[MT4][ORDER_UPDATE_FAILED] " + ex);
                            _Log.LogInfo("[MT4][ORDER_UPDATE_FAILED] " + ex.Message);
                        }
                    }
                    break;
                case (int)Comm.Enum.Platform.V4:
                    if (_V4._OrderProgressEvent != null)
                    {
                        _V4.OrderUpdate(_IsAutoOperationFlag, _SendTime, orderId);
                        uClient.Comm.Position positionv4 = _V4._OrderProgressEvent.postion;
                        switch (_V4._OrderProgressEvent.Type)
                        {
                            case Comm.ProgressType.Opened:
                                //发送锁单消息
                                //参数说明M|BUY/SELL/SELL_CLOSE_/BUY_CLOSE|Lot|Price|Point
                                if (_IsAutoOperationFlag)
                                {
                                    string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}|{5}", Comm.Enum.Buy.Contains(positionv4.BuySell) ? "SELL" : "BUY", positionv4.Lot, positionv4.OpenPrice,
                                         Comm.Enum.Buy.Contains(positionv4.BuySell) ? _TradePara.BuyOpen : _TradePara.SellOpen, _TradePara.OnlyCloseFlag ? "OCO" : "CO", orderId);
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
                                    string sendMessage = string.Format("M|{0}|{1}|{2}|{3}|{4}|{5}", Comm.Enum.Buy.Contains(positionv4.BuySell) ? "SELL_CLOSE" : "BUY_CLOSE", positionv4.Lot, positionv4.OpenPrice,
                                        Comm.Enum.Buy.Contains(positionv4.BuySell) ? _TradePara.BuyClose : _TradePara.SellClose, _TradePara.OnlyCloseFlag ? "OCO" : "CO", orderId);
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
                                _Log.LogInfo("[异常参数][_OrderProgressEventArgs.Type=" + _MT4._OrderProgressEventArgs.Type + "]");
                                StopAutoTrade();
                                break;
                        }
                    }
                    //_V4.UpdateAccountCaptial();
                    //_V4.UpdatePositionGrid();
                    break;
                case (int)Comm.Enum.Platform.MT5:
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
                        /*if (_TradePara.EventFlag)
                        {
                            _DSQuote.NewQuoteForEventTrade((double)_MF4._QuoteUpdatedEventArgs.Quote.BidPrice, _StrategyConfig.dataTimeDuration, _StrategyConfig.dataPriceDiffSize, _StrategyConfig.dataPriceDiffTotalCount);
                        }*/
                    }
                    break;
                case (int)Comm.Enum.Platform.MT4:
                    if (_MT4._QuoteEventArgs.Symbol == _TradeSymbol)
                    {
                        _MT4.NewQuote();
                        _MT4.UpdateQuote();
                        /*if (_TradePara.EventFlag)
                        {
                            _DSQuote.NewQuoteForEventTrade(_MT4._QuoteEventArgs.Bid, _StrategyConfig.dataTimeDuration, _StrategyConfig.dataPriceDiffSize, _StrategyConfig.dataPriceDiffTotalCount);
                        }*/
                    }
                    break;
                case (int)Comm.Enum.Platform.V4:
                    if (_V4._Quote != null && _V4._Quote.Symbol == _TradeSymbol)
                    {
                        _V4.NewQuote();
                        _V4.UpdateQuote();
                    }
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    if (_MT5._QuoteEventArgs.Symbol == _TradeSymbol)
                    {
                        _MT5.NewQuote();
                        _MT5.UpdateQuote();
                        /*if (_TradePara.EventFlag)
                        {
                            _DSQuote.NewQuoteForEventTrade(_MT5._QuoteEventArgs.Bid, _StrategyConfig.dataTimeDuration, _StrategyConfig.dataPriceDiffSize, _StrategyConfig.dataPriceDiffTotalCount);
                        }*/
                    }
                    break;
            }
            //买多单，使用报价是Quote.Ask
            //卖空单，使用报价是Quote.Bid
            //买多平仓单，使用报价是Quote.Bid
            //卖空平仓单，使用报价是Quote.A
            //自动交易已启动并且上次执行已完成
            if (_TradePara.EventFlag && _DSQuote._EventValuehanged && _EnableAutoTrade &&
                (chkBuyOpen.Checked || chkSellOpen.Checked || chkBuyClose.Checked || chkSellClose.Checked) &&
                TryBeginExecuting())
            {
                try
                {
                    int tradeType = -1;
                    //事件交易策略
                    if (_TradePara.EventFlag)
                    {
                        lock (_eventTradeLock)
                        {
                            tradeType = EventAnalysis();
                            if (tradeType > 0 && _DSQuote.BidDiffForEventTrade != null)
                            {
                                string PriceDiffList = string.Join(",", _DSQuote.BidDiffForEventTrade);
                                _DSQuote.BidDiffForEventTrade.Clear();
                                //_Log.LogInfo("事件单[tradeType=" + tradeType + "][" + PriceDiffList + "](1:BUY,2:SELL,3:CLOSE_BUY,4:CLOSE_SELL)");
                            }
                        }
                    }

                    //跳次分析完成，执行订单逻辑
                    //上次订单已执行完成（订单返回更新状态后才算订单执行完成）
                    //if (isOrderCompleted && tradeType != -1 && (DateTime.Now- priceCount).TotalSeconds >= 5 && TradeUtils.isTradeTime())
                    if (isOrderCompleted && tradeType != -1)
                    {
                        //执行自动交易订单时，暂时取消自动交易功能，待订单完成后再回复自动交易                       
                        isOrderCompleted = false;
                        _IsAutoOperationFlag = true;
                        _BrokerAndDSPriceDiffType = "0";
                        //自动交易已启动，并且在锁仓情况下执行
                        if (checkBox_AutoLock.Checked)
                        {
                            //_Log.LogInfo("执行自动订单开始");
                            _OrderCreateType = "H";
                            executedFlag = false;
                            switch (_Platform.PlatformNo)
                            {
                                case (int)Comm.Enum.Platform.MF4:
                                    _MF4._OrderCreateType = _OrderCreateType;
                                    executedFlag = _MF4.AutoTradeTransaction2(tradeType, _IsAutoOperationFlag, out _SendTime);
                                    break;
                                case (int)Comm.Enum.Platform.MT4:
                                    _MT4._OrderCreateType = _OrderCreateType;
                                    executedFlag = _MT4.AutoTradeTransaction2(tradeType, _IsAutoOperationFlag, out _SendTime);
                                    break;
                                case (int)Comm.Enum.Platform.V4:
                                    _V4._OrderCreateType = _OrderCreateType;
                                    executedFlag = _V4.AutoTradeTransaction2(tradeType, _IsAutoOperationFlag, out _SendTime);
                                    break;
                                case (int)Comm.Enum.Platform.MT5:
                                    _MT5._OrderCreateType = _OrderCreateType;
                                    executedFlag = _MT5.AutoTradeTransaction2(tradeType, _IsAutoOperationFlag, out _SendTime);
                                    break;
                            }
                            //发送订单完成后，停止自动交易，待订单更新后修改自动交易标识
                            if (executedFlag)
                            {
                                isOrderCompleted = true;
                                _Log.LogInfo("执行自动订单完成");
                            }
                            else
                            {
                                isOrderCompleted = true;
                                _Log.LogInfo("无自动订单生成");
                            }
                        }
                        //独立锁单,即不依赖于主窗口发送的锁单消息，只需要根据跳价自动进行锁单操作
                        else if (checkBoxIndLock.Checked)
                        {
                            //_Log.LogInfo("执行独立锁单");
                            _OrderCreateType = "C";
                            switch (_Platform.PlatformNo)
                            {
                                case (int)Comm.Enum.Platform.MF4:
                                    _MF4._OrderCreateType = _OrderCreateType;
                                    break;
                                case (int)Comm.Enum.Platform.MT4:
                                    _MT4._OrderCreateType = _OrderCreateType;
                                    break;
                                case (int)Comm.Enum.Platform.V4:
                                    _V4._OrderCreateType = _OrderCreateType;
                                    break;
                                case (int)Comm.Enum.Platform.MT5:
                                    _MT5._OrderCreateType = _OrderCreateType;
                                    break;
                            }
                            string AutoLockTradeSide = "";
                            switch (tradeType)
                            {
                                case 1:
                                    AutoLockTradeSide = "SELL";
                                    break;
                                case 2:
                                    AutoLockTradeSide = "BUY";
                                    break;
                                case 3:
                                    //独立锁单平仓只能是单边操作，所以出现买多的时候就直接平多单，对应主窗口平空单
                                    AutoLockTradeSide = "SELL_CLOSE";
                                    break;
                                case 4:
                                    //独立锁单平仓只能是单边操作，所以出现买空的时候就直接平空单，对应主窗口平多单
                                    AutoLockTradeSide = "BUY_CLOSE";
                                    break;
                            }
                            executedFlag = AutoLockProcess(AutoLockTradeSide);
                            //发送订单完成后，停止自动交易，待订单更新后修改自动交易标识
                            if (!executedFlag)
                            {
                                isOrderCompleted = true;
                                _Log.LogInfo("无独立锁单生成");
                            }
                        }
                        else
                        {
                            _Log.LogInfo("未选择消息锁单或独立锁单，不执行自动订单操作");
                            isOrderCompleted = true;
                        }
                        //取消勾选，不用停止自动交易
                        CancelCheck();
                        _PriceCount = DateTime.Now;
                    }
                    // V20261022_EVENT_KEEP_CHECK：事件模式由 EventAnalysis 判断开平仓。
                    // CancelCheckForSelected 使用的是非事件模式的倒平阈值（BuyClose/SellClose），
                    // 第一个达到阈值的跳次（如 -6）就会取消全部勾选，导致后续跳次凑齐
                    // （如 [-5,-6] 满足 2,5,2,2）时勾选已被清空、总开关进不去，事件单永远无法触发。
                    // 因此事件模式下不再调用。
                    if (!_TradePara.EventFlag)
                    {
                        CancelCheckForSelected();
                    }
                }
                catch (Exception ex)
                {
                    isOrderCompleted = true;
                    _logger.Error("[平台行情] 跳次分析/下单异常: " + ex.Message + "  " + ex.StackTrace);
                    _Log.LogInfo("[平台行情] 跳次分析/下单异常: " + ex.Message);
                }
                finally
                {
                    EndExecuting();
                }
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
        /// 锁单消息
        /// </summary>
        public string _LockReceiveMsg = "";
        /// <summary>
        /// 自动锁仓
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void bgwAutoLock_DoWork(object sender, DoWorkEventArgs e)
        {
            DateTime ts = DateTime.Now;
            //接收锁单信息
            while (true)
            {
                string automsg = _LockSubscriber.ReceiveFrameString();
                if (!string.IsNullOrEmpty(automsg))
                {
                    if (automsg.Equals(_LockReceiveMsg))
                    {
                        //Thread.Sleep(10);
                        continue;
                    }
                    else
                    {
                        _LockReceiveMsg = automsg;
                        isOrderCompleted = false;
                        _OrderCreateType = "C";
                        switch (_Platform.PlatformNo)
                        {
                            case (int)Comm.Enum.Platform.MF4:
                                _MF4._OrderCreateType = _OrderCreateType;
                                break;
                            case (int)Comm.Enum.Platform.MT4:
                                _MT4._OrderCreateType = _OrderCreateType;
                                break;
                            case (int)Comm.Enum.Platform.V4:
                                _V4._OrderCreateType = _OrderCreateType;
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                _MT5._OrderCreateType = _OrderCreateType;
                                break;
                        }
                        _IsAutoOperationFlag = true;
                        _OnlyCloseOrder = false;
                        CancelCheck();
                    }
                    _Log.LogInfo("接收自动锁单消息:" + _LockReceiveMsg);
                    string[] splitmsg = _LockReceiveMsg.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    //参数说明M|BUY/SELL/SELL_CLOSE/BUY_CLOSE|Lot|Price|Point|OnlyCloseFlag|OrderId
                    //只有在锁单价格范围内才能进行锁单操作
                    //20210503:锁单逻辑：超过锁单时间后，按照设置的点数和手数锁单
                    string AutoLockTradeSide = splitmsg[1];
                    //该订单是否为只平不开订单
                    //true: H端停止自动交易,C端启动自动交易
                    //false:H端启动自动交易,C端停止自动交易
                    _OnlyCloseOrder = string.Equals(splitmsg[5], "OCO");
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
        /// 获取锁单OrderId
        /// </summary>
        /// <param name="LockReceiveMsg"></param>
        /// <returns></returns>
        public string GetLockOrderId(string LockReceiveMsg)
        {
            if (!string.IsNullOrEmpty(LockReceiveMsg) && LockReceiveMsg.Contains("|"))
            {
                string[] splitmsg = LockReceiveMsg.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitmsg.Length == 7)
                {
                    return splitmsg[6];
                }
            }
            return "";
        }

        /// <summary>
        /// 自动锁单，开仓前都会检查是否有订单
        /// </summary>
        /// <param name="AutoLockTradeSide"></param>
        /// <returns></returns>
        public bool AutoLockProcess(string AutoLockTradeSide)
        {
            bool mexecutedFlag = false;
            _Log.LogInfo("启动锁单,设置时长:[" + _TradePara.AutoLockTimeDuration + "s]点数:[" + _TradePara.AutoLockPoint + "]");
            DateTime currentTime = DateTime.Now;
            int timelogcount = 0;
            while (true)
            {
                //_Log.LogInfo((double)_Config.TradePara.AutoLockPoint+";"+ price+";"+ Convert.ToDouble(splitmsg[3])+";"+ Convert.ToDouble(splitmsg[4])+";"+ splitmsg[1]);
                //bool IsAutoLock = TradeUtils.GetAutoLockPrice((double)_Config.TradePara.AutoLockPoint, CurrentPrice, AutoLockPrice, AutoLockPoint, AutoLockTradeSide);
                double delayTime = (DateTime.Now - currentTime).TotalMilliseconds;
                if (delayTime >= (double)_TradePara.AutoLockTimeDuration * 1000 || _TradePara.AutoLockTimeDuration == 0)
                {
                    if ("BUY".Equals(AutoLockTradeSide) && (_DSQuote.BidDiff[0] >= (double)_TradePara.AutoLockPoint || _TradePara.AutoLockPoint == 0))
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
                            case (int)Comm.Enum.Platform.V4:
                                _V4._OrderCreateType = _OrderCreateType;
                                mexecutedFlag = _V4.AutoTradeTransaction2(1, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                _MT5._OrderCreateType = _OrderCreateType;
                                mexecutedFlag = _MT5.AutoTradeTransaction2(1, true, out _SendTime);
                                break;
                        }
                        if (mexecutedFlag)
                        {
                            _Log.LogInfo("自动锁单[BUY]手数:[" + _TradePara.BuyLots + "]跳次:[" + _DSQuote.BidDiff[0] + "]延时:[" + Convert.ToInt32(delayTime) + "ms]");
                        }
                        break;
                    }
                    else if ("SELL".Equals(AutoLockTradeSide) && (_DSQuote.BidDiff[0] <= (double)-_TradePara.AutoLockPoint || _TradePara.AutoLockPoint == 0))
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
                            case (int)Comm.Enum.Platform.V4:
                                _V4._OrderCreateType = _OrderCreateType;
                                mexecutedFlag = _V4.AutoTradeTransaction2(2, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                _MT5._OrderCreateType = _OrderCreateType;
                                mexecutedFlag = _MT5.AutoTradeTransaction2(2, true, out _SendTime);
                                break;
                        }
                        if (mexecutedFlag)
                        {
                            _Log.LogInfo("自动锁单[SELL]手数:[" + _TradePara.SellLots + "]跳次:[" + _DSQuote.BidDiff[0] + "]延时:[" + Convert.ToInt32(delayTime) + "ms]");
                        }
                        break;
                    }
                    else if ("BUY_CLOSE".Equals(AutoLockTradeSide) && (_DSQuote.BidDiff[0] <= (double)-_TradePara.AutoLockPoint || _TradePara.AutoLockPoint == 0))
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
                            case (int)Comm.Enum.Platform.V4:
                                mexecutedFlag = _V4.AutoTradeTransaction2(3, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                mexecutedFlag = _MT5.AutoTradeTransaction2(3, true, out _SendTime);
                                break;
                        }
                        if (mexecutedFlag)
                        {
                            _Log.LogInfo("自动锁单[Close][" + ("BUY_CLOSE".Equals(AutoLockTradeSide) ? -_DSQuote.BidDiff[0] : _DSQuote.BidDiff[0]) + "]延时:[" + Convert.ToInt32(delayTime) + "ms]");
                        }
                        break;
                    }
                    else if ("SELL_CLOSE".Equals(AutoLockTradeSide) && (_DSQuote.BidDiff[0] >= (double)_TradePara.AutoLockPoint || _TradePara.AutoLockPoint == 0))
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
                            case (int)Comm.Enum.Platform.V4:
                                mexecutedFlag = _V4.AutoTradeTransaction2(4, true, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                mexecutedFlag = _MT5.AutoTradeTransaction2(4, true, out _SendTime);
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
                if ((int)(delayTime / 1000) > 120)
                {
                    _Log.LogInfo("已延时[" + (int)(delayTime / 1000) + "]s,未完成锁单,退出自动锁单");
                    break;
                }
                if ((double)_TradePara.AutoLockTimeDuration > 3)
                {
                    Thread.Sleep(10000);
                }
                else
                {
                    Thread.Sleep(100);
                }
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
            DSDisconnect();
            if (this.bgwMarketQuote.IsBusy)
            {
                this.bgwMarketQuote.CancelAsync();
            }
            if (this.bgwBroberQuote.IsBusy)
            {
                this.bgwBroberQuote.CancelAsync();
            }
            if (this.bgwOrderUpdate.IsBusy)
            {
                this.bgwOrderUpdate.CancelAsync();
            }
            if (this.bgwAutoLock.IsBusy)
            {
                this.bgwAutoLock.CancelAsync();
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
                case (int)Comm.Enum.Platform.V4:
                    if (_V4 != null && _V4.isConnect())
                    {
                        _V4.DisConnect();
                    }
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    if (_MT5 != null && _MT5.isConnect())
                    {
                        _MT5.DisConnect();
                    }
                    break;
            }
            SaveConfig();
        }
        public void buttonDSConnect_Click(object sender, EventArgs e)
        {
            AsynExec(this.buttonDSConnect, () =>
            {
                if (buttonDSConnect.Text.Equals("数据源连接"))
                {
                    DSConnection();
                }
                else if (buttonDSConnect.Text.Equals("数据源断开"))
                {
                    DSDisconnect();
                }
            });
        }
        public string getDSConnectionAddress()
        {
            string dsAddress = _Config.PublishAddressT4;
            if (string.Equals(_Config.DSType, "T4"))
            {
                dsAddress = _Config.PublishAddressT4;
            }
            if (string.Equals(_Config.DSType, "OEC"))
            {
                dsAddress = _Config.PublishAddressOEC;
            }
            if (string.Equals(_Config.DSType, "PS"))
            {
                dsAddress = _Config.SysConfig["PS"];
            }
            if (string.Equals(_Config.DSType, "EA"))
            {
                IDictionary<string, string> eaInfo = DBHelper.getAutoInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo);
                if (eaInfo != null)
                {
                    string ea_type = eaInfo["ea_type"];
                    string nEATypeAddress = _Config.SysConfig[ea_type];
                    _Config.PublishAddressEA = "tcp://" + nEATypeAddress;
                }
                else
                {

                    _Config.PublishAddressEA = "tcp://" + "8.217.6.50:5656";
                }
                dsAddress = _Config.PublishAddressEA;
            }
            //本地数据源
            if (string.Equals(_Config.DSType, "LOC"))
            {
                //tcp://3.135.230.59:5558 sample
                string port = "5558";
                string address = string.Format("tcp://{0}:{1}", Utils.GetLocalIP(), port);
                dsAddress = address;
            }
            return dsAddress;
        }
        /// <summary>
        /// 数据源连接标志
        /// </summary>
        public volatile bool _IsDSConnectFlag = false;

        /// <summary>
        /// 数据源是否真正就绪。子类可增加 EA 心跳等额外条件。
        /// </summary>
        protected virtual bool IsDataSourceReady()
        {
            return _IsDSConnectFlag;
        }

        /// <summary>
        /// 连接数据源
        /// </summary>
        /// <returns></returns>
        public virtual bool DSConnection()
        {
            string dsAddress;
            try
            {
                dsAddress = getDSConnectionAddress();
            }
            catch (Exception ex)
            {
                _logger.Error("[数据源] 获取连接地址失败: " + ex.Message + "  " + ex.StackTrace);
                _Log.LogInfo("[数据源] 获取连接地址失败: " + ex.Message);
                return false;
            }

            if (string.IsNullOrWhiteSpace(dsAddress))
            {
                MessageBox.Show(string.Format("{0}服务器地址未配置", _Config.DSType));
                return false;
            }

            lock (_dsLifecycleLock)
            {
                if (bgwMarketQuote.IsBusy)
                {
                    if (string.Equals(_dsEndpoint, dsAddress, StringComparison.OrdinalIgnoreCase) &&
                        !_dsStopRequested)
                    {
                        _Log.LogInfo("[数据源] 行情线程已经运行，无需重复连接");
                        return true;
                    }

                    _dsStopRequested = true;
                    if (bgwMarketQuote.WorkerSupportsCancellation)
                        bgwMarketQuote.CancelAsync();
                }

                if (!_dsWorkerStopped.Wait(TimeSpan.FromSeconds(5)) || bgwMarketQuote.IsBusy)
                {
                    _Log.LogInfo("[数据源] 上一行情线程尚未退出，拒绝重复启动");
                    return false;
                }

                _dsEndpoint = dsAddress;
                _dsStopRequested = false;
                _dsWorkerStopped.Reset();

                try
                {
                    _Log.LogInfo(string.Format("正在启动{0}数据源连接：{1}", _Config.DSType, dsAddress));
                    bgwMarketQuote.RunWorkerAsync();
                    _IsDSConnectFlag = false; // 连接成功前不允许启动自动交易。
                    return true;
                }
                catch (Exception ex)
                {
                    _dsWorkerStopped.Set();
                    _IsDSConnectFlag = false;
                    _logger.Error("[数据源] 启动行情线程失败: " + ex.Message + "  " + ex.StackTrace);
                    _Log.LogInfo("[数据源] 启动行情线程失败: " + ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// 请求行情线程退出。当前线程不直接操作 NetMQ socket。
        /// </summary>
        public virtual void DSDisconnect()
        {
            _Log.LogInfo("[数据源] 请求断开数据源");
            _dsStopRequested = true;
            _IsDSConnectFlag = false;

            try
            {
                if (bgwMarketQuote.IsBusy && bgwMarketQuote.WorkerSupportsCancellation)
                    bgwMarketQuote.CancelAsync();
            }
            catch (Exception ex)
            {
                _logger.Warn("[数据源] CancelAsync 异常: " + ex.Message);
            }

            _dsWorkerStopped.Wait(TimeSpan.FromSeconds(3));
            try
            {
                RenderUI(buttonDSConnect, () =>
                {
                    buttonDSConnect.Text = "数据源连接";
                    buttonDSConnect.BackColor = Color.Red;
                    label_Price_Connect.BackColor = Color.Red;
                    label_Price_Connect.Text = "断开";
                    ClearDSDiff();
                    StopAutoTrade();
                });
            }
            catch (Exception ex)
            {
                _logger.Warn("[数据源] 更新断开状态UI失败: " + ex.Message);
            }
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
            _Log.LogInfo("****************************************");
            _Log.LogInfo("*** 大道至简，知易行难，知行合一，得到成功 ***");
            _Log.LogInfo("****************************************");

            _Log.LogInfo("加载Form开始");
            this.Text = _MainFormPara.FormNo;
            LoadConfigFile();

            DBUtils.connAddress = _Config.DefaultDBAddress;
            if (!string.IsNullOrEmpty(_Config.EAInfoAddress))
            {
                DBUtils.connEAInfoAddress = _Config.EAInfoAddress;
            }
            _Log.LogInfo("EAInfo地址:" + DBUtils.connEAInfoAddress);
            DBUtils.createConn(null);
            DBUtils.createEaConn(null);
            _Log.LogInfo("EAInfo 数据库连接地址:" + DBUtils.eaConnStr);
            _Log.LogInfo("默认数据库连接地址:" + DBUtils.connStr);
            LoadConfig();
            if (_Broker != null)
            {
                //先初始化平台对象,UI加载后再修改平台应用对象
                GeneratePlatformObj();
                IniUI();
            }
            InitialPara();
            _IsLoadConfigCompleted = true;
            _Log.LogInfo("加载Form完成");
        }

        public void comboBoxAutoLockTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            _TradePara.AutoLockTimeDuration = Convert.ToInt32(comboBoxAutoLockTime.SelectedItem);
            _Log.LogInfo("自动锁仓时间修改为:" + _TradePara.AutoLockTimeDuration + "s");
        }

        public void timerAutoTradeParameterRefresh_Tick(object sender, EventArgs e)
        {
            AutoTradeParameterRefresh();
        }

        /// <summary>
        /// 自动更新自动交易参数设置
        /// </summary>
        public void AutoTradeParameterRefresh()
        {
            if (isConnect())
            {
                if (_TradePara.AutoChecked && !isExecuting && isOrderCompleted)
                {
                    Task.Run(() =>
                    {
                        int t = TradeUtils.GetCloseTimeDuration(_TradePara.AutoCloseTimeDuration);
                        if (t <= 0)
                        {
                            _Log.LogInfo("未选择自动平仓时间");
                        }
                        else
                        {
                            TradeUtils.SetAutoTradeParameters(t, IsOverOpenOrderTime(), gvPositions, chkBuyOpen, chkSellOpen, chkBuyClose, chkSellClose);
                        }
                    });
                }
            }
        }

        public void comboBoxCloseTimeDuration_SelectedIndexChanged(object sender, EventArgs e)
        {
            _TradePara.AutoCloseTimeDuration = comboBoxCloseTimeDuration.SelectedItem.ToString();

            _Log.LogInfo("自动平仓时间修改为:" + _TradePara.AutoCloseTimeDuration);
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
        /// 如果你的后台线程在更新一个UI控件的状态后不需要等待，而是要继续往下处理，那么你就应该使用Invoke来进行异步处理。
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
        /// <summary>
        /// 如果你的后台线程在更新一个UI控件的状态后不需要等待，而是要继续往下处理，那么你就应该使用Invoke来进行异步处理。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        protected void AsynRenderUI(Control control, Action action)
        {
            if (control.IsHandleCreated)
            {
                control.Invoke(new Action(delegate ()
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

            Task task = new Task(() =>
            {
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

        public void comboBoxLockPoint_SelectedIndexChanged(object sender, EventArgs e)
        {
            _TradePara.AutoLockPoint = Convert.ToDecimal(comboBoxLockPoint.SelectedItem.ToString());
            _Log.LogInfo("自动锁单点数修改为:" + _TradePara.AutoLockPoint);
        }

        public void checkBox_AutoLock_CheckedChanged(object sender, EventArgs e)
        {
            if (_TradePara != null)
            {
                _TradePara.AutoLock = checkBox_AutoLock.Checked;
                if (_TradePara.AutoLock)
                {
                    if (string.IsNullOrEmpty(comboBox_AutoLockForm.Text))
                    {
                        _TradePara.AutoLock = false;
                        checkBox_AutoLock.Checked = _TradePara.AutoLock;
                        _Log.LogInfo("锁仓启动失败,请选择锁单主窗口");
                        MessageBox.Show("请选择锁单主窗口");
                    }
                    else
                    {
                        string port = uClient.Comm.Utils.GetPort(comboBox_AutoLockForm.Text);
                        string uaddress = string.Format("tcp://{0}:{1}", uClient.Comm.Utils.GetLocalIP(), port);
                        string address = string.Format("tcp://{0}:{1}", uClient.Comm.Utils.GetLocalIP(), port);
                        if (string.IsNullOrEmpty(_Config.LockSubscriberAddress))
                        {
                            uaddress = string.Format("tcp://{0}:{1}", _Config.LockSubscriberAddress, port);
                            address = string.Format("tcp://{0}:{1}", _Config.LockSubscriberAddress, port);
                        }
                        try
                        {
                            _LockSubscriber.Connect(address);
                            _LockSubscriber.Subscribe("");
                            _Log.LogInfo("锁单消息接收成功,锁单窗口:" + comboBox_AutoLockForm.Text + ",地址:" + address);
                        }
                        catch (Exception e1)
                        {
                            _Log.LogInfo("连接[" + address + "]异常:" + e1.Message);
                            _LockSubscriber.Disconnect(uaddress);
                            _Log.LogInfo("取消锁单消息接收成功,地址:" + uaddress);
                            _LockSubscriber.Connect(address);
                            _LockSubscriber.Subscribe("");
                            _Log.LogInfo("锁单消息接收成功,锁单窗口:" + comboBox_AutoLockForm.Text + ",地址:" + address);
                        }
                    }
                    //启动锁仓消息处理
                    if (bgwAutoLock.IsBusy != true)
                    {
                        bgwAutoLock.RunWorkerAsync();
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(comboBox_AutoLockForm.Text))
                    {
                        string port = uClient.Comm.Utils.GetPort(comboBox_AutoLockForm.Text);
                        string address = string.Format("tcp://{0}:{1}", uClient.Comm.Utils.GetLocalIP(), port);
                        try
                        {
                            _LockSubscriber.Unsubscribe("");
                            _LockSubscriber.Disconnect(address);
                            _Log.LogInfo("取消锁单消息接收成功,锁单窗口:" + comboBox_AutoLockForm.Text + ",地址:" + address);
                        }
                        catch (Exception e1)
                        {
                            _Log.LogInfo("取消锁单消息接收[" + address + "]异常:" + e1.Message);
                        }
                    }
                }
                if (_TradePara.AutoLock)
                {
                    checkBoxIndLock.Checked = false;
                    label_AutoLock.Text = "自动锁单";
                    label_AutoLock.BackColor = Color.Green;
                }
                if (!_TradePara.AutoLock && !_TradePara.IndLockFlag)
                {
                    label_AutoLock.Text = "未锁单";
                    label_AutoLock.BackColor = Color.Red;
                }
            }
        }
        public void checkBoxAutoChecked_CheckedChanged_1(object sender, EventArgs e)
        {
            if (_TradePara != null)
            {
                _TradePara.AutoChecked = checkBoxAutoChecked.Checked;
                _Log.LogInfo((_TradePara.AutoChecked ? "启动" : "停止") + "自动勾选");
            }
            if (!checkBoxAutoChecked.Checked)
            {
                CancelCheck();
            }
        }

        public void comboBox_Slippage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_TradePara != null)
            {
                _TradePara.Slippage = Convert.ToInt32(comboBox_Slippage.Text);
                _Log.LogInfo("交易偏差点数修改为:" + _TradePara.Slippage);
            }
        }

        public void comboBox_AutoLockForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.Equals(_MainFormPara.FormNo, comboBox_AutoLockForm.Text))
            {
                comboBox_AutoLockForm.SelectedItem = "";
            }
            else
            {
                if (_TradePara != null)
                {
                    _TradePara.AutoLockForm = comboBox_AutoLockForm.Text;
                    _Log.LogInfo("自动锁单主窗口修改为:" + _TradePara.AutoLockForm);
                }
            }
        }
        public void checkBox_V4QuoteDS_Click(object sender, EventArgs e)
        {
            if (isConnect())
            {
                _Log.LogInfo("请先断开连接后再修改获取报价方式");
            }
            else
            {
            }
        }

        public void checkBox_V4QuoteDS_CheckedChanged(object sender, EventArgs e)
        {
            if (isConnect())
            {
                _Log.LogInfo("请先断开连接后再修改获取报价方式");
            }
            else
            {
            }
        }
        public bool isTradingDataExecuting = false;

        public bool PlatformConnectCheckFlag = false;
        /// <summary>
        /// 数据源连接正常，平台连接不正常时，自动刷新平台的连接状态并发送断链通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timerCheckPlatformConnectStatus_Tick(object sender, EventArgs e)
        {
            if (!PlatformConnectCheckFlag)
            {
                PlatformConnectCheckFlag = true;
                if (_IsDSConnectFlag && !isConnect())
                {
                    switch (_Platform.PlatformNo)
                    {
                        case (int)Comm.Enum.Platform.MF4:
                            _MF4.CheckPlatformConnectStatus();
                            if ((DateTime.Now - _MF4._QuoteData.Time).TotalMilliseconds > 60000)
                            {
                                DisConnectBrkoer();
                            }
                            break;
                        case (int)Comm.Enum.Platform.MT4:
                            _MT4.CheckPlatformConnectStatus();
                            if ((DateTime.Now - _MT4._QuoteData.Time).TotalMilliseconds > 60000)
                            {
                                DisConnectBrkoer();
                            }
                            break;
                        case (int)Comm.Enum.Platform.V4:
                            _V4.CheckPlatformConnectStatus();
                            if ((DateTime.Now - _V4._QuoteData.Time).TotalMilliseconds > 60000)
                            {
                                DisConnectBrkoer();
                            }
                            break;
                        case (int)Comm.Enum.Platform.MT5:
                            _MT5.CheckPlatformConnectStatus();
                            if ((DateTime.Now - _MT5._QuoteData.Time).TotalMilliseconds > 60000)
                            {
                                DisConnectBrkoer();
                            }
                            break;
                    }
                }
            }
        }
        public void DisConnectBrkoer()
        {
            _Log.LogInfo("断开连接");
            switch (_Platform.PlatformNo)
            {
                case (int)Comm.Enum.Platform.MF4:
                    _MF4.DisConnect();
                    _MF4._BgwBroberQuote.CancelAsync();
                    _MF4._BgwOrderUpdate.CancelAsync();
                    break;
                case (int)Comm.Enum.Platform.MT4:
                    _MT4.DisConnect();
                    break;
                case (int)Comm.Enum.Platform.V4:
                    _V4.DisConnect();
                    _V4._BgwBroberQuote.CancelAsync();
                    _V4._BgwOrderUpdate.CancelAsync();
                    break;
                case (int)Comm.Enum.Platform.MT5:
                    _MT5.DisConnect();
                    break;
            }
            uClient.Comm.Utils.ClearAccountCaptial(accountDisplay);
            UpdateConnectStatus(false);
            CancelCheck();
            _Log.LogInfo("自动交易停止");
            lblTradeStatus.Text = "自动交易已停止";
            btnStart.Enabled = true;
            btnStart.BackColor = Color.Red;
            btnStop.Enabled = false;
            lblTradeStatus.BackColor = Color.Red;
        }
        public void checkBoxNotify_CheckedChanged(object sender, EventArgs e)
        {
            if (_TradePara != null)
            {
                _Log.LogInfo(_TradePara.NotifyFlag ? "发送通知" : "取消通知");
            }
        }
        public void checkBoxIndLock_CheckedChanged(object sender, EventArgs e)
        {
            if (_TradePara != null)
            {
                _TradePara.IndLockFlag = checkBoxIndLock.Checked;
                if (_TradePara.IndLockFlag)
                {
                    checkBox_AutoLock.Checked = false;
                    label_AutoLock.Text = "独立锁单";
                    label_AutoLock.BackColor = Color.Green;
                }
                if (!_TradePara.AutoLock && !_TradePara.IndLockFlag)
                {
                    label_AutoLock.Text = "未锁单";
                    label_AutoLock.BackColor = Color.Red;
                }
                _Log.LogInfo(_TradePara.IndLockFlag ? "独立锁单启动" : "取消独立锁单");

            }
        }

        public void buttonSaveTestInfo_Click(object sender, EventArgs e)
        {
            /*if (!string.Equals(_Broker.BrokerCode, textBoxBrokerCode.Text))
            {
                textBoxBrokerCode.Text = _Broker.BrokerCode;
                MessageBox.Show("不能修改编码");
                return;
            }*/
            if (_Platform.GetBrokerByName(textBoxBrokerName.Text) != null)
            {
                _Broker = _Platform.GetBrokerByName(textBoxBrokerName.Text);
            }
            if (_Broker == null)
            {
                MessageBox.Show("该平台数据不存在,请新增保存");
                return;
            }
            bool isChangeBrokerName = false;
            if (!string.Equals(_Broker.BrokerName, textBoxBrokerName.Text))
            {
                isChangeBrokerName = true;
                _Broker.BrokerName = textBoxBrokerName.Text;
            }
            if (string.IsNullOrEmpty(textBoxTestSymbol.Text))
            {
                MessageBox.Show("请输入产品信息，多个产品用逗号(,)分割");
                return;
            }
            else
            {
                _Broker.Symbol = textBoxTestSymbol.Text.Split(',');
            }
            if (string.IsNullOrEmpty(textBoxTestIP.Text))
            {
                MessageBox.Show("请输入IP信息，如 DemoIP,LiveIP");
                return;
            }
            else
            {
                if (textBoxTestIP.Text.Contains(','))
                {
                    _Broker.DemoIP = textBoxTestIP.Text.Split(',')[0];
                    _Broker.LiveIP = textBoxTestIP.Text.Split(',')[1];
                }
                else
                {
                    _Broker.DemoIP = textBoxTestIP.Text;
                    _Broker.LiveIP = textBoxTestIP.Text;
                }
            }
            if (string.IsNullOrEmpty(textBoxTestPort.Text))
            {
                MessageBox.Show("请输入Port信息，如 DemoPort,LivePort");
                return;
            }
            else
            {
                if (textBoxTestPort.Text.Contains(','))
                {
                    _Broker.DemoPort = textBoxTestPort.Text.Split(',')[0];
                    _Broker.LivePort = textBoxTestPort.Text.Split(',')[1];
                }
                else
                {
                    _Broker.DemoPort = textBoxTestPort.Text;
                    _Broker.LivePort = textBoxTestPort.Text;
                }
            }
            //先删除再保存
            if (_Config.PlatformList.ContainsKey("MT4"))
            {
                if (_Config.PlatformList["MT4"].BrokerList.ContainsKey(_Broker.BrokerCode))
                {
                    _Config.PlatformList["MT4"].BrokerList.Remove(_Broker.BrokerCode);
                }
                for (int i = _Config.PlatformList["MT4"].Broker.Count - 1; i >= 0; i--)
                {
                    if (string.Equals(_Config.PlatformList["MT4"].Broker[i].BrokerCode, _Broker.BrokerCode))
                    {
                        _Config.PlatformList["MT4"].Broker.RemoveAt(i); // 安全删除
                    }
                }
            }
            if (_Config.PlatformList.ContainsKey("MT5"))
            {
                if (_Config.PlatformList["MT5"].BrokerList.ContainsKey(_Broker.BrokerCode))
                {
                    _Config.PlatformList["MT5"].BrokerList.Remove(_Broker.BrokerCode);
                }
                for (int i = _Config.PlatformList["MT5"].Broker.Count - 1; i >= 0; i--)
                {
                    if (string.Equals(_Config.PlatformList["MT5"].Broker[i].BrokerCode, _Broker.BrokerCode))
                    {
                        _Config.PlatformList["MT5"].Broker.RemoveAt(i); // 安全删除
                    }
                }
            }

            if (string.Equals(comboBox_PlatFormType.Text, "MT4"))
            {
                _Broker.PlatformNo = 2;
                if (_Config.PlatformList.ContainsKey("MT4"))
                {
                    _Platform = _Config.PlatformList["MT4"];
                }
                else
                {
                    _Platform = new Platform();
                    _Platform.PlatformNo = 2;
                    _Platform.PlatformCode = "MT4";
                    _Platform.PlatformName = "MT4";
                    _Config.PlatformList.Add(_Platform.PlatformCode, _Platform);
                    _Config.Platform.Add(_Platform);
                }
                if (_Platform.BrokerList.ContainsKey(_Broker.BrokerCode))
                {
                    _Platform.BrokerList.Remove(_Broker.BrokerCode);
                }
                _Platform.BrokerList.Add(_Broker.BrokerCode, _Broker);
                if (_Platform.Broker != null && _Platform.Broker.Count > 0)
                {
                    bool existFlag = false;
                    for (int i = 0; i < _Platform.Broker.Count; i++)
                    {
                        if (string.Equals(_Platform.Broker[i].BrokerCode, _Broker.BrokerCode))
                        {
                            _Platform.Broker[i] = _Broker;
                            existFlag = true;
                        }
                    }
                    if (!existFlag)
                    {
                        _Platform.Broker.Add(_Broker);
                    }
                }
                else
                {
                    _Platform.Broker = new List<Comm.Broker>() { _Broker };
                }
            }
            if (string.Equals(comboBox_PlatFormType.Text, "MT5"))
            {
                _Broker.PlatformNo = 4;
                if (_Config.PlatformList.ContainsKey("MT5"))
                {
                    _Platform = _Config.PlatformList["MT5"];
                }
                else
                {
                    _Platform = new Platform();
                    _Platform.PlatformNo = 4;
                    _Platform.PlatformCode = "MT5";
                    _Platform.PlatformName = "MT5";
                    _Config.PlatformList.Add(_Platform.PlatformCode, _Platform);
                    _Config.Platform.Add(_Platform);
                }
                if (_Platform.BrokerList.ContainsKey(_Broker.BrokerCode))
                {
                    _Platform.BrokerList.Remove(_Broker.BrokerCode);
                }
                _Platform.BrokerList.Add(_Broker.BrokerCode, _Broker);
                if (_Platform.Broker != null && _Platform.Broker.Count > 0)
                {
                    bool existFlag = false;
                    for (int i = 0; i < _Platform.Broker.Count; i++)
                    {
                        if (string.Equals(_Platform.Broker[i].BrokerCode, _Broker.BrokerCode))
                        {
                            _Platform.Broker[i] = _Broker;
                        }
                    }
                    if (!existFlag)
                    {
                        _Platform.Broker.Add(_Broker);
                    }
                }
                else
                {
                    _Platform.Broker = new List<Comm.Broker>() { _Broker };
                }
            }
            _Account = new Account();
            _TradePara = new TradeParametre();
            _Account.TradePara = _TradePara;
            _Account.BrokerCode = _Broker.BrokerCode;
            _Account.Type = "Demo";

            _TradePara.Timezone = Double.Parse(string.IsNullOrEmpty(textBoxTZ.Text) ? "0" : textBoxTZ.Text);
            _Account.TradePara.Timezone = _TradePara.Timezone;
            _Platform.DefaultBrokerCode = _Broker.BrokerCode;
            SaveConfig();
            //保存完成有如果有修改名字
            if (isChangeBrokerName)
            {
                comboBox_Broker.Items[comboBox_Broker.SelectedIndex] = _Broker.BrokerName;
            }
            _Log.LogInfo("信息保存成功");
            MessageBox.Show("保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void checkBox_QuotaCheck_CheckedChanged(object sender, EventArgs e)
        {
            _TradePara.QuotaCheckFlag = checkBox_QuotaCheck.Checked;
            _Log.LogInfo("价格检查功能" + (_TradePara.QuotaCheckFlag ? "启用" : "取消"));
        }

        private void buttonAddBrokerInfo_Click(object sender, EventArgs e)
        {
            if (_Platform.GetBrokerByName(textBoxBrokerName.Text) != null)
            {
                MessageBox.Show("平台名称已存在，请重新输入");
                return;
            }
            if (_Platform.BrokerList.ContainsKey(textBoxBrokerCode.Text))
            {
                MessageBox.Show("平台编码已存在，请重新输入");
                return;
            }
            _Broker = new Comm.Broker();
            _Broker.BrokerCode = textBoxBrokerCode.Text;
            _Broker.BrokerName = textBoxBrokerName.Text;
            if (string.IsNullOrEmpty(textBoxTestSymbol.Text))
            {
                MessageBox.Show("请输出产品信息，多个产品用逗号(,)分割");
                return;
            }
            else
            {
                _Broker.Symbol = textBoxTestSymbol.Text.Split(',');
            }
            _Account = new Account();
            _TradePara = new TradeParametre();
            _Account.TradePara = _TradePara;
            _Account.BrokerCode = _Broker.BrokerCode;
            _Account.Type = "Demo";
            _TradePara.Timezone = Double.Parse(string.IsNullOrEmpty(textBoxTZ.Text) ? "0" : textBoxTZ.Text);
            _Account.TradePara.Timezone = _TradePara.Timezone;

            if (string.IsNullOrEmpty(textBoxTestIP.Text))
            {
                MessageBox.Show("请输入IP信息，如 DemoIP,LiveIP");
                return;
            }
            else
            {
                if (textBoxTestIP.Text.Contains(','))
                {
                    _Broker.DemoIP = textBoxTestIP.Text.Split(',')[0];
                    _Broker.LiveIP = textBoxTestIP.Text.Split(',')[1];
                }
                else
                {
                    _Broker.DemoIP = textBoxTestIP.Text;
                    _Broker.LiveIP = textBoxTestIP.Text;
                }
            }
            if (string.IsNullOrEmpty(textBoxTestPort.Text))
            {
                MessageBox.Show("请输入Port信息，如 DemoPort,LivePort");
                return;
            }
            else
            {
                if (textBoxTestPort.Text.Contains(','))
                {
                    _Broker.DemoPort = textBoxTestPort.Text.Split(',')[0];
                    _Broker.LivePort = textBoxTestPort.Text.Split(',')[1];
                }
                else
                {
                    _Broker.DemoPort = textBoxTestPort.Text;
                    _Broker.LivePort = textBoxTestPort.Text;
                }
            }
            if (string.Equals(comboBox_PlatFormType.Text, "MT4"))
            {
                _Broker.PlatformNo = 2;
            }
            if (string.Equals(comboBox_PlatFormType.Text, "MT5"))
            {
                _Broker.PlatformNo = 4;
            }
            _Broker.DemoAccount = _Account;
            _Broker.LiveAccount = _Account;
            _Broker.TradePara = _TradePara;
            _Broker.DefaultAccountType = "Demo";
            if (string.Equals(comboBox_PlatFormType.Text, "MT4"))
            {
                if (_Config.PlatformList.ContainsKey("MT4"))
                {
                    _Platform = _Config.PlatformList["MT4"];
                }
                else
                {
                    _Platform = new Platform();
                    _Platform.PlatformNo = 2;
                    _Platform.PlatformCode = "MT4";
                    _Platform.PlatformName = "MT4";
                    _Config.PlatformList.Add(_Platform.PlatformCode, _Platform);
                    _Config.Platform.Add(_Platform);
                }
            }
            if (string.Equals(comboBox_PlatFormType.Text, "MT5"))
            {
                if (_Config.PlatformList.ContainsKey("MT5"))
                {
                    _Platform = _Config.PlatformList["MT5"];
                }
                else
                {
                    _Platform = new Platform();
                    _Platform.PlatformNo = 4;
                    _Platform.PlatformCode = "MT5";
                    _Platform.PlatformName = "MT5";
                    _Config.PlatformList.Add(_Platform.PlatformCode, _Platform);
                    _Config.Platform.Add(_Platform);
                }
            }
            _Platform.BrokerList.Add(_Broker.BrokerCode, _Broker);
            _Platform.Broker.Add(_Broker);
            _Platform.DefaultBrokerCode = _Broker.BrokerCode;
            SaveConfig();
            LoadConfig();
            IniUI();
            comboBox_Broker.SelectedItem = _Broker.BrokerName;
            _Log.LogInfo("信息保存成功");
            MessageBox.Show("保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox_Event_CheckedChanged(object sender, EventArgs e)
        {
            _TradePara.EventFlag = checkBox_Event.Checked;
            _Log.LogInfo("事件单功能" + (_TradePara.EventFlag ? "启用" : "取消"));
        }
    }
}