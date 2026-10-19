using NetMQ;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using uClient.Comm;

namespace uClient.Broker
{
    public partial class AutoTradeClientForm : uClient.TradeForm
    {
        // 心跳内容最后一次变化时的本机时间；不依赖策略端与本机时钟同步
        private string _lastHeartbeatText = string.Empty;
        private DateTime _lastHeartbeatChangedAt = DateTime.MinValue;
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
                        if (!_IsDSConnectFlag)
                        {
                            DSConnection();
                        }
                        //启动自动交易
                        if (!_EnableAutoTrade)
                        {
                            //启动自动交易
                            if (isConnect() && _IsDSConnectFlag)
                            {
                                StartAutoTrade();
                                //启动自动交易成功
                                DBHelper.updateAutoInfo(uClient.Comm.Utils.GetLocalIP(), _MainFormPara.FormNo, _Account.UserCode, "Y");
                            }
                        }
                    }
                }

                // 心跳文本连续3分钟没有变化，使用本机计时判断策略连接状态
                if (string.Equals(_Config.DSType, "EA") &&
                    _IsDSConnectFlag &&
                    _lastHeartbeatChangedAt != DateTime.MinValue &&
                    DateTime.Now - _lastHeartbeatChangedAt > TimeSpan.FromMinutes(3))
                {
                    _Log.LogInfo("策略心跳文本超过3分钟未变化，调用DSDisconnect断开连接");
                    DSDisconnect();
                    _Log.LogInfo("策略自动断开连接");
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
            AsynExec(this.buttonDSConnect, () =>
            {
                if (buttonDSConnect.Text.Equals("数据源连接"))
                {
                    _IsDSConnectFlag = DSConnection();
                }
                else if (buttonDSConnect.Text.Equals("数据源断开"))
                {
                    DSDisconnect();
                    _IsDSConnectFlag = false;
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
            string dsAddress = _Config.PublishAddressT4;
            //连接之前先断连,避免出现端口被占用情况
            if (string.Equals("EA", _Config.DSType))
            {
                dsAddress = _Config.PublishAddressEA;
            }
            if (string.Equals("T4", _Config.DSType))
            {
                dsAddress = _Config.PublishAddressT4;
            }
            if (string.Equals("OEC", _Config.DSType))
            {
                dsAddress = _Config.PublishAddressOEC;
            }
            if (dsAddress.Contains("127.0.0.1"))
            {
                dsAddress = dsAddress.Replace("127.0.0.1", uClient.Comm.Utils.GetLocalIP());
            }
            DSDisconnect();
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
                _IsDSConnectFlag = true;
                _lastHeartbeatText = string.Empty;
                _lastHeartbeatChangedAt = DateTime.Now;
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
            _IsDSConnectFlag = false;
            try
            {
                // 🔧 V20260927_DISCONNECT_NOT_UNBIND：日志里实测抛出了
                // NetMQ.EndpointNotFoundException: "Endpoint was not found and cannot
                // be disconnected"，根因是 Connect/Disconnect 和 Bind/Unbind 在 NetMQ
                // 里是两套不对称的配对操作：DSConnection() 里用的是 _Subscriber.Connect(dsAddress)
                // 建立的连接，这里却用 Unbind(dsAddress) 去撤销——Unbind 撤销的是 Bind 建立的
                // 监听端点，这个地址上根本没有 Bind 过，所以必然抛异常。
                // 更关键的是：这个异常被 catch 吞掉只打了一行日志，Unbind 失败意味着
                // Connect() 建立的那个连接其实从来没有被真正断开过——socket 内部仍然保持着
                // 到旧地址的连接。紧接着 DSConnection() 又会 Connect() 一个新地址，
                // 这样一个 SUB socket 可能同时连着新旧两个地址，导致"重连"之后仍然会
                // 收到旧服务器的消息（或者旧连接一直占着，行为不可预期）——这和之前
                // MarketQuote() 接收循环卡死的问题是两个独立的根因，都会造成"看起来断线
                // 重连了，但实际状态没有真正清干净"的现象，建议一起修。
                _Subscriber.Disconnect(dsAddress);
                _Log.LogInfo("取消连接");
            }
            catch (Exception e)
            {
                _Log.LogInfo("取消连接," + e.Message);
            }
            // 🔧 V20260927_MARKETQUOTE_CANCELLABLE_RECEIVE：
            // 之前 MarketQuote() 循环里用的是无超时、不可取消的 ReceiveFrameString()，
            // CancelAsync() 只是设置了 CancellationPending=true，但循环从来没检查过，
            // 所以这一行调用本身其实从来没有真正让后台线程退出过（见下方 MarketQuote 方法
            // 处的详细说明）。现在 MarketQuote() 循环已经会在每次超时轮询时检查
            // CancellationPending 并正常退出，这里的 CancelAsync() 才算真正生效。
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

            // 🆕 V20260927_MARKETQUOTE_DIAG_LOGGING：定位"完全没有接收消息日志"问题的诊断埋点。
            // 单靠现有日志无法区分三种情况：①方法根本没被调用（RunWorkerAsync 没触发）
            // ②方法跑起来了但循环一直超时、从没收到过消息 ③循环中途抛异常被 BackgroundWorker
            // 静默吞掉。加上下面这几行后：完全看不到"MarketQuote 接收循环启动"这行 → 是①；
            // 能看到启动日志、也能看到下面的心跳日志、但一直没有"接收消息："→ 是②，去查服务端
            // 有没有真的在往这个地址发消息、Connect 的地址对不对；启动日志之后突然没有任何
            // 日志（包括心跳）→ 是③，看 catch 块打出来的异常堆栈。
            _Log.LogInfo("MarketQuote 接收循环启动");
            int _timeoutHeartbeatCount = 0;
            try
            {

                // 🔧 V20260927_MARKETQUOTE_CANCELLABLE_RECEIVE：
                // 原来这里是 `string results = _Subscriber.ReceiveFrameString();`——
                // 这是无超时、不可中途取消的阻塞调用，会一直卡到 socket 上有消息到达为止。
                // 这不只是调试器 QuickWatch 里看到的那种"求值超时"的小麻烦，而是会引发
                // 两个连锁的真实运行期 bug：
                //
                // 1) DSDisconnect() 里调用的 bgwMarketQuote.CancelAsync() 只是把
                //    CancellationPending 置为 true，但这个 while(true) 循环从来没有
                //    检查过这个标志——所以"取消"从来没有真正生效过，这个后台线程会
                //    一直卡在 ReceiveFrameString() 里等下一条消息，bgwMarketQuote.IsBusy
                //    永远是 true，直到真的有新消息把这一轮 while 循环唤醒（但唤醒后循环体
                //    执行完还是会立刻回到 ReceiveFrameString() 继续阻塞，永远不会退出）。
                //
                // 2) 因为 IsBusy 永远是 true，DSConnection() 里
                //    `if (bgwMarketQuote.IsBusy != true) { bgwMarketQuote.RunWorkerAsync(); }`
                //    这个守卫条件在第一次连接成功、这个后台线程启动之后，就再也不会满足了。
                //    也就是说：只要发生过一次断线重连（timerCheckPlatformConnectStatus_Tick
                //    的自动拉起、timerRefreshAutoTrade_Tick 里 EA 地址变化触发的重连、或者
                //    3 分钟无心跳触发的 DSDisconnect），行情接收循环就再也不会重新启动，
                //    界面上的价格会静默停在最后一次收到的值上，不会有任何异常或日志提示。
                //
                // 3) 旧线程既然从未真正退出、一直卡在 _Subscriber 这个 socket 上，
                //    DSConnection() 里又会在同一个 _Subscriber 实例上调用 Connect()——
                //    NetMQ 的 socket 并不是设计给多线程并发访问的，一个线程卡在
                //    ReceiveFrameString() 里，另一个线程同时去 Connect() 同一个 socket，
                //    行为是未定义的，可能抛异常也可能让 socket 内部状态错乱。
                //
                // 现在改用 TryReceiveFrameString(timeout, out result)：收不到消息时最多
                // 阻塞 3 秒就返回 false，循环借这个间隙检查一次 CancellationPending，
                // 这样 DSDisconnect() 触发的取消才能在最多 3 秒内真正生效、线程真正退出、
                // IsBusy 恢复 false，DSConnection() 下次才能重新启动接收循环。
                while (true)
                {
                    if (!_Subscriber.TryReceiveFrameString(TimeSpan.FromSeconds(3), out string results))
                    {
                        // 每超时 120 次（约 6 分钟）打一次心跳日志，
                        // 证明循环还活着、只是没收到消息，
                        // 避免和"方法根本没跑起来"或"循环已经异常退出"这两种情况混淆。
                        _timeoutHeartbeatCount++;

                        if (_timeoutHeartbeatCount % 120 == 0)
                        {
                            _Log.LogInfo(string.Format(
                                "MarketQuote 循环存活中，已连续 {0} 次超时未收到消息（约 {1} 分钟），DSType={2}",
                                _timeoutHeartbeatCount,
                                (_timeoutHeartbeatCount * 3) / 60,
                                _Config.DSType));
                        }

                        // 3 秒内没有新消息，只是正常的超时轮询，回去再检查一次取消标志，不是错误
                        continue;
                    }
                    _timeoutHeartbeatCount = 0;
                    //_Log.LogInfo("接收消息：" + results);
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
                                // 心跳文本变化时记录本机时间；不解析心跳文本中的时间
                                string[] split = results.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                if (split.Length < 2 || string.IsNullOrWhiteSpace(split[1]))
                                {
                                    label_Price_Connect.Text = "未连接";
                                    label_Price_Connect.BackColor = Color.Red;
                                }
                                else
                                {
                                    string heartbeatText = split[1];
                                    if (!string.Equals(heartbeatText, _lastHeartbeatText, StringComparison.Ordinal))
                                    {
                                        _lastHeartbeatText = heartbeatText;
                                        _lastHeartbeatChangedAt = DateTime.Now;
                                    }

                                    label_Price_Connect.Text = heartbeatText;
                                    label_Price_Connect.BackColor = Color.Green;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 🆕 关键诊断点：之前循环体没有任何 try/catch，一旦 TryReceiveFrameString 或
                // EA1/EA2/EA3Async 抛出未捕获异常，异常会直接从 DoWork 方法冒出去，被
                // BackgroundWorker 捕获并塞进 RunWorkerCompletedEventArgs.Error——如果
                // bgwMarketQuote 的 RunWorkerCompleted 事件里没检查 e.Error（多半没检查），
                // 这个异常就会彻底消失，界面上和日志里都不会有任何提示，只会看到"接收消息："
                // 这行日志从某个时间点开始再也不出现。加上这个 catch 后，异常至少会被记录下来，
                // 不会再是"什么都没发生"式的静默死亡。
                _logger.Error(ex, "MarketQuote 接收循环异常终止");
                _Log.LogInfo("MarketQuote 接收循环异常终止：" + ex.Message);
            }
            // 🆕 循环因取消请求正常退出（或异常终止），把状态标志复位，这样 bgwMarketQuote.IsBusy
            // 才能在 BackgroundWorker 的 RunWorkerCompleted 之后恢复 false。
            _EAAnalysisRunning = false;
            _Log.LogInfo("MarketQuote 接收循环已结束");
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
        /// EA2的公共交易参数，作为全局变量保存
        /// </summary>
        string _EA2_TradeRecord = "";
        /// <summary>
        /// 对手反向交易策略
        /// </summary>
        public async void EA2(string results)
        {
            string[] split = results.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //参数说明：XAUUSD:2004.79:BUY:1:50:50 HH:mm:ss orderTypeDetail:openPrice:closePrice:profit
            //{symbol}:{price}:{optType}:{lotProportion}:{takeProfit}:{stopLoss}
            //品类:价格:BUY/SELL/CLOSE:手数:止盈值:止损值
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
                double lots = Convert.ToDouble(opt.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[3]);
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
                                executedFlag = _MT4.AutoTradeTransaction4(tradeType, lots, _IsAutoOperationFlag, out _SendTime);
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
                                executedFlag = _MT5.AutoTradeTransaction4(tradeType, lots, _IsAutoOperationFlag, out _SendTime);
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
