using NetMQ;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using uClient.Comm;

namespace uClient.Broker
{
    public partial class AutoTradeClientForm : uClient.TradeForm
    {
        /// <summary>
        /// 需要初始化平台本地变量,再赋值给父类变量
        /// </summary>
        //MT4 平台 begin
        public new MT4 _MT4;
        //MT4 平台 end

        //MF4 平台 beign
        public new MF4 _MF4;
        //MF4 平台 end

        //V4 平台 beign
        public new V4 _V4;
        //V4 平台 end

        //V4 平台 beign
        public new MT5 _MT5;
        //V4 平台 end

        /// <summary>
        /// 构造方法,Required
        /// </summary>
        public AutoTradeClientForm()
        {
            MainFormPara mainFormPara = new MainFormPara();
            mainFormPara.PlatformCode = "";
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
        /// 构造方法,Required
        /// </summary>
        /// <param name="mainFormPara"></param>
        public AutoTradeClientForm(MainFormPara mainFormPara)
        {
            if (mainFormPara == null)
            {
                mainFormPara = new MainFormPara();
                mainFormPara.PlatformCode = "MT4";
                mainFormPara.BrokerCode = "";
                mainFormPara.BrokerName = "";
                mainFormPara.FormType = "MT4";
                mainFormPara.FormNo = "";
            }
            _MainFormPara = mainFormPara;
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            InitObject();
        }
        /// <summary>
        /// Required
        /// 初始化对象,该方法会在父类Form初始化时调用
        /// </summary>
        public new void InitObject()
        { 
            this.timerCheckPlatformConnectStatus.Enabled = true;
            this.timerCheckPlatformConnectStatus.Interval = 60000;
            this.timerRefreshAutoTrade.Enabled = true;
            this.timerRefreshAutoTrade.Interval = 30000;
            _Log.LogInfo("制定对象初始化完成");
        }
        /// <summary>
        /// Required
        /// Client 中初始化平台对象,才能加载子类的方法
        /// </summary>
        public override void GeneratePlatformObj()
        {                     
            switch (_Platform.PlatformNo)
            {
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
                    }
                    _Log.LogInfo("GOLD:"+ _MT4._GOLD+ ",SILVER:"+ _MT4._SILVER);
                    //需要把变量赋值到父类变量,才能覆盖父类方法
                    base._MT4 = _MT4;
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
                    }
                    _Log.LogInfo("GOLD:" + _MT5._GOLD + ",SILVER:" + _MT5._SILVER);
                    //需要把变量赋值到父类变量,才能覆盖父类方法
                    base._MT5 = _MT5;
                    break;
            }
            _Log.LogInfo("ClientForm平台对象初始化完成");
        }
        /// <summary>
        /// 加载eaType地址信息,默认使用EA主数据服务器地址,平台配置参数等
        /// </summary>
        public void LoadEAType()
        {
            _Config.PlatformCode = _MainFormPara.PlatformCode;           
            IDictionary<string, string> eaInfo = DBHelper.getAutoInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo);
            if (eaInfo != null)
            {
                _Config.PublishAddressEA = "tcp://" + _Config.SysConfig[eaInfo["ea_type"]] + ":5553";
                _Config.DSType = "EA";
                _Config.clientType = "EA";
                _Log.LogInfo(string.Format("加载EA初始化参数:PublishAddressEA={0},DSType={1},clientType={2}", _Config.PublishAddressEA, _Config.DSType, _Config.clientType));
            }
            else
            {
                _Log.LogInfo("查询EA账户参数:Execute IP=" + Utils.GetLocalIP() + ",FormNo=" + _MainFormPara.FormNo);
                _Log.LogInfo("无EA账户信息");
            }
        }
        /// <summary>
        /// 重写该方法,自动加载EA配置信息
        /// </summary>
        public override void LoadConfig()
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
                            string defaultBrokerCode = string.IsNullOrEmpty(_MainFormPara.BrokerCode) ? _Platform.DefaultBrokerCode : _MainFormPara.BrokerCode;
                            _Broker = _Platform.BrokerList[defaultBrokerCode];
                            _Log.LogInfo("查询EA账户参数:Execute IP="+ Utils.GetLocalIP()+ ",FormNo="+ _MainFormPara.FormNo);
                            //替换成数据库中配置的交易账户信息
                            Comm.Broker queryBroker = DBHelper.getBrokerInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo);
                            if (queryBroker != null)
                            {
                                _Broker = queryBroker;

                                if (!_Platform.BrokerList.ContainsKey(queryBroker.BrokerCode))
                                {
                                    _Platform.Broker.Add(queryBroker);
                                    _Platform.BrokerList.Add(queryBroker.BrokerCode, queryBroker);
                                }
                                _Log.LogInfo("EA账户信息:"+ _Broker.toString());
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
                                _logger.Fatal("代理商[" + defaultBrokerCode + "]信息不存在，请检查配置文件");
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

            LoadEAType();
        }

        /// <summary>
        /// 连接拉起重试次数,最大不超过3次
        /// </summary>
        private int _ConnectRetryCount = 0;
        /// <summary>
        /// 数据源连接正常，平台连接不正常时，自动刷新平台的连接状态并发送断链通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public new void timerCheckPlatformConnectStatus_Tick(object sender, EventArgs e)
        {
            //加载完成后
            if (_IsLoadConfigCompleted)
            {
                bool isTradeTime = uClient.Comm.Utils.checkIsTradeTime(_Config.SysConfig["TradeNotradeDuration"], DateTime.Now.ToString(), _Config.SysConfig["TradeTime"]);
                //_Log.LogInfo("平台拉起判断交易时间参数:[TradeNotradeDuration=" + _Config.SysConfig["TradeNotradeDuration"] + "][TradeTime=" + _Config.SysConfig["TradeTime"] + "][isTradeTime="+ isTradeTime + "]");
                //在EA策略执行时间内,MT4平台才启动自动拉起
                //isTradeTime = true;
                if (isTradeTime && (_Platform.PlatformNo == 2 || _Platform.PlatformNo == 4))
                {
                    //如果是自动端口连接,系统自动拉起,只针对MT平台
                    if (string.Equals(lblMT4Bid.Text, "0") && _ConnectRetryCount <= 99)
                    {
                        if (_Platform.PlatformNo == 2 && !_MT4.isConnect())
                        {
                            Task.Run(() =>
                            {
                                _Log.LogInfo("MT4平台自动断开连接,由系统自动拉起开始");
                                _Log.LogInfo("第[" + _ConnectRetryCount + "]拉起平台");
                                _ConnectRetryCount++;
                                //先断开连接,再重新连接
                                _Log.LogInfo("断开连接");
                                _MT4._IsManullyDisconnect = true;
                                _MT4.DisConnect();
                                uClient.Comm.Utils.ClearAccountCaptial(accountDisplay);
                                UpdateConnectStatus(false);
                                PlatformConnectCheckFlag = true;
                                StopAutoTrade();
                                CancelCheck();
                                _MT4.Connect();
                                if (isConnect())
                                {
                                    _ConnectRetryCount = 0;
                                    AfterConnectCompleted();
                                }
                                _Log.LogInfo("MT4平台自动断开连接,由系统自动拉起完成");
                            });
                        }

                        if (_Platform.PlatformNo == 4 && !_MT5.isConnect())
                        {
                            Task.Run(() =>
                            {
                                _Log.LogInfo("MT5平台自动断开连接,由系统自动拉起开始");
                                _Log.LogInfo("第[" + _ConnectRetryCount + "]拉起平台");
                                _ConnectRetryCount++;
                                //先断开连接,再重新连接
                                _Log.LogInfo("断开连接");
                                _MT5._IsManullyDisconnect = true;
                                _MT5.DisConnect();
                                uClient.Comm.Utils.ClearAccountCaptial(accountDisplay);
                                UpdateConnectStatus(false);
                                PlatformConnectCheckFlag = true;
                                StopAutoTrade();
                                CancelCheck();
                                _MT5.Connect();
                                if (isConnect())
                                {
                                    _ConnectRetryCount = 0;
                                    AfterConnectCompleted();
                                }
                                _Log.LogInfo("_MT5平台自动断开连接,由系统自动拉起完成");
                            });
                        }
                    }
                }
            }
        }

        public override void UpdateConnectStatus(bool Status)
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
                    DBHelper.updateLoginInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo, _Account.UserCode, "Y");
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
                //isTradeTime = true;
                if (isTradeTime && (_Platform.PlatformNo == 2 || _Platform.PlatformNo == 4))
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
                                nudBuyLots.Value = _TradePara.BuyLots;
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
                            string nEATypeAddress = "tcp://" + eaInfo["ea_ip"] + ":5553"; ;

                            if (!string.Equals(nEATypeAddress, _Config.PublishAddressEA))
                            {
                                DSDisconnect();
                                _Config.PublishAddressEA = nEATypeAddress;
                                DSConnection();                            
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
                        }
                        //连接EA数据源
                        if (!_IsEAConnectFlag)
                        {
                            DSConnection();
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

                //检查EA连接状态
                if (label_Price_Connect.Text.IndexOf(":") > 0)
                {
                    DateTime dt1 = Convert.ToDateTime(label_Price_Connect.Text);
                    dt1 = dt1.AddMinutes(3);
                    DateTime dt2 = DateTime.Now;
                    if (DateTime.Compare(dt1, dt2) < 0)
                    {
                        DSDisconnect();
                        _Log.LogInfo("策略自动断开连接");
                    }
                }
            }
        }
        /// <summary>
        /// 数据源连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void buttonDSConnectClient_Click(object sender, EventArgs e)
        {
            AsynExec(this.buttonDSConnect, () => {
                if (buttonDSConnect.Text.Equals("数据源连接"))
                {
                    DSConnection();
                    _IsEAConnectFlag = true;
                }
                else if (buttonDSConnect.Text.Equals("数据源断开"))
                {
                    DSDisconnect();
                    _IsEAConnectFlag = false;
                }
            });
        }


        /// <summary>
        /// 连接数据源
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public  override bool DSConnection()
        {
            string dsAddress = _Config.PublishAddressT4;
            //连接之前先断连,避免出现端口被占用情况
            if (string.Equals("EA", _Config.DSType))
            {
                dsAddress = _Config.PublishAddressEA;
                DSDisconnect();
            }
            if (string.Equals("T4", _Config.DSType))
            {
                dsAddress = _Config.PublishAddressT4;
                DSDisconnect();
            }
            if (string.Equals("OEC", _Config.DSType))
            {
                dsAddress = _Config.PublishAddressOEC;
                DSDisconnect();
            }
            if (dsAddress.Contains("127.0.0.1"))
            {
                dsAddress = dsAddress.Replace("127.0.0.1", uClient.Comm.Utils.GetLocalIP());
            }
            _Log.LogInfo(string.Format("连接到{0}服务器：{1}", _Config.DSType, dsAddress));
            try
            {
                if (dsAddress == null || dsAddress == "")
                {
                    MessageBox.Show(string.Format("{0}服务器地址未配置", _Config.DSType));
                    return false;
                }
                _Subscriber.Options.TcpKeepalive = true;
                _Subscriber.Options.TcpKeepaliveIdle = new TimeSpan(5, 0, 0);
                _Subscriber.Options.TcpKeepaliveInterval = new TimeSpan(0, 0, 1);
                _Subscriber.Connect(dsAddress);
                _Subscriber.Subscribe("");
                buttonDSConnect.Text = "数据源断开";
                buttonDSConnect.BackColor = Color.Green;
                label_DSName.Text = _Config.DSType;
                label_Price_Connect.Text = "已连接";
                label_Price_Connect.BackColor = Color.Green;
                _IsEAConnectFlag = true;
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
            return true;
        }
        /// <summary>
        /// EA连接标识
        /// </summary>
        public bool _IsEAConnectFlag = false;
        /// <summary>
        /// 断开数据源连接
        /// </summary>
        public override void DSDisconnect()
        {
            string dsAddress = _Config.PublishAddressT4;
            if (string.Equals("T4", _Config.DSType))
            {
                dsAddress = _Config.PublishAddressT4;
            }
            if (string.Equals("OEC", _Config.DSType))
            {
                dsAddress = _Config.PublishAddressOEC;
            }
            if (string.Equals("EA", _Config.DSType))
            {
                dsAddress = _Config.PublishAddressEA;
            }
            if (dsAddress.Contains("127.0.0.1"))
            {
                dsAddress = dsAddress.Replace("127.0.0.1", uClient.Comm.Utils.GetLocalIP());
            }
            _Log.LogInfo(string.Format("断开{0}服务器：{1}", _Config.DSType, dsAddress));
            buttonDSConnect.Text = "数据源连接";
            buttonDSConnect.BackColor = Color.Red;
            label_Price_Connect.Text = "未连接";
            label_Price_Connect.BackColor = Color.Red;
            _IsEAConnectFlag = false;
            try
            {
                _Subscriber.Unbind(dsAddress);
                _Log.LogInfo("取消连接");
            }
            catch (Exception e)
            {
                _Log.LogInfo("取消连接," + e.Message);
            }
            bgwMarketQuote.CancelAsync();
            ClearDSDiff();
            StopAutoTrade();
        }
        /// <summary>
        /// 策略数据分析是否运行
        /// </summary>
        public bool _EAAnalysisRunning = false;
        /// <summary>
        /// 数据源行情获取并计算是否自动交易
        /// </summary>
        public override void MarketQuote()
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
                    if (string.Equals(_Config.DSType, "T4") || string.Equals(_Config.DSType, "OEC"))
                    {
                        if (results.StartsWith("Gold"))
                        {
                            EA1(results);
                        }
                    }
                    if (string.Equals(_Config.DSType, "EA"))
                    {
                        if (results.StartsWith("XAUUSD"))
                        {
                            if (_TradePara.AutoTrade)
                            {
                                EA2(results);
                            }
                        }
                        else if (results.StartsWith("GS"))
                        {
                            if (_TradePara.AutoTrade)
                            {
                                EA3Async(results);
                            }
                        }
                        else
                        {
                            //心跳更新
                            string[] split = results.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (string.IsNullOrEmpty(split[1]))
                            {
                                label_Price_Connect.Text = "未连接";
                                label_Price_Connect.BackColor = Color.Red;
                            }
                            else
                            {
                                label_Price_Connect.Text = split[1];
                                label_Price_Connect.BackColor = Color.Green;
                            }                                                      
                        }
                    }                    
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
            _DSQuote.NewQuoute(split[0], double.Parse(split[1]), double.Parse(split[2]));
            //买多单，使用报价是Quote.Ask
            //卖空单，使用报价是Quote.Bid
            //买多平仓单，使用报价是Quote.Bid
            //卖空平仓单，使用报价是Quote.A
            //自动交易已启动并且上次执行已完成
            if (_DSQuote.ValueChanged && _EnableAutoTrade && !isExecuting && (chkBuyOpen.Checked || chkSellOpen.Checked || chkBuyClose.Checked || chkSellClose.Checked))
            {
                int tradeType = -1;
                isExecuting = true;
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
                //跳次分析完成，执行订单逻辑
                //上次订单已执行完成（订单返回更新状态后才算订单执行完成）
                //if (isOrderCompleted && tradeType != -1 && (DateTime.Now- priceCount).TotalSeconds >= 5 && TradeUtils.isTradeTime())
                if (isOrderCompleted && tradeType != -1)
                {
                    //执行自动交易订单时，暂时取消自动交易功能，待订单完成后再回复自动交易                       
                    isOrderCompleted = false;
                    _IsAutoOperationFlag = true;
                    //自动交易已启动，并且在锁仓情况下执行
                    if (checkBox_AutoLock.Checked)
                    {
                        _Log.LogInfo("执行自动订单开始");
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
                            _OrderCreateType = "H";
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
                        _Log.LogInfo("执行独立锁单");
                        _OrderCreateType = "C";
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
                    priceCount = DateTime.Now;
                }
                CancelCheckForSelected();
                isExecuting = false;
            }
            if (_DSQuote.ValueChanged)
            {
                UpdateDSQuote();
                OutputDiffList();
            }
            //Thread.Sleep(3);
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
                            case (int)Comm.Enum.Platform.V4:
                                _V4._OrderCreateType = _OrderCreateType;
                                _V4._EA2_TradeRecord = _EA2_TradeRecord;
                                executedFlag = _V4.AutoTradeTransaction2(tradeType, _IsAutoOperationFlag, out _SendTime);
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                _MT5._OrderCreateType = _OrderCreateType;
                                _MT5._EA2_TradeRecord = _EA2_TradeRecord;
                                _MT5._CurrentPrice = _StrategyConfig.CurrentPrice;
                                executedFlag = _MT5.AutoTradeTransaction3(tradeType, _IsAutoOperationFlag, out _SendTime);
                                _StrategyConfig.CurrentPrice = _MT5._CurrentPrice;
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
        /// 黄金白银变化率分析策略
        /// </summary>
        /// <param name="results"></param>
        public async void EA3Async(string results)
        {
            _Log.LogInfo(results);
            string[] split = results.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //参数说明：
            // XAUUSD:2664.74:SELL_GS:1:50:50 00:51:20 SELL:2664.74:-99.00:-99.00
            // XAUUSD:2570.93-30.638:CLOSE_GS:1:50:50 10:21:45 CLOSE_GS:2565.96-30.58:2570.93-30.638:497
            string opt = split[0];

            if (split.Length >= 3)
            {
                _EA2_TradeRecord = split[2];
            }
            if (!string.IsNullOrEmpty(opt))
            {
                int tradeType = -1;
                string optt = opt.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[2];
                //手数比例
                string lotRatio = opt.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[3];
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
                        tradeType = 3;
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
                        switch (_Platform.PlatformNo)
                        {
                            case (int)Comm.Enum.Platform.MT4:
                                _MT4._OrderCreateType = _OrderCreateType;
                                _MT4._EA2_TradeRecord = _EA2_TradeRecord;
                                _MT4._CurrentPrice = _StrategyConfig.CurrentPrice;
                                executedFlag = _MT4.AutoTradeTransaction4(_MT4._GOLD, tradeType, _IsAutoOperationFlag, Convert.ToDouble(lotRatio), out _SendTime);
                                await DelayExecution(); // 调用延迟执行的函数
                                executedFlag = _MT4.AutoTradeTransaction4(_MT4._SILVER, tradeType, _IsAutoOperationFlag, Convert.ToDouble(lotRatio), out _SendTime);
                                _StrategyConfig.CurrentPrice = _MT4._CurrentPrice;
                                break;
                            case (int)Comm.Enum.Platform.MT5:
                                _MT5._OrderCreateType = _OrderCreateType;
                                _MT5._EA2_TradeRecord = _EA2_TradeRecord;
                                _MT5._CurrentPrice = _StrategyConfig.CurrentPrice;
                                executedFlag = _MT5.AutoTradeTransaction4(_MT5._GOLD, tradeType, _IsAutoOperationFlag, Convert.ToDouble(lotRatio), out _SendTime);
                                await DelayExecution(); // 调用延迟执行的函数
                                executedFlag = _MT5.AutoTradeTransaction4(_MT5._SILVER, tradeType, _IsAutoOperationFlag, Convert.ToDouble(lotRatio), out _SendTime);
                                _StrategyConfig.CurrentPrice = _MT5._CurrentPrice;
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
            int randomNumber = random.Next(3, 10);
            _Log.LogInfo("等待[" + randomNumber + "]秒");
            await Task.Delay(TimeSpan.FromSeconds(randomNumber)); // 设置延迟
            _Log.LogInfo("延迟执行完成");
        }
    }
}
