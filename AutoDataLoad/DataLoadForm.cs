using mtapi.mt5;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using NLog;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using uClient.Comm;


namespace uClient.Broker
{
    public partial class DataLoadForm : Form
    {
        //MT4 平台 begin
        public uClient.Broker.MT4 _MT4;
        //MT4 平台 end

        //MT5 平台 begin
        public MT5 _MT5;
        //MT4 平台 end
        /// <summary>
        /// 公共参数
        /// </summary>
        public Config _Config;
        public MyConfig _MyConfig;
        public Platform _Platform;
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
        public string _TradeSymbol = "";
        public TradeParametre _TradePara;
        public uClient.Comm.Broker _Broker;
        public Account _Account;
        /// <summary>
        /// 报价显示面板
        /// </summary>
        public QuotaDisplayPanel quotaDisplayPanel = new QuotaDisplayPanel();
        /// <summary>
        /// 账户显示面板
        /// </summary>
        public AccountDisplay accountDisplay = new AccountDisplay();


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
        /// 日志
        /// </summary>
        public uClient.Comm.Log _Log;
        
        /// <summary>
        /// EA配置数据
        /// </summary>
        public StrategyConfig _StrategyConfig;
        /// <summary>
        /// 全局变量,上下文数据
        /// </summary>
        public IDictionary<string, string> _Context = new Dictionary<String, string>();

