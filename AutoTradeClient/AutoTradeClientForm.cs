using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using uClient.Comm;

namespace uClient.Broker
{
    public partial class AutoTradeClientForm : uClient.TradeForm
    {
        // EA 心跳状态统一由 AutoTradeClientForm 管理。
        // 时间均使用本机接收时间，不依赖 EA 服务器时钟同步。
        private string _lastHeartbeatText = string.Empty;
        private DateTime _lastHeartbeatReceivedAt = DateTime.MinValue;
        private DateTime _marketQuoteStartedAt = DateTime.MinValue;
        private bool _firstHeartbeatReceived = false;
        private bool _heartbeatTimeoutHandled = false;

        // BackgroundWorker 与 NetMQ SUB socket 生命周期同步。
        // SubscriberSocket 只由 MarketQuote worker 创建、连接、接收和释放。
        private readonly object _marketQuoteLifecycleLock = new object();
        private readonly ManualResetEventSlim _marketQuoteStopped = new ManualResetEventSlim(true);
        private volatile bool _marketQuoteStopRequested = false;
        private string _marketQuoteEndpoint = string.Empty;

        private static readonly TimeSpan FirstHeartbeatTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan RunningHeartbeatTimeout = TimeSpan.FromMinutes(3);
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
            _Log._TradeWinName = _MainFormPara.FormName;
            this.timerCheckPlatformConnectStatus.Enabled = true;
            this.timerCheckPlatformConnectStatus.Interval = 5 * 60000;
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
                    _Log.LogInfo("GOLD:" + _MT4._GOLD + ",SILVER:" + _MT4._SILVER);
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
            _Log.LogInfo("加载EA账户信息");
            //_Config.PlatformCode = _MainFormPara.PlatformCode;
            IDictionary<string, string> eaInfo = DBHelper.getAutoInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo);
            if (eaInfo != null)
            {
                string ea_type = eaInfo["ea_type"];
                //string eaAddress = _Config.SysConfig[ea_type];
                string eaAddress = eaInfo["ea_ip"];
                _Config.PublishAddressEA = "tcp://" + eaAddress;
                _Config.DSType = "EA";
                _Config.clientType = "EA";
                _Log.LogInfo(string.Format("加载EA初始化参数:PublishAddressEA={0},DSType={1},clientType={2},eaType={3},eaAddress={4}", _Config.PublishAddressEA, _Config.DSType, _Config.clientType, ea_type, eaAddress));
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
            _Log.LogInfo("查询EA账户参数:Execute IP=" + Utils.GetLocalIP() + ",FormNo=" + _MainFormPara.FormNo);
            //替换成数据库中配置的交易账户信息
            Comm.Broker queryBroker = DBHelper.getBrokerInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo);
            if (queryBroker != null)
            {
                if (_Config.Platform == null || _Config.Platform.Count == 0)
                {
                    _Config.Platform = new List<uClient.Comm.Platform>();
                }
                if (_Platform == null)
                {
                    _Platform = new uClient.Comm.Platform();
                    _Config.Platform.Add(_Platform);
                }
                _Broker = queryBroker;

                List<uClient.Comm.Broker> brokerList = new List<uClient.Comm.Broker>();
                brokerList.Add(queryBroker);
                _Platform.Broker = brokerList;

                _Platform.PlatformNo = queryBroker.PlatformNo;
                _Platform.PlatformCode = queryBroker.PlatformCode;

                if (!_Platform.BrokerList.ContainsKey(queryBroker.BrokerCode))
                {
                    _Platform.BrokerList.Add(queryBroker.BrokerCode, queryBroker);
                }
                _Log.LogInfo("EA账户信息:" + _Broker.toString());
                _Account = _Broker.DefaultAccountType.Equals("Demo") ? _Broker.DemoAccount : _Broker.LiveAccount;
                _TradeSymbol = _Broker.DefaultSymbol;
                if (_Broker.TradePara == null)
                {
                    _Broker.TradePara = _Platform.TradePara;
                }
                _TradePara = _Account.TradePara == null ? _Broker.TradePara : _Account.TradePara;
                _Broker.TradePara = _TradePara;
            }
            //获取数据库配置数据
            _Config.SysConfig = DBHelper.getSysConfig();
            _logger.Info("开始加载配置参数完成");
            //加载Account信息后初始化到Log对象
            _Log._Account = _Account;
            LoadEAType();
        }

        /// <summary>
        /// 连接拉起重试次数
        /// </summary>
        private int _ConnectRetryCount = 0;
        /// <summary>
        /// 可以重试最大次数
        /// </summary>
        private int _ConnectRetryAmount = 6;
        /// <summary>
        /// 判断价格是否卡死
        /// </summary>
        private double _ConnectPrice = 0;
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
                bool isTradeTime = uClient.Comm.Utils.checkIsTradeTime(_Config.SysConfig["TradeNotradeDuration"], DBUtils.getDateTime());
                //_Log.LogInfo("平台拉起判断交易时间参数:[TradeNotradeDuration=" + _Config.SysConfig["TradeNotradeDuration"] + "][TradeTime=" + _Config.SysConfig["TradeTime"] + "][isTradeTime="+ isTradeTime + "]");
                //在EA策略执行时间内,MT4平台才启动自动拉起
                //isTradeTime = true;
                if (_Platform.PlatformNo == 2 || _Platform.PlatformNo == 4)
                {
                    if (isTradeTime)
                    {
                        if ((_MT4 != null) || (_MT5 != null))
                        {
                            if (string.Equals(lblMT4Bid.Text, "0") || _ConnectPrice == Convert.ToDouble(lblMT4Bid.Text))
                            {
                                _Log.LogInfo("价格未变化[价格:" + _ConnectPrice + "],自动拉起开始");
                                if (_ConnectRetryCount < _ConnectRetryAmount)
                                {
                                    _Log.LogInfo("自动拉起开始");
                                    _Log.LogInfo("第[" + _ConnectRetryCount + "]拉起平台");
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

                                    Task.Run(() =>
                                    {
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
                                        _Log.LogInfo("自动拉起完成");
                                    });
                                }
                                else
                                {
                                    _Log.LogInfo("自动拉起超过[" + _ConnectRetryCount + "]次,需要手工拉起");
                                }
                            }
                            else
                            {
                                _Log.LogInfo("平台连接正常,价格已变化");
                                _ConnectPrice = Convert.ToDouble(lblMT4Bid.Text);
                                _ConnectRetryCount = 0;
                            }
                        }
                        else
                        {
                            _Log.LogInfo("平台未初始化完成,不能执行自动拉起");
                        }
                    }
                    else
                    {
                        _Log.LogInfo("非交易时间,不启动平台自动拉起");
                    }
                }
                else
                {
                    _Log.LogInfo("非MT4/MT5平台,不启动平台自动拉起");
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
        /// 刷新托管数据,包括策略,手数,自动交易
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerRefreshAutoTrade_Tick(object sender, EventArgs e)
        {
            //加载完成后
            if (_IsLoadConfigCompleted)
            {
                bool isTradeTime = uClient.Comm.Utils.checkIsTradeTime(_Config.SysConfig["TradeNotradeDuration"], DBUtils.getDateTime());
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
                            string ea_type = eaInfo["ea_type"];
                            string nEATypeAddress = "tcp://" + _Config.SysConfig[ea_type];
                            if (!string.Equals(nEATypeAddress, _Config.PublishAddressEA))
                            {
                                DSDisconnect();
                                //修改策略地址,重新连接
                                _Log.LogInfo("修改EA地址");
                                _Log.LogInfo("原地址:" + _Config.PublishAddressEA);
                                _Log.LogInfo("新地址:" + nEATypeAddress);
                                _Config.PublishAddressEA = nEATypeAddress;
                                _Log.LogInfo("更新EA地址后重新连接");
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
                        // _IsDSConnectFlag 在 EA 首条心跳前保持 false；
                        // 只有 worker 已完全停止时才允许自动重连，避免连接中每个定时周期重复重启。
                        if (_marketQuoteStopped.IsSet && !bgwMarketQuote.IsBusy)
                        {
                            DSConnection();
                        }
                        //启动自动交易
                        if (!_EnableAutoTrade)
                        {
                            //启动自动交易
                            if (isConnect() && IsDataSourceReady())
                            {
                                StartAutoTrade();
                                //启动自动交易成功
                                DBHelper.updateAutoInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo, _Account.UserCode, "Y");
                            }
                        }
                    }
                }

                // 首次心跳等待超时（30秒）或运行中心跳丢失（3分钟）。
                // 不比较心跳文本是否变化；只要收到有效心跳，每次都刷新接收时间。
                CheckHeartbeatTimeout();
            }
        }
        /// <summary>
        /// 数据源连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void buttonDSConnectClient_Click(object sender, EventArgs e)
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


        /// <summary>
        /// 连接数据源
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override bool DSConnection()
        {
            string dsAddress = GetDataSourceAddress();
            if (string.IsNullOrWhiteSpace(dsAddress))
            {
                _Log.LogInfo(string.Format("{0}服务器地址未配置", _Config.DSType));
                return false;
            }

            // 本次连接前先请求旧接收循环退出，并等待它释放 socket。
            DSDisconnect();

            if (!WaitForMarketQuoteStopped(TimeSpan.FromSeconds(8)))
            {
                _Log.LogInfo("[DS][CONNECT_ABORT] 等待旧 MarketQuote/BackgroundWorker 完全退出超时，取消本次重连");
                return false;
            }

            lock (_marketQuoteLifecycleLock)
            {
                if (bgwMarketQuote.IsBusy)
                {
                    _Log.LogInfo("[DS][CONNECT_ABORT] BackgroundWorker 仍处于 Busy，拒绝重复启动");
                    return false;
                }

                try
                {
                    _marketQuoteEndpoint = dsAddress;
                    _marketQuoteStopRequested = false;
                    _IsDSConnectFlag = false;

                    lock (_marketQuoteLifecycleLock)
                    {
                        _firstHeartbeatReceived = false;
                        _heartbeatTimeoutHandled = false;
                        _lastHeartbeatText = string.Empty;
                        _lastHeartbeatReceivedAt = DateTime.MinValue;
                        _marketQuoteStartedAt = DateTime.Now;
                    }

                    _Log.LogInfo(string.Format("连接到{0}服务器：{1}", _Config.DSType, dsAddress));

                    // NetMQ socket 在本类中由接收线程独占。
                    // 这里仅保存 endpoint 并启动 worker，Connect 在 MarketQuote 内执行。
                    _marketQuoteStopped.Reset();
                    try
                    {
                        bgwMarketQuote.RunWorkerAsync();
                    }
                    catch
                    {
                        _marketQuoteStopped.Set();
                        throw;
                    }

                    buttonDSConnect.Text = "数据源断开";
                    buttonDSConnect.BackColor = Color.Green;
                    label_DSName.Text = _Config.DSType;
                    label_Price_Connect.Text = "连接中";
                    label_Price_Connect.BackColor = Color.Orange;
                    _IsDSConnectFlag = false; // worker 启动不代表数据源就绪；EA 必须等待首条有效心跳。

                    _Log.LogInfo("[DS][CONNECT_START] MarketQuote worker 已启动，等待数据源就绪");
                    return true;
                }
                catch (Exception ex)
                {
                    _IsDSConnectFlag = false;
                    _marketQuoteStopped.Set();
                    _logger.Error(ex, "DSConnection 启动失败");
                    _Log.LogInfo(string.Format("连接到{0}服务器时错误：{1}", _Config.DSType, ex.Message));
                    return false;
                }
            }
        }

        /// <summary>
        /// 等待接收循环释放 socket，并等待 BackgroundWorker 完成状态切换。
        /// MarketQuote 的 finally 会先释放 socket，再设置 _marketQuoteStopped；
        /// BackgroundWorker 的 IsBusy 则在 DoWork/RunWorkerCompleted 生命周期结束后才会变为 false。
        /// </summary>
        private bool WaitForMarketQuoteStopped(TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < deadline)
            {
                if (_marketQuoteStopped.IsSet && !bgwMarketQuote.IsBusy)
                    return true;

                Thread.Sleep(50);
            }

            return _marketQuoteStopped.IsSet && !bgwMarketQuote.IsBusy;
        }

        /// <summary>
        /// 获取当前数据源 endpoint。
        /// </summary>
        private string GetDataSourceAddress()
        {
            string dsAddress = string.Empty;

            if (string.Equals(_Config.DSType, "EA", StringComparison.OrdinalIgnoreCase))
                dsAddress = _Config.PublishAddressEA;
            else if (string.Equals(_Config.DSType, "T4", StringComparison.OrdinalIgnoreCase))
                dsAddress = _Config.PublishAddressT4;
            else if (string.Equals(_Config.DSType, "OEC", StringComparison.OrdinalIgnoreCase))
                dsAddress = _Config.PublishAddressOEC;

            if (!string.IsNullOrWhiteSpace(dsAddress) && dsAddress.Contains("127.0.0.1"))
                dsAddress = dsAddress.Replace("127.0.0.1", uClient.Comm.Utils.GetLocalIP());

            return dsAddress;
        }

        /// <summary>
        /// 请求数据源接收循环停止。
        /// 不在 UI/timer 线程操作 NetMQ socket，避免跨线程访问 socket。
        /// </summary>
        public override void DSDisconnect()
        {
            string dsAddress = GetDataSourceAddress();
            _Log.LogInfo(string.Format("断开{0}服务器：{1}", _Config.DSType, dsAddress));

            _IsDSConnectFlag = false;
            _marketQuoteStopRequested = true;
            lock (_marketQuoteLifecycleLock)
            {
                _firstHeartbeatReceived = false;
                _lastHeartbeatReceivedAt = DateTime.MinValue;
                _marketQuoteStartedAt = DateTime.MinValue;
                _heartbeatTimeoutHandled = false;
            }

            if (bgwMarketQuote.IsBusy && !bgwMarketQuote.CancellationPending)
            {
                try
                {
                    bgwMarketQuote.CancelAsync();
                }
                catch (Exception ex)
                {
                    _Log.LogInfo("[DS][CANCEL_ERROR] " + ex.Message);
                }
            }

            // TryReceiveFrameString 使用 1 秒超时，正常情况下 worker 会很快退出。
            // 不在这里 Disconnect socket；由 MarketQuote 的 finally 在 socket 所属线程释放。
            if (!WaitForMarketQuoteStopped(TimeSpan.FromSeconds(8)))
            {
                _Log.LogInfo("[DS][DISCONNECT_WAIT_TIMEOUT] MarketQuote/BackgroundWorker 尚未完全退出；后续重连必须等待旧 worker 完成");
            }

            buttonDSConnect.Text = "数据源连接";
            buttonDSConnect.BackColor = Color.Red;
            label_Price_Connect.Text = "未连接";
            label_Price_Connect.BackColor = Color.Red;

            ClearDSDiff();
            StopAutoTrade();
        }

        /// <summary>
        /// EA 数据源就绪条件：必须已经收到首条有效心跳。
        /// 非 EA 数据源沿用 socket 连接成功即就绪的规则。
        /// </summary>
        protected override bool IsDataSourceReady()
        {
            if (!string.Equals(_Config.DSType, "EA", StringComparison.OrdinalIgnoreCase))
                return _IsDSConnectFlag;

            lock (_marketQuoteLifecycleLock)
            {
                return _IsDSConnectFlag && _firstHeartbeatReceived;
            }
        }

        /// <summary>
        /// 处理 EA 心跳。当前协议沿用原代码：普通心跳文本的第二个空格分隔字段为心跳内容。
        /// XAUUSD 行情和 GS 策略消息不作为心跳。
        /// </summary>
        private bool ProcessEAHeartbeat(string message)
        {
            if (_marketQuoteStopRequested ||
                string.IsNullOrWhiteSpace(message) ||
                message.Contains("XAUUSD:") ||
                message.StartsWith("GS:", StringComparison.Ordinal))
                return false;

            string[] parts = message.Split(
                new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                SetPriceConnectStatus("等待有效心跳", Color.Orange);
                return false;
            }

            DateTime now = DateTime.Now;
            bool firstHeartbeat = false;
            string heartbeatText = parts[1];

            lock (_marketQuoteLifecycleLock)
            {
                // 断开请求可能与当前接收帧并发发生，禁止断开后重新置为就绪。
                if (_marketQuoteStopRequested)
                    return false;

                _lastHeartbeatReceivedAt = now;

                if (!_firstHeartbeatReceived)
                {
                    _firstHeartbeatReceived = true;
                    firstHeartbeat = true;
                }

                if (!string.Equals(_lastHeartbeatText, heartbeatText, StringComparison.Ordinal))
                    _lastHeartbeatText = heartbeatText;

                _heartbeatTimeoutHandled = false;
            }

            if (firstHeartbeat)
            {
                _IsDSConnectFlag = true;
                _Log.LogInfo("[DS][HEARTBEAT_READY] 已收到 EA 首条有效心跳，数据源正式就绪");
            }

            SetPriceConnectStatus(heartbeatText, Color.Green);
            return true;
        }

        /// <summary>
        /// 由 UI 定时器调用，检查首条心跳等待超时及运行中心跳超时。
        /// </summary>
        private void CheckHeartbeatTimeout()
        {
            if (!string.Equals(_Config.DSType, "EA", StringComparison.OrdinalIgnoreCase))
                return;

            bool shouldDisconnect = false;
            string reason = string.Empty;
            DateTime now = DateTime.Now;

            lock (_marketQuoteLifecycleLock)
            {
                if (_marketQuoteStartedAt == DateTime.MinValue || _heartbeatTimeoutHandled)
                    return;

                if (!_firstHeartbeatReceived &&
                    now - _marketQuoteStartedAt > FirstHeartbeatTimeout)
                {
                    shouldDisconnect = true;
                    reason = "启动后30秒未收到首条有效EA心跳";
                }
                else if (_firstHeartbeatReceived &&
                         _lastHeartbeatReceivedAt != DateTime.MinValue &&
                         now - _lastHeartbeatReceivedAt > RunningHeartbeatTimeout)
                {
                    shouldDisconnect = true;
                    reason = "连续3分钟未收到有效EA心跳";
                }

                if (shouldDisconnect)
                    _heartbeatTimeoutHandled = true;
            }

            if (shouldDisconnect)
            {
                _Log.LogInfo("[DS][HEARTBEAT_TIMEOUT] " + reason + "，停止自动交易并断开数据源");
                DSDisconnect();
            }
        }

        /// <summary>
        /// 策略数据分析是否运行
        /// </summary>
        public bool _EAAnalysisRunning = false;
        /// <summary>
        /// 从行情接收线程安全更新数据源连接状态控件。
        /// </summary>
        private void SetPriceConnectStatus(string text, Color backColor)
        {
            if (label_Price_Connect == null || label_Price_Connect.IsDisposed)
                return;

            if (label_Price_Connect.InvokeRequired)
            {
                try
                {
                    label_Price_Connect.BeginInvoke(new Action<string, Color>(SetPriceConnectStatus), text, backColor);
                }
                catch (InvalidOperationException)
                {
                    // 窗体正在关闭或控件已释放，忽略 UI 更新。
                }
                return;
            }

            label_Price_Connect.Text = text;
            label_Price_Connect.BackColor = backColor;
        }

        /// <summary>
        /// 数据源行情获取并计算是否自动交易
        /// </summary>
        public override void MarketQuote()
        {
            _EAAnalysisRunning = true;
            string PreReceiveMsg = string.Empty;
            int timeoutHeartbeatCount = 0;
            string endpoint = _marketQuoteEndpoint;
            SubscriberSocket subscriber = null;

            _Log.LogInfo("[DS][WORKER_START] MarketQuote 接收循环启动，endpoint=" + endpoint);

            try
            {
                if (string.IsNullOrWhiteSpace(endpoint))
                    throw new InvalidOperationException("MarketQuote endpoint 为空");

                // 每次启动接收线程时，在当前 worker 线程创建独立 SUB socket。
                // 不再依赖父类中可能尚未初始化的 _Subscriber。
                // NetMQ socket 必须由创建它的线程负责 Connect/Receive/Dispose。
                subscriber = new SubscriberSocket();
                subscriber.Options.TcpKeepalive = true;
                subscriber.Options.TcpKeepaliveIdle = TimeSpan.FromHours(5);
                subscriber.Options.TcpKeepaliveInterval = TimeSpan.FromSeconds(1);
                subscriber.Connect(endpoint);
                subscriber.Subscribe(string.Empty);

                _Log.LogInfo("[DS][CONNECTED] NetMQ SUB 已连接：" + endpoint);

                if (!string.Equals(_Config.DSType, "EA", StringComparison.OrdinalIgnoreCase))
                {
                    _IsDSConnectFlag = true;
                    SetPriceConnectStatus("已连接", Color.Green);
                }
                else
                {
                    _IsDSConnectFlag = false;
                    SetPriceConnectStatus("等待EA心跳", Color.Orange);
                }

                while (true)
                {
                    // CancelAsync 只设置 CancellationPending，不会强制终止线程。
                    if (_marketQuoteStopRequested || bgwMarketQuote.CancellationPending)
                    {
                        _Log.LogInfo("[DS][CANCELLED] 收到停止请求，准备退出接收循环");
                        break;
                    }

                    string results;
                    bool received = subscriber.TryReceiveFrameString(
                        TimeSpan.FromSeconds(1), out results);

                    // 收到消息后也再次检查取消标志，避免断开期间继续执行交易逻辑。
                    if (_marketQuoteStopRequested || bgwMarketQuote.CancellationPending)
                    {
                        _Log.LogInfo("[DS][CANCELLED] 收到停止请求，丢弃当前消息并退出");
                        break;
                    }

                    if (!received)
                    {
                        timeoutHeartbeatCount++;
                        if (timeoutHeartbeatCount % 60 == 0)
                        {
                            _Log.LogInfo(string.Format(
                                "[DS][WORKER_ALIVE] 连续 {0} 次超时未收到消息（约 {1} 分钟），DSType={2}",
                                timeoutHeartbeatCount,
                                timeoutHeartbeatCount / 60,
                                _Config.DSType));
                        }
                        continue;
                    }

                    timeoutHeartbeatCount = 0;

                    bool isEA = string.Equals(
                        _Config.DSType, "EA", StringComparison.OrdinalIgnoreCase);

                    // 必须先处理心跳，再做重复报文过滤；相同心跳内容也代表仍然存活。
                    if (isEA)
                        ProcessEAHeartbeat(results);

                    if (string.IsNullOrEmpty(results) ||
                        string.Equals(results, PreReceiveMsg, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    PreReceiveMsg = results;

                    if (string.Equals(_Config.DSType, "T4", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(_Config.DSType, "OEC", StringComparison.OrdinalIgnoreCase))
                    {
                        if (results.StartsWith("Gold", StringComparison.Ordinal))
                            EA1(results);
                    }
                    else if (string.Equals(_Config.DSType, "EA", StringComparison.OrdinalIgnoreCase))
                    {
                        if (results.Contains("XAUUSD:"))
                        {
                            if (_TradePara.AutoTrade)
                                EA2(results);
                        }
                        else if (results.StartsWith("GS:", StringComparison.Ordinal))
                        {
                            if (_TradePara.AutoTrade)
                                EA3Async(results);
                        }
                        else
                        {
                            // EA 心跳已在重复报文过滤之前由 ProcessEAHeartbeat 处理。
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "MarketQuote 接收循环异常终止");
                _Log.LogInfo("[DS][WORKER_EXCEPTION] " + ex);
            }
            finally
            {
                // Disconnect 和 Dispose 均在 socket 创建线程执行。
                // 即使 Connect 失败，也必须释放已经创建的 socket。
                try
                {
                    if (subscriber != null)
                    {
                        if (!string.IsNullOrWhiteSpace(endpoint))
                            subscriber.Disconnect(endpoint);

                        subscriber.Dispose();
                        _Log.LogInfo("[DS][SOCKET_DISPOSED] " + endpoint);
                    }
                }
                catch (Exception ex)
                {
                    _Log.LogInfo("[DS][SOCKET_DISPOSE_ERROR] " + ex.Message);
                }

                _EAAnalysisRunning = false;
                _IsDSConnectFlag = false;
                lock (_marketQuoteLifecycleLock)
                {
                    _firstHeartbeatReceived = false;
                    _lastHeartbeatReceivedAt = DateTime.MinValue;
                    _marketQuoteStartedAt = DateTime.MinValue;
                }
                _marketQuoteStopped.Set();
                _Log.LogInfo("[DS][WORKER_STOPPED] MarketQuote 接收循环已结束");
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
                    _PriceCount = DateTime.Now;
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
        /// EA2收到的完整策略交易记录。保留原始内容，供平台执行层记录审计信息。
        /// </summary>
        private string _EA2_TradeRecord = string.Empty;

        /// <summary>
        /// MQL5 ToLegacyCommand() 输出的 EA2 指令结构。
        ///
        /// 当前格式：
        /// DETAIL SYMBOL:PRICE:OPERATION:LOT:TP:SL COMMAND_TIME TRACE...
        ///
        /// 示例开仓：
        /// SELL XAUUSD:4077.30:SELL:1:15:4092.3 18:45:00 18:45:00 SELL:4077.30:-99.00:-99.00
        ///
        /// 示例平仓：
        /// CLOSE_SELL_TRAILING_STOP XAUUSD:4087.23:CLOSE_SELL:1:50:50 17:41:50 17:41:50 CLOSE_SELL:4094.87:4087.23:7.64:12.67:-0.62
        ///
        /// 第一段 DETAIL 是策略详情；第二段中的 OPERATION 才是实际交易动作。
        /// </summary>
        private sealed class EA2ParsedCommand
        {
            public string Detail;
            public string Symbol;
            public double Price;
            public string Operation;
            public double Lots;
            public double TakeProfit;
            public double StopLoss;
            public string CommandTime;
            public string TradeTrace;
            public int TradeType;
        }

        private static bool TryParseEA2Number(string value, out double number)
        {
            return double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out number);
        }

        /// <summary>
        /// 解析 MQL5 EA2 指令。
        /// 同时兼容旧格式：SYMBOL:PRICE:OPERATION:LOT:TP:SL TIME TRACE
        /// </summary>
        private static bool TryParseEA2Command(
            string results,
            out EA2ParsedCommand command,
            out string error)
        {
            command = null;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(results))
            {
                error = "指令为空";
                return false;
            }

            string[] tokens = results.Trim().Split(
                new[] { ' ', '\t', '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 2)
            {
                error = "指令字段不足，至少需要交易参数和时间";
                return false;
            }

            string detail;
            string transactionToken;
            string commandTime;
            int traceStartIndex;

            // 新 MQL5 格式：DETAIL + SYMBOL:PRICE:OP:LOT:TP:SL + TIME + TRACE
            string[] firstTokenFields = tokens[0].Split(':');
            string[] secondTokenFields = tokens[1].Split(':');

            if (tokens.Length >= 3 && secondTokenFields.Length >= 6)
            {
                detail = tokens[0];
                transactionToken = tokens[1];
                commandTime = tokens[2];
                traceStartIndex = 3;
            }
            // 旧格式兼容：SYMBOL:PRICE:OP:LOT:TP:SL + TIME + TRACE
            else if (firstTokenFields.Length >= 6)
            {
                detail = firstTokenFields[2];
                transactionToken = tokens[0];
                commandTime = tokens.Length >= 2 ? tokens[1] : string.Empty;
                traceStartIndex = 2;
            }
            else
            {
                error = "无法识别指令格式，预期 DETAIL SYMBOL:PRICE:OPERATION:LOT:TP:SL TIME TRACE";
                return false;
            }

            string[] fields = transactionToken.Split(':');
            if (fields.Length < 6)
            {
                error = "交易参数必须包含 SYMBOL:PRICE:OPERATION:LOT:TP:SL";
                return false;
            }

            double price;
            double lots;
            double takeProfit;
            double stopLoss;

            if (!TryParseEA2Number(fields[1], out price) ||
                !TryParseEA2Number(fields[3], out lots) ||
                !TryParseEA2Number(fields[4], out takeProfit) ||
                !TryParseEA2Number(fields[5], out stopLoss))
            {
                error = "价格、手数、TP 或 SL 包含无效数字";
                return false;
            }

            string symbol = fields[0].Trim().ToUpperInvariant();
            string operation = fields[2].Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(symbol))
            {
                error = "交易品种为空";
                return false;
            }

            if (price <= 0 || double.IsNaN(price) || double.IsInfinity(price))
            {
                error = "交易价格无效";
                return false;
            }

            if (lots <= 0 || double.IsNaN(lots) || double.IsInfinity(lots))
            {
                error = "交易手数必须大于 0";
                return false;
            }

            int tradeType;
            switch (operation)
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
                    tradeType = 5;
                    break;
                case "IN_SELL":
                    tradeType = 6;
                    break;
                case "P_CLOSE_BUY":
                    tradeType = 7;
                    break;
                case "P_CLOSE_SELL":
                    tradeType = 8;
                    break;
                default:
                    error = "不支持的交易操作：" + operation;
                    return false;
            }

            command = new EA2ParsedCommand
            {
                Detail = detail,
                Symbol = symbol,
                Price = price,
                Operation = operation,
                Lots = lots,
                TakeProfit = takeProfit,
                StopLoss = stopLoss,
                CommandTime = commandTime,
                TradeTrace = tokens.Length > traceStartIndex
                    ? string.Join(" ", tokens, traceStartIndex, tokens.Length - traceStartIndex)
                    : string.Empty,
                TradeType = tradeType
            };

            return true;
        }

        /// <summary>
        /// 根据 MQL5 生成的交易指令执行开仓、加仓、平仓或部分平仓。
        ///
        /// 注意：
        /// 1. Detail 只用于记录，不用于判断交易方向。
        /// 2. Operation 决定交易动作。
        /// 3. Lots 传给原有 MT4/MT5 AutoTradeTransaction4 执行层。
        /// 4. executedFlag=true 表示平台执行层接受/生成了订单，不等于最终成交；
        ///    最终完成状态仍由平台订单更新回调设置 isOrderCompleted。
        /// </summary>
        public async void EA2(string results)
        {
            EA2ParsedCommand cmd;
            string parseError;

            if (!TryParseEA2Command(results, out cmd, out parseError))
            {
                _Log.LogInfo("[EA2][REJECT] " + parseError +
                             " | raw=" + (results ?? string.Empty));
                return;
            }

            // 保留完整记录，不再只保存 split[2] 的时间字段。
            _EA2_TradeRecord = results;

            if (!_EnableAutoTrade)
            {
                _Log.LogInfo("[EA2][DISABLED] 自动交易未启动，忽略指令：" +
                             cmd.Operation + " " + cmd.Symbol);
                return;
            }

            // 在第一个 await 之前设置锁，防止多个消息并发进入。
            if (isExecuting)
            {
                _Log.LogInfo("[EA2][BUSY] 正在执行上一条指令，拒绝本次并发指令：" +
                             cmd.Operation + " " + cmd.Symbol);
                return;
            }

            if (!isOrderCompleted)
            {
                _Log.LogInfo("[EA2][WAIT] 上一笔订单尚未收到完成确认，拒绝本次指令：" +
                             cmd.Operation + " " + cmd.Symbol);
                return;
            }

            isExecuting = true;
            isOrderCompleted = false;
            executedFlag = false;

            try
            {
                _IsAutoOperationFlag = true;
                _OrderCreateType = "H";

                _Log.LogInfo(string.Format(
                    CultureInfo.InvariantCulture,
                    "[EA2][PARSED] detail={0}; symbol={1}; operation={2}; tradeType={3}; price={4}; lots={5}; tp={6}; sl={7}; time={8}; trace={9}",
                    cmd.Detail,
                    cmd.Symbol,
                    cmd.Operation,
                    cmd.TradeType,
                    cmd.Price,
                    cmd.Lots,
                    cmd.TakeProfit,
                    cmd.StopLoss,
                    cmd.CommandTime,
                    cmd.TradeTrace));

                // 保留原有账号延迟逻辑。
                bool delayRequired = true;
                if (_MyConfig.SpecialNumber != null &&
                    _MyConfig.SpecialNumber.Length > 0)
                {
                    string accountCode = _Account.UserCode;
                    bool isSpecialAccount = Array.Exists(
                        _MyConfig.SpecialNumber,
                        element => element == accountCode);

                    delayRequired = !isSpecialAccount;
                }

                if (delayRequired)
                {
                    _Log.LogInfo("[EA2] 启动锁单，延时处理中");
                    await DelayExecution();
                }

                switch (_Platform.PlatformNo)
                {
                    case (int)Comm.Enum.Platform.MF4:
                        if (_MF4 == null)
                            throw new InvalidOperationException("MF4 平台对象未初始化");

                        _MF4._OrderCreateType = _OrderCreateType;
                        _MF4._EA2_TradeRecord = _EA2_TradeRecord;
                        _MF4._CurrentPrice = _StrategyConfig.CurrentPrice;

                        // MF4 原接口不接收 lots，保持既有接口行为。
                        executedFlag = _MF4.AutoTradeTransaction3(
                            cmd.TradeType,
                            _IsAutoOperationFlag,
                            out _SendTime);

                        _StrategyConfig.CurrentPrice = _MF4._CurrentPrice;
                        break;

                    case (int)Comm.Enum.Platform.MT4:
                        if (_MT4 == null)
                            throw new InvalidOperationException("MT4 平台对象未初始化");

                        _MT4._OrderCreateType = _OrderCreateType;
                        _MT4._EA2_TradeRecord = _EA2_TradeRecord;
                        _MT4._CurrentPrice = _StrategyConfig.CurrentPrice;

                        executedFlag = _MT4.AutoTradeTransaction4(
                            cmd.TradeType,
                            cmd.Lots,
                            _IsAutoOperationFlag,
                            out _SendTime);

                        _StrategyConfig.CurrentPrice = _MT4._CurrentPrice;
                        break;

                    case (int)Comm.Enum.Platform.V4:
                        if (_V4 == null)
                            throw new InvalidOperationException("V4 平台对象未初始化");

                        _V4._OrderCreateType = _OrderCreateType;
                        _V4._EA2_TradeRecord = _EA2_TradeRecord;

                        // V4 原接口不接收 lots，保持既有接口行为。
                        executedFlag = _V4.AutoTradeTransaction2(
                            cmd.TradeType,
                            _IsAutoOperationFlag,
                            out _SendTime);
                        break;

                    case (int)Comm.Enum.Platform.MT5:
                        if (_MT5 == null)
                            throw new InvalidOperationException("MT5 平台对象未初始化");

                        _MT5._OrderCreateType = _OrderCreateType;
                        _MT5._EA2_TradeRecord = _EA2_TradeRecord;
                        _MT5._CurrentPrice = _StrategyConfig.CurrentPrice;

                        executedFlag = _MT5.AutoTradeTransaction4(
                            cmd.TradeType,
                            cmd.Lots,
                            _IsAutoOperationFlag,
                            out _SendTime);

                        _StrategyConfig.CurrentPrice = _MT5._CurrentPrice;
                        break;

                    default:
                        throw new NotSupportedException(
                            "不支持的平台编号：" + _Platform.PlatformNo);
                }

                if (executedFlag)
                {
                    _Log.LogInfo("[EA2][SUBMITTED] 平台执行层已接受指令：" +
                                 cmd.Operation + " " + cmd.Symbol +
                                 " lots=" + cmd.Lots.ToString(CultureInfo.InvariantCulture) +
                                 "；等待平台成交/订单更新回调确认");
                }
                else
                {
                    isOrderCompleted = true;
                    _Log.LogInfo("[EA2][FAILED] 平台未生成订单：" +
                                 cmd.Operation + " " + cmd.Symbol);
                }
            }
            catch (Exception ex)
            {
                // 执行层抛出异常时释放等待状态，允许后续指令重试。
                isOrderCompleted = true;
                _Log.LogInfo("[EA2][EXCEPTION] " + ex);
            }
            finally
            {
                isExecuting = false;
            }
        }
        /// <summary>
        /// 黄金白银变化率分析策略
        /// </summary>
        /// <param name="results"></param>
        public async void EA3Async(string results)
        {
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
            int randomNumber = random.Next(1, 500);
            _Log.LogInfo("等待[" + randomNumber + "]毫秒");
            await Task.Delay(TimeSpan.FromMilliseconds(randomNumber)); // 设置延迟
            _Log.LogInfo("延迟执行完成");
        }
    }
}
