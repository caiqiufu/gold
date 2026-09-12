using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using NLog;
using Quartz.Impl;
using Quartz;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using uClient.Comm;
using System.Security.Policy;


namespace uClient.Broker
{
    public partial class DataSourceForm : Form
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
        public DataSourceForm(MainFormPara mainFormPara)
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
        public DataSourceForm()
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
            if (_Broker != null)
            {
                //先初始化平台对象,UI加载后再修改平台应用对象
                GeneratePlatformObj();
                IniUI();
            }
            InitialPara();
            _Log.LogInfo("加载Form完成");
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
            //平台设置

            //锁单设置

            //交易商
            comboBox_Broker.Items.Clear();
            foreach (Comm.Broker bro in _Platform.Broker)
            {
                comboBox_Broker.Items.Add(bro.BrokerName);
            }
 
            //默认选择的平台
            comboBox_Broker.SelectedItem = _Broker.BrokerName;


            //用户账号密码
            textBox_UserCode.Text = _Account.UserCode;
            textBox_Password.Text = _Account.Password;
            //初始化订单列表
            //停止按钮颜色改变事件
            //锁单设置
            //平台设置
            //基础配置数据
            checkBox_NotifyFlag.Checked = _StrategyConfig.NotifyFlag;
        }
        /// <summary>
        /// 加载配置参数
        /// </summary>
        public void LoadConfig()
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
                                //指定平台的Live账户和Symbol
                                _Broker.DefaultAccountType = "Live";
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
            _logger.Info("开始加载配置参数完成");
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
                    _Account.BrokerCode = _Broker.BrokerCode;
                    _Account.UserCode = textBox_UserCode.Text;
                    _Account.Password = textBox_Password.Text;
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
                            _MT4._TradeSymbol = _TradeSymbol;
                            _MT4._QuoteData = new QuotePanelData(_TradeSymbol, 0.1);
                            _MT4.Connect();
                            _MT4._IsManullyDisconnect = false;
                            break;
                        case (int)Comm.Enum.Platform.MT5:
                            _MT5._TradeSymbol = _TradeSymbol;
                            _MT5._QuoteData = new QuotePanelData(_TradeSymbol, 0.1);
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
            textBox_UserCode.Enabled = !status;
            textBox_Password.Enabled = !status;
            AutoLockBind(status);
        }
        private void bgwBroberQuote_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Task.Run(() => {
                BroberQuote();
            });
        }           
        /// <summary>
        /// 平台商报价刷新
        /// </summary>
        public void BroberQuote()
        {
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
                            Task.Run(() =>
                            {
                                sendMessage(prices, pricesAsk);
                            });
                            Task.Run(() =>
                            {
                                updatePrice(prices);
                            });                            
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
                            Task.Run(() =>
                            {
                                sendMessage(prices, pricesAsk);
                            });
                            Task.Run(() =>
                            {
                                updatePrice(prices);
                            });
                        }
                    }
                    break;
            }
        }

        bool saveQuoteFlag = true;
        /// <summary>
        /// 保存实时报价的间隔
        /// </summary>
        int saveQuoteDuration = 1 * 60 * 1000;

        /// <summary>
        /// 发送价格消息
        /// </summary>
        /// <param name="Bid"></param>
        /// <param name="Ask"></param>
        public void sendMessage(double Bid, double Ask)
        {
            string strLastVolTotal = "";
            string symbol = "Gold";
            _MasterPublisher.SendFrame($"{symbol} {Bid} {Ask} {strLastVolTotal}");
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

        private void button_AccountSave_Click(object sender, EventArgs e)
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
                bool isTradeTime = Utils.checkIsTradeTime(_Config.SysConfig["EANotradeDuration"], DateTime.Now.ToString(), _Config.SysConfig["TradeTime"]);
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
                                _MT4.Connect();
                                if (isConnect())
                                {
                                    AfterConnectCompleted();
                                }
                                _Log.LogInfo("MT4平台自动断开连接,由系统自动拉起完成");
                            }
                            break;
                        case (int)uClient.Comm.Enum.Platform.MT5:
                            if (!_MT5.isConnect() || string.Equals(label_BrokePrice.Text, "0"))
                            {
                                _Log.LogInfo("MT5平台自动断开连接,由系统自动拉起开始");
                                //先断开连接,再重新连接
                                _Log.LogInfo("断开连接");
                                //先断开平台连接,再启动连接
                                _MT5.DisConnect();
                                //连接平台
                                _MT5.Connect();
                                if (isConnect())
                                {
                                    AfterConnectCompleted();
                                }
                                _Log.LogInfo("MT5平台自动断开连接,由系统自动拉起完成");
                            }
                            break;
                    }
                }
            }
        }

        private void comboBox_Broker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_IsLoadConfigCompleted)
            {
                string brokerName = comboBox_Broker.SelectedItem.ToString();
                uClient.Comm.Broker bb = _Platform.GetBrokerByName(brokerName);
                _Platform.DefaultBrokerCode = bb.BrokerCode;
                _Broker = bb;
                _Broker.DefaultAccountType = "Live";
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

                    textBox_UserCode.Text = _Account.UserCode;
                    textBox_Password.Text = _Account.Password;                    
                }
                //加载了默认的平台信息,需要重新加载平台对象
                SaveConfig();
            }
        }

        /// <summary>
        /// 消息发送绑定
        /// </summary>
        public void AutoLockBind(bool isActive)
        {
            //string port = Utils.GetPort(_MainFormPara.FormNo);
            string port = "5558";
            //string address = _Config.PublishAddressEAS;
            string address = string.Format("tcp://{0}:{1}", Utils.GetLocalIP(), port);
            if (isActive)
            {               
                try
                {
                    _MasterPublisher.Unbind(address);
                    _Log.LogInfo("取消消息绑定，地址:" + address);
                    _MasterPublisher.Bind(address);
                    _Log.LogInfo("消息发送绑定成功，窗口:" + _MainFormPara.FormNo + ",地址:" + address);
                }
                catch (Exception e2)
                {
                    _Log.LogInfo("消息发送取消[" + address + "]异常:" + e2.Message);
                    _MasterPublisher.Bind(address);
                    _Log.LogInfo("消息发送绑定成功，窗口:" + _MainFormPara.FormNo + ",地址:" + address);
                }
            }
            else
            {
                try
                {
                    _MasterPublisher.Unbind(address);
                    _Log.LogInfo("取消消息绑定，地址:" + address);
                }
                catch (Exception e)
                {
                    _Log.LogInfo("取消绑定[" + address + "]异常:" + e.Message);
                }
            }
        }
    }
}