        /// <summary>
        /// 配置文件是否加载完成
        /// </summary>
        public bool _IsLoadConfigCompleted = false;
        public DataLoadForm(MainFormPara mainFormPara)
        {
            if (mainFormPara == null)
            {
                mainFormPara = new MainFormPara();
                mainFormPara.PlatformCode = "MT5";
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
        public DataLoadForm()
        {
            MainFormPara mainFormPara = new MainFormPara();
            mainFormPara.PlatformCode = "MT5";
            mainFormPara.BrokerCode = "";
            mainFormPara.BrokerName = "";
            mainFormPara.FormType = "";
            mainFormPara.FormNo = "报价数据采集平台";
            _MainFormPara = mainFormPara;
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            InitObject();
        }
        /// <summary>
        /// Form加载前实例化对象，该方法只能调用一次
        /// </summary>
        public void InitObject()
        {
            //Log 实例只初始化一次
            _Log = new uClient.Comm.Log(lstTradeRecord, lstLog);
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
        

        private void DataLoadForm_Load(object sender, EventArgs e)
        {
            SetMainFormSerialize(this.Handle);
            this.Text = _MainFormPara.FormNo;
            LoadConfigFile();
            //初始化参数
            DBUtils.connAddress = _Config.DefaultDBAddress;
            if (!string.IsNullOrEmpty(_Config.EAInfoAddress))
            {
                DBUtils.connEAInfoAddress = _Config.EAInfoAddress;
            }
            _Log.LogInfo("EAInfo地址:" + DBUtils.connEAInfoAddress);
            DBUtils.createConn(null);
            DBUtils.createEaConn(null);

            LoadConfig();
            _IsLoadConfigCompleted = true;
            InitialPara();
            GeneratePlatformObj();
            InitObjectAfterLoad();
            IniUI();
        }
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
        /// 将当前主窗体序列化到文件
        /// </summary>
        /// <param name="hWnd"></param>
        public static void SetMainFormSerialize(IntPtr hWnd)
        {
            //序列化
            FileStream fs = new FileStream("MainFormSerialize", FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, hWnd);
            fs.Close();
        }
        /// <summary>
        /// Form加载完成后，再加载其他数据
        /// </summary>
        public void InitObjectAfterLoad()
        {
            //timer 启用
            timerCheckPlatformConnectStatus.Enabled = true;
            timer_JR.Enabled = true;
            timer_JR.Interval = 1000 * 60* 10;           
            _Log.LogInfo("timer_JR时钟启动,执行周期[" + timer_JR.Interval / 1000 / 60 + "]min");
            timer_WZ.Enabled = true;
            timer_WZ.Interval = 1000 * 60 * 12;
            _Log.LogInfo("timer_WZ,执行周期[" + timer_WZ.Interval / 1000 / 60 + "]min");
            //金十数据
            timer_DataCenter.Enabled = true;           
            timer_DataCenter.Interval = 1000 * 60 * 10;
            _Log.LogInfo("timer_DataCenter,执行周期[" + timer_DataCenter.Interval / 1000 / 60 + "]min");
            //Dukascopy数据
            timer_Dukascopy.Enabled = true;
            timer_Dukascopy.Interval = 1000 * 60 * 15;
            _Log.LogInfo("timer_Dukascopy,执行周期[" + timer_Dukascopy.Interval / 1000 / 60 + "]min");

            timer_KLineCheck.Enabled = true;
            timer_KLineCheck.Interval = 1000 * 60 * 3;
            _Log.LogInfo("timer_KLineCheck,执行周期[" + timer_KLineCheck.Interval / 1000 / 60 + "]min");


            _ = ClearDataAsync();
            _Log.LogInfo("ClearDataAsync数据清除任务已启动");
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
            quotaDisplayPanel.lblMT4Bid = label_BrokePrice;
        }
        /// <summary>
        /// 初始化平台对象
        /// </summary>
        public virtual void GeneratePlatformObj()
        {
            switch (_Platform.PlatformNo)
            {
                case (int)uClient.Comm.Enum.Platform.MT4:
                    _MT4 = new uClient.Broker.MT4(_Config, _MyConfig, _Broker, _Account, _TradeSymbol, _Log, bgwBroberQuote, null, null, quotaDisplayPanel, accountDisplay);
                    _MT4._QuoteData = new QuotePanelData(_TradeSymbol, 0.1);
                    if (!string.IsNullOrEmpty(_Config.SysConfig["QuotaSymbol"]))
                    {
                        string[] symbols = _Config.SysConfig["QuotaSymbol"].Split(new char[] { ',' });
                        _MT4._GOLD = symbols[0];
                        _MT4._SILVER = symbols[1];
                    }
                    break;
                case (int)uClient.Comm.Enum.Platform.MT5:
                    _MT5 = new uClient.Broker.MT5(_Config, _MyConfig, _Broker, _Account, _TradeSymbol, _Log, bgwBroberQuote, null, null, quotaDisplayPanel, accountDisplay);
                    _MT5._QuoteData = new QuotePanelData(_TradeSymbol, 0.1);
                    if (!string.IsNullOrEmpty(_Config.SysConfig["QuotaSymbol"]))
                    {
                        string[] symbols = _Config.SysConfig["QuotaSymbol"].Split(new char[] { ',' });
                        _MT5._GOLD = symbols[0];
                        _MT5._SILVER = symbols[1];
                    }
                    break;
            }
        }
        public string rootPath = "C:\\iAutoTrade";
        /// <summary>
        /// 初始化UI参数
        /// </summary>
        public void IniUI()
        {
            _logger.Info("开始初始化UI");
            //默认数据采集账户
            /*_Broker.BrokerCode = "";
            _Broker.BrokerName = "";
            _Broker.DemoIP = "";
            _Broker.DemoPort = "";
            _Broker.LiveIP = "";
            _Broker.LivePort = "";
            _Broker.LiveAccount = _Account;
            _Account.UserCode = "";
            _Account.Password = "";*/

            label_Platform.Text = _Broker.BrokerName;
            textBox_account.Text = _Account.UserCode;
            textBox_password.Text = _Account.Password;           

            //基础配置数据
            checkBox_NotifyFlag.Checked = _StrategyConfig.NotifyFlag;
        }
        /// <summary>
        /// 加载配置参数
        /// </summary>
        public void LoadConfig()
        {
            string newMyConfigFile = rootPath + _MyConfigFile;
            string newConfigFile = rootPath + _ConfigFile;          
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
                                foreach (uClient.Comm.Broker broker in platform.Broker)
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
                        }
                        _Platform = _Config.PlatformList[_MainFormPara.PlatformCode];
                        _TradePara = _Platform.TradePara;
                        //修改默认平台商,数据采集只用金荣和万州两个平台 WZ1,JRJR,WCG dukascopy
                        //_Platform.DefaultBrokerCode = "dukascopy";
                        if (_Platform.BrokerList.Keys.Contains(_Platform.DefaultBrokerCode))
                        {
                            string defaultBrokerCode = string.IsNullOrEmpty(_MainFormPara.BrokerCode) ? _Platform.DefaultBrokerCode : _MainFormPara.BrokerCode;
                            _Broker = _Config.PlatformList[_MainFormPara.PlatformCode].BrokerList[defaultBrokerCode];
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
            else
            {
                _logger.Fatal("没有初始化配置文件，系统初始化异常");
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void SaveConfig()
        {
            string newMyConfigFile = rootPath + _MyConfigFile;
            string newConfigFile = rootPath + _ConfigFile;
            try
            {
                if (_Config != null)
                {
                    //交易信息设置

                    //平台设置

                    _StrategyConfig.NotifyFlag = checkBox_NotifyFlag.Checked;

                    File.WriteAllText(newConfigFile, JsonConvert.SerializeObject(_Config));
                    File.WriteAllText(newMyConfigFile, JsonConvert.SerializeObject(_MyConfig));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                _Log.LogInfo(ex.Message + ex.Source);
            }
        }
        /// <summary>
        /// 是否连接
        /// </summary>
        /// <returns></returns>
        public bool IsConnect()
        {
            if (_MT4.isConnect())
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 连接成功后
        /// </summary>
        public void AfterConnectCompleted()
        {
            //2.更新连接状态
            UpdateConnectStatus(true);
            AutoLockBind(true);
        }
        private void button_platformConnect_Click(object sender, EventArgs e)
        {
            AsynExec(this.button_platformConnect, () =>
            {
                if (button_platformConnect.Text.Equals("平台连接"))
                {
                    switch (_Platform.PlatformNo)
                    {
                        case (int)uClient.Comm.Enum.Platform.MT4:
                            _MT4.Connect();
                            _MT4._IsManullyDisconnect = false;
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
                else if (button_platformConnect.Text.Equals("平台断开"))
                {
                    _Log.LogInfo("断开连接");
                    switch (_Platform.PlatformNo)
                    {
                        case (int)uClient.Comm.Enum.Platform.MT4:
                            _MT4.DisConnect();
                            _MT4._IsManullyDisconnect = true;
                            break;
                        case (int)uClient.Comm.Enum.Platform.MT5:
                            _MT5.DisConnect();
                            _MT5._IsManullyDisconnect = true;
                            break;
                    }
                    UpdateConnectStatus(false);
                }
            });
        }
        /// <summary>
        /// 是否连接
        /// </summary>
        /// <returns></returns>
        public bool isConnect()
        {
            switch (_Platform.PlatformNo)
            {
                case (int)uClient.Comm.Enum.Platform.MT4:
                    if (_MT4.isConnect())
                    {
                        return true;
                    }
                    break;
                case (int)uClient.Comm.Enum.Platform.MT5:
                    if (_MT5.isConnect())
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 更新帐户连接状态
        /// </summary>
        /// <param name="Status">true= 连接，false=断开</param>
        public void UpdateConnectStatus(bool status)
        {
            if (lblConnectStatus.InvokeRequired)
            {
                lblConnectStatus.Invoke(new Action<bool>(UpdateConnectStatus), new object[] { status });
            }
            else {
                lblConnectStatus.BackColor = Color.Red;
                label_BrokePrice.Text ="0";
            }
            lblConnectStatus.Text = status ? "已连接" : "断开";
            lblConnectStatus.BackColor = status ? Color.Green : Color.Red;
            button_platformConnect.Text = !status ? "平台连接" : "平台断开";
            button_platformConnect.BackColor = !status ? Color.Red : Color.Green;
            textBox_account.Enabled = !status;
            textBox_password.Enabled = !status;
        }
        private void bgwBroberQuote_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Task.Run(() => {
                BroberQuote();
            });
        }

        /// <summary>
        /// 自动绑定锁单消息,在平台连接完成后自动启动
        /// </summary>
        public void AutoLockBind(bool isActive)
        {
            string port = uClient.Comm.Utils.GetPort("PS");
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
        /// 平台商报价刷新
        /// </summary>
        public void BroberQuote()
        {
            updateDsRunningStatus("Q","1");
            switch (_Platform.PlatformNo)
            {                
                case (int)uClient.Comm.Enum.Platform.MT4:
                    if (_MT4._QuoteEventArgs != null)
                    {
                        string symbols = _MT4._QuoteEventArgs.Symbol;
                        double prices = _MT4._QuoteEventArgs.Bid;
                        double pricesAsk = _MT4._QuoteEventArgs.Ask;                        
                        if (string.Equals(symbols, _TradeSymbol))
                        {
                            _MasterPublisher.SendFrame($"{"Gold"} {prices} {pricesAsk} {0}");
                            updatePrice(prices);
                        }
                        if (prices > 0)
                        {
                            if (string.Equals(symbols, _MT4._GOLD))
                            {
                                _MT4._GOLD_BID_PRICE = prices;
                                _MT4._GOLD_ASK_PRICE = pricesAsk;
                                _StrategyConfig.GOLD_PRICE = _MT4._GOLD_BID_PRICE;
                            }
                            if (string.Equals(symbols, _MT4._SILVER))
                            {
                                _MT4._SILVER_BID_PRICE = prices;
                                _MT4._SILVER_ASK_PRICE = pricesAsk;
                                _StrategyConfig.SILVER_PRICE = _MT4._SILVER_BID_PRICE;
                            }
                            saveQuoteGS(_MT4._GOLD_BID_PRICE, _MT4._SILVER_BID_PRICE);
                        }
                    }
                    break;
                case (int)uClient.Comm.Enum.Platform.MT5:
                    if (_MT5._QuoteEventArgs != null)
                    {
                        string symbols = _MT5._QuoteEventArgs.Symbol;
                        double prices = _MT5._QuoteEventArgs.Bid;
                        double pricesAsk = _MT5._QuoteEventArgs.Ask;
                        if (string.Equals(symbols, _TradeSymbol))
                        {
                            _MasterPublisher.SendFrame($"{"Gold"} {prices} {pricesAsk} {0}");
                            updatePrice(prices);
                        }
                        if (prices > 0)
                        {
                            if (string.Equals(symbols, _MT5._GOLD))
                            {
                                _MT5._GOLD_BID_PRICE = prices;
                                _MT5._GOLD_ASK_PRICE = pricesAsk;
                                _StrategyConfig.GOLD_PRICE = _MT5._GOLD_BID_PRICE;
                            }
                            if (string.Equals(symbols, _MT5._SILVER))
                            {
                                _MT5._SILVER_BID_PRICE = prices;
                                _MT5._SILVER_ASK_PRICE = pricesAsk;
                                _StrategyConfig.SILVER_PRICE = _MT5._SILVER_BID_PRICE;
                            }
                            saveQuoteGS(_MT5._GOLD_BID_PRICE, _MT5._SILVER_BID_PRICE);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 保存实时报价的间隔（毫秒）
        /// </summary>
        private int saveQuoteDuration = 10 * 1000;

        /// <summary>
        /// 0=空闲，1=执行中
        /// </summary>
        private int saveQuoteFlag = 0;

        /// <summary>
        /// 上一次保存时间（毫秒）
        /// </summary>
        private long lastSaveTime = 0;

        /// <summary>
        /// 同时保存黄金和白银报价
        /// </summary>
        private void saveQuoteGS(double goldPrices, double silverPrices)
        {
            if (goldPrices <= 0 || silverPrices <= 0)
                return;

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // =========================
            // 1. 时间节流（快速判断）
            // =========================
            if (now - lastSaveTime < saveQuoteDuration)
                return;

            // =========================
            // 2. 原子锁（防并发）
            // =========================
            if (Interlocked.CompareExchange(ref saveQuoteFlag, 1, 0) != 0)
                return;

            try
            {
                // =========================
                // 3. 双重检查（防止并发穿透）
                // =========================
                now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (now - lastSaveTime < saveQuoteDuration)
                    return;

                // 更新时间
                lastSaveTime = now;

                updateDsRunningStatus("Q", "2");

                DBHelper.saveQuoteGS(goldPrices, silverPrices, _Account.BrokerCode);
            }
            finally
            {
                // 释放锁（立即释放，不再Sleep）
                Interlocked.Exchange(ref saveQuoteFlag, 0);
            }
        }



        public void updatePrice(double price)
        {
            if (label_BrokePrice.InvokeRequired)
            {
                label_BrokePrice.Invoke(new Action<double>(updatePrice), new object[] { price });
            }
            else
            {
                label_BrokePrice.Text = price.ToString("f2");
            }          
        }
        /// <summary>
        /// 采集数据状态变化
        /// </summary>
        /// <param name="status"></param>
        public void updateDsRunningStatus(string type,string status)
        {
            if (label_BrokePrice.InvokeRequired)
            {
                label_dataCollectionStatus.Invoke(new Action<string,string>(updateDsRunningStatus), new object[] { type, status });
            }
            else
            {
                string time = DateTime.Now.ToString("HH:mm:ss.fff");
                string statusDesc = "No Running";
                if (string.Equals(status,"1"))
                {
                    statusDesc = "Running";
                }
                if (string.Equals(status, "2"))
                {
                    statusDesc = "Data Saving";
                }
                if (string.Equals(status, "3"))
                {
                    statusDesc = "Data Save Success";
                }
                if (string.Equals(type,"Q"))
                {
                    label_dataCollectionStatus.Text = time + " " + statusDesc;
                }
                if (string.Equals(type, "JR"))
                {
                    label_JRCollectionStatus.Text = time + " " + statusDesc;
                }
                if (string.Equals(type, "WZ"))
                {
                    label_WZCollectionStatus.Text = time + " " + statusDesc;
                }
            }
        }


        private void button_AccountSave_Click(object sender, EventArgs e)
        {
            if (textBox_account.Text == null || textBox_account.Text == "")
            {
                MessageBox.Show("账号不能为空");
                return;
            }
            if (textBox_password.Text == null || textBox_password.Text == "")
            {
                MessageBox.Show("密码不能为空");
                return;
            }
            SaveConfig();
            _Log.LogInfo("保存成功");
        }

        private void DataLoadForm_Closed(object sender, FormClosedEventArgs e)
        {
            SaveConfig();
        }
        private void button_savePlatformConfig_Click(object sender, EventArgs e)
        {
            SaveConfig();
            _Log.LogInfo("保存成功");
        }

        /// <summary>
        /// 检查平台连接是否正常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerCheckPlatformConnectStatus_Tick(object sender, EventArgs e)
        {
            //加载完成后,回测数据时不执行如下操作
            if (_IsLoadConfigCompleted)
            {
                bool isTradeTime = Utils.checkIsTradeTime(_Config.SysConfig["EANotradeDuration"], DBUtils.getDateTime());
                //_Log.LogInfo("平台拉起判断交易时间参数:[EANotradeDuration=" + _Config.SysConfig["EANotradeDuration"] + "][TradeTime=" + _Config.SysConfig["TradeTime"] + "][isTradeTime=" + isTradeTime + "]");
                //在EA策略执行时间内,MT4平台才启动自动拉起
                if (isTradeTime)
                {
                    switch (_Platform.PlatformNo)
                    {
                        case (int)uClient.Comm.Enum.Platform.MT4:
                            if (!_MT4.isConnect() || string.Equals(label_BrokePrice.Text, "0"))
                            {
                                _Log.LogInfo("MT4平台自动断开连接,由系统自动拉起开始");
                                //先断开连接,再重新连接
                                _Log.LogInfo("断开连接");
                                //先断开平台连接,再启动连接
                                _MT4.DisConnect();
                                //连接平台
                                Task.Run(() => {
                                    _MT4.Connect();
                                    if (isConnect())
                                    {
                                        AfterConnectCompleted();
                                    }
                                    _Log.LogInfo("MT4平台自动断开连接,由系统自动拉起完成");
                                });                                
                            }
                            break;
                        case (int)uClient.Comm.Enum.Platform.MT5:
                            if (!_MT5.isConnect() || string.Equals(label_BrokePrice.Text, "0"))
                            {
                                _Log.LogInfo("MT5平台自动断开连接,由系统自动拉起开始");
                                //先断开连接,再重新连接
                                _Log.LogInfo("断开连接");
                                //先断开平台连接,再启动连接
                                Task.Run(() => {
                                    _MT5.DisConnect();
                                    //连接平台
                                    _MT5.Connect();
                                    if (isConnect())
                                    {
                                        AfterConnectCompleted();
                                    }
                                    _Log.LogInfo("MT5平台自动断开连接,由系统自动拉起完成");
                                });                              
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// JR 情绪指数采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_JR_Tick(object sender, EventArgs e)
        {
            _Log.LogInfo("JR数据采集开始");
            updateDsRunningStatus("JR","1");
            Task.Run(() => {
                updateDsRunningStatus("JR", "2");
                HTTPHelper.JRSummaryData();
                updateDsRunningStatus("JR", "3");
            });            
            _Log.LogInfo("JR数据采集完成");
        }
        /// <summary>
        /// 万州情绪指数采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_WZ_Tick(object sender, EventArgs e)
        {
            _Log.LogInfo("WZ数据采集开始");
            updateDsRunningStatus("WZ", "1");
            Task.Run(() => {
                updateDsRunningStatus("WZ", "2");
                HTTPHelper.WZSummaryData();
                updateDsRunningStatus("WZ", "3");
            });           
            _Log.LogInfo("WZ数据采集完成");
        }

        private void timer_DataCenter_Tick(object sender, EventArgs e)
        {
            _Log.LogInfo("DataCenter数据采集开始");
            Task.Run(() => {
                HTTPHelper.DataCenterData();
            });           
            _Log.LogInfo("DataCenter数据采集完成");
        }
        public async Task ClearDataAsync()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            // 启动调度器
            await scheduler.Start();

            // 定义任务
            IJobDetail job = JobBuilder.Create<WeeklyTask>()
                .WithIdentity("weeklyTask", "group1")
                .Build();

            // 定义触发器
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("weeklyTrigger", "group1")
                .StartNow()
                .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Sunday, 9, 0))
                //.WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Monday, 23, 26))
                .Build();

            // 将任务和触发器加入调度器
            await scheduler.ScheduleJob(job, trigger);
            //Console.WriteLine("Task scheduled. Press [Enter] to exit.");
            //Console.ReadLine();
            // 关闭调度器
            //await scheduler.Shutdown();
        }

        private void timer_Dukascopy_Tick(object sender, EventArgs e)
        {
            _Log.LogInfo("Dukascopy数据采集开始");
            Task.Run(() => {
                HTTPHelper.DukascopySummaryData();
            });
            _Log.LogInfo("Dukascopy数据采集完成");
        }
        /// <summary>
        /// 异常后的提醒次数
        /// </summary>
        int KLineFailedNotifyCount = 10;
        /// <summary>
        /// 日志输出次数
        /// </summary>
        int KLineNotifyLogCount = 10;
        private void timer_KLineCheck_Tick(object sender, EventArgs e)
        {
            bool isTradeTime = Utils.checkIsTradeTime(_Config.SysConfig["EANotradeDuration"], DBUtils.getDateTime());
            if (isTradeTime) 
            {
                bool flag = Utils.checkKLineCollectionStatus();
                if (!flag)
                {
                    if (KLineFailedNotifyCount > 0)
                    {
                        string[] mails = new string[] { "caiqiufu@hotmail.com"};
                        StringBuilder content = new StringBuilder();
                        content.AppendFormat("【数据采集提醒】[{0}]", "K线数据采集失败,时间：" + DBUtils.getDateTime());
                        EmailHelper.SendEmail(mails, "【深圳龙知易科技】异常提醒", content.ToString());
                        KLineFailedNotifyCount--;
                        _Log.LogInfo("K线数据采集异常");
                        KLineNotifyLogCount = 10;
                    }
                }
                else
                {
                    KLineFailedNotifyCount = 10;
                    if (KLineNotifyLogCount > 0)
                    {
                        _Log.LogInfo("K线数据采集正常");
                        KLineNotifyLogCount--;
                    }
                }
                bool flagQuota = Utils.checkQuotaCollectionStatus();
                if (!flagQuota)
                {
                    if (KLineFailedNotifyCount > 0)
                    {
                        _Log.LogInfo("报价线数据采集异常,重新拉起平台链接");
                        switch (_Platform.PlatformNo)
                        {
                            case (int)uClient.Comm.Enum.Platform.MT4:
                                if (!_MT4.isConnect() || string.Equals(label_BrokePrice.Text, "0"))
                                {
                                    _Log.LogInfo("MT4平台自动断开连接,由系统自动拉起开始");
                                    //先断开连接,再重新连接
                                    _Log.LogInfo("断开连接");
                                    //先断开平台连接,再启动连接
                                    _MT4.DisConnect();
                                    //连接平台
                                    Task.Run(() =>
                                    {
                                        _MT4.Connect();
                                        if (isConnect())
                                        {
                                            AfterConnectCompleted();
                                        }
                                        _Log.LogInfo("MT4平台自动断开连接,由系统自动拉起完成");
                                    });
                                }
                                break;
                            case (int)uClient.Comm.Enum.Platform.MT5:
                                if (!_MT5.isConnect() || string.Equals(label_BrokePrice.Text, "0"))
                                {
                                    _Log.LogInfo("MT5平台自动断开连接,由系统自动拉起开始");
                                    //先断开连接,再重新连接
                                    _Log.LogInfo("断开连接");
                                    //先断开平台连接,再启动连接
                                    Task.Run(() =>
                                    {
                                        _MT5.DisConnect();
                                        //连接平台
                                        _MT5.Connect();
                                        if (isConnect())
                                        {
                                            AfterConnectCompleted();
                                        }
                                        _Log.LogInfo("MT5平台自动断开连接,由系统自动拉起完成");
                                    });
                                }
                                break;
                        }

                        string[] mails = new string[] { "caiqiufu@hotmail.com"};
                        StringBuilder content = new StringBuilder();
                        content.AppendFormat("【数据采集提醒】[{0}]", "报价数据采集失败,时间：" + DBUtils.getDateTime()+",已启动平台自动拉起功能");
                        EmailHelper.SendEmail(mails, "【深圳龙知易科技】异常提醒", content.ToString());
                        KLineFailedNotifyCount--;
                        _Log.LogInfo("报价数据采集异常");
                        KLineNotifyLogCount = 10;
                    }
                    else
                    {
                        KLineFailedNotifyCount = 10;
                        if (KLineNotifyLogCount > 0)
                        {
                            _Log.LogInfo("报价线数据采集正常");
                            KLineNotifyLogCount--;
                        }
                    }                   
                }

            }           
        }
    }
    // 定义任务
    public class WeeklyTask : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            //Console.WriteLine($"Task executed at: {DateTime.Now}");
            //DBHelper.ClearData();
            return Task.CompletedTask;
        }
    }
}
