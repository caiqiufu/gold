using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using NLog;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uClient.Comm;


namespace uClient.Broker
{
    public partial class EAForm : Form
    {
        //MT4 平台 begin
        public uClient.Broker.MT4 _MT4;
        //MT4 平台 end

        //MF4 平台 beign
        public uClient.Broker.MF4 _MF4;
        //MF4 平台 end

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
        /// AI推理消息
        /// </summary>
        public SubscriberSocket _SubscriberAIMessage = new SubscriberSocket();
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
        /// 数据源
        /// </summary>
        public CustomQuotePanel _DSQuote = new CustomQuotePanel("Gold", 0.1);

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
        /// <summary>
        /// EA 测试数据列表
        /// </summary>
        public string _StrategyConfigTestDataFile = "\\StrategyConfigTestData.json";

        /// <summary>
        /// AI CNN 模型
        /// </summary>
        public string _AICNN_lstm_ModeFile = "\\cnn_lstm_model.onnx";


        /// <summary>
        /// AI Scaler 参数文件
        /// </summary>
        public string _ScalerFile = "\\scaler.json";

        public NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 日志
        /// </summary>
        public Log _Log;
        /// <summary>
        /// EA Chart
        /// </summary>
        public EAChart _EAChart;
        /// <summary>
        /// EA数据
        /// </summary>
        public EAConfig _EAConfig;

        /// <summary>
        /// EA配置数据
        /// </summary>
        public EAStrategyConfig _StrategyConfig;
        /// <summary>
        /// EA 测试数据
        /// </summary>
        public StrategyConfigTestData _StrategyConfigTestData;
        /// <summary>
        /// 全局变量,上下文数据
        /// price:当前报价
        /// openPrice:开仓价
        /// inOpenPrice:加仓开仓价
        /// closePrice:平仓价
        /// CurrentOrderType:StrategyConfig.CurrentOrderType
        /// CurrentOrderDetail:StrategyConfig.CurrentOrderDetail
        /// CurrentOrderTypeDetail:StrategyConfig.CurrentOrderTypeDetail
        /// overKeepProfit:是否超过利润保持阈值
        /// </summary>
        public IDictionary<string, Object> _Context = new Dictionary<string, Object>();
        /// <summary>
        /// 策略执行
        /// </summary>
        EAExecute _EAExecute;

        /// <summary>
        /// 配置文件是否加载完成
        /// </summary>
        public bool _IsLoadConfigCompleted = false;

