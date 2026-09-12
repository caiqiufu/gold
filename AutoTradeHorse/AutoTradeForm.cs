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
using Newtonsoft.Json.Linq;
using System.Management;
using java.util;
using System.Security.Principal;
using System.Globalization;
using System.ServiceModel.Channels;
using static ICSharpCode.SharpZipLib.Zip.ZipEntryFactory;
using System.Text;
using System.Media;
using System.Web.UI.WebControls;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices.ActiveDirectory;
using Google.Protobuf;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;


namespace uClient.Broker
{
    public partial class AutoTradeForm : Form
    {
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
        /// Tickets 列表
        /// </summary>
        public uClient.Comm.Log _Tickets;

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

        /// <summary>
        /// 回测交易日志目录，每次回测的所有文件生成在同一个目录中
        /// </summary>
        private string _SimulateTestTradeInfoDirectoryPath = "";
        private string _SimulateTestDate = "";

        public AutoTradeForm(MainFormPara mainFormPara)
        {
            if (mainFormPara == null)
            {
                mainFormPara = new MainFormPara();
                mainFormPara.PlatformCode = "H";
                mainFormPara.BrokerCode = "";
                mainFormPara.BrokerName = "";
                mainFormPara.FormType = "";
                mainFormPara.FormNo = "下注数据采集平台";
            }
            _MainFormPara = mainFormPara;
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            InitObject();
        }
        public AutoTradeForm()
        {
            MainFormPara mainFormPara = new MainFormPara();
            mainFormPara.PlatformCode = "H";
            mainFormPara.BrokerCode = "";
            mainFormPara.BrokerName = "";
            mainFormPara.FormType = "";
            mainFormPara.FormNo = "下注数据采集平台";
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
            _Log = new Log(lstTradeRecord, lstLog);
            //Ticktes List
            _Tickets = new Log(listTicketsList, lstLog);
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
            timer_Token.Enabled = true;
            timer_Token.Interval = 1000 * 60 * 9;
            _Log.LogInfo("timer_Token时钟启动,执行周期[" + timer_Token.Interval / 1000 / 60 + "]min");
            timer_Token.Stop();
            _Log.LogInfo("timer_Token时钟启动已启动执行");
            timer_TicketList.Enabled = true;
            timer_TicketList.Interval = 1000 * 30;           
            _Log.LogInfo("timer_TicketList时钟启动,执行周期[" + timer_TicketList.Interval/1000 + "]s");
            timer_TicketList.Stop();
            
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
            
        }
        public string rootPath = "C:\\iAutoTrade";
        /// <summary>
        /// 初始化UI参数
        /// </summary>
        public void IniUI()
        {
            _logger.Info("开始初始化UI");
            uClient.Comm.Broker hkhBroker = _Platform.BrokerList["HKH"];
            textBox_account.Text = hkhBroker.LiveAccount.UserCode;
            textBox_password.Text = hkhBroker.LiveAccount.Password;
            //跑马时间
            textBox_RaceTime.Text = hkhBroker.DefaultSymbol;
            //最后采集下注列表时间段
            comboBox_LastBetTimeDuration.SelectedItem = Convert.ToInt32(_TradePara.AutoLockTimeDuration).ToString();
            //最后采集下注采集时间点,开赛前x秒
            comboBox_LastBetTime.SelectedItem = _TradePara.AutoCloseTimeDuration;
            //下注价格检查,10K
            if (_TradePara.QuotaCheckValue <= 1)
            {
                _TradePara.QuotaCheckValue = 1;
            }

            numericUpDown_lastMaxBetAmount.Value = _TradePara.QuotaCheckValue;
            //最大下注数量
            if (_TradePara.AutoLockPoint <= 1)
            {
                _TradePara.AutoLockPoint = 10;
            }
            numericUpDown_MaxBets.Value = _TradePara.AutoLockPoint;

            //基础配置数据
            checkBox_NotifyFlag.Checked = _StrategyConfig.NotifyFlag;
            _Context["RaceTime"] = textBox_RaceTime.Text;
            _Context["LastBetTimeDuration"] = comboBox_LastBetTimeDuration.Text;
            _Context["LastBetTime"] = comboBox_LastBetTime.Text;
            _Context["LastMaxBetAmount"] = numericUpDown_lastMaxBetAmount.Value.ToString();
            _Context["MaxBets"] = numericUpDown_MaxBets.Value.ToString();
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
                        LoadBetPlatform();
                        _TradePara = _Platform.TradePara;
                        //修改默认平台商,数据采集只用金荣和万州两个平台 WZ0,JRJR,WCG
                        _Platform.DefaultBrokerCode = "HKH";
                        if (_Platform.BrokerList.Keys.Contains(_Platform.DefaultBrokerCode))
                        {
                            string defaultBrokerCode = string.IsNullOrEmpty(_MainFormPara.BrokerCode) ? _Platform.DefaultBrokerCode : _MainFormPara.BrokerCode;
                            _Broker = _Config.PlatformList[_MainFormPara.PlatformCode].BrokerList[defaultBrokerCode];
                            if (_Broker != null)
                            {
                                _Account = _Broker.LiveAccount;
                                _TradeSymbol = _Broker.DefaultSymbol;
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
        /// 加载赛马平台信息
        /// </summary>
        public void LoadBetPlatform()
        { 
            if (!_Config.PlatformList.ContainsKey("H"))
            {
                Platform horsePlatform = new Platform();
                horsePlatform.PlatformNo = 5;
                horsePlatform.PlatformName = "跑马";
                horsePlatform.PlatformCode = "H";
                horsePlatform.DefaultBrokerCode = "CC";
                _TradePara = new TradeParametre();
                //最后采集下注列表时间
                _TradePara.AutoLockTimeDuration = 5;
                //最大下注数量
                _TradePara.AutoLockPoint = 10;
                //下注价格检查,10K
                _TradePara.QuotaCheckValue = 10;
                //启动自动交易
                _TradePara.AutoTrade = true;
                _Platform.TradePara = _TradePara;
                _Config.PlatformList.Add(horsePlatform.PlatformCode, horsePlatform);
                _Config.Platform.Add(horsePlatform);
                _Platform = horsePlatform;
            }
            if (!_Platform.BrokerList.ContainsKey("HKH")) {
                uClient.Comm.Broker hkhBroker = new uClient.Comm.Broker();
                hkhBroker.BrokerCode = "HKH";
                hkhBroker.BrokerName = "香港赛马";
                hkhBroker.DefaultAccountType = "Live";
                hkhBroker.PlatformNo = 5;
                //赛马开始时间,10:00,12:00,16:50
                hkhBroker.DefaultSymbol = "10:00,12:00,16:50";
                _Platform.BrokerList.Add("HKH", hkhBroker);
                _Platform.Broker.Add(hkhBroker);
                Account liveAccount = new Account();
                liveAccount.BrokerCode = "HKH";
                liveAccount.UserCode = "";
                liveAccount.Password = "";
                liveAccount.Type = "Live";
                liveAccount.TradePara = _TradePara;
                hkhBroker.LiveAccount = liveAccount;
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
                    uClient.Comm.Broker hkhBroker = _Platform.BrokerList["HKH"];
                    hkhBroker.LiveAccount.UserCode = textBox_account.Text;
                    hkhBroker.LiveAccount.Password = textBox_password.Text;
                    hkhBroker.DefaultSymbol = textBox_RaceTime.Text;
                    //最后采集下注列表时间
                    _TradePara.AutoLockTimeDuration = Convert.ToDecimal(comboBox_LastBetTimeDuration.Text);
                    //最后采集下注采集时间点,开赛前x秒
                    _TradePara.AutoCloseTimeDuration = comboBox_LastBetTime.Text;
                    //下注价格检查,10K
                    _TradePara.QuotaCheckValue = numericUpDown_lastMaxBetAmount.Value;
                    //最大下注数量
                    _TradePara.AutoLockPoint = numericUpDown_MaxBets.Value;
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
        /// 自动绑定锁单消息,在平台连接完成后自动启动
        /// </summary>
        public void AutoLockBind(bool isActive)
        {
            if (isActive)
            {
                //string port = Utils.GetPort(_MainFormPara.FormNo);
                string port = "5653";
               //string port = uClient.Comm.Utils.GetPort(_MainFormPara.FormNo);
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
                string port = "5653";
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
        /// 赛马平台连接状态
        /// </summary>
        public bool _HorsePlatformConnectStatus = false;
        /// <summary>
        /// 是否连接
        /// </summary>
        /// <returns></returns>
        public bool IsConnect()
        {           
            return _HorsePlatformConnectStatus;
        }
        /// <summary>
        /// 连接成功后
        /// </summary>
        public void AfterConnectCompleted()
        {
            if (button_platformConnect.Text.Equals("数据源连接"))
            {
                //2.更新连接状态
                UpdateConnectStatus(true);
                AutoLockBind(true);
            }
            else 
            {
                //2.更新连接状态
                UpdateConnectStatus(false);
                AutoLockBind(false);
            }         
        }
        /// <summary>
        /// Token,十分钟刷新一次
        /// </summary>
        public string _Token = "";
        /// <summary>
        /// 根据场次获取的最新数据
        /// </summary>
        public string[] _GetFirstPage = new string[] {"","" };
        /// <summary>
        /// 当前赛马场次,场次切换后需要重新获取_GetFirstPage 数据
        /// </summary>
        public string _CurrentRace = "";
        /// <summary>
        /// 获取Token
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string GetToken(string userCode, string password)
        {
            //HTTPUtils.GetToken();
            _Token = HTTPUtils.HorseGetToken(userCode, password);
            if (string.IsNullOrEmpty(_Token))
            {
                _Log.LogInfo("获取Token失败");
                _Token = null;
                return null;
            }
            else
            {
                _Log.LogInfo("获取Token完成");
                return _Token;
            }

        }
        private void button_platformConnect_Click(object sender, EventArgs e)
        {
            uClient.Comm.Broker hkhBroker = _Platform.BrokerList["HKH"];
            if (string.IsNullOrEmpty(hkhBroker.LiveAccount.UserCode))
            {
                _Log.LogInfo("账号不能为空");
                MessageBox.Show("账号不能为空");
                return;
            }
            if (string.IsNullOrEmpty(hkhBroker.LiveAccount.Password))
            {
                _Log.LogInfo("密码不能为空");
                MessageBox.Show("密码不能为空");
                return;
            }
            GetToken(hkhBroker.LiveAccount.UserCode, hkhBroker.LiveAccount.Password);
            if (button_platformConnect.Text.Equals("数据源连接"))
            {
                AfterConnectCompleted();
                //注意：需要在相同的线程中stop/start timer,否则不生效
                timer_Token.Start();
                timer_TicketList.Start();
                _Log.LogInfo("数据源采集启动");
                if (bgwBroberQuote.IsBusy != true) 
                {
                    bgwBroberQuote.RunWorkerAsync();
                }
            }
            else if (button_platformConnect.Text.Equals("数据源断开"))
            {
                timer_Token.Stop();
                timer_TicketList.Stop();
                _Log.LogInfo("数据源采集断开");
                bgwBroberQuote.CancelAsync();
                AfterConnectCompleted();
            }            
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
            _HorsePlatformConnectStatus = status;
            lblConnectStatus.Text = status ? "已连接" : "断开";
            lblConnectStatus.BackColor = status ? Color.Green : Color.Red;
            button_platformConnect.Text = !status ? "数据源连接" : "数据源断开";
            button_platformConnect.BackColor = !status ? Color.Red : Color.Green;
            textBox_account.Enabled = !status;
            textBox_password.Enabled = !status;
        }
        private void bgwBroberQuote_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Task.Run(() => {
                _Log.LogInfo("下注分析启动");
                button_executeAnalysis.Enabled = false;
                button_executeAnalysis.BackColor = Color.Red;
                BroberQuote();
                _Log.LogInfo("下注分析完成");
                button_executeAnalysis.Enabled = true;
                button_executeAnalysis.BackColor = Color.Green;
            });
        }
        /// <summary>
        /// 是否执行数据分析
        /// </summary>
        public bool _IsExcuteAnalysis = false;
        /// <summary>
        /// 执行状态,手动控制该状态
        /// true：继续执行
        /// false: 停止执行
        /// </summary>
        public bool _ExcuteAnalysisStatus = true;

        /// <summary>
        /// 赛马下注刷新,在最后指定的时间段内实时获取数据
        /// </summary>
        public void BroberQuote()
        {
            while (true)
            {
                if (!_ExcuteAnalysisStatus)
                {
                    break;
                }
                _IsExcuteAnalysis = true;
                if (!IsConnect()) 
                { 
                    break;
                }
                if (IsLastBetTimeDuration())
                {
                    _Log.LogInfo("执行下注分析");
                    _LastBetList.Clear();
                    List<Dictionary<string, object>> datas = GetTicketList();
                    _Log.LogInfo("获取最新下注数据");                   
                    //获取所有最后下注信息,就放入_LastBetList列表中,在达到下注时间时
                    if (datas != null && datas.Count > 0)
                    {   // 截取前 numericUpDown_MaxBets 个数据
                        if (datas.Count >= 100)
                        {
                            datas = datas.Take(100).ToList();
                        }
                        foreach (var item in datas)
                        {
                            if (string.Equals(item["BET_TYPE"].ToString(), "W") || string.Equals(item["BET_TYPE"].ToString(), "Q") || string.Equals(item["BET_TYPE"].ToString(), "P") || string.Equals(item["BET_TYPE"].ToString(), "QP"))
                            {
                                if (IsInBetTimeDuration(item["BET_TIMES"].ToString()))
                                {
                                    if (IsInBetAmount(item["BET_AMOUNT"].ToString()))
                                    {
                                        _LastBetList.Add(item);
                                    }
                                }
                            }
                        }
                    }
                    _Log.LogInfo("符合分析条件下注数据量[" + _LastBetList.Count + "]");
                    if (_LastBetList != null && _LastBetList.Count > 0)
                    {                      
                        List<string> results = new List<string>();
                        foreach (Dictionary<string, object> rr in _LastBetList)
                        {
                            results.Add(rr["BET_TYPE"].ToString() + "|" + rr["HORSE_NO"].ToString());
                        }
                        results = results.Distinct().ToList();
                        // 截取前 numericUpDown_MaxBets 个数据
                        if (results.Count >= Convert.ToInt32(numericUpDown_MaxBets.Value))
                        {
                            results = results.Take(Convert.ToInt32(numericUpDown_MaxBets.Value)).ToList();
                        }
                        string result = string.Join(", ", results);
                        ExecuteTrade(result);
                        Task.Run(() =>
                        {
                            System.DateTime currentTime = System.DateTime.Now;
                            string currentDate = currentTime.ToString("yyyy-MM-dd");
                            string rfileName = currentDate + "_" + "summary_HKH" + ".txt" ;
                            //输出结果
                            if (string.IsNullOrEmpty(_SimulateTestTradeInfoDirectoryPath))
                            {
                                _SimulateTestTradeInfoDirectoryPath = Utils.GenerateDirectoryPath("EATest");
                            }
                            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***赛事结果**********************************");
                            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "设置参数:时间["+ _GetFirstPage[1] + "]场次["+_GetFirstPage[0] + "]赛事时间[" + textBox_RaceTime.Text + "]最小跟注金额[" + numericUpDown_lastMaxBetAmount.Value.ToString() + "]跟注时间段[" + comboBox_LastBetTimeDuration.Text + "]最后跟注时间[" + comboBox_LastBetTime.Text + "]最大跟注数量[" + numericUpDown_MaxBets.Value + "]");
                            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***赛事结果**********************************");
                            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***跟注指令**********************************");
                            if (results != null && results.Count > 0)
                            {
                                foreach (var item in results)
                                {
                                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, item);
                                }
                            }
                            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***跟注指令**********************************");
                            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***跟注数据**********************************");
                            if (results != null && _LastBetList.Count > 0)
                            {
                                foreach (var item in _LastBetList)
                                {
                                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, string.Join(", ", item));
                                }
                            }
                            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***跟注数据**********************************");
                            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***所有下注数据**********************************");
                            if (datas != null && datas.Count > 0)
                            {
                                foreach (var item in datas)
                                {
                                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, string.Join(", ", item));
                                }
                            }
                            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***所有下注数据**********************************");
                        });
                    }
                    //执行一次后就退出
                    break;
                }
                if (IsOverBetTime())
                {
                    _Log.LogInfo("当前时间已经超过分析时间,停止下注分析");
                    break;
                }
                Thread.Sleep(100);
            }
            _IsExcuteAnalysis = false;
        }

        /// <summary>
        /// 执行策略
        /// </summary>
        /// <param name="betInfo"></param>
        public void ExecuteTrade(string betInfo)
        {
            _Log.LogInfo("执行下注开始");
            string tradeInfo = "Trade Info:" + betInfo;
            string result = "HKH:" + betInfo;
            StringBuilder executeDesc = new StringBuilder();
            //HKH:2004.79:BUY:1:50:50 HH:mm:ss
            //executeDesc.AppendFormat("{0} {1}", result, DateTime.Now.ToString("HH:mm:ss"));
            executeDesc.AppendFormat("{0} {1} {2}", result, DateTime.Now.ToString("HH:mm:ss"), "");
            string message = executeDesc.ToString();
            try
            {
                _Log.LogInfo("发送指令消息开始:" + message);
                _MasterPublisher.SendFrame(message);
                _Log.LogInfo("发送指令消息结束");
            }
            catch (NetMQ.FaultException ex)
            {
                _Log.LogInfo("发送消息异常:" + ex.ToString());
            }
            catch (Exception ex)
            {
                _Log.LogInfo("发送消息异常:" + ex.ToString());
            }
            _Log.LogInfo(tradeInfo);
            _Log.LogTradeRecord("", tradeInfo);
            Task.Run(() =>
            {
                _Log.LogInfo("保存EA策略开始");
                DBHelper.saveEACommand("-1", "Y", JsonConvert.SerializeObject(_StrategyConfig), JsonConvert.SerializeObject(_Context), "", message, "HKH");
                _Log.LogInfo("保存EA策略完成");
                string eaType = _StrategyConfig.analysisDataType;
                DBHelper.saveNotify("EA", eaType, message, "", "");
                SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\Ring08.wav");
                player.PlaySync();
                if (_StrategyConfig.NotifyFlag)
                {
                    string content = DateTime.Now.ToString("HH:mm:ss.fff") + " " + message;
                    EmailHelper.SendEmail(_MyConfig.NotifyEmail, "EA策略提醒", content);
                    _Log.LogInfo("发送EA策略提醒完成");
                }
                else
                {
                    _Log.LogInfo("EA策略提醒未开启");
                }
            });
            _Log.LogInfo("执行下注完成");
        }
        /// <summary>
        /// 下注记录是否在指定的分析时间区间内
        /// </summary>
        /// <param name="betTime">Wed Mar 05 22:25:30 GMT+08:00 2025</param>
        /// <returns></returns>
        public bool IsInBetTimeDuration(string betTime)
        {
            //Wed Mar 05 22:25:30 GMT+08:00 2025
            //2025-03-08 15:30:58.0
            if (betTime.Contains("GMT"))
            {
                betTime = betTime.Split(' ')[3];
            }
            else 
            {
                betTime = betTime.Split(' ')[1];
            }
            //_Log.LogInfo("待分析下注时间["+ betTime + "]");
                // 指定的时间字符串
                //string targetTimesStr = "10:00,12:00,16:50";
                // 设定时间范围（秒）
                //int rangeInSeconds = 120; // 例如 120 秒（2 分钟）

                //10:00,12:00,16:50
            string raceTime = textBox_RaceTime.Text;
            string closestPastTime = GetClosestPastTime(raceTime);

            int lastBetTime = Convert.ToInt32(comboBox_LastBetTime.Text);
            int lastBetTimeDuration = Convert.ToInt32(comboBox_LastBetTimeDuration.Text);

            //string targetTimeStr = "10:00:00";  // 指定的目标时间
            //int lastBetTimeDuration = 300;   // 指定时间前的秒数（单位：秒），如300秒（5分钟）
            //string checkTimeStr = "09:25:00";   // 需要判断的时间

            bool isInRange = IsTimeInBetRange(closestPastTime, lastBetTime, lastBetTimeDuration, betTime);
            //_Log.LogInfo("下注时间[" + betTime + "]" + (isInRange ? "在" : "不在") + "分析时间区间["+ lastBetTimeDuration + "s]["+ closestPastTime + "]内");
            return isInRange;
        }
        /// <summary>
        /// 判断下注金额是否大于设置的下注金额
        /// </summary>
        /// <param name="betAmountStr"></param>
        /// <returns></returns>
        public bool IsInBetAmount(string betAmountStr)
        {
           int lastBetTimeDuration = Convert.ToInt32(numericUpDown_lastMaxBetAmount.Value);
            double betAmount = 0;
            if (betAmountStr.EndsWith("K"))
            {
                betAmountStr = betAmountStr.TrimEnd('K'); // 去掉 'K'
                betAmount = Convert.ToDouble(betAmountStr);
            }
            if (betAmountStr.EndsWith("M"))
            {
                betAmountStr = betAmountStr.TrimEnd('M'); // 去掉 'M'
                betAmount = Convert.ToDouble(betAmountStr) * 10; // 乘以10
            }
            return betAmount >= lastBetTimeDuration;
        }
        /// <summary>
        /// 判断当前时间是否是指定的最后下注时间区间内
        /// </summary>
        /// <returns></returns>
        public bool IsLastBetTimeDuration() 
        {
            //10:00:00,12:00:00,16:50:00
            string raceTime = textBox_RaceTime.Text;

            //string targetTimeStr = "10:00";  // 指定时间字符串
            int lastBetTime = Convert.ToInt32(comboBox_LastBetTime.Text);
            string closestPastTime = GetClosestPastTime(raceTime);
            

            bool isApproaching = IsApproachingTargetTime(closestPastTime, lastBetTime);
            //_Log.LogInfo("现在时间[" + DateTime.Now + "]"+(isApproaching?"在":"不在") +"下注分析时间区间内[" + lastBetTimeDuration + "][" + raceTime + "]");
            return isApproaching;
        }
        /// <summary>
        /// 判断当前时间是否已经超过指定时间5秒,如果已经超过就停止下注实时数据获取
        /// </summary>
        /// <returns></returns>
        public bool IsOverBetTime()
        {
            //10:00,12:00,16:50
            string raceTime = textBox_RaceTime.Text;
            string closestPastTime = GetClosestPastTime(raceTime);
            return IsExceededBy5Seconds(closestPastTime);
        }

        /// <summary>
        /// 判断给定时间是否在目标时间前 lastBetTimeDuration 秒的时间区间内
        /// </summary>
        public bool IsTimeInBetRange(string targetTimeStr,int lastBetTime, int lastBetTimeDuration, string checkTimeStr)
        {
            DateTime targetTime = DateTime.Today.Add(TimeSpan.Parse(targetTimeStr));   
            DateTime endTime = targetTime.AddSeconds(- lastBetTime);// 目标时间
            DateTime startTime = targetTime.AddSeconds(-lastBetTimeDuration- lastBetTime);          // 计算区间开始时间
            DateTime checkTime = DateTime.Today.Add(TimeSpan.Parse(checkTimeStr));     // 需要检查的时间

            return checkTime >= startTime && checkTime <= endTime;
        }

        /// <summary>
        /// 判断当前时间是否在 lastBetTimeDuration 秒内即将到达指定时间
        /// </summary>
        public bool IsApproachingTargetTime(string targetTimeStr, int lastBetTime)
        {
            DateTime now = DateTime.Now;
            DateTime targetTime = DateTime.Today.Add(TimeSpan.Parse(targetTimeStr));

            double timeDiff = (targetTime - now).TotalSeconds;

            return timeDiff > 0 && timeDiff <= lastBetTime;
        }

        /// <summary>
        /// 获取最接近当前时间的过去时间字符串
        /// </summary>
        public string GetClosestPastTime(string timeString)
        {
            var timeList = timeString.Split(',')
                             .Select(t => new
                             {
                                 TimeStr = t,
                                 Time = DateTime.Today.Add(TimeSpan.Parse(t))
                             })
                             .ToList();

            DateTime now = DateTime.Now;

            // 查找最接近当前时间的未来时间
            var closestFuture = timeList.Where(t => t.Time > now)
                                        .OrderBy(t => t.Time)
                                        .FirstOrDefault();

            // 如果所有时间都已过，返回最早的时间（第二天的）
            string closestPastTime = closestFuture?.TimeStr ?? timeList.Min(t => t.TimeStr);
            if (!string.Equals(_CurrentRace, closestPastTime))
            {
                //获取是赛马场次切换,需要重新获取参数
                _CurrentRace = closestPastTime;
                _GetFirstPage = HTTPUtils.GetFirstPage(_Token);
            }
            return closestPastTime;
        }
        /// <summary>
        /// 判断指定时间字符串是否已经超过当前时间 5 秒
        /// </summary>
        public bool IsExceededBy5Seconds(string timeStr)
        {
            DateTime targetTime = DateTime.Today.Add(TimeSpan.Parse(timeStr));
            DateTime now = DateTime.Now;
            bool isExceed = (now - targetTime).TotalSeconds > 5;
            //_Log.LogInfo("现在时间[" + now + "]"+(isExceed? "超过": "未超过") +"下注时间[" + timeStr + "]5秒");
            return (DateTime.Now - targetTime).TotalSeconds > 5;
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
        /// 状态变化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="status"></param>
        public void updateDsRunningStatus(string type,string status)
        {
            if (label_BrokePrice.InvokeRequired)
            {
                label_BrokePrice.Invoke(new Action<string,string>(updateDsRunningStatus), new object[] { type, status });
            }
            else
            {
                string time = DateTime.Now.ToString("HH:mm:ss.fff");
                string statusDesc = "No Running";
                if (string.Equals(status, "0"))
                {
                    statusDesc = "";
                }
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
                if (string.Equals(type,"H"))
                {
                    label_BrokePrice.Text = time + " " + statusDesc;
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
                bool isTradeTime = Utils.checkIsTradeTime(_Config.SysConfig["EANotradeDuration"], DateTime.Now.ToString(), _Config.SysConfig["TradeTime"]);
                //_Log.LogInfo("平台拉起判断交易时间参数:[EANotradeDuration=" + _Config.SysConfig["EANotradeDuration"] + "][TradeTime=" + _Config.SysConfig["TradeTime"] + "][isTradeTime=" + isTradeTime + "]");
                //在EA策略执行时间内,MT4平台才启动自动拉起
                if (isTradeTime)
                {
                    
                }
            }
        }

        /// <summary>
        /// 最新下注列表,根据设置的最后时间开始获取
        /// </summary>
        List<Dictionary<string, object>> _LastBetList = new List<Dictionary<string, object>>();

        /// <summary>
        /// 下注数据采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_TicketList_Tick(object sender, EventArgs e)
        {
            Task.Run(() => {
                updateDsRunningStatus("H", "0");
                List<Dictionary<string, object>> datas = GetTicketList();
                if (datas!=null && datas.Count>0)
                {
                    datas.Reverse();
                    _Tickets._LstTradeRecord.Items.Clear();
                    //显示最新50条数据
                    if (datas.Count > 50)
                    {                        
                        List<Dictionary<string, object>> last50 = datas.Take(50).ToList();                       
                        // 输出结果
                        foreach (var item in last50)
                        {
                            string times = item["BET_TIMES"].ToString();
                            if (times.Contains("GMT"))
                            {
                                times = times.Split(' ')[3];
                            }
                            else
                            {
                                times = times.Split(' ')[1];
                            }
                            _Tickets.LogTradeRecord("", times + " "+ item["BET_TYPE"] +" "+ item["HORSE_NO"] + " " + item["BET_AMOUNT"]);
                        }
                    }
                    else 
                    {
                        foreach (var item in datas)
                        {
                            string times = item["BET_TIMES"].ToString();
                            if (times.Contains("GMT"))
                            {
                                times = times.Split(' ')[3];
                            }
                            else
                            {
                                times = times.Split(' ')[1];
                            }
                            _Tickets.LogTradeRecord("", times + " " + item["BET_TYPE"] + " " + item["HORSE_NO"] + " " + item["BET_AMOUNT"]);
                        }
                    }
                    //保存数据到数据库
                    DBHelper.saveTickets(datas, _GetFirstPage[0], _GetFirstPage[1]);
                }
                
                if (!_IsExcuteAnalysis)
                {
                    if (IsCloseRaceTime2Min())
                    {
                        if (bgwBroberQuote.IsBusy != true)
                        {
                            bgwBroberQuote.RunWorkerAsync();
                        }
                    }                   
                }
            });            
        }

        /// <summary>
        /// 判断是否接近赛马时间,以便启动下注时间采集
        /// </summary>
        /// <returns></returns>
        public bool IsCloseRaceTime2Min()
        {
            //10:00,12:00,16:50
            string raceTime = textBox_RaceTime.Text;
            string closestPastTime = GetClosestPastTime(raceTime);
            bool isApproaching =  IsWithinTimeRange(closestPastTime,60,120);
            //_Log.LogInfo("现在时间[" + DateTime.Now + "]" + (isApproaching ? "在" : "不在") + "[" + closestPastTime + "]前[60-120秒]内");
            return isApproaching;
        }

        /// <summary>
        /// 判断当前时间是否在 给定时间的 60 秒到 120 秒之前 这个时间范围内
        /// </summary>
        /// <param name="timeString"></param>
        /// <param name="minSeconds"></param>
        /// <param name="maxSeconds"></param>
        /// <returns></returns>
        public bool IsWithinTimeRange(string timeString, int minSeconds, int maxSeconds)
        {
            // 当前时间（去掉毫秒）
            DateTime now = DateTime.Now.AddMilliseconds(-DateTime.Now.Millisecond);

            // 解析目标时间
            DateTime givenTime = DateTime.Today.Add(TimeSpan.Parse(timeString));

            // 计算时间区间
            DateTime rangeStart = givenTime.AddSeconds(-maxSeconds); // 120 秒前
            DateTime rangeEnd = givenTime.AddSeconds(-minSeconds);   // 60 秒前

            // 判断当前时间是否在范围内
            return now >= rangeStart && now <= rangeEnd;
        }

        /// <summary>
        /// 获取下注数据
        /// </summary>
        public List<Dictionary<string, object>> GetTicketList()
        {
            _Log.LogInfo("获取下注数据");
            _GetFirstPage = HTTPUtils.GetFirstPage(_Token);
            _Log.LogInfo("下注参数:[raceId="+ _GetFirstPage[0] + "][currentRaceDate="+ _GetFirstPage[1] + "]");
            List<List<string>> dataList = HTTPUtils.GetFirstData(_Token, _GetFirstPage[0], _GetFirstPage[1]);
            if (dataList != null)
            {
                List<Dictionary<string, object>> datas = new List<Dictionary<string, object>>();
                string datetime = DBUtils.getDateTime();
                // 遍历 Data 数组
                foreach (var item in dataList)
                {
                    /*
                     <tr valign="top" class="nt">	
                        <td width="15" style="border-right: solid 1px;border-right-color: black;">
                            &nbsp;
                        </td>                            
                        <td width="10">
                            &nbsp;
                        </td>
                        <td nowrap="nowrap" width="35" align="right" dir="ltr">
                            150K
                        </td>
                        <td nowrap="nowrap" width="45" align="left" dir="ltr" style="white-space: nowrap;">2</td>
                        <td nowrap="nowrap" width="23" align="left" dir="ltr"><b>W</b></td>
                        <td nowrap="nowrap" align="left" dir="ltr">22:28&nbsp;</td>
                        <!-- td nowrap="nowrap" align="left" dir="ltr">Wed Mar 05 22:28:54 GMT+08:00 2025 &nbsp;</td-->
                    </tr>
                    数组中的数据为表格中的数据逆序
                    历史数据
                    ||280K|7|W|22:25|Wed Mar 05 22:25:30 GMT+08:00 2025
                    实时数据          2025-03-08 15:30:58.0
                     */
                    Dictionary<string, object> data = new Dictionary<string, object>();
                    data.Add("RACE_ID", _GetFirstPage[0]);
                    data.Add("RACE_DATE", _GetFirstPage[1]);
                    data.Add("BET_TIME", item[5]);
                    data.Add("BET_TIMES", item[6]);
                    data.Add("BET_TYPE", item[4]);
                    data.Add("HORSE_NO", item[3]);
                    data.Add("BET_AMOUNT", item[2]);
                    data.Add("CREATE_DATE", datetime);
                    datas.Add(data);
                }
                return datas;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取Token
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Token_Tick(object sender, EventArgs e)
        {
            _Log.LogInfo("获取Token开始");
            Task.Run(() => {
                uClient.Comm.Broker hkhBroker = _Platform.BrokerList["HKH"];
                GetToken(hkhBroker.LiveAccount.UserCode, hkhBroker.LiveAccount.Password);
            });           
            _Log.LogInfo("获取Token完成");
        }

        private void timer_DataCenter_Tick(object sender, EventArgs e)
        {
            _Log.LogInfo("DataCenter数据采集开始");
            Task.Run(() => {
                HTTPHelper.DataCenterData();
            });           
            _Log.LogInfo("DataCenter数据采集完成");
        }

        private void button_simulateTest_Click(object sender, EventArgs e)
        {
            
            if (string.IsNullOrEmpty(_SimulateTestTradeInfoDirectoryPath))
            {
                _SimulateTestTradeInfoDirectoryPath = Utils.GenerateDirectoryPath("EATest");
            }
            if (string.IsNullOrEmpty(_SimulateTestDate))
            {
                System.DateTime currentTime = System.DateTime.Now;
                _SimulateTestDate = currentTime.ToString("yyyy-MM-dd");
            }
            
            string raceTimes = textBox_testRaceTime.Text;
            string fileName = textBox_filePatch.Text; // 从文本框获取文件名
            if (!string.IsNullOrEmpty(raceTimes) && !string.IsNullOrEmpty(fileName))
            {
                string[] raceTimeList = raceTimes.Split(',');
                for (int i = 0; i < raceTimeList.Length; i++)
                {
                    //fileName = fileName.Split('-')[0] + "-"+ (i+1) +".txt";
                    string sfileName = Regex.Replace(fileName, @"-(\d+)\.txt", $"-{i+1}.txt");
                    SimulateTest(raceTimeList[i], sfileName);
                }
                MessageBox.Show("回测完成", "提示");
                button_simulateTest.Enabled = true;
            }
            else
            {
                MessageBox.Show("赛马时间和赛马结果文件不正确", "提示");
                button_simulateTest.Enabled = true;
            }
        }
        /// <summary>
        /// 回测
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="raceTime"></param>
        public void SimulateTest(string raceTime, string fileName)
        {
            textBox_RaceTime.Text = raceTime;
            string testType = "HKH";
            double oddsAmount = 0;
            List<string> results = new List<string>();

            if (string.IsNullOrEmpty(fileName))
            {
                _Log.LogInfo("文件名不能为空");
                MessageBox.Show("文件名不能为空");
                return;
            }
            //string raceResult = textBox_raceResult.Text;
            string raceResult = GetBetResultJson(fileName);
            if (string.IsNullOrEmpty(raceResult))
            {
                _Log.LogInfo("赛果不能为空");
                MessageBox.Show("赛果不能为空");
                return;
            }
            button_simulateTest.Enabled = false;
            _LastBetList.Clear();
            _OddsResultList.Clear();
            List<List<string>> testDataList = HTTPUtils.GetSimulateDataList(fileName);
            if (testDataList != null && testDataList.Count > 0)
            {
                foreach (List<string> data in testDataList)
                {
                    _Log.LogInfo("回测数据:" + string.Join(",", data));
                }
            }
            List<Dictionary<string, object>> datas = new List<Dictionary<string, object>>();
            if (testDataList != null)
            {
                string datetime = DBUtils.getDateTime();
                // 遍历 Data 数组
                foreach (var item in testDataList)
                {
                    /*
                    数组中的数据为表格中的数据逆序
                    ||280K|7|W|22:25|Wed Mar 05 22:25:30 GMT+08:00 2025
                     */
                    Dictionary<string, object> data = new Dictionary<string, object>();
                    data.Add("RACE_ID", _GetFirstPage[0]);
                    data.Add("RACE_DATE", _GetFirstPage[1]);
                    data.Add("BET_TIME", item[5]);
                    data.Add("BET_TIMES", ConvertHistoryDatetime(fileName.Split('-')[0], item[6]));
                    data.Add("BET_TYPE", item[4]);
                    data.Add("HORSE_NO", item[3]);
                    data.Add("BET_AMOUNT", item[2]);
                    data.Add("CREATE_DATE", datetime);
                    datas.Add(data);
                }
                //获取所有最后下注信息,就放入_LastBetList列表中,在达到下注时间时
                if (datas != null && datas.Count > 0)
                {
                    foreach (var item in datas)
                    {
                        if (string.Equals(item["BET_TYPE"].ToString(), "W") || string.Equals(item["BET_TYPE"].ToString(), "Q") || string.Equals(item["BET_TYPE"].ToString(), "P") || string.Equals(item["BET_TYPE"].ToString(), "QP"))
                        {
                            if (IsInBetTimeDuration(item["BET_TIMES"].ToString()))
                            {
                                if (IsInBetAmount(item["BET_AMOUNT"].ToString()))
                                {
                                    _LastBetList.Add(item);
                                }
                            }
                        }
                    }
                    if (_LastBetList != null && _LastBetList.Count > 0)
                    {
                        // 截取前 numericUpDown_MaxBets 个数据
                        if (_LastBetList.Count >= Convert.ToInt32(numericUpDown_MaxBets.Value))
                        {
                            _LastBetList = _LastBetList.Take(Convert.ToInt32(numericUpDown_MaxBets.Value)).ToList();
                        }
                        results = new List<string>();
                        foreach (Dictionary<string, object> rr in _LastBetList)
                        {
                            results.Add(rr["BET_TYPE"].ToString() + "|" + rr["HORSE_NO"].ToString());
                        }
                        results = results.Distinct().ToList();
                        oddsAmount = CalculateOdds(raceResult, results);
                    }
                }
            }
            string rfileName = _SimulateTestDate + "_" + "summary_" + testType + "_" + fileName;
            //输出回测结果
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***回测结果**********************************");
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "总投注数:[" + results.Count + "]");
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "中奖数:[" + _OddsResultList.Count + "]");
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "未中数:[" + (results.Count - _OddsResultList.Count) + "]");
            string percentageStr = "0%";
            if (results.Count > 0)
            {
                double percentage = (double)_OddsResultList.Count / results.Count;
                percentageStr = (percentage * 100).ToString("F2") + "%";
            }
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "胜率:[" + percentageStr + "]");
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***回测结果**********************************");
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "回测参数:赛事时间[" + textBox_RaceTime.Text + "]最小跟注金额[" + numericUpDown_lastMaxBetAmount.Value.ToString() + "]跟注时间段[" + comboBox_LastBetTimeDuration.Text + "]最后跟注时间[" + comboBox_LastBetTime.Text + "]最大跟注数量[" + numericUpDown_MaxBets.Value + "]");
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "盈亏:[" + oddsAmount.ToString() + "]下注量[" + results.Count + "]");
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***跟注成功赔率**********************************");
            if (_OddsResultList != null && _OddsResultList.Count > 0)
            {
                foreach (var item in _OddsResultList)
                {
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, item);
                }
            }
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***跟注成功赔率**********************************");
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***所有跟注指令**********************************");
            if (results != null && results.Count > 0)
            {
                foreach (var item in results)
                {
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, string.Join(", ", item));
                }
            }
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***所有跟注指令**********************************");
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***所有跟注数据**********************************");
            if (_LastBetList != null && _LastBetList.Count > 0)
            {
                foreach (var item in _LastBetList)
                {
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, string.Join(", ", item));
                }
            }
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***所有跟注数据**********************************");
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***所有下注数据**********************************");
            if (datas != null && datas.Count > 0)
            {
                foreach (var item in datas)
                {
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, string.Join(", ", item));
                }
            }
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***所有下注数据**********************************");
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***赛事结果**********************************");
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, raceResult);
            Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, rfileName, "***赛事结果**********************************");
        }

        public List<string> _OddsResultList = new List<string>();
        /// <summary>
        /// 计算赔率
        /// </summary>
        /// <param name="raceResult"></param>
        /// <param name="betList"></param>
        /// <returns></returns>
        public double CalculateOdds(string raceResult, List<string> betList)
        {
            double oddsAmount = 0;
            if (betList != null && betList.Count > 0)
            {
                JObject jsonObject = JObject.Parse(raceResult);               
                foreach (string betInfo in betList)
                {
                    oddsAmount = oddsAmount - 10;
                    string[] parts = betInfo.Split('|');
                    string betType = parts[0];
                    string horseNo = parts[1];
                    if (jsonObject.ContainsKey(betType))
                    {
                        JArray winBets = (JArray)jsonObject[betType];
                        if (winBets != null && winBets.Count > 0)
                        {
                            foreach (var bet in winBets)
                            {
                                //Console.WriteLine("下注类型:"+ betType + "下注马号:"+ horseNo+",赛事结果马号:"+ bet["HORSE_NO"]);
                                if (AreNumbersEqual(horseNo, bet["HORSE_NO"].ToString()))
                                {
                                    _OddsResultList.Add("买中下注数据:下注类型[" + betType + "]下注马号[" + horseNo + "]赛事结果马号[" + bet["HORSE_NO"]+"]下注赔率["+ bet["ODDS"] + "]");
                                    oddsAmount = oddsAmount + Convert.ToDouble(bet["ODDS"]) + 10;
                                }
                            }
                        }
                    }
                }
            }
            return oddsAmount;
        }
        public bool AreNumbersEqual(string str1, string str2)
        {
            var setA = new HashSet<string>(str1.Split(new[] { ',', '-' }, StringSplitOptions.RemoveEmptyEntries));
            var setB = new HashSet<string>(str2.Split(new[] { ',', '-' }, StringSplitOptions.RemoveEmptyEntries));

            return setA.SetEquals(setB);
        }
        /// <summary>
        /// 由于历史数据中的日期格式和实时数据不一致，需要转换
        /// </summary>
        /// <param name="dateStr"></param>
        /// <param name="timeStr">6:40:37 PM</param>
        /// <returns></returns>
        public string ConvertHistoryDatetime(string dateString, string timeString)
        {
            // **1️⃣ 解析日期**
            DateTime date = DateTime.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture);

            // **2️⃣ 预处理时间字符串**
            timeString = Regex.Replace(timeString, @"\s+", " ").Trim();  // 清除多余空格

            // **3️⃣ 解析时间**
            /*if (!DateTime.TryParseExact(timeString, new[] { "h:mm:ss tt", "hh:mm:ss tt" },
                                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
            {
                Console.WriteLine("时间格式错误：" + timeString);
                return timeString;
            }*/
            if (!DateTime.TryParse(timeString, out DateTime time))
            {
                Console.WriteLine("时间格式错误：" + timeString);
                return timeString;
            }

            // **4️⃣ 合并日期和时间**
            DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);

            // **5️⃣ 格式化输出**
            string formattedDate = dateTime.ToString("ddd MMM dd HH:mm:ss 'GMT+08:00' yyyy", new CultureInfo("en-US"));
            return formattedDate;
        }
        // 清理时间字符串，去除不可见字符
        public string CleanTimeString(string input)
        {
            return input.Replace("\u202F", "").Replace("\u200B", "").Trim();
        }
        /// <summary>
        /// 获取赛事结果
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetBetResultJson(string fileName)
        {
            string baseDirectory = "C:\\eadata\\TestData\\horsedata";
            string filePath = Path.Combine(baseDirectory, fileName);
            // 读取前 7 行
            var first8Lines = File.ReadLines(filePath).Take(8).ToList();
            var resultDict = new Dictionary<string, List<Dictionary<string, string>>>();
            List < Dictionary<string, string> > W = new List<Dictionary<string, string>>();
            resultDict.Add("W",W);
            Dictionary<string, string> WB = new Dictionary<string, string>();
            W.Add(WB);
            string[] parts = first8Lines[0].Split('\t');
            WB.Add("HORSE_NO", parts[1]);
            WB.Add("ODDS", parts[2]);
            List<Dictionary<string, string>> P = new List<Dictionary<string, string>>();
            resultDict.Add("P", P);
            Dictionary<string, string> PB1 = new Dictionary<string, string>();
            P.Add(PB1);
            parts = first8Lines[1].Split('\t');
            PB1.Add("HORSE_NO", parts[1]);
            PB1.Add("ODDS", parts[2]);
            Dictionary<string, string> PB2 = new Dictionary<string, string>();
            P.Add(PB2);
            parts = first8Lines[2].Split('\t');
            PB2.Add("HORSE_NO", parts[0]);
            PB2.Add("ODDS", parts[1]);
            Dictionary<string, string> PB3 = new Dictionary<string, string>();
            P.Add(PB3);
            parts = first8Lines[3].Split('\t');
            PB3.Add("HORSE_NO", parts[0]);
            PB3.Add("ODDS", parts[1]);
            List<Dictionary<string, string>> Q = new List<Dictionary<string, string>>();
            resultDict.Add("Q", Q);
            Dictionary<string, string> QB = new Dictionary<string, string>();
            Q.Add(QB);
            parts = first8Lines[4].Split('\t');
            QB.Add("HORSE_NO", parts[1]);
            QB.Add("ODDS", parts[2]);
            List<Dictionary<string, string>> QP = new List<Dictionary<string, string>>();
            resultDict.Add("QP", QP);
            Dictionary<string, string> QPB1 = new Dictionary<string, string>();
            QP.Add(QPB1);
            parts = first8Lines[5].Split('\t');
            QPB1.Add("HORSE_NO", parts[1]);
            QPB1.Add("ODDS", parts[2]);
            Dictionary<string, string> QPB2 = new Dictionary<string, string>();
            QP.Add(QPB2);
            parts = first8Lines[6].Split('\t');
            QPB2.Add("HORSE_NO", parts[0]);
            QPB2.Add("ODDS", parts[1]);
            Dictionary<string, string> QPB3 = new Dictionary<string, string>();
            QP.Add(QPB3);
            parts = first8Lines[7].Split('\t');
            QPB3.Add("HORSE_NO", parts[0]);
            QPB3.Add("ODDS", parts[1]);
            string jsonResult = JsonConvert.SerializeObject(resultDict, Formatting.Indented);
            return jsonResult;
        }

        private void button_executeAnalysis_Click(object sender, EventArgs e)
        {
            if (bgwBroberQuote.IsBusy != true)
            {
                bgwBroberQuote.RunWorkerAsync();
            }
            else 
            {
                _Log.LogInfo("下注分析运行中");
            }
        }
    }
}