        public EAForm(MainFormPara mainFormPara)
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
        public EAForm()
        {
            MainFormPara mainFormPara = new MainFormPara();
            mainFormPara.PlatformCode = "MT4";
            mainFormPara.BrokerCode = "";
            mainFormPara.BrokerName = "";
            mainFormPara.FormType = "";
            mainFormPara.FormNo = "策略分析平台";
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
                catch (System.Exception ex)
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


        private void EAForm_Load(object sender, EventArgs e)
        {
            _Log.LogInfo("*******************************************");
            _Log.LogInfo("*** 大道至简，知易行难，知行合一，得到成功 ***");
            _Log.LogInfo("*******************************************");
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
            loadEAListFromFile();
            InitObjectAfterLoad();
            IniUI();
            loadEAList();
            string modelPath = rootPath + _AICNN_lstm_ModeFile;
            string scalerPath = rootPath + _ScalerFile;

        }
        /// <summary>
        /// Form加载完成后，再加载其他数据
        /// </summary>
        public void InitObjectAfterLoad()
        {
            _EAChart = new EAChart(_StrategyConfig, cartesianChart_Candle, chart_daily, chart_latest);
            _EAChart._ChunkSize = Convert.ToInt32(_StrategyConfig.timeDuration);
            _EAChart._Slope = Convert.ToDouble(_StrategyConfig.slope);
            _EAChart._KLine_Period = _Config.SysConfig["dukascopy.k.period"];
            _EAChart._KLine_Symbol = _Config.SysConfig["dukascopy.instrument"];
            _EAChart.initChart();
            _EAExecute = new EAExecute(_Log);
            //动态止损值默认为设置的止损值
            if (_StrategyConfig.dynamicLost == 0)
            {
                _StrategyConfig.dynamicLost = _StrategyConfig.stopLoss;
            }
            //EA 策略加载完成后
            symbol = "XAUUSD";
            //timer 启用
            timerCheckPlatformConnectStatus.Enabled = true;
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
            accountDisplay.lblBlance = label_Balance;
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
                    break;
                case (int)uClient.Comm.Enum.Platform.MT5:
                    _MT5 = new uClient.Broker.MT5(_Config, _MyConfig, _Broker, _Account, _TradeSymbol, _Log, bgwBroberQuote, null, null, quotaDisplayPanel, accountDisplay);
                    _MT5._QuoteData = new QuotePanelData(_TradeSymbol, 0.1);
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
            //交易商
            comboBox_broker.Items.Clear();
            foreach (uClient.Comm.Broker bro in _Platform.Broker)
            {
                comboBox_broker.Items.Add(bro.BrokerName);
            }
            comboBox_broker.SelectedItem = _Broker.BrokerName;
            //账户类型
            comboBox_AccType.Items.Clear();
            comboBox_AccType.Items.Add("Demo");
            comboBox_AccType.Items.Add("Live");
            //交易品类
            if (_Account != null && _Broker.Symbol != null && _Broker.Symbol.Length > 0)
            {
                comboBox_AccType.SelectedItem = _Account.Type;
                comboBox_symbol.Items.Clear();
                foreach (string ss in _Broker.Symbol)
                {
                    comboBox_symbol.Items.Add(ss);
                }
                comboBox_symbol.SelectedItem = _TradeSymbol;
            }
            //用户账号密码
            textBox_account.Text = _Account.UserCode;
            textBox_password.Text = _Account.Password;

            //EA配置
            if (_StrategyConfig.EAEntity.ContainsKey("BUY"))
            {
                EAEntity buyEAEntity = _StrategyConfig.EAEntity["BUY"];
                numericUpDown_buySum.Value = (decimal)buyEAEntity.sum;
                numericUpDown_buySum1.Value = (decimal)buyEAEntity.sum1;
                numericUpDown_buyDaily.Value = (decimal)buyEAEntity.daily;
                numericUpDown_buyDaily1.Value = (decimal)buyEAEntity.daily1;
                numericUpDown_buyHourly.Value = (decimal)buyEAEntity.hourly;
                numericUpDown_buyHourly1.Value = (decimal)buyEAEntity.hourly1;
                numericUpDown_buyGS.Value = (decimal)buyEAEntity.gs;
                numericUpDown_buyGS1.Value = (decimal)buyEAEntity.gs1;
            }
            if (_StrategyConfig.EAEntity.ContainsKey("SELL"))
            {
                EAEntity sellEAEntity = _StrategyConfig.EAEntity["SELL"];
                numericUpDown_sellSum.Value = (decimal)sellEAEntity.sum;
                numericUpDown_sellSum1.Value = (decimal)sellEAEntity.sum1;
                numericUpDown_sellDaily.Value = (decimal)sellEAEntity.daily;
                numericUpDown_sellDaily1.Value = (decimal)sellEAEntity.daily1;
                numericUpDown_sellHourly.Value = (decimal)sellEAEntity.hourly;
                numericUpDown_sellHourly1.Value = (decimal)sellEAEntity.hourly1;
                numericUpDown_sellGS.Value = (decimal)sellEAEntity.gs;
                numericUpDown_sellGS1.Value = (decimal)sellEAEntity.gs1;
            }

            if (_StrategyConfig.EAEntity.ContainsKey("CLOSE_BUY"))
            {
                EAEntity closeBuyEAEntity = _StrategyConfig.EAEntity["CLOSE_BUY"];
                numericUpDown_closeBuySum.Value = (decimal)closeBuyEAEntity.sum;
                numericUpDown_closeBuySum1.Value = (decimal)closeBuyEAEntity.sum1;
                numericUpDown_closeBuyDaily.Value = (decimal)closeBuyEAEntity.daily;
                numericUpDown_closeBuyDaily1.Value = (decimal)closeBuyEAEntity.daily1;
                numericUpDown_closeBuyHourly.Value = (decimal)closeBuyEAEntity.hourly;
                numericUpDown_closeBuyHourly1.Value = (decimal)closeBuyEAEntity.hourly1;
                numericUpDown_closeBuyGS.Value = (decimal)closeBuyEAEntity.gs;
                numericUpDown_closeBuyGS1.Value = (decimal)closeBuyEAEntity.gs1;
            }

            if (_StrategyConfig.EAEntity.ContainsKey("CLOSE_SELL"))
            {
                EAEntity closeSellEAEntity = _StrategyConfig.EAEntity["CLOSE_SELL"];
                numericUpDown_closeSellSum.Value = (decimal)closeSellEAEntity.sum;
                numericUpDown_closeSellSum1.Value = (decimal)closeSellEAEntity.sum1;
                numericUpDown_closeSellDaily.Value = (decimal)closeSellEAEntity.daily;
                numericUpDown_closeSellDaily1.Value = (decimal)closeSellEAEntity.daily1;
                numericUpDown_closeSellHourly.Value = (decimal)closeSellEAEntity.hourly;
                numericUpDown_closeSellHourly1.Value = (decimal)closeSellEAEntity.hourly1;
                numericUpDown_closeSellGS.Value = (decimal)closeSellEAEntity.gs;
                numericUpDown_closeSellGS1.Value = (decimal)closeSellEAEntity.gs1;
            }
            numericUpDown_timeDuration.Value = (decimal)_StrategyConfig.timeDuration;
            numericUpDown_slope.Value = (decimal)_StrategyConfig.slope;
            numericUpDown_takeProfit.Value = (decimal)_StrategyConfig.takeProfit;
            numericUpDown_stopLoss.Value = (decimal)_StrategyConfig.stopLoss;
            numericUpDown_keepProfit.Value = (decimal)_StrategyConfig.keepProfit;
            numericUpDown_minChangeDiff.Value = (decimal)_StrategyConfig.minChangeDiff;
            numericUpDown_maxChangeDiff.Value = (decimal)_StrategyConfig.maxChangeDiff;
            numericUpDown_maxChangeDiffDuration.Value = (decimal)_StrategyConfig.maxDiffChangeDuration;
            comboBox_analysisDataType.SelectedItem = _StrategyConfig.analysisDataType;
            numericUpDown_keepProfitDiff.Value = (decimal)_StrategyConfig.keepProfitDiff;
            checkBox_continuousOrder.Checked = _StrategyConfig.continuousOrder;
            checkBox_reverseProportion.Checked = _StrategyConfig.reverseProportion;
            checkBox_indexClose.Checked = _StrategyConfig.indexClose;
            checkBox_limitOrder.Checked = _StrategyConfig.limitOrder;
            checkBox_EAKP.Checked = _StrategyConfig.EAKP;
            checkBox_EAKL.Checked = _StrategyConfig.EAKL;

            //基础配置数据
            textBox_dsAddress.Text = _Config.PublishAddressDS;
            comboBox_dsType.SelectedItem = _StrategyConfig.DSType;
            //默认采用EANotradeDuration
            if (string.IsNullOrEmpty(_StrategyConfig.NotradeDuration))
            {
                _StrategyConfig.NotradeDuration = _Config.SysConfig["EANotradeDuration"];
            }
            textBox_notradeDuration.Text = _StrategyConfig.NotradeDuration;
            checkBox_NotifyFlag.Checked = _StrategyConfig.NotifyFlag;
            comboBox_CurrentOrderTypeDetail.SelectedItem = _StrategyConfig.CurrentOrderType;
            label_currentOrderDetail.Text = _StrategyConfig.CurrentOrderDetail;
            numericUpDown_trendOrder.Value = (decimal)_StrategyConfig.trendOrder;
            numericUpDown_trendOrder1.Value = (decimal)_StrategyConfig.trendOrder1;
            numericUpDown_increase.Value = (decimal)_StrategyConfig.increasePositionPoint;
            comboBox_SelectedStrategy.SelectedItem = _StrategyConfig.SelectedStrategy;
            numericUpDown_CommandTimeDuration.Value = (decimal)_StrategyConfig.commandTimeDuration;

            //回测参数
            textBox_TestDuration.Text = _StrategyConfig.TestDuration;
            textBox_retestNumDuration.Text = _StrategyConfig.retestNumDuration;
            numericUpDown_openMax.Value = _StrategyConfig.openMax;
            numericUpDown_openDuration.Value = _StrategyConfig.openDuration;
            numericUpDown_closeDuration.Value = _StrategyConfig.closeDuration;
            numericUpDown_offset.Value = _StrategyConfig.offset;
            numericUpDown_hourlyOffset.Value = _StrategyConfig.hourlyOffset;

            //权限初始化
            button_test.Visible = _MyConfig.Permission_Button_SimulatorTest;

        }


        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void LoadConfigFile()
        {
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
                _StrategyConfig = JsonConvert.DeserializeObject<EAStrategyConfig>(File.ReadAllText(newStrategyFile));
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
                _Config.SysConfig = DBHelper.getSysConfig();
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
            //DirectoryInfo pathInfo = new DirectoryInfo(Application.StartupPath);
            //string path = pathInfo.Parent.FullName + "\\iAutoTrade\\";
            string newMyConfigFile = rootPath + _MyConfigFile;
            string newConfigFile = rootPath + _ConfigFile;
            try
            {
                if (_Config != null)
                {
                    _Account.BrokerCode = _Broker.BrokerCode;
                    _Account.UserCode = textBox_account.Text;
                    _Account.Password = textBox_password.Text;
                    _Account.Type = comboBox_AccType.Text;
                    bool existFlag = false;
                    for (int i = 0; i < _MyConfig.Account.Length; i++)
                    {
                        Account acc = _MyConfig.Account[i];
                        if (string.Equals(acc.BrokerCode, _Account.BrokerCode) && string.Equals(acc.Type, _Account.Type))
                        {
                            acc = _Account;
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

                    //平台设置

                    //EA参数设置

                    //交易商配置信息覆盖平台配置信息
                    _Broker.DefaultSymbol = comboBox_symbol.Text;
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
                case (int)uClient.Comm.Enum.Platform.MF4:
                    _MF4._Config = _Config;
                    _MF4._MyConfig = _MyConfig;
                    _MF4._Broker = _Broker;
                    _MF4._Account = _Account;
                    _MF4._TradeSymbol = _TradeSymbol;
                    break;
                case (int)uClient.Comm.Enum.Platform.MT4:
                    _MT4._Config = _Config;
                    _MT4._MyConfig = _MyConfig;
                    _MT4._Broker = _Broker;
                    _MT4._Account = _Account;
                    _MT4._TradeSymbol = _TradeSymbol;
                    break;
                case (int)uClient.Comm.Enum.Platform.MT5:
                    _MT5._Config = _Config;
                    _MT5._MyConfig = _MyConfig;
                    _MT5._Broker = _Broker;
                    _MT5._Account = _Account;
                    _MT5._TradeSymbol = _TradeSymbol;
                    break;
            }
        }
        //策略品类
        string symbol = "";
        /// <summary>
        /// 从配置文件中加载EA数据
        /// </summary>
        public void loadEAListFromFile()
        {
            _Log.LogInfo("加载EA数据开始");
            string newStrategyFile = rootPath + _StrategyFile;
            string strategyConfigTestDataFile = rootPath + _StrategyConfigTestDataFile;
            if (File.Exists(newStrategyFile))
            {
                _StrategyConfig = JsonConvert.DeserializeObject<EAStrategyConfig>(File.ReadAllText(newStrategyFile));
                //加仓不改变原开单价格，避免止损采用新的开仓价
                if (_Context.ContainsKey("CurrentPrice"))
                {
                    _StrategyConfig.CurrentPrice = Convert.ToDouble(_Context["CurrentPrice"]);
                }
            }
            else
            {
                _logger.Fatal("没有策略配置文件，系统初始化异常");
            }
            if (File.Exists(strategyConfigTestDataFile))
            {
                _StrategyConfigTestData = JsonConvert.DeserializeObject<StrategyConfigTestData>(File.ReadAllText(strategyConfigTestDataFile));
                if (_StrategyConfigTestData != null && _StrategyConfigTestData.EAEntityList != null && _StrategyConfigTestData.EAEntityList.Count > 0)
                {
                    comboBox_SelectedStrategy.Items.Clear();
                    for (int num = 0; num < _StrategyConfigTestData.EAEntityList.Count; num++)
                    {
                        comboBox_SelectedStrategy.Items.Add(num.ToString());
                    }
                    comboBox_SelectedStrategy.SelectedItem = _StrategyConfig.SelectedStrategy;
                }
            }
            else
            {
                _StrategyConfigTestData = new StrategyConfigTestData();
            }
        }
        /// <summary>
        /// 保存自动生成的EA数据
        /// </summary>
        public void saveEATestData()
        {
            string strategyConfigTestDataFile = rootPath + _StrategyConfigTestDataFile;
            System.DateTime currentTime = System.DateTime.Now;
            string fileNameSuffix = currentTime.ToString("yyyyMMdd");
            string fileName = "StrategyConfigTestData_List_" + fileNameSuffix + ".txt";
            Utils.deleteFile(rootPath, fileName);
            File.WriteAllText(strategyConfigTestDataFile, JsonConvert.SerializeObject(_StrategyConfigTestData));
            _logger.Info("保存测试策略数据完成");
            if (_StrategyConfigTestData.EAEntityList != null && _StrategyConfigTestData.EAEntityList.Count > 0)
            {
                int i = 0;
                foreach (Dictionary<string, EAEntity> result in _StrategyConfigTestData.EAEntityList)
                {
                    EAEntity buy = result["BUY"];
                    EAEntity sell = result["SELL"];
                    EAEntity closeBuy = result["CLOSE_BUY"];
                    EAEntity closeSell = result["CLOSE_SELL"];
                    string resultStr = "No.:" + i + ",平仓上限:" + closeBuy.daily + ",开仓上限:" + buy.daily + ",开仓下限:" + buy.daily1 + ",平仓下限:" + closeBuy.daily1 + ",GS开仓:" + buy.gs + ",GS平仓:" + closeBuy.gs;
                    Utils.AddMsgToTXT(rootPath, fileName, resultStr);
                    i++;
                }
            }
        }
        /// <summary>
        /// 加载策略数据,把内存中的测试数据加载到全局变量中
        /// </summary>
        private void loadEAList()
        {
            _Log.LogInfo("加载EA列表数据开始");
            if (_StrategyConfig != null)
            {
                if (_EAConfig == null)
                {
                    _EAConfig = new EAConfig();
                }
                _StrategyConfig.EAEntity.Clear();
                _EAConfig.EAList.Clear();
                _EAConfig.EA.Clear();
                StrategyUtils.initStrategyList(_StrategyConfig, _EAConfig, _Config);

                _Log.LogInfo("加载EA数据完成,共计加载EA[" + _EAConfig.EAList.Count + "]条");

                _Context = _StrategyConfig.Context;
                _Context["symbol"] = symbol;
                _Context["IsSimulateTest"] = _IsSimulateTest ? "T" : "F";
                _Context["TestBatchNo"] = _TestBatchNo;
                _Context["analysisDataType"] = _StrategyConfig.analysisDataType;

                _Log.LogInfo("初始化参数运行参数");
                _Log.LogInfo("CurrentOrderType:" + _StrategyConfig.CurrentOrderType);
                _Log.LogInfo("CurrentOrderTypeDetail:" + _StrategyConfig.CurrentOrderTypeDetail);
                _Log.LogInfo("CurrentOrderDetail:" + _StrategyConfig.CurrentOrderDetail);

                //由于配置文件参数会覆盖预定义固定值,所以需要重新初始化
                //_StrategyConfig.maxOrderCount = 20;
                //_StrategyConfig.maxLossPoint = 35;
                //_StrategyConfig.lossOrderContinuousCount = 3;
                //_StrategyConfig.lossOrderTotalCount = 6;
            }
            else
            {
                _StrategyConfig = new EAStrategyConfig();
            }
            _Log.LogInfo("加载EA列表数据完成");
        }
        /// <summary>
        /// 获取策略服务端口
        /// 555*  交易端锁单端口
        /// 565*  EA 端口
        /// 575*  系统间交互端口
        /// </summary>
        /// <returns></returns>
        public string GetEAServerPort()
        {
            string port = "5653";
            switch (_StrategyConfig.analysisDataType)
            {
                case "D":
                    port = "5653";
                    break;
                case "H":
                    port = "5655";
                    break;
                case "K":
                    port = "5656";
                    break;
                default:
                    port = "5658";
                    break;
            }
            return port;
        }
        /// <summary>
        /// 消息发送绑定
        /// </summary>
        public void AutoLockBind(bool isActive)
        {
            if (isActive)
            {
                //string port = Utils.GetPort(_MainFormPara.FormNo);
                string port = GetEAServerPort();
                //string address = _Config.PublishAddressEAS;
                string address = string.Format("tcp://{0}:{1}", Utils.GetLocalIP(), port);
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
                //string port = Utils.GetPort(_MainFormPara.FormNo);
                string port = GetEAServerPort();
                string address = string.Format("tcp://{0}:{1}", Utils.GetLocalIP(), port);
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
        private void button_dsConnect_Click(object sender, EventArgs e)
        {
            AsynExec(this.button_dsConnect, () =>
            {
                if (this.button_dsConnect.Text.Equals("数据源断开"))
                {
                    _Log.LogInfo("数据源连接已启动");
                    updateDsRunningStatus(true);
                    _Log.LogInfo("连接到AI推理消息");
                    AIMessageConnection("AI");
                }
                else if (this.button_dsConnect.Text.Equals("数据源连接"))
                {
                    _Log.LogInfo("数据源连接已停止");
                    updateDsRunningStatus(false);
                }
            });
        }
        public void updateDsRunningStatus(Boolean status)
        {

            if (label_dsRunning.InvokeRequired)
            {
                label_dsRunning.Invoke(new Action<bool>(updateDsRunningStatus), new object[] { status });
            }
            else
                if (status)
                {
                    button_dsConnect.Text = "数据源连接";
                    button_dsConnect.BackColor = Color.Green;
                    label_dsRunning.Text = "源数据连接中";
                    timer_dsRunningProcessing.Enabled = true;
                    timer_dsRunningProcessing.Start();
                    timer_chartDataLoad.Enabled = true;
                    timer_chartDataLoad.Start();
                    _Log.LogInfo("源数据加载已启动,执行周期为[" + timer_chartDataLoad.Interval / 1000 / 60 + "]分钟");
                    loadChartData();
                }
                else
                {
                    button_dsConnect.Text = "数据源断开";
                    button_dsConnect.BackColor = Color.Red;
                    label_dsRunning.Text = "源数据连接断开";
                    timer_dsRunningProcessing.Stop();
                    timer_chartDataLoad.Stop();
                    _EAChart.clearChartData();
                }
        }
        private void button_startEA_Click(object sender, EventArgs e)
        {
            AsynExec(this.button_startEA, () =>
            {
                if (this.button_startEA.Text.Equals("EA分析停止"))
                {
                    updateEaRunningStatus(true);
                }
                else if (this.button_startEA.Text.Equals("EA分析启动"))
                {
                    updateEaRunningStatus(false);
                }
            });
            //启动或者停止策略时,重置该标志位
            _IsExecuting = false;
        }

        public void updateEaRunningStatus(Boolean status)
        {

            if (label_EARunning.InvokeRequired)
            {
                label_EARunning.Invoke(new Action<bool>(updateEaRunningStatus), new object[] { status });
            }
            else
                if (status)
                {
                    button_startEA.Text = "EA分析启动";
                    label_EARunning.Text = "EA执行中";
                    label_EARunning.Text = "策略分析中";
                    button_startEA.BackColor = Color.Green;
                    timer_eaRunningProcessing.Enabled = true;
                    timer_eaRunningProcessing.Start();
                    timer_eAExecute.Enabled = true;
                    timer_eAExecute.Start();
                    _Log.LogInfo("EA分析已启动,执行周期为[" + timer_eAExecute.Interval / 1000 / 60 + "]分钟");
                    timer_heartBeat.Enabled = true;
                    timer_heartBeat.Start();
                    AutoLockBind(true);
                }
                else
                {
                    button_startEA.Text = "EA分析停止";
                    label_EARunning.Text = "EA执行停止";
                    label_EARunning.Text = "策略分析停止";
                    button_startEA.BackColor = Color.Red;
                    timer_eaRunningProcessing.Stop();
                    timer_eAExecute.Stop();
                    timer_heartBeat.Stop();
                    _Log.LogInfo("EA分析已停止");
                    AutoLockBind(false);
                    //timer_ParaEAExcute.Stop();
                }
        }
        private void loadChartData()
        {
            DBUtils.connAddress = _Config.PublishAddressDS;
            if (chart_daily.InvokeRequired)
            {
                chart_daily.Invoke(new Action(loadChartData));
            }
            else
            {
                _Log.LogInfo("加载图表开始");
                _EAChart._StrategyConfig = _StrategyConfig;
                _EAChart.loadChartData();
                _Log.LogInfo("加载图表完成");
            }
        }
        /// <summary>
        /// 加载回测数据
        /// </summary>
        private void loadTestChartData(DateTime time)
        {
            DBUtils.connAddress = _Config.PublishAddressDS;
            if (chart_daily.InvokeRequired)
            {
                chart_daily.Invoke(new Action<DateTime>(loadTestChartData), new object[] { time });
            }
            else
            {
                //_Log.LogInfo("加载图表开始");
                _EAChart._StrategyConfig = _StrategyConfig;
                _EAChart.loadTestChartData(time);
                //Console.WriteLine("time="+ time + ",latestGSValueList=" + string.Join(",", _EAChart.latestGSValueList));
                //_Log.LogInfo("加载图表完成");
            }
        }

        private void bgw_loadChartData_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            loadChartData();
        }

        private void timer_eaRunningProcessing_Tick(object sender, EventArgs e)
        {
            if (label_EARunning.BackColor == Color.Red)
            {
                label_EARunning.BackColor = Color.Green;
            }
            else
            {
                label_EARunning.BackColor = Color.Red;
            }
        }

        private void timer_dsRunningProcessing_Tick(object sender, EventArgs e)
        {
            if (label_dsRunning.BackColor == Color.Red)
            {
                label_dsRunning.BackColor = Color.Green;
            }
            else
            {
                label_dsRunning.BackColor = Color.Red;
            }
        }
        /// <summary>
        /// 执行策略前，加载上下文数据
        /// </summary>
        private void loadContext()
        {

            _Context["price"] = _CurrentQuotePrice;
            _Context["GOLD_PRICE"] = _StrategyConfig.GOLD_PRICE.ToString();
            _Context["SILVER_PRICE"] = _StrategyConfig.SILVER_PRICE.ToString();
            _Context["CurrentDateTime"] = _CurrentDateTime;
            _Context["CurrentOrderType"] = _StrategyConfig.CurrentOrderType;
            _Context["CurrentOrderDetail"] = _StrategyConfig.CurrentOrderDetail;
            _Context["CurrentOrderTypeDetail"] = _StrategyConfig.CurrentOrderTypeDetail;

            _Context["stopLoss"] = _StrategyConfig.stopLoss.ToString();
            _Context["timeDuration"] = _StrategyConfig.timeDuration.ToString();
            _Context["slope"] = _StrategyConfig.slope.ToString();
            _Context["keepProfit"] = _StrategyConfig.keepProfit.ToString();
            _Context["takeProfit"] = _StrategyConfig.takeProfit.ToString();
            _Context["keepProfitDiff"] = _StrategyConfig.keepProfitDiff.ToString();
            _Context["SelectedStrategy"] = _StrategyConfig.SelectedStrategy;

            _Context["overKeepProfit"] = _StrategyConfig.overKeepProfit ? "T" : "F";
            _Context["overKeepProfit2"] = _StrategyConfig.overKeepProfit2 ? "T" : "F";
            _Context["dynamicProfit"] = _StrategyConfig.dynamicProfit.ToString();
            _Context["dynamicLost"] = _StrategyConfig.dynamicLost.ToString();
            _Context["gsMaxProfitValue"] = _StrategyConfig.gsMaxProfitValue.ToString();
            _Context["gsMaxLossValue"] = _StrategyConfig.gsMaxLossValue.ToString();

            _Context["MartingaleAddPosition1"] = _StrategyConfig.MartingaleAddPosition1;
            _Context["MartingaleAddPosition2"] = _StrategyConfig.MartingaleAddPosition2;
            _Context["MartingaleAddPosition3"] = _StrategyConfig.MartingaleAddPosition3;

            _Context["CommandCreateTime"] = _StrategyConfig.CommandCreateTime;

            _Context["analysisDataType"] = _StrategyConfig.analysisDataType;
            _Context["IsSimulateTest"] = _IsSimulateTest ? "T" : "F";
            _Context["symbol"] = symbol;
            _Context["TestBatchNo"] = _TestBatchNo;
            //加载仓位信息
            if (_StrategyConfig.TradePositionInfo != null)
            {
                _Context["TradePositionInfo"] = _StrategyConfig.TradePositionInfo;
            }
            //动态修改后,需要同步到内存参数中
            _Context["maxDiffChangeDuration"] = _StrategyConfig.maxDiffChangeDuration.ToString();
            _Context["lastGSSlopeValue"] = _EAChart.lastGSSlopeValue;
            _Context["lastGSLotRatioValue"] = _EAChart.lastGSLotRatioValue;

            //马丁加仓手数
            _Context["MartingaleAddPositionLot1"] = _Config.SysConfig["MartingaleAddPositionLot1"];
            _Context["MartingaleAddPositionLot2"] = _Config.SysConfig["MartingaleAddPositionLot2"];
            _Context["MartingaleAddPositionLot3"] = _Config.SysConfig["MartingaleAddPositionLot3"];
            //马丁加仓价格网格
            _Context["MartingaleAddPositionPrice1"] = _Config.SysConfig["MartingaleAddPositionPrice1"];
            _Context["MartingaleAddPositionPrice2"] = _Config.SysConfig["MartingaleAddPositionPrice2"];
            _Context["MartingaleAddPositionPrice3"] = _Config.SysConfig["MartingaleAddPositionPrice3"];
            //分批平仓
            _Context["ManagePositionTakeProfitLot1"] = _Config.SysConfig["ManagePositionTakeProfitLot1"];
            _Context["ManagePositionTakeProfitLot2"] = _Config.SysConfig["ManagePositionTakeProfitLot2"];


        }
        private void comboBox_broker_SelectedIndexChanged(object sender, EventArgs e)
        {
            string BrokerName = comboBox_broker.SelectedItem.ToString();
            uClient.Comm.Broker bb = _Platform.GetBrokerByName(BrokerName);
            _Platform.DefaultBrokerCode = bb.BrokerCode;
            _Broker = bb;
            _Account = _Broker.DefaultAccountType == "Demo" ? _Broker.DemoAccount : _Broker.LiveAccount;
            _TradeSymbol = _Broker.DefaultSymbol;
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
                _TradePara = _Account.TradePara;
                comboBox_AccType.SelectedItem = _Account.Type;
                comboBox_symbol.Items.Clear();
                if (_Account != null && _Broker.Symbol != null && _Broker.Symbol.Length > 0)
                {
                    comboBox_AccType.SelectedItem = _Account.Type;
                    foreach (string ss in _Broker.Symbol)
                    {
                        comboBox_symbol.Items.Add(ss);
                    }
                    comboBox_symbol.SelectedItem = _TradeSymbol;
                }
                textBox_account.Text = _Account.UserCode;
                textBox_password.Text = _Account.Password;

            }
        }
        /// <summary>
        /// 是否连接
        /// </summary>
        /// <returns></returns>
        public bool IsConnect()
        {
            switch (_Platform.PlatformNo)
            {
                case (int)uClient.Comm.Enum.Platform.MF4:
                    if (_MF4.isConnect())
                    {
                        return true;
                    }
                    break;
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
                    //2.更新连接状态
                    UpdateConnectStatus(false);
                }
            });
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
            else
            {
                lblConnectStatus.BackColor = Color.Red;
                accountDisplay.lblBlance.Text = "8888.88";
                label_BrokePrice.Text = "0";
            }
            lblConnectStatus.Text = status ? "已连接" : "断开";
            lblConnectStatus.BackColor = status ? Color.Green : Color.Red;
            button_platformConnect.Text = !status ? "平台连接" : "平台断开";
            button_platformConnect.BackColor = !status ? Color.Red : Color.Green;
            comboBox_broker.Enabled = !status;
            comboBox_AccType.Enabled = !status;
            comboBox_symbol.Enabled = !status;
            textBox_account.Enabled = !status;
            textBox_password.Enabled = !status;
        }
        private void bgwBroberQuote_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Task.Run(() =>
            {
                BroberQuote();
            });
        }
        /// <summary>
        /// 测试平台商报价
        /// </summary>
        public void BroberQuoteForTest()
        {
            if (_QuoteList != null && _QuoteList.Count > 0)
            {
                IDictionary<string, string> data = _QuoteList[quoteSeq];
                //string time = data["time"];
                string price = data["price"];
                updatePrice(Convert.ToDouble(price));
                if (quoteSeq == _QuoteList.Count - 1)
                {
                    quoteSeq = 0;
                }
                else
                {
                    quoteSeq++;
                }
            }
        }


        /// <summary>
        /// 平台商报价刷新
        /// </summary>
        public void BroberQuote()
        {
            switch (_Platform.PlatformNo)
            {
                case (int)uClient.Comm.Enum.Platform.MF4:
                    if (_MF4._QuoteUpdatedEventArgs != null && _MF4._QuoteUpdatedEventArgs.Quote.Symbol == _TradeSymbol)
                    {
                        double prices = (double)_MF4._QuoteUpdatedEventArgs.Quote.BidPrice;
                        updatePrice(prices);
                    }
                    break;
                case (int)uClient.Comm.Enum.Platform.MT4:
                    if (_MT4._QuoteEventArgs != null)
                    {
                        string symbols = _MT4._QuoteEventArgs.Symbol;
                        double prices = _MT4._QuoteEventArgs.Bid;
                        double pricesAsk = _MT4._QuoteEventArgs.Ask;
                        if (string.Equals(symbols, _TradeSymbol))
                        {
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
                        }
                    }
                    break;
            }
        }
        /// <summary>
        /// 当前报价,和显示价格同步更新
        /// </summary>
        string _CurrentQuotePrice = "0";
        /// <summary>
        /// 更新报价
        /// </summary>
        public void updatePrice(double prices)
        {
            if (prices > 100)
            {
                updateBrokePrice(prices.ToString("f2"));
                //_IsExecuting=true timer正在执行策略,待执行完成后才执行该逻辑
                if (!_IsExecuting)
                {
                    //修改锁状态,避免重复执行/和定时策略执行冲突
                    //避免日志输出过多
                    //_Log.LogInfo("执行止盈止损策略开始");
                    _IsExecuting = true;
                    bool exeFlag = keepProfitAndLoss(prices);
                    if (exeFlag)
                    {
                        _Log.LogInfo("执行止盈止损策略开始");
                        StringBuilder desc = new StringBuilder();
                        desc.AppendFormat("当前价格[{0}]开单价格[{1}]止盈值[{2}]止损值[{3}]动态止盈点数[{4}]", prices, _StrategyConfig.CurrentPrice, _StrategyConfig.takeProfit, _StrategyConfig.stopLoss, _StrategyConfig.keepProfit);
                        _Log.LogInfo(desc.ToString());
                        eaExecute();
                        _Log.LogInfo("执行止盈止损策略结束");
                    }
                    _IsExecuting = false;
                    //_Log.LogInfo("策略执行结束");
                }
            }
        }

        /// <summary>
        /// 更新连接状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_AccountPosionRefresh_Tick(object sender, EventArgs e)
        {
            switch (_Platform.PlatformNo)
            {
                case (int)uClient.Comm.Enum.Platform.MF4:
                    _MF4.UpdateAccountCaptial();
                    if (!_MF4.isConnect())
                    {
                        UpdateConnectStatus(false);
                    }
                    break;
                case (int)uClient.Comm.Enum.Platform.MT4:
                    _MT4.UpdateAccountCaptial();
                    if (!_MT4.isConnect())
                    {
                        UpdateConnectStatus(false);
                    }
                    break;
                case (int)uClient.Comm.Enum.Platform.MT5:
                    _MT5.UpdateAccountCaptial();
                    if (!_MT5.isConnect())
                    {
                        UpdateConnectStatus(false);
                    }
                    break;
            }

            if (_IsDebutTest)
            {
                BroberQuoteForTest();
            }
        }
        /// <summary>
        ///  执行策略,并且判断是周六交易结束就清空动态数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_eAExecute_Tick(object sender, EventArgs e)
        {
            _Log.LogInfo("执行定时策略");
            //loadChartData();
            //加载指数数据后再执行策略
            //Thread.Sleep(10000);
            //不判断平台是否已经连,都正常执行策略,如果平台未连接,只影响止盈止损策略执行
            //回测期间不执行,周末不执行
            bool isSatEnd = Utils.checkIsSaturdayEndTime(DBUtils.getDateTime(), _StrategyConfig.NotradeDuration);
            //_Log.LogInfo("_IsSimulateTest="+ _IsSimulateTest+ ",isSatEnd="+ isSatEnd);
            if (!_IsSimulateTest && !isSatEnd)
            {
                if (!_IsExecuting)
                {
                    bool isTradeTime = Utils.checkIsTradeTime(_StrategyConfig.NotradeDuration, DBUtils.getDateTime());
                    if (isTradeTime)
                    {
                        _Log.LogInfo("定时策略执行开始");
                        _IsExecuting = true;
                        eaExecute();
                        _IsExecuting = false;
                        _Log.LogInfo("定时策略执行结束");
                    }
                }
                else
                {
                    _Log.LogInfo("策略执行中");
                }
            }
        }
        //策略执行状态,解决止盈止损和timer执行策略之间的冲突
        bool _IsExecuting = false;
        /// <summary>
        /// 执行EA策略
        /// </summary>
        private void eaExecute()
        {
            //_Log.LogInfo("执行EA策略开始:共计[" + _EAConfig.EAList.Count + "]条EA");
            //_CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            //_CurrentTime = DBUtils.getTime();
            if (!_IsSimulateTest)
            {
                _CurrentDateTime = DBUtils.getDateTime();
            }
            loadContext();
            _EAExecute._EAConfig = _EAConfig;
            string beforeOrderTypeDetail = _StrategyConfig.CurrentOrderTypeDetail;
            string result = _EAExecute.execute(_EAChart, _Context);
            //_Log.LogInfo("执行EA策略完成");
            aftercEAExecute();
            if (string.IsNullOrEmpty(result))
            {
                _Log.LogInfo("无策略");
                StringBuilder executeDesc = new StringBuilder();
                //XAUUSD:2004.79:BUY:1:50:50 HH:mm:ss
                executeDesc.AppendFormat("{0} {1}", "HEARTBEAT", DateTime.Now.ToString("HH:mm:ss"));
                string message = executeDesc.ToString();
                try
                {
                    //_Log.LogInfo("发送心跳消息开始:" + message);
                    _MasterPublisher.SendFrame(message);
                    //_Log.LogInfo("发送心跳消息完成");
                }
                catch (NetMQ.FaultException ex)
                {
                    _Log.LogInfo("发送消息异常:" + ex.ToString());
                }
                catch (Exception ex)
                {
                    _Log.LogInfo("发送消息异常:" + ex.ToString());
                }
            }
            else
            {
                sendOrderAfterEAExecuted(true, null, result, beforeOrderTypeDetail);
                saveEADynamicParameter();
            }
            //周六清空数据
            bool isSatEnd = Utils.checkIsSaturdayEndTime(DBUtils.getDateTime(), _StrategyConfig.NotradeDuration);
            //清空动态参数
            if (isSatEnd)
            {
                clearContext();
            }
        }

        /// <summary>
        /// EAPara策略执行,根据执行结果更新相关的配置参数,如修改单个交易日订单总量,修改
        /// </summary>
        private void eaParaExecute()
        {
            _Log.LogInfo("执行EAPara策略开始:共计[" + _EAConfig.ParaEAList.Count + "]条EA");
            _EAExecute._EAConfig = _EAConfig;
            string result = _EAExecute.executePara(_EAChart, _Context);
            _Log.LogInfo("执行EAPara策略完成");
            if (!string.IsNullOrEmpty(result))
            {
                updateEaParaDetail(result);
            }
            else
            {
                _Log.LogInfo("无策略");
            }
        }
        /// <summary>
        /// 策略执行完成后需要保存的动态参数,需要把上下文的参数保存到持久化变量中,待执行策略前再重新加载
        /// </summary>
        public void aftercEAExecute()
        {
            //替换动态止盈值,根据价格的变化不断刷新动态止盈值
            _StrategyConfig.dynamicProfit = Convert.ToDouble(_Context["dynamicProfit"]);
            _StrategyConfig.dynamicLost = Convert.ToDouble(_Context["dynamicLost"]);
            _StrategyConfig.overKeepProfit = string.Equals(_Context["overKeepProfit"], "T");
            _StrategyConfig.overKeepProfit2 = string.Equals(_Context["overKeepProfit2"], "T");
            _StrategyConfig.gsMaxProfitValue = Convert.ToDouble(_Context["gsMaxProfitValue"]);
            _StrategyConfig.gsMaxLossValue = Convert.ToDouble(_Context["gsMaxLossValue"]);

            _StrategyConfig.MartingaleAddPosition1 = Convert.ToBoolean(_Context["MartingaleAddPosition1"]);
            _StrategyConfig.MartingaleAddPosition2 = Convert.ToBoolean(_Context["MartingaleAddPosition2"]);
            _StrategyConfig.MartingaleAddPosition3 = Convert.ToBoolean(_Context["MartingaleAddPosition3"]);

            //实时更新
            _StrategyConfig.latestGoldSumValue = _EAChart.latestGoldSumValue;
            _Context["latestGoldSumValue"] = _StrategyConfig.latestGoldSumValue.ToString();
            _StrategyConfig.latestGoldHourlyValue = _EAChart.latestGoldHourlyValue;
            _Context["latestGoldHourlyValue"] = _StrategyConfig.latestGoldHourlyValue.ToString();
            _StrategyConfig.latestGoldDailyValue = _EAChart.latestGoldDailyValue;
            _Context["latestGoldDailyValue"] = _StrategyConfig.latestGoldDailyValue.ToString();

            _StrategyConfig.latestGSValue = _EAChart.latestGSValue;
            _Context["latestGSValue"] = _StrategyConfig.latestGSValue.ToString();

            _StrategyConfig.lastGSSlopeValue = _EAChart.lastGSSlopeValue;
            _Context["lastGSSlopeValue"] = _StrategyConfig.lastGSSlopeValue;

            _StrategyConfig.lastGSLotRatioValue = _EAChart.lastGSLotRatioValue;
            _Context["lastGSLotRatioValue"] = _StrategyConfig.lastGSLotRatioValue;
        }
        /// <summary>
        /// 回测策略其实数据
        /// </summary>
        private int _RetestStartNum = 0;
        /// <summary>
        /// 回测策略结束数据
        /// </summary>
        private int _RetestEndNum = 0;
        /// <summary>
        /// 回测执行EA策略
        /// </summary>
        private void simulateTestEAExecute(string type)
        {
            loadContext();
            _EAExecute._EAConfig = _EAConfig;
            string beforeOrderTypeDetail = _StrategyConfig.CurrentOrderTypeDetail;
            string result = _EAExecute.execute(_EAChart, _Context);
            aftercEAExecute();
            if (!string.IsNullOrEmpty(result))
            {
                sendOrderAfterEAExecuted(true, type, result, beforeOrderTypeDetail);
            }
            //周六清空数据
            bool isSatEnd = Utils.checkIsSaturdayEndTime(_CurrentDateTime, _StrategyConfig.NotradeDuration);
            //清空动态参数
            if (isSatEnd)
            {
                clearContext();
            }
        }
        /// <summary>
        /// 万州黄金点差
        /// </summary>
        public double _SPREAD_WZ = 0;

        /// <summary>
        /// 策略执行完成后，发送指令
        /// </summary>
        /// <param name="isAutoCommand">是否自动指令</param>
        /// <param name="type">交易类型,主要区分Trade Record 文件名称</param>
        /// <param name="result">指令</param>
        /// <param name="beforeOrderType">之前的指令</param>
        private void sendOrderAfterEAExecuted(bool isAutoCommand, string type, string result, string beforeOrderTypeDetail)
        {
            _Context["CurrentOrderDetail"] = result;
            //同步内存变量到持久化变量中,整个程序中都采用持久化变量进行逻辑判断
            _StrategyConfig.CurrentOrderType = _Context["CurrentOrderType"].ToString();
            _StrategyConfig.CurrentOrderTypeDetail = _Context["CurrentOrderTypeDetail"].ToString();
            _StrategyConfig.CurrentOrderDetail = _Context["CurrentOrderDetail"].ToString();

            //加仓不改变原开单价格，避免止损采用新的开仓价
            string[] excludeTypes = { "IN_SELL", "IN_BUY", "M1_BUY", "M2_BUY", "M3_BUY", "M1_SELL", "M2_SELL", "M3_SELL" };

            if (!excludeTypes.Contains(_StrategyConfig.CurrentOrderTypeDetail))
            {
                _StrategyConfig.CurrentPrice = Convert.ToDouble(_Context["price"]);
            }

            //发送指令时的价格
            _Context["CurrentPrice"] = _StrategyConfig.CurrentPrice.ToString();

            //更新执行指令时的判断指数
            _StrategyConfig.timeDurationGoldSumValue = 0;
            _Context["timeDurationGoldSumValue"] = _StrategyConfig.timeDurationGoldSumValue.ToString();
            _StrategyConfig.timeDurationGoldHourlyValue = 0;
            _Context["timeDurationGoldHourlyValue"] = _StrategyConfig.timeDurationGoldHourlyValue.ToString();
            _StrategyConfig.timeDurationGoldDailyValue = 0;
            _Context["timeDurationGoldDailyValue"] = _StrategyConfig.timeDurationGoldDailyValue.ToString();
            _StrategyConfig.timeDurationGSValue = 0;
            _Context["timeDurationGSValue"] = _StrategyConfig.timeDurationGSValue.ToString();

            //EA_TRADEID
            TradePosition tradePositionInfo = (TradePosition)_Context["TradePositionInfo"];
            string eaTradeId = tradePositionInfo.eaTradeId;

            string afterOrderTypeDetail = _StrategyConfig.CurrentOrderTypeDetail;
            _Log.LogInfo("EA执行前后指令:before:" + beforeOrderTypeDetail + ",after:" + afterOrderTypeDetail);
            _Log.LogInfo("EA策略为:" + result);
            //反向开单指令只发到客户端,服务端数据还是原始指令
            //开反向单时,不能修改原始指令,否则会出现根据新的反向单开平仓,和原始单开单不一致情况,比如原始开单10单,反向单应该也是10单,如果修改原始指令,就会出现反向单不是10单的情况
            if (_StrategyConfig.reverseProportion)
            {
                string originalR = result;
                if ((originalR.Contains(":BUY:") || originalR.Contains(":IN_BUY:")) && !originalR.Contains("TREND_BUY"))
                {
                    result = result.Replace("BUY", "SELL");
                    _Log.LogInfo("反向单已勾选,开反向单,原始单[" + originalR + "],反向单[" + result + "]");
                }
                if ((originalR.Contains(":SELL:") || originalR.Contains(":IN_SELL:")) && !originalR.Contains("TREND_SELL"))
                {
                    result = result.Replace("SELL", "BUY");
                    _Log.LogInfo("反向单已勾选,开反向单,原始单[" + originalR + "],反向单[" + result + "]");
                }
                if (originalR.Contains("CLOSE_BUY"))
                {
                    result = result.Replace("CLOSE_BUY", "CLOSE_SELL");
                    _Log.LogInfo("反向单已勾选,开反向单,原始单[" + originalR + "],反向单[" + result + "]");
                }
                if (originalR.Contains("CLOSE_SELL"))
                {
                    result = result.Replace("CLOSE_SELL", "CLOSE_BUY");
                    _Log.LogInfo("反向单已勾选,开反向单,原始单[" + originalR + "],反向单[" + result + "]");
                }
            }

            //指令明细:orderTypeDetail:openPrice:closePrice:profit
            string orderTypeDetailRecord = "";
            double profit = 0;

            IDictionary<string, Object> profitResult = Utils.ClosePositionsInProfit(_Context);

            profit = Math.Round(Convert.ToDouble(profitResult["profit"]), 2);
            orderTypeDetailRecord = profitResult["orderTypeDetailRecord"].ToString();

            StringBuilder executeDesc = new StringBuilder();
            //XAUUSD:2004.79:BUY:1:50:50 HH:mm:ss
            //executeDesc.AppendFormat("{0} {1}", result, DateTime.Now.ToString("HH:mm:ss"));
            //XAUUSD:2004.79:BUY:1:50:50 HH:mm:ss orderType:openPrice:closePrice:profit
            executeDesc.AppendFormat("{0} {1} {2}", result, DateTime.Now.ToString("HH:mm:ss"), orderTypeDetailRecord);

            string message = executeDesc.ToString();

            if (!_IsSimulateTest)
            {
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
                string isAutoStr = isAutoCommand ? "自动" : "手动";
                _Log.LogInfo("发送" + isAutoStr + "交易消息:" + message);
                if (isAutoCommand)
                {
                    updateCurrentOrderTypeDetail();
                }
                updateCurrentOrderDetail();
                _Log.LogTradeRecord("", message);
            }
            if (!_IsSimulateTest)
            {
                SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\Ring08.wav");
                //简单播放一遍
                //player.Play();
                //循环播放
                //player.PlayLooping();
                //另起线程播放
                //需要在Task.Run之前获取该变量,否则该参数会被重置
                string PriceDiffList = "";
                if (_Context.ContainsKey("PriceDiffList"))
                {
                    PriceDiffList = _Context["PriceDiffList"].ToString();
                }
                Task.Run(() =>
                {
                    _Log.LogInfo("保存EA策略开始");
                    DBHelper.saveEACommand(_StrategyConfig.SelectedStrategy, isAutoCommand ? "Y" : "N", JsonConvert.SerializeObject(_StrategyConfig), JsonConvert.SerializeObject(_Context), JsonConvert.SerializeObject(_EAExecute._EAResult), _StrategyConfig.CurrentOrderTypeDetail + " " + message, _StrategyConfig.analysisDataType, eaTradeId);
                    _Log.LogInfo("保存EA策略完成");
                    DBHelper.saveNotify("EA", _StrategyConfig.analysisDataType, message, "", "");
                    player.Play();
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
            }
            else
            {
                _Log.LogInfo("保存EA策略开始");
                DBHelper.saveEATestCommand(_StrategySeq.ToString(), JsonConvert.SerializeObject(_StrategyConfig), JsonConvert.SerializeObject(_Context), JsonConvert.SerializeObject(_EAExecute._EAResult), _StrategyConfig.CurrentOrderTypeDetail + " " + message, _CurrentQuotePrice, _CurrentDateTime, _StrategyConfig.TestDuration, _StrategyConfig.keepProfitDiff.ToString(), _StrategyConfig.analysisDataType, profit, _TestBatchNo, eaTradeId);
                _Log.LogInfo("保存EA策略完成");
            }
            //平仓后需要重置的参数
            if (string.Equals(_StrategyConfig.CurrentOrderType, "CLOSE_BUY") || string.Equals(_StrategyConfig.CurrentOrderType, "CLOSE_SELL"))
            {
                //有策略执行后,需要重置的参数
                _StrategyConfig.overKeepProfit = false;
                _Context["overKeepProfit"] = _StrategyConfig.overKeepProfit ? "T" : "F";
                _StrategyConfig.overKeepProfit2 = false;
                _Context["overKeepProfit2"] = _StrategyConfig.overKeepProfit2 ? "T" : "F";
                //重置动态止盈值
                _StrategyConfig.dynamicProfit = 0;
                _Context["dynamicProfit"] = _StrategyConfig.dynamicProfit.ToString();
                //重置动态止损值,动态止损值默认为配置的止损值
                _StrategyConfig.dynamicLost = _StrategyConfig.stopLoss;
                _Context["dynamicLost"] = _StrategyConfig.dynamicLost.ToString();
                //重置最大盈亏为0
                _StrategyConfig.gsMaxProfitValue = 0;
                _Context["gsMaxProfitValue"] = _StrategyConfig.gsMaxProfitValue.ToString();
                _StrategyConfig.gsMaxLossValue = 0;
                _Context["gsMaxLossValue"] = _StrategyConfig.gsMaxLossValue.ToString();
                _Context["PriceDiffList"] = "";
                _StrategyConfig.CommandCreateTime = _CurrentDateTime;
                _Context["CommandCreateTime"] = _StrategyConfig.CommandCreateTime;


                _Context["MartingaleAddPosition1"] = false;
                _StrategyConfig.MartingaleAddPosition1 = false;
                _Context["MartingaleAddPosition2"] = false;
                _StrategyConfig.MartingaleAddPosition2 = false;
                _Context["MartingaleAddPosition3"] = false;
                _StrategyConfig.MartingaleAddPosition3 = false;
            }


        }
        /// <summary>
        /// 更新OrderTypeDetail下拉框值
        /// </summary>
        private void updateCurrentOrderTypeDetail()
        {
            if (comboBox_CurrentOrderTypeDetail.InvokeRequired)
            {
                comboBox_CurrentOrderTypeDetail.Invoke(new Action(updateCurrentOrderTypeDetail));
            }
            else
            {
                comboBox_CurrentOrderTypeDetail.SelectedItem = _StrategyConfig.CurrentOrderTypeDetail;
            }
        }

        /// <summary>
        /// 更新EA策略参数
        /// </summary>
        private void updateEaParaDetail(string bizCode)
        {
            if (numericUpDown_maxChangeDiffDuration.InvokeRequired)
            {
                numericUpDown_maxChangeDiffDuration.Invoke(new Action<string>(updateEaParaDetail), new object[] { bizCode });
            }
            else
            {
                if (string.Equals(bizCode, "10001"))
                {
                    numericUpDown_maxChangeDiffDuration.Value = 2;
                    _StrategyConfig.maxDiffChangeDuration = (double)numericUpDown_maxChangeDiffDuration.Value;
                }
                if (string.Equals(bizCode, "10002"))
                {
                    numericUpDown_maxChangeDiffDuration.Value = 1;
                    _StrategyConfig.maxDiffChangeDuration = (double)numericUpDown_maxChangeDiffDuration.Value;
                }
            }
        }

        /// <summary>
        /// 更新OrderDetail Label显示值
        /// </summary>
        private void updateCurrentOrderDetail()
        {
            if (label_currentOrderDetail.InvokeRequired)
            {
                label_currentOrderDetail.Invoke(new Action(updateCurrentOrderDetail));
            }
            else
            {
                label_currentOrderDetail.Text = _StrategyConfig.CurrentOrderDetail;
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
            //修改配置后要重新加载配置数据
            loadEAList();
        }

        private void button_EAConfigSave_Click(object sender, EventArgs e)
        {
            saveEAConfig();
            //保存完成后重新加载EA数据
            loadEAListFromFile();
            _EAChart._ChunkSize = Convert.ToInt32(_StrategyConfig.timeDuration);
            _EAChart._Slope = Convert.ToInt32(_StrategyConfig.slope);
            //修改配置后要重新加载配置数据
            loadEAList();
        }
        /// <summary>
        /// 保存配置参数到文件StrategyConfig
        /// </summary>
        public void saveEAConfig()
        {
            string newStrategyFile = rootPath + _StrategyFile;
            _StrategyConfig.EAEntityList.Clear();
            _StrategyConfig.EAEntity.Clear();
            EAEntity buyEAEntity = new EAEntity();
            buyEAEntity.code = "BUY";
            buyEAEntity.name = "买多";
            buyEAEntity.sum = (double)numericUpDown_buySum.Value;
            buyEAEntity.sum1 = (double)numericUpDown_buySum1.Value;
            buyEAEntity.daily = (double)numericUpDown_buyDaily.Value;
            buyEAEntity.daily1 = (double)numericUpDown_buyDaily1.Value;
            buyEAEntity.hourly = (double)numericUpDown_buyHourly.Value;
            buyEAEntity.hourly1 = (double)numericUpDown_buyHourly1.Value;
            buyEAEntity.gs = (double)numericUpDown_buyGS.Value;
            buyEAEntity.gs1 = (double)numericUpDown_buyGS1.Value;
            buyEAEntity.active = true;
            _StrategyConfig.EAEntityList.Add(buyEAEntity);
            _StrategyConfig.EAEntity.Add(buyEAEntity.code, buyEAEntity);

            EAEntity sellEAEntity = new EAEntity();
            sellEAEntity.code = "SELL";
            sellEAEntity.name = "卖空";
            sellEAEntity.sum = (double)numericUpDown_sellSum.Value;
            sellEAEntity.sum1 = (double)numericUpDown_sellSum1.Value;
            sellEAEntity.daily = (double)numericUpDown_sellDaily.Value;
            sellEAEntity.daily1 = (double)numericUpDown_sellDaily1.Value;
            sellEAEntity.hourly = (double)numericUpDown_sellHourly.Value;
            sellEAEntity.hourly1 = (double)numericUpDown_sellHourly1.Value;
            sellEAEntity.gs = (double)numericUpDown_sellGS.Value;
            sellEAEntity.gs1 = (double)numericUpDown_sellGS1.Value;
            sellEAEntity.active = true;
            _StrategyConfig.EAEntityList.Add(sellEAEntity);
            _StrategyConfig.EAEntity.Add(sellEAEntity.code, sellEAEntity);


            EAEntity closeBuyEAEntity = new EAEntity();
            closeBuyEAEntity.code = "CLOSE_BUY";
            closeBuyEAEntity.name = "买平";
            closeBuyEAEntity.sum = (double)numericUpDown_closeBuySum.Value;
            closeBuyEAEntity.sum1 = (double)numericUpDown_closeBuySum1.Value;
            closeBuyEAEntity.daily = (double)numericUpDown_closeBuyDaily.Value;
            closeBuyEAEntity.daily1 = (double)numericUpDown_closeBuyDaily1.Value;
            closeBuyEAEntity.hourly = (double)numericUpDown_closeBuyHourly.Value;
            closeBuyEAEntity.hourly1 = (double)numericUpDown_closeBuyHourly1.Value;
            closeBuyEAEntity.gs = (double)numericUpDown_closeBuyGS.Value;
            closeBuyEAEntity.gs1 = (double)numericUpDown_closeBuyGS1.Value;
            closeBuyEAEntity.active = true;
            _StrategyConfig.EAEntityList.Add(closeBuyEAEntity);
            _StrategyConfig.EAEntity.Add(closeBuyEAEntity.code, closeBuyEAEntity);

            EAEntity closeSellEAEntity = new EAEntity();
            closeSellEAEntity.code = "CLOSE_SELL";
            closeSellEAEntity.name = "卖平";
            closeSellEAEntity.sum = (double)numericUpDown_closeSellSum.Value;
            closeSellEAEntity.sum1 = (double)numericUpDown_closeSellSum1.Value;
            closeSellEAEntity.daily = (double)numericUpDown_closeSellDaily.Value;
            closeSellEAEntity.daily1 = (double)numericUpDown_closeSellDaily1.Value;
            closeSellEAEntity.hourly = (double)numericUpDown_closeSellHourly.Value;
            closeSellEAEntity.hourly1 = (double)numericUpDown_closeSellHourly1.Value;
            closeSellEAEntity.gs = (double)numericUpDown_closeSellGS.Value;
            closeSellEAEntity.gs1 = (double)numericUpDown_closeSellGS1.Value;
            closeSellEAEntity.active = true;
            _StrategyConfig.EAEntityList.Add(closeSellEAEntity);
            _StrategyConfig.EAEntity.Add(closeSellEAEntity.code, closeSellEAEntity);

            //公共参数配置
            _StrategyConfig.timeDuration = (double)numericUpDown_timeDuration.Value;
            _StrategyConfig.slope = (double)numericUpDown_slope.Value;
            _StrategyConfig.takeProfit = (double)numericUpDown_takeProfit.Value;
            _StrategyConfig.stopLoss = (double)numericUpDown_stopLoss.Value;
            _StrategyConfig.NotradeDuration = textBox_notradeDuration.Text;
            _StrategyConfig.keepProfit = (double)numericUpDown_keepProfit.Value;
            _StrategyConfig.keepProfitDiff = (double)numericUpDown_keepProfitDiff.Value;
            _StrategyConfig.minChangeDiff = (double)numericUpDown_minChangeDiff.Value;
            _StrategyConfig.maxChangeDiff = (double)numericUpDown_maxChangeDiff.Value;
            _StrategyConfig.maxDiffChangeDuration = (double)numericUpDown_maxChangeDiffDuration.Value;
            _StrategyConfig.analysisDataType = comboBox_analysisDataType.Text;
            _StrategyConfig.limitOrder = checkBox_limitOrder.Checked;
            _StrategyConfig.openDiff = 0;
            _StrategyConfig.continuousOrder = checkBox_continuousOrder.Checked;
            _StrategyConfig.reverseProportion = checkBox_reverseProportion.Checked;
            _StrategyConfig.indexClose = checkBox_indexClose.Checked;
            _StrategyConfig.EAKP = checkBox_EAKP.Checked;
            _StrategyConfig.EAKL = checkBox_EAKL.Checked;

            //基础配置数据
            _Config.PublishAddressDS = textBox_dsAddress.Text;
            _StrategyConfig.DSType = comboBox_dsType.Text;
            _StrategyConfig.NotifyFlag = checkBox_NotifyFlag.Checked;
            _StrategyConfig.trendOrder = (double)numericUpDown_trendOrder.Value;
            _StrategyConfig.trendOrder1 = (double)numericUpDown_trendOrder1.Value;
            //_StrategyConfig.CurrentOrderTypeDetail = comboBox_CurrentOrderTypeDetail.Text;
            _StrategyConfig.increasePositionPoint = (double)numericUpDown_increase.Value;
            _StrategyConfig.commandTimeDuration = (double)numericUpDown_CommandTimeDuration.Value;

            //回测参数
            _StrategyConfig.TestDuration = textBox_TestDuration.Text;
            _StrategyConfig.retestNumDuration = textBox_retestNumDuration.Text;
            _StrategyConfig.openMax = (int)numericUpDown_openMax.Value;
            _StrategyConfig.openDuration = (int)numericUpDown_openDuration.Value;
            _StrategyConfig.closeDuration = (int)numericUpDown_closeDuration.Value;
            _StrategyConfig.offset = (int)numericUpDown_offset.Value;
            _StrategyConfig.hourlyOffset = (int)numericUpDown_hourlyOffset.Value;

            File.WriteAllText(newStrategyFile, JsonConvert.SerializeObject(_StrategyConfig));
            _Log.LogInfo("保存成功");
        }

        /// <summary>
        /// 保存运行中的动态参数
        /// </summary>
        private void saveEADynamicParameter()
        {
            string newStrategyFile = rootPath + _StrategyFile;
            //File.WriteAllText(newStrategyFile, JsonConvert.SerializeObject(_StrategyConfig));

            //运行数据
            _StrategyConfig.Context = _Context;
            if (_Context.ContainsKey("CurrentOrderType"))
            {
                _StrategyConfig.CurrentOrderType = _Context["CurrentOrderType"].ToString();
            }
            else
            {
                _StrategyConfig.CurrentOrderType = "";
            }
            if (_Context.ContainsKey("CurrentOrderTypeDetail"))
            {
                _StrategyConfig.CurrentOrderTypeDetail = _Context["CurrentOrderTypeDetail"].ToString();
            }
            else
            {
                _StrategyConfig.CurrentOrderTypeDetail = "";
            }
            if (_Context.ContainsKey("CurrentOrderDetail"))
            {
                _StrategyConfig.CurrentOrderDetail = _Context["CurrentOrderDetail"].ToString();
            }
            else
            {
                _StrategyConfig.CurrentOrderDetail = "";
            }

            if (_Context.ContainsKey("gsMaxProfitValue"))
            {
                _StrategyConfig.gsMaxProfitValue = Convert.ToDouble(_Context["gsMaxProfitValue"]);
            }
            else
            {
                _StrategyConfig.gsMaxProfitValue = 0;
            }
            if (_Context.ContainsKey("gsMaxLossValue"))
            {
                _StrategyConfig.gsMaxLossValue = Convert.ToDouble(_Context["gsMaxLossValue"]);
            }
            else
            {
                _StrategyConfig.gsMaxLossValue = 0;
            }
            if (_Context.ContainsKey("TradePositionInfo") && _Context["TradePositionInfo"] != null && _Context["TradePositionInfo"] is TradePosition)
            {
                _StrategyConfig.TradePositionInfo = (TradePosition)_Context["TradePositionInfo"];
            }
            else
            {
                _StrategyConfig.TradePositionInfo = null;
            }

            File.WriteAllText(newStrategyFile, JsonConvert.SerializeObject(_StrategyConfig));
            _Log.LogInfo("保存成功");
        }

        private void EAForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            saveEAConfig();
            saveEADynamicParameter();
        }

        private void button_savePlatformConfig_Click(object sender, EventArgs e)
        {
            //保存回测参数
            MgrEATestData.SetInitData((int)numericUpDown_openMax.Value, (int)numericUpDown_openDuration.Value, (int)numericUpDown_closeDuration.Value, (int)numericUpDown_offset.Value, (int)numericUpDown_hourlyOffset.Value);
            if (!string.IsNullOrEmpty(comboBox_eaData.Text))
            {
                string typeNum = comboBox_eaData.Text.Split('|')[0];
                if (string.Equals(typeNum, "1"))
                {
                    _StrategyConfigTestData.EAEntityList = MgrEATestData.GenerateEAEntityList();
                }
                if (string.Equals(typeNum, "2"))
                {
                    _StrategyConfigTestData.EAEntityList = MgrEATestData.GenerateEAEntityList1();
                }
                if (string.Equals(typeNum, "3"))
                {
                    _StrategyConfigTestData.EAEntityList = MgrEATestData.GenerateEAEntityList2();
                }
                _StrategyConfigTestData.paras = "生成策略参数设置[eaDataType=" + comboBox_eaData.Text + "][OpenMax=" + numericUpDown_openMax.Value + "][OpenDuration=" + numericUpDown_openDuration.Value + "][CloseDuration=" + numericUpDown_closeDuration.Value + "][Offset=" + numericUpDown_offset.Value + "][HourlyOffset=" + numericUpDown_hourlyOffset.Value + "]";
            }
            if (!string.IsNullOrEmpty(textBox_batchNo.Text))
            {
                _TestBatchNo = textBox_batchNo.Text;
            }
            SaveConfig();
            saveEATestData();
            ////////////////////
            saveEAConfig();
            saveEADynamicParameter();
            loadEAListFromFile();
            //修改配置后要重新加载配置数据
            loadEAList();
        }

        /// <summary>
        /// 回测批次,默认为当前时间戳
        /// </summary>
        private string _TestBatchNo = DateTime.Now.ToString("yyyyMMddHHmmss");
        /// <summary>
        /// 是否运行调测
        /// </summary>
        private bool _IsDebutTest = false;
        ///<summary>
        /// 是否运行回测
        /// </summary>
        private bool _IsSimulateTest = false;
        /// <summary>
        /// 回测交易日志目录，每次回测的所有文件生成在同一个目录中
        /// </summary>
        private string _SimulateTestTradeInfoDirectoryPath = "";
        private string _SimulateTestDate = "";
        /// <summary>
        /// 回测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_test_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("是否启动指令回测?", "提示", MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Yes)
            {
                label_testProgress.Visible = true;
                clearContext();
                AsynExec(this.button_test, () =>
                {
                    _IsSimulateTest = true;
                    if (string.IsNullOrEmpty(_SimulateTestTradeInfoDirectoryPath))
                    {
                        _SimulateTestTradeInfoDirectoryPath = Utils.GenerateDirectoryPath("EATest");
                    }
                    if (string.IsNullOrEmpty(_SimulateTestDate))
                    {
                        System.DateTime currentTime = System.DateTime.Now;
                        _SimulateTestDate = currentTime.ToString("yyyy-MM-dd");
                    }
                    simulateTest();
                    //button_test.Enabled = true;
                    _IsSimulateTest = false;
                });
            }
        }
        /// <summary>
        /// 动态止盈止损值刷新
        /// </summary>
        /// <param name="prices"></param>
        /// <returns></returns>
        private bool keepProfitAndLoss(double prices)
        {
            if (string.Equals(_StrategyConfig.CurrentOrderType, "BUY"))
            {
                //止盈
                if (prices - _StrategyConfig.CurrentPrice >= _StrategyConfig.takeProfit)
                {
                    return true;
                }
                //止损
                if (_StrategyConfig.CurrentPrice - prices >= _StrategyConfig.stopLoss)
                {
                    return true;
                }
                if (_StrategyConfig.TradePositionInfo != null && (decimal)prices < _StrategyConfig.TradePositionInfo.StopLoss)
                {
                    return true;
                }
                //keepProfitDiffPoint=当前价格-开仓价格,正数为盈利,负数为亏损
                double keepProfitDiffPoint = Math.Round(prices - _StrategyConfig.CurrentPrice, 2);
                //_Log.LogInfo("BUY=prices="+ prices + ",CurrentPrice="+ _StrategyConfig.CurrentPrice + ",keepProfitDiffPoint=" + keepProfitDiffPoint);
                //越过动态止盈线
                if (keepProfitDiffPoint >= _StrategyConfig.keepProfit)
                {
                    _StrategyConfig.overKeepProfit = true;
                    //动态止盈值跟随盈利值动态修改                          
                    if (keepProfitDiffPoint - _StrategyConfig.keepProfitDiff > _StrategyConfig.dynamicProfit)
                    {
                        _StrategyConfig.dynamicProfit = Math.Round(keepProfitDiffPoint - _StrategyConfig.keepProfitDiff, 2);
                    }
                }

                if (keepProfitDiffPoint >= _StrategyConfig.keepProfit - _StrategyConfig.keepProfitDiff * 0.5)
                {
                    _StrategyConfig.overKeepProfit2 = true;
                }
                //修改动态止损值
                if (keepProfitDiffPoint > 0 && keepProfitDiffPoint > (_StrategyConfig.stopLoss - _StrategyConfig.dynamicLost))
                {
                    _StrategyConfig.dynamicLost = Math.Round(_StrategyConfig.stopLoss - keepProfitDiffPoint, 2);
                }

                if (keepProfitDiffPoint > _StrategyConfig.gsMaxProfitValue)
                {
                    _StrategyConfig.gsMaxProfitValue = keepProfitDiffPoint;
                }
                if (keepProfitDiffPoint < _StrategyConfig.gsMaxLossValue)
                {
                    _StrategyConfig.gsMaxLossValue = keepProfitDiffPoint;
                }

                //越过动态止盈线后利润回测平仓
                if (_StrategyConfig.overKeepProfit && keepProfitDiffPoint <= _StrategyConfig.dynamicProfit)
                {
                    return true;
                }

                //越过动态止盈线后利润回测平仓
                if (_StrategyConfig.overKeepProfit2 && keepProfitDiffPoint <= _StrategyConfig.keepProfit - _StrategyConfig.keepProfitDiff)
                {
                    return true;
                }

                //动态止损
                if (-keepProfitDiffPoint > _StrategyConfig.dynamicLost)
                {
                    return true;
                }

                //动态加仓,避免重复加仓
                if (-keepProfitDiffPoint >= _StrategyConfig.increasePositionPoint && -keepProfitDiffPoint > 5 && (string.Equals(_StrategyConfig.CurrentOrderTypeDetail, "BUY")
                    || string.Equals(_StrategyConfig.CurrentOrderTypeDetail, "TREND_BUY")))
                {
                    return true;
                }
            }
            if (string.Equals(_StrategyConfig.CurrentOrderType, "SELL"))
            {
                //止盈
                if (_StrategyConfig.CurrentPrice - prices >= _StrategyConfig.takeProfit)
                {
                    return true;
                }
                //止损
                if (prices - _StrategyConfig.CurrentPrice >= _StrategyConfig.stopLoss)
                {
                    return true;
                }

                if (_StrategyConfig.TradePositionInfo != null && (decimal)prices > _StrategyConfig.TradePositionInfo.StopLoss)
                {
                    return true;
                }

                //keepProfitDiffPoint = 当前价-开仓价
                double keepProfitDiffPoint = Math.Round(_StrategyConfig.CurrentPrice - prices, 2);
                //_Log.LogInfo("keepProfitAndLoss:prices=" + prices + ",CurrentPrice=" + _StrategyConfig.CurrentPrice + ",keepProfitDiffPoint=" + keepProfitDiffPoint + ",_StrategyConfig.dynamicLost="+ _StrategyConfig.dynamicLost);
                //越过动态止盈线
                if (keepProfitDiffPoint >= _StrategyConfig.keepProfit)
                {
                    _StrategyConfig.overKeepProfit = true;
                    //动态止盈值跟随盈利值动态修改                          
                    if (keepProfitDiffPoint - _StrategyConfig.keepProfitDiff > _StrategyConfig.dynamicProfit)
                    {
                        _StrategyConfig.dynamicProfit = Math.Round(keepProfitDiffPoint - _StrategyConfig.keepProfitDiff, 2);
                    }
                }
                //越过动态止盈线
                if (keepProfitDiffPoint >= _StrategyConfig.keepProfit - _StrategyConfig.keepProfitDiff * 0.5)
                {
                    _StrategyConfig.overKeepProfit2 = true;
                }
                //修改动态止损值
                if (keepProfitDiffPoint > 0 && keepProfitDiffPoint > (_StrategyConfig.stopLoss - _StrategyConfig.dynamicLost))
                {
                    _StrategyConfig.dynamicLost = Math.Round(_StrategyConfig.stopLoss - keepProfitDiffPoint, 2);
                }
                if (keepProfitDiffPoint > _StrategyConfig.gsMaxProfitValue)
                {
                    _StrategyConfig.gsMaxProfitValue = keepProfitDiffPoint;
                }
                if (keepProfitDiffPoint < _StrategyConfig.gsMaxLossValue)
                {
                    _StrategyConfig.gsMaxLossValue = keepProfitDiffPoint;
                }

                //越过动态止盈线后利润回测平仓
                if (_StrategyConfig.overKeepProfit && keepProfitDiffPoint <= _StrategyConfig.dynamicProfit)
                {
                    return true;
                }

                //越过动态止盈线后利润回测平仓
                if (_StrategyConfig.overKeepProfit2 && keepProfitDiffPoint <= _StrategyConfig.keepProfit - _StrategyConfig.keepProfitDiff)
                {
                    return true;
                }

                //动态止损
                if (-keepProfitDiffPoint > _StrategyConfig.dynamicLost)
                {
                    return true;
                }

                //动态加仓,避免重复加仓
                if (-keepProfitDiffPoint >= _StrategyConfig.increasePositionPoint && -keepProfitDiffPoint > 5 && (string.Equals(_StrategyConfig.CurrentOrderTypeDetail, "SELL")
                    || string.Equals(_StrategyConfig.CurrentOrderTypeDetail, "TREND_SELL")))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 模拟测试
        /// </summary>
        private void simulateTest()
        {
            timer_eAExecute.Stop();
            timer_chartDataLoad.Stop();
            timer_AccountPosionRefresh.Stop();
            //timer_ParaEAExcute.Stop();
            if (_StrategyConfigTestData != null && _StrategyConfigTestData.EAEntityList != null && _StrategyConfigTestData.EAEntityList.Count > 0)
            {
                //清除历史回测数据
                //DBHelper.deleteTestData(_StrategyConfig.TestDuration, _StrategyConfig.keepProfitDiff.ToString(), _StrategyConfig.analysisDataType, _RetestStartNum);
                _Log.LogInfo("共计[" + _StrategyConfigTestData.EAEntityList.Count + "]组回回测数据");
                simulateTestProcess(_StrategyConfig.analysisDataType);
                simulateTestResult(_StrategyConfig.analysisDataType);
            }
            timer_eAExecute.Start();
            timer_chartDataLoad.Start();
            timer_AccountPosionRefresh.Start();
        }
        /// <summary>
        /// 系统当前时间,在策略执行时自动更新
        /// </summary>
        string _CurrentDateTime = null;
        /// <summary>
        /// 回测策略序号
        /// </summary>
        int _StrategySeq = 0;
        /// <summary>
        /// 按照价格时间维度回测
        /// </summary>
        /// <param name="testType">D,H</param>
        public void simulateTestProcess(string testType)
        {
            if (string.IsNullOrEmpty(_StrategyConfig.retestNumDuration))
            {
                _RetestStartNum = 0;
                _RetestEndNum = _StrategyConfigTestData.EAEntityList.Count;
            }
            else
            {
                if (_StrategyConfig.retestNumDuration.Contains("-"))
                {
                    string[] durationNum = textBox_retestNumDuration.Text.Split('-');
                    _RetestStartNum = Convert.ToInt16(durationNum[0]);
                    _RetestEndNum = Convert.ToInt16(durationNum[1]);
                }
                else
                {
                    _RetestStartNum = Convert.ToInt16(_StrategyConfig.retestNumDuration);
                    _RetestEndNum = _StrategyConfigTestData.EAEntityList.Count;
                }
            }
            _StrategySeq = _RetestStartNum;
            if (!string.Equals(_StrategyConfig.analysisDataType, "D"))
            {
                _RetestEndNum = _RetestStartNum + 1;
            }
            DBHelper.deleteTestData(_TestBatchNo, _RetestStartNum, _RetestEndNum);
            DateTime startTime = DateTime.Now;
            //如果程序出现问题，可以从选择的策略开始重新测试
            for (int i = _RetestStartNum; i < _RetestEndNum; i++)
            {
                TimeSpan interval = DateTime.Now - startTime;
                updateTestProgress("Time:" + (int)interval.TotalMinutes + "min," + (i + 1 - _RetestStartNum) + "/" + (_RetestEndNum - _RetestStartNum) + "," + Math.Round(((double)(i + 1 - _RetestStartNum) / (_RetestEndNum - _RetestStartNum) * 100), 2) + "%");
                //_Log.LogInfo("第["+i+"]组回测数据开始");
                //加载第i组策略配置信息
                Dictionary<string, EAEntity> eAEntityList = _StrategyConfigTestData.EAEntityList[i];
                _StrategyConfig.CurrentOrderType = "CLOSE_BUY";
                _StrategyConfig.CurrentOrderDetail = "";
                _StrategyConfig.CurrentOrderTypeDetail = "CLOSE_BUY";
                _StrategyConfig.CommandCreateTime = "";
                _StrategyConfig.EAEntityList.Clear();
                _StrategyConfig.EAEntityList.Add(eAEntityList["BUY"]);
                _StrategyConfig.EAEntityList.Add(eAEntityList["SELL"]);
                _StrategyConfig.EAEntityList.Add(eAEntityList["CLOSE_BUY"]);
                _StrategyConfig.EAEntityList.Add(eAEntityList["CLOSE_SELL"]);
                //当前的策略数据
                _StrategyConfig.EAEntity = eAEntityList;
                //当前策略编号
                _StrategyConfig.SelectedStrategy = _StrategySeq.ToString();

                //修改配置后要重新加载配置数据
                loadEAList();
                _QuoteList = DSHelper.getQuoteList(_StrategyConfig.TestDuration);
                if (_QuoteList != null && _QuoteList.Count > 0)
                {
                    //_Log.LogInfo("回测开始,回测数据:" + quoteList.Count + "条");
                    int count = 0;
                    foreach (IDictionary<string, string> data in _QuoteList)
                    {
                        count++;
                        //_Log.LogInfo("已执行回测数据:" + count);
                        //_Log.LogInfo("回测数据:id:" + data["id"] + ",symbol:" + data["symbol"] + ",price:" + data["price"] + ",time:" + data["time"]);
                        string time = data["time"];
                        _CurrentDateTime = time;
                        string price = "";
                        price = data["price"];
                        if (string.IsNullOrEmpty(price) || string.Equals(price, "0"))
                        {
                            continue;
                        }
                        _CurrentQuotePrice = price;
                        _StrategyConfig.GOLD_PRICE = Convert.ToDouble(price);
                        updateBrokePrice(price);

                        bool isTradeTime = Utils.checkIsTradeTime(_Config.SysConfig["EANotradeDuration"], _CurrentDateTime);
                        if (isTradeTime)
                        {
                            loadTestChartData(DateTime.Parse(_CurrentDateTime));
                            keepProfitAndLoss(Convert.ToDouble(price));//只需要判断是否需要止盈止损即可,后续策略会执行止盈止损
                            simulateTestEAExecute("simulateTest_" + _StrategyConfig.analysisDataType + "_" + i);
                        }
                    }
                }
                else
                {
                    _Log.LogInfo("回测结束,无回测数据");
                }
                _StrategySeq++;
            }
        }

        /// <summary>
        /// 统计回测结果
        /// </summary>
        /// <param name="testType"></param>
        /// 
        private void simulateTestResult(string testType)
        {
            if (string.IsNullOrEmpty(_StrategyConfig.retestNumDuration))
            {
                _RetestStartNum = 0;
                _RetestEndNum = _StrategyConfigTestData.EAEntityList.Count;
            }
            else
            {
                if (_StrategyConfig.retestNumDuration.Contains("-"))
                {
                    string[] durationNum = textBox_retestNumDuration.Text.Split('-');
                    _RetestStartNum = Convert.ToInt16(durationNum[0]);
                    _RetestEndNum = Convert.ToInt16(durationNum[1]);
                }
                else
                {
                    _RetestStartNum = Convert.ToInt16(_StrategyConfig.retestNumDuration);
                    _RetestEndNum = _StrategyConfigTestData.EAEntityList.Count;
                }
            }
            if (!string.Equals(_StrategyConfig.analysisDataType, "D"))
            {
                _RetestEndNum = _RetestStartNum + 1;
            }
            _Log.LogInfo("开始统计测试结果");
            //string[] files = Directory.GetFiles(_SimulateTestTradeInfoDirectoryPath);
            List<IDictionary<string, string>> testDates = DBHelper.getTestResult(testType, _StrategyConfig.TestDuration, _StrategyConfig.keepProfitDiff.ToString(), _TestBatchNo);
            _Log.LogInfo("共计回测策略[" + testDates.Count() + "]组");
            if (testDates != null && testDates.Count > 0)
            {
                List<string> results = new List<string>();
                double maxProfit = -999;
                double maxLoss = 999;
                //盈亏策略数
                int lossEACount = 0, profitEACount = 0;
                //盈亏总单数
                int lossCount = 0, profitCount = 0;
                double lossEAPoint = 0, profitEAPoint = 0; ;
                List<(int x, double y)> apoints = new List<(int x, double y)>();
                foreach (IDictionary<string, string> data in testDates)
                {
                    string eaNo = data["ea_no"];
                    double points = Math.Round(Double.Parse(data["points"]), 2);
                    apoints.Add((int.Parse(eaNo), points));
                    int orderCount = int.Parse(data["orderCount"]);
                    int profitOrderCount = int.Parse(data["profitOrderCount"]);
                    profitCount = profitCount + profitOrderCount;
                    int lostOrderCount = int.Parse(data["lostOrderCount"]);
                    lossCount = lossCount + lostOrderCount;
                    string orderList = data["orderList"];
                    results.Add(eaNo + ":" + points.ToString() + ",orderCount:" + orderCount + ",profitOrderCount:" + profitOrderCount + ",lostOrderCount:" + lostOrderCount + ":" + orderList);
                    if (points > maxProfit)
                    {
                        maxProfit = points;
                    }
                    if (points < maxLoss)
                    {
                        maxLoss = points;
                    }
                    if (points >= 0)
                    {
                        profitEACount++;
                        profitEAPoint = profitEAPoint + points;
                    }
                    else
                    {
                        lossEACount++;
                        lossEAPoint = lossEAPoint + points;
                    }
                }
                IDictionary<string, string> analysisResut = Utils.AnalysisEA(apoints);
                double maxPoints = Math.Round(Double.Parse(analysisResut["bestProfitPoint"]), 2);
                string maxEaNo = analysisResut["bestEA"];
                string allEaNos = analysisResut["area"];
                IDictionary<string, List<string>> resultD = new Dictionary<string, List<string>>();
                foreach (IDictionary<string, string> data in testDates)
                {
                    string eaNo = data["ea_no"];
                    string points = data["points"];
                    List<string> eaList;
                    if (resultD.ContainsKey(points))
                    {
                        eaList = resultD[points];
                    }
                    else
                    {
                        eaList = new List<string>();
                        resultD.Add(points, eaList);
                    }
                    eaList.Add(eaNo);
                }
                string fileName = _SimulateTestDate + "_" + "summary_" + testType + "_" + _StrategyConfig.keepProfitDiff;
                if (results != null && results.Count > 0)
                {
                    foreach (string result in results)
                    {
                        Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", result);
                    }
                    string resultPara1 = "TestBatchNo=" + _TestBatchNo + ",TestDuration=" + _StrategyConfig.TestDuration + ",lossEAPoint=" + lossEAPoint + ",profitEAPoint=" + profitEAPoint + ",lossEACount=" + lossEACount + ",profitEACount=" + profitEACount + ",totalLossOrderCount=" + lossCount + ",totalProfitOrderCount=" + profitCount;//测试时间段
                    string resultPara2 = Math.Round((lossEAPoint + profitEAPoint) / testDates.Count, 2).ToString();//平均盈亏
                    string resultPara3 = Math.Round((double)((lossCount + profitCount) / testDates.Count), 2).ToString();//平均单数
                    string resultPara4 = profitEACount + "/" + lossEACount;//盈亏策略
                    string resultPara5 = Math.Round((double)(profitCount / testDates.Count), 2).ToString() + "/" + Math.Round((double)(lossCount / testDates.Count), 2).ToString();//盈亏单数
                    string resultPara6 = Math.Round(Convert.ToDouble(resultPara2) - Convert.ToDouble(resultPara3) * 0.2, 2).ToString();//平均净盈亏
                    int aEaNo = Utils.GetMedianX(apoints);
                    IDictionary<string, string> adata = new Dictionary<string, string>();
                    foreach (IDictionary<string, string> data in testDates)
                    {
                        if (string.Equals(aEaNo.ToString(), data["ea_no"]))
                        {
                            adata = data;
                        }
                    }
                    //Console.WriteLine("testDates:"+ testDates.Count + ",aEaNo="+ aEaNo+ ",_RetestStartNum="+ _RetestStartNum);
                    double points = Math.Round(Double.Parse(adata["points"]), 2);
                    string resultPara7 = points.ToString();//盈亏中位数点数
                    int aorderCount = int.Parse(adata["orderCount"]);
                    int profitOrderCount = int.Parse(adata["profitOrderCount"]);
                    int lostOrderCount = int.Parse(adata["lostOrderCount"]);
                    string resultPara8 = Math.Round(points - aorderCount * 0.2, 2).ToString();//盈亏中位数对应的净盈亏
                    string resultPara9 = "策略编号=" + aEaNo + ",盈利单数=" + profitOrderCount + ",亏损单数=" + lostOrderCount + ",盈利点数=" + points + ",净盈利点数=" + Math.Round(points - aorderCount * 0.2, 2).ToString();//中位数策略执行情况
                    string resultPara10 = "平均盈亏点数(总盈亏/总策略数):" + resultPara2 + ",盈亏策略数(盈利策略数/亏损策略数):" + resultPara4 + ",平均净盈亏点数(平均盈亏-平均单数*点差):" + resultPara6 + ",盈亏中位数对应的净盈亏(中位数盈亏-中位数单量*点差):" + resultPara8;


                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "***汇总统计*************************************************************************************");
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "***测试批次和时间段:" + resultPara1);
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "***平均盈亏点数(总盈亏/总策略数):" + resultPara2);
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "***平均单数(总单数/总策略数):" + resultPara3);
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "***盈亏策略数(盈利策略数/亏损策略数):" + resultPara4);
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "***平均盈亏单数(平均盈利单数/平均亏损单数):" + resultPara5);
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "***平均净盈亏点数(平均盈亏-平均单数*点差):" + resultPara6);
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "***盈亏中位点数(中位数盈亏):" + resultPara7);
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "***盈亏中位数对应的净盈亏(中位数盈亏-中位数单量*点差):" + resultPara8);
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "***盈亏中位数策略测试结果:" + resultPara9);
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "***测试结果汇总:" + resultPara10);
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "****************************************************************************************");
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "最优策略:" + "Best EA No.=" + maxEaNo + ",Points=" + maxPoints + ",EA Area:" + allEaNos);
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "数据统计:ProfitEACount=" + profitEACount + ",LossEACount=" + lossEACount + ",Max Profit=" + maxProfit + ",Max Loss=" + maxLoss);
                    Dictionary<string, EAEntity> myDictionary = _StrategyConfigTestData.EAEntityList[int.Parse(maxEaNo)];
                    string para = "";
                    foreach (KeyValuePair<string, EAEntity> pair in myDictionary)
                    {
                        para = para + "," + pair.Value.ToString();
                    }
                    para = para.Substring(para.IndexOf(",") + 1);
                    Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "指数设置:" + para);
                }
                System.Text.StringBuilder strategyParameters = new System.Text.StringBuilder();
                strategyParameters.AppendFormat("timeDuration={0},takeProfit={1},stopLoss={2},keepProfit={3},keepProfitDiff={4},minChangeDiff={5},maxChangeDiff={6}," +
                    "maxDiffChangeDuration={7},analysisDataType={8},NotradeDuration={9},trendOrder={10},trendOrder1={11},continuousOrder={12},limitOrder={13},TestDuration={14},increasePositionPoint={15},CommandTimeDuration={16}", _StrategyConfig.timeDuration, _StrategyConfig.takeProfit, _StrategyConfig.stopLoss, _StrategyConfig.keepProfit, _StrategyConfig.keepProfitDiff, _StrategyConfig.minChangeDiff, _StrategyConfig.maxChangeDiff,
                    _StrategyConfig.maxDiffChangeDuration, _StrategyConfig.analysisDataType, _StrategyConfig.NotradeDuration, _StrategyConfig.trendOrder, _StrategyConfig.trendOrder1,
                    _StrategyConfig.continuousOrder, _StrategyConfig.limitOrder, _StrategyConfig.TestDuration, _StrategyConfig.increasePositionPoint, _StrategyConfig.commandTimeDuration);
                Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "公共参数设置:" + strategyParameters.ToString());
                Utils.AddMsgToTXT(_SimulateTestTradeInfoDirectoryPath, fileName + ".txt", "生成策略参数设置[OpenMax=" + numericUpDown_openMax.Value + "][OpenDuration=" + numericUpDown_openDuration.Value + "][CloseDuration=" + numericUpDown_closeDuration.Value + "][Offset=" + numericUpDown_offset.Value + "][HourlyOffset=" + numericUpDown_hourlyOffset.Value + "][DSType=" + _StrategyConfig.DSType + "]");

            }
            _Log.LogInfo("统计测试结果结束");
        }
        /// <summary>
        /// 更新全局变量_CurrentPrice 和价格显示
        /// </summary>
        /// <param name="price"></param>
        private void updateBrokePrice(string price)
        {
            if (label_BrokePrice.InvokeRequired)
            {
                label_BrokePrice.Invoke(new Action<string>(updateBrokePrice), new object[] { price });
            }
            else
            {
                label_BrokePrice.Text = price;
                _CurrentQuotePrice = price;
            }
        }
        /// <summary>
        /// 回测进度
        /// </summary>
        /// <param name="progress"></param>
        private void updateTestProgress(string progress)
        {
            if (label_testProgress.InvokeRequired)
            {
                label_testProgress.Invoke(new Action<string>(updateTestProgress), new object[] { progress });
            }
            else
            {
                label_testProgress.Text = progress;
            }
        }
        private void button_manualDsLoad_Click(object sender, EventArgs e)
        {
            loadChartData();
        }

        private void timer_chartDataLoad_Tick(object sender, EventArgs e)
        {
            //先加载图表数据，待数据源获取完成后再重新加载
            if (bgw_loadChartData.IsBusy != true)
            {
                bgw_loadChartData.RunWorkerAsync();
            }
        }

        private void timer_heartBeat_Tick(object sender, EventArgs e)
        {
            StringBuilder executeDesc = new StringBuilder();
            //XAUUSD:2004.79:BUY:1:50:50 HH:mm:ss
            executeDesc.AppendFormat("{0} {1}", "HEARTBEAT", DateTime.Now.ToString("HH:mm:ss"));
            string message = executeDesc.ToString();
            try
            {
                if (!_IsSimulateTest)
                {
                    //_Log.LogInfo("发送心跳消息开始:" + message);
                    _MasterPublisher.SendFrame(message);
                    //_Log.LogInfo("发送心跳消息完成");
                }
            }
            catch (NetMQ.FaultException ex)
            {
                _Log.LogInfo("发送消息异常:" + ex.ToString());
            }
            catch (Exception ex)
            {
                _Log.LogInfo("发送消息异常:" + ex.ToString());
            }
            //_Log.LogInfo("发送自动交易消息:" + message);
        }

        private void comboBox_symbol_SelectedIndexChanged(object sender, EventArgs e)
        {
            _TradeSymbol = comboBox_symbol.SelectedItem.ToString();
            InitialPara();
            GeneratePlatformObj();
            //重新订阅行情
            //重新订阅行情
            switch (_Platform.PlatformNo)
            {
                case (int)uClient.Comm.Enum.Platform.MF4:
                    if (_MF4.isConnect())
                    {
                        _MF4.SymbolSubscription(_TradeSymbol);
                    }
                    break;
                case (int)uClient.Comm.Enum.Platform.MT4:
                    if (_MT4.isConnect())
                    {
                        _MT4.SymbolSubscription(_TradeSymbol);
                    }
                    break;
                case (int)uClient.Comm.Enum.Platform.MT5:
                    if (_MT5.isConnect())
                    {
                        _MT5.SymbolSubscription(_TradeSymbol);
                    }
                    break;
            }
        }

        private void comboBox_AccType_SelectedIndexChanged(object sender, EventArgs e)
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
            textBox_account.Text = _Account.UserCode;
            textBox_password.Text = _Account.Password;
        }

        private void button_clearContext_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("清空Context数据会导致所有策略运行数据清空并从新加载策略，请确认是否清空？", "清空数据提示", MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Yes)
            {
                clearContext();
                loadEAList();
            }
        }
        private void clearContext()
        {
            _Context.Clear();
            _StrategyConfig.Context = _Context;
            _StrategyConfig.CurrentPrice = 0;
            _StrategyConfig.CurrentOrderType = "CLOSE_BUY";
            _StrategyConfig.CurrentOrderDetail = "CLOSE_BUY";
            _StrategyConfig.GOLD_PRICE = 0;
            _StrategyConfig.SILVER_PRICE = 0;
            _StrategyConfig.dynamicLost = 0;
            _StrategyConfig.gsMaxProfitValue = 0;
            _StrategyConfig.gsMaxLossValue = 0;

            saveEAConfig();
            _Log.LogInfo("清空Context数据完成");
        }
        /// <summary>
        /// 订单明细格式 symbol, price, orderType, "1", takeProfit, stopLoss
        /// </summary>
        public string orderDetailStr = "{0}:{1}:{2}:{3}:{4}:{5}";

        private void button_manualOrder_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox_CurrentOrderTypeDetail.Text))
            {
                _Log.LogInfo("清空交易指令和内存数据");
                label_currentOrderDetail.Text = "";
                _Context.Clear();
                saveEAConfig();
                saveEADynamicParameter();
                //保存完成后重新加载EA数据
                loadEAListFromFile();
            }
            else
            {
                string currentOrderTypeDetail = comboBox_CurrentOrderTypeDetail.Text;
                DialogResult dr = MessageBox.Show("是否需要手动发送策略指令[" + currentOrderTypeDetail + "]", "发送指令提示", MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Yes)
                {
                    manualOrder(currentOrderTypeDetail);
                }
            }
        }
        /// <summary>
        /// 手动发送指令
        /// </summary>
        private void manualOrder(string currentOrderTypeDetail)
        {
            _Context["price"] = _CurrentQuotePrice;
            //执行前OrderType
            string beforeOrderType = _StrategyConfig.CurrentOrderType;
            //执行前OrderTypeDetail
            //string beforeOrderTypeDetail = _StrategyConfig.CurrentOrderTypeDetail;
            //内存值修改未当前需要执行的指令
            _Context["CurrentOrderTypeDetail"] = currentOrderTypeDetail;
            _StrategyConfig.CurrentOrderTypeDetail = _Context["CurrentOrderTypeDetail"].ToString();

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "BUY"))
            {
                _Context["CurrentOrderType"] = "BUY";
            }
            if (string.Equals(_Context["CurrentOrderTypeDetail"], "TREND_BUY"))
            {
                _Context["CurrentOrderType"] = "BUY";
            }
            if (string.Equals(_Context["CurrentOrderTypeDetail"], "IN_BUY"))
            {
                _Context["CurrentOrderType"] = "BUY";
            }
            if (string.Equals(_Context["CurrentOrderTypeDetail"], "BUY_K"))
            {
                _Context["CurrentOrderType"] = "BUY";
            }
            if (string.Equals(_Context["CurrentOrderTypeDetail"], "SELL"))
            {
                _Context["CurrentOrderType"] = "SELL";
            }
            if (string.Equals(_Context["CurrentOrderTypeDetail"], "TREND_SELL"))
            {
                _Context["CurrentOrderType"] = "SELL";
            }
            if (string.Equals(_Context["CurrentOrderTypeDetail"], "IN_SELL"))
            {
                _Context["CurrentOrderType"] = "SELL";
            }
            if (string.Equals(_Context["CurrentOrderTypeDetail"], "SELL_K"))
            {
                _Context["CurrentOrderType"] = "SELL";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "BUY_E"))
            {
                _Context["CurrentOrderType"] = "BUY";
            }
            if (string.Equals(_Context["CurrentOrderTypeDetail"], "SELL_E"))
            {
                _Context["CurrentOrderType"] = "SELL";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_BUY"))
            {
                _Context["CurrentOrderType"] = "CLOSE_BUY";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_SELL"))
            {
                _Context["CurrentOrderType"] = "CLOSE_SELL";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_BUY_STOP_LOSS"))
            {
                _Context["CurrentOrderType"] = "CLOSE_BUY";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_SELL_STOP_LOSS"))
            {
                _Context["CurrentOrderType"] = "CLOSE_SELL";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_BUY_TAKE_PROFIT"))
            {
                _Context["CurrentOrderType"] = "CLOSE_BUY";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_SELL_TAKE_PROFIT"))
            {
                _Context["CurrentOrderType"] = "CLOSE_SELL";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_KEEP_BUY_PROFIT"))
            {
                _Context["CurrentOrderType"] = "CLOSE_BUY";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_KEEP_SELL_PROFIT"))
            {
                _Context["CurrentOrderType"] = "CLOSE_SELL";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_KEEP_BUY_LOSS"))
            {
                _Context["CurrentOrderType"] = "CLOSE_BUY";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_KEEP_SELL_LOSS"))
            {
                _Context["CurrentOrderType"] = "CLOSE_SELL";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_BUY_WEEKEND"))
            {
                _Context["CurrentOrderType"] = "CLOSE_BUY";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_SELL_WEEKEND"))
            {
                _Context["CurrentOrderType"] = "CLOSE_SELL";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_BUY_TRENDORDER"))
            {
                _Context["CurrentOrderType"] = "CLOSE_BUY";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_SELL_TRENDORDER"))
            {
                _Context["CurrentOrderType"] = "CLOSE_SELL";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "BUY_GS"))
            {
                _Context["CurrentOrderType"] = "BUY";
            }
            if (string.Equals(_Context["CurrentOrderTypeDetail"], "SELL_GS"))
            {
                _Context["CurrentOrderType"] = "SELL";
            }
            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_BUY_GS"))
            {
                _Context["CurrentOrderType"] = "CLOSE_BUY";
            }
            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_SELL_GS"))
            {
                _Context["CurrentOrderType"] = "CLOSE_SELL";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_BUY_E"))
            {
                _Context["CurrentOrderType"] = "CLOSE_BUY";
            }

            if (string.Equals(_Context["CurrentOrderTypeDetail"], "CLOSE_SELL_E"))
            {
                _Context["CurrentOrderType"] = "CLOSE_SELL";
            }

            _StrategyConfig.CurrentOrderType = _Context["CurrentOrderType"].ToString();
            StringBuilder result = new StringBuilder();
            string takeProfit = "50";
            string stopLoss = "50";
            string orderType = _StrategyConfig.CurrentOrderType;
            if (string.Equals(_StrategyConfig.CurrentOrderTypeDetail, "IN_BUY"))
            {
                orderType = "IN_BUY";
            }
            if (string.Equals(_StrategyConfig.CurrentOrderTypeDetail, "BUY_K"))
            {
                orderType = "BUY";
            }
            if (string.Equals(_StrategyConfig.CurrentOrderTypeDetail, "BUY_E"))
            {
                orderType = "BUY";
            }
            if (string.Equals(_StrategyConfig.CurrentOrderTypeDetail, "IN_SELL"))
            {
                orderType = "IN_SELL";
            }
            if (string.Equals(_StrategyConfig.CurrentOrderTypeDetail, "SELL_K"))
            {
                orderType = "SELL";
            }
            if (string.Equals(_StrategyConfig.CurrentOrderTypeDetail, "SELL_E"))
            {
                orderType = "SELL";
            }
            if (string.Equals(orderType, "CLOSE_BUY") || string.Equals(orderType, "CLOSE_SELL"))
            {
                _Context["closePrice"] = _Context["price"];
            }
            else
            {
                _Context["openPrice"] = _Context["price"];
            }
            if (string.Equals(orderType, "BUY") || string.Equals(orderType, "SELL"))
            {
                result.AppendFormat(orderDetailStr, symbol, _Context["openPrice"], orderType, "1", takeProfit, stopLoss);
            }
            if (string.Equals(orderType, "CLOSE_BUY") || string.Equals(orderType, "CLOSE_SELL"))
            {
                result.AppendFormat(orderDetailStr, symbol, _Context["closePrice"], orderType, "1", takeProfit, stopLoss);
            }

            sendOrderAfterEAExecuted(false, null, result.ToString(), beforeOrderType);
            saveEADynamicParameter();
        }

        private void button_tradeInfo_Click(object sender, EventArgs e)
        {
            string path = uClient.Comm.Utils.GenerateDirectoryPath("TradeInfo");
            System.DateTime currentTime = System.DateTime.Now;
            string fileName = currentTime.ToString("yyyy-MM-dd") + "_999999";
            string destFile = System.IO.Path.Combine(path, fileName + ".txt");
            if (File.Exists(destFile))
            {
                System.Diagnostics.Process.Start(destFile);
            }
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            lstTradeRecord.Items.Clear();
        }

        private void comboBox_dsType_SelectedIndexChanged(object sender, EventArgs e)
        {
            _StrategyConfig.DSType = comboBox_dsType.Text;
        }

        private void comboBox_TestDays_SelectedIndexChanged(object sender, EventArgs e)
        {
            _StrategyConfig.TestDuration = textBox_TestDuration.Text;
        }

        private void comboBox_SelectedStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Log.LogInfo("变更前选择的策略值为[" + _StrategyConfig.SelectedStrategy + "]");
            _StrategyConfig.SelectedStrategy = comboBox_SelectedStrategy.Text;
            _Log.LogInfo("变更后选择的策略值为[" + _StrategyConfig.SelectedStrategy + "]");
            Dictionary<string, EAEntity> eAEntityList = _StrategyConfigTestData.EAEntityList[Convert.ToInt16(_StrategyConfig.SelectedStrategy)];
            _StrategyConfig.EAEntityList.Clear();
            _StrategyConfig.EAEntityList.Add(eAEntityList["BUY"]);
            _StrategyConfig.EAEntityList.Add(eAEntityList["SELL"]);
            _StrategyConfig.EAEntityList.Add(eAEntityList["CLOSE_BUY"]);
            _StrategyConfig.EAEntityList.Add(eAEntityList["CLOSE_SELL"]);
            _StrategyConfig.EAEntity = eAEntityList;
            IniUI();
        }
        /// <summary>
        /// 检查平台连接是否正常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerCheckPlatformConnectStatus_Tick(object sender, EventArgs e)
        {
            //加载完成后,回测数据时不执行如下操作
            if (_IsLoadConfigCompleted && !_IsSimulateTest)
            {
                _CurrentDateTime = DBUtils.getDateTime();
                bool isTradeTime = Utils.checkIsTradeTime(_StrategyConfig.NotradeDuration, _CurrentDateTime);
                //_Log.LogInfo("平台拉起判断交易时间参数:[EANotradeDuration=" + _Config.SysConfig["EANotradeDuration"] + "][TradeTime=" + _Config.SysConfig["TradeTime"] + "][isTradeTime=" + isTradeTime + "]");
                //在EA策略执行时间内,MT4平台才启动自动拉起
                if (isTradeTime)
                {
                    switch (_Platform.PlatformNo)
                    {
                        case (int)uClient.Comm.Enum.Platform.MF4:
                            if (!_MF4.isConnect())
                            {
                                _Log.LogInfo("MF4平台自动断开连接,由系统自动拉起开始");
                                //先断开连接,再重新连接
                                _Log.LogInfo("断开连接");
                                //先断开平台连接,再启动连接
                                _MF4.DisConnect();
                                Task.Run(() =>
                                {
                                    //连接平台
                                    _MF4.Connect();
                                    if (isConnect())
                                    {
                                        AfterConnectCompleted();
                                    }
                                    _Log.LogInfo("MF4平台自动断开连接,由系统自动拉起完成");
                                });
                            }
                            break;
                        case (int)uClient.Comm.Enum.Platform.MT4:
                            if (!_MT4.isConnect() || string.Equals(label_BrokePrice.Text, "0"))
                            {
                                _Log.LogInfo("MT4平台自动断开连接,由系统自动拉起开始");
                                //先断开连接,再重新连接
                                _Log.LogInfo("断开连接");
                                //先断开平台连接,再启动连接
                                _MT4.DisConnect();
                                Task.Run(() =>
                                {
                                    //连接平台
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
                                _MT5.DisConnect();
                                Task.Run(() =>
                                {
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
        /// 测试报价数据
        /// </summary>
        List<IDictionary<string, string>> _QuoteList = null;
        /// <summary>
        /// 指数列表数据
        /// </summary>
        List<IDictionary<string, string>> _IndexList = null;

        string[] pts = new string[] { "00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00" };
        int ptSeq = 0;
        /// <summary>
        /// 生成测试数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerGenereateTestData_Tick(object sender, EventArgs e)
        {
            MgrEATestData.GenerateDailyTestData();
            MgrEATestData.GenerateHourlyTestData(pts[ptSeq]);
            _Log.LogInfo("生成策略测试数据完成");
            if (ptSeq == 23)
            {
                ptSeq = 0;
            }
            else
            {
                ptSeq++;
            }
        }
        /// <summary>
        /// 报价序列号
        /// </summary>
        int quoteSeq = 0;


        public string executeResultStr = "{0}:{1}:{2}:{3}:{4}:{5}";

        /// <summary>
        /// 更新跳次列表显示
        /// </summary>
        /// <param name="priceDiffList"></param>
        private void updatePriceDiffList(string priceDiffList)
        {
            if (label_priceDiffList.InvokeRequired)
            {
                label_priceDiffList.Invoke(new Action<string>(updatePriceDiffList), new object[] { priceDiffList });
            }
            else
            {
                label_priceDiffList.Text = priceDiffList;
            }
        }

        private void timer_ParaEAExcute_Tick(object sender, EventArgs e)
        {
            bool isSatEnd = Utils.checkIsSaturdayEndTime(DBUtils.getDateTime(), _StrategyConfig.NotradeDuration);
            if (!isSatEnd)
            {
                _Log.LogInfo("执行定时策略[EA参数]");
                //不判断平台是否已经连,都正常执行策略,如果平台未连接,只影响止盈止损策略执行
                eaParaExecute();
                _Log.LogInfo("定时策略执行结束[EA参数]");
            }
            else
            {
                _Log.LogInfo("非交易时间不执行");
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            saveEAConfig();
            //保存完成后重新加载EA数据
            loadEAListFromFile();
        }

        public void buttonDSConnect_Click(object sender, EventArgs e)
        {
            AsynExec(this.buttonDSConnect, () =>
            {
                if (buttonDSConnect.Text.Equals("DS连接"))
                {
                    DSConnection("DS");
                }
                else if (buttonDSConnect.Text.Equals("DS断开"))
                {
                    DSDisconnect("DS");
                }
                updateButtonDSConnectStatus(buttonDSConnect.Text);
            });
        }
        public void updateButtonDSConnectStatus(string status)
        {

            if (buttonDSConnect.InvokeRequired)
            {
                buttonDSConnect.Invoke(new Action<string>(updateButtonDSConnectStatus), new object[] { status });
            }
            else
            {
                if (string.Equals(status, "DS连接"))
                {
                    buttonDSConnect.Text = "DS断开";
                    buttonDSConnect.BackColor = Color.Green;
                }
                if (string.Equals(status, "DS断开"))
                {
                    buttonDSConnect.Text = "DS连接";
                    buttonDSConnect.BackColor = Color.Red;
                }
            }
        }
        /// <summary>
        /// 连接数据源
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public void DSConnection(string type)
        {
            string dsAddress;
            //连接之前先断连,避免出现端口被占用情况
            dsAddress = _Config.DSType == "T4" ? _Config.PublishAddressT4 : _Config.PublishAddressOEC;
            DSDisconnect("DS");
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
                if (bgwMarketQuote.IsBusy != true)
                {
                    bgwMarketQuote.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                _Log.LogInfo(string.Format("连接到{0}服务器时错误：{1}", _Config.DSType, ex.Message));
            }
        }

        /// <summary>
        /// 连接AI推理消息
        /// </summary>
        /// <param name="type"></param>
        public void AIMessageConnection(string type)
        {
            string dsAddress = "tcp://127.0.0.1:5751";
            //连接之前先断连,避免出现端口被占用情况
            try
            {
                _SubscriberAIMessage.Unbind(dsAddress);
                _Log.LogInfo("取消连接");
            }
            catch (Exception e)
            {
                _Log.LogInfo("取消连接," + e.Message);
            }
            _Log.LogInfo(string.Format("连接到{0}服务器：{1}", type, dsAddress));
            try
            {
                if (dsAddress == null || dsAddress == "")
                {
                    MessageBox.Show(string.Format("{0}服务器地址未配置", type));
                    return;
                }
                _SubscriberAIMessage.Options.TcpKeepalive = true;
                _SubscriberAIMessage.Options.TcpKeepaliveIdle = new TimeSpan(5, 0, 0);
                _SubscriberAIMessage.Options.TcpKeepaliveInterval = new TimeSpan(0, 0, 1);
                _SubscriberAIMessage.Connect(dsAddress);
                _SubscriberAIMessage.Subscribe("");

                if (bgwAIResult.IsBusy != true)
                {
                    bgwAIResult.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                _Log.LogInfo(string.Format("连接到{0}服务器时错误：{1}", _Config.DSType, ex.Message));
            }
        }

        public void DSDisconnect(string type)
        {
            bgwMarketQuote.CancelAsync();
            bgwAIResult.CancelAsync();
            string dsAddress = _Config.DSType == "T4" ? _Config.PublishAddressT4 : _Config.PublishAddressOEC;
            if (dsAddress.Contains("127.0.0.1"))
            {
                dsAddress = dsAddress.Replace("127.0.0.1", uClient.Comm.Utils.GetLocalIP());
            }
            _Log.LogInfo(string.Format("断开{0}服务器：{1}", type, dsAddress));
            if (string.Equals("DS", type))
            {
                //buttonDSConnect.Text = "DS连接";
                //buttonDSConnect.BackColor = Color.Red;
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
        }
        /// <summary>
        /// 策略数据分析是否运行
        /// </summary>
        public bool _EAAnalysisRunning = false;
        private void bgwMarketQuote_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            MarketQuote();
        }
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
                }

                if (results.StartsWith("Gold"))
                {
                    EA1(results);
                }
            }
        }
        /// <summary>
        /// 时差交易策略
        /// </summary>
        public void EA1(string results)
        {
            string[] split = results.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //参数说明：Gold 1893.4 1893.6 145158
            //_logger.Info("OnMarketQuote :" + split[0] + " " + split[1] + " " + split[2] + " " + split[3]);
            //_Log.LogInfo("接收到消息时间："+DateTime.Now.ToString("HH:mm:ss.fff"));
            //_DSQuote.NewQuoute(split[0], double.Parse(split[1]), double.Parse(split[2]));
            if (split != null && split.Length == 4)
            {
                updateDSPrice(double.Parse(split[1]));
            }
        }
        /// <summary>
        /// 更新DS价格
        /// </summary>
        /// <param name="prices"></param>
        public void updateDSPrice(double prices)
        {
            if (prices > 0)
            {
                updateDSPriceLabel(prices.ToString("f2"));
            }
        }
        /// <summary>
        /// 更新数据源价格显示
        /// </summary>
        /// <param name="price"></param>
        private void updateDSPriceLabel(string price)
        {
            if (label_DSPrice.InvokeRequired)
            {
                label_DSPrice.Invoke(new Action<string>(updateDSPriceLabel), new object[] { price });
            }
            else
            {
                label_DSPrice.Text = price;
            }
        }

        private void numericUpDown_timeDuration_ValueChanged(object sender, EventArgs e)
        {
            //及时把变化变量赋值到内存变量,避免未保存
            _StrategyConfig.timeDuration = (double)numericUpDown_timeDuration.Value;
        }

        private void comboBox_analysisDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            _StrategyConfig.analysisDataType = comboBox_analysisDataType.Text;
            //EA 策略加载完成后
            symbol = "XAUUSD";
            this.Text = "EA分析平台[策略" + _StrategyConfig.analysisDataType + "]";
            _Log._TradeWinName = _StrategyConfig.analysisDataType;
        }

        private void comboBox_eaData_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// AI推理消息接受
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bgwAIResult_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            _EAAnalysisRunning = true;
            string PreReceiveMsg = "";
            while (true)
            {
                string results = _SubscriberAIMessage.ReceiveFrameString();
                //_Log.LogInfo("接收消息："+ results);
                //{"timestamp": "2025-10-08 21:34:01.342057", "symbol": "XAUUSD", "close": 4033.875, "prob_long": 0.6571319699287415, "direction": "LONG"}
                if (results.Equals(PreReceiveMsg) || string.IsNullOrEmpty(results))
                {
                    continue;
                }
                else
                {
                    PreReceiveMsg = results;
                }
                if (!string.IsNullOrEmpty(PreReceiveMsg))
                {
                    _Log.LogInfo("接收消息：" + PreReceiveMsg);
                    dynamic obj = JsonConvert.DeserializeObject(PreReceiveMsg);
                    _Context["PredictedDirection"] = obj.direction;
                    _Context["PredictedProbability"] = obj.prob_long;
                    if (string.Equals(_StrategyConfig.CurrentOrderType, "CLOSE_BUY") || string.Equals(_StrategyConfig.CurrentOrderType, "CLOSE_SELL") || string.IsNullOrEmpty(_StrategyConfig.CurrentOrderType))
                    {
                        double PredictedProbability = Convert.ToDouble(obj.prob_long);
                        if ((string.Equals(obj.direction, "LONG") || string.Equals(obj.direction, "SHORT")) && PredictedProbability > _StrategyConfig.slope)
                        {
                            _Log.LogInfo("根据AI推理结果执行策略");
                            eaExecute();
                        }
                    }
                }

            }
        }

        private void chart_daily_Click(object sender, EventArgs e)
        {

        }
    }
}
