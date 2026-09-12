using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using uClient.Comm;

namespace iAutoTrade
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// 公共参数
        /// </summary>
        private Config _Config;
        private MyConfig _MyConfig;
        private StrategyConfig _StrategyConfig;
        /// <summary>
        /// 订单接收时间
        /// </summary>
        //private DateTime _ReceiveTime;
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

        public string rootPath = "C:\\iAutoTrade";

        private NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        public MainForm()
        {
            InitializeComponent();

            // 禁用最大化按钮和最小化按钮
            this.MaximizeBox = false;
            //this.MinimizeBox = false;

            // 设置窗口边框样式为固定单框
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            // 添加窗口大小改变事件处理程序
            this.Resize += MainForm_Resize;
        }
        //主窗口宽度
        //private int _FormWidth = 1412;
        private int _FormWidth = 886;
        //主窗口高度
        private int _FormHeigth = 252;
        private void MainForm_Resize(object sender, EventArgs e)
        {
            // 恢复窗口的大小，防止用户手动调整窗口大小
            this.Size = new System.Drawing.Size(_FormWidth, _FormHeigth);
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
                _StrategyConfig = JsonConvert.DeserializeObject<StrategyConfig>(File.ReadAllText(newStrategyFile));
            }
            else
            {
                _logger.Fatal("没有策略配置文件，系统初始化异常");
            }
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadConfigFile();

            //初始化参数
            DBUtils.connAddress = _Config.DefaultDBAddress;
            if (!string.IsNullOrEmpty(_Config.EAInfoAddress))
            {
                DBUtils.connEAInfoAddress = _Config.EAInfoAddress;
            }
            _logger.Info("EAInfo地址:" + DBUtils.connEAInfoAddress);

            DBUtils.createConn(null);
            DBUtils.createEaConn(null);

            LoadConfig();
            InitData();
            IniUI();
        }
        /// <summary>
        /// 加载配置参数
        /// </summary>
        private void LoadConfig()
        {
            if (_Config != null)
            {
                if (_Config.Platform.Count > 0)
                {
                    foreach (uClient.Comm.Platform platform in _Config.Platform)
                    {
                        _Config.PlatformList.Remove(platform.PlatformCode);
                        _Config.PlatformList.Add(platform.PlatformCode, platform);
                        if (platform.Broker != null && platform.Broker.Count > 0)
                        {
                            foreach (uClient.Comm.Broker broker in platform.Broker)
                            {
                                if (_MyConfig != null && _MyConfig.Account.Length > 0)
                                {
                                    foreach (uClient.Comm.Account account in _MyConfig.Account)
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
                                        }
                                    }
                                }
                                platform.BrokerList.Remove(broker.BrokerCode);
                                platform.BrokerList.Add(broker.BrokerCode, broker);
                            }
                        }
                    }
                }
            }
            else
            {
                _logger.Fatal("没有初始化配置文件，系统初始化异常");
            }
        }
        /// <summary>
        /// 初始化参数
        /// </summary>
        private void InitData()
        {
            //把所有按钮加载到数组中
            _ButtonList.Add("Trade1", button_MF4_Trade1);
            _ButtonList.Add("Trade2", button_MF4_Trade2);
            _ButtonList.Add("Trade3", button_MF4_Trade3);
            _ButtonList.Add("Trade4", button_MF4_Trade4);
            _ButtonList.Add("Trade5", button_MF4_Trade5);
            _ButtonList.Add("Trade6", button_MF4_Trade6);
            //_ButtonList.Add("Trade7", button_MF4_Trade7);
            //_ButtonList.Add("Trade8", button_MF4_Trade8);
            //_ButtonList.Add("Trade9", button_MF4_Trade9);
            //_ButtonList.Add("Trade10", button_MF4_Trade10);
            _ButtonList.Add("Trade7", button_EAC_Trade1);
            _ButtonList.Add("Trade8", button_EAC_Trade2);
            _ButtonList.Add("Trade9", button_EAC_Trade3);
            _ButtonList.Add("Trade10", button_EAC_Trade4);
            _ButtonList.Add("Trade11", button_EAC_Trade5);
            _ButtonList.Add("Trade12", button_EAC_Trade6);
            //_ButtonList.Add("Trade17", button_EAC_Trade7);
            //_ButtonList.Add("Trade18", button_EAC_Trade8);
            //_ButtonList.Add("Trade19", button_EAC_Trade9);
            //_ButtonList.Add("Trade20", button_EAC_Trade10);
            _ButtonList.Add("Trade13", button_MT4_Trade1);
            _ButtonList.Add("Trade14", button_MT4_Trade2);
            _ButtonList.Add("Trade15", button_MT4_Trade3);
            _ButtonList.Add("Trade16", button_MT4_Trade4);
            _ButtonList.Add("Trade17", button_MT4_Trade5);
            _ButtonList.Add("Trade18", button_MT4_Trade6);
            //_ButtonList.Add("Trade27", button_MT4_Trade7);
            //_ButtonList.Add("Trade28", button_MT4_Trade8);
            //_ButtonList.Add("Trade29", button_MT4_Trade9);
            //_ButtonList.Add("Trade30", button_MT4_Trade10);

            //启动自动刷新托管列表时钟
            timer_RefreshAutoList.Start();
        }
        /// <summary>
        /// 初始化UI参数
        /// </summary>
        private void IniUI()
        {
            String version = Application.ProductVersion;
            this.Text = "AutoTrade[" + version + "]";
            if (string.Equals(_Config.clientType, "EA"))
            {
                label_Type1.Text = "EAC1";
                label_Type2.Text = "EAC2";
                label_Type3.Text = "EAC3";
                foreach (KeyValuePair<String, Button> kvp in _ButtonList)
                {
                    _ButtonList[kvp.Key].Text = kvp.Key;
                }
                List<IDictionary<string, string>> eaList = DBHelper.getAutoList(Utils.GetLocalIP());
                if (eaList != null && eaList.Count > 0)
                {
                    int i = 1;
                    foreach (IDictionary<string, string> data in eaList)
                    {
                        //string brokerCode = data["broker_code"];
                        string formNo = data["form_no"];
                        //获取该指定服务器上的所有托管列表,该列表中有新增托管,也有取消托管,针对取消托管的只需要修改按钮未不可用
                        //string platform = string.Equals(brokerCode, "SUI") ? "MF4" : "MT4";
                        Button button = _ButtonList[formNo];
                        button.Text = "[" + formNo + "]Ready";
                        button.BackColor = Color.Red;
                        i++;
                    }
                }
            }
        }
        Dictionary<String, Button> _Buttons = new Dictionary<String, Button>();
        Dictionary<String, Process> _Processs = new Dictionary<String, Process>();
        /// <summary>
        /// 所有按钮列表
        /// </summary>
        Dictionary<String, Button> _ButtonList = new Dictionary<String, Button>();

        /// <summary>
        /// 打开Broker程序
        /// </summary>
        /// <param name="formNo"></param>
        /// <param name="formName"></param>
        /// <param name="platFormCode"></param>
        /// <param name="brokerCode"></param>
        private void openWindow(string fileName, string formNo, string formName, string platFormCode, string brokerCode, string brokerName)
        {
            string paras = formNo.Replace("EAC", "MT5") + "," + formName + "," + platFormCode + "," + brokerCode + "," + brokerName;
            //FileInfo file = new FileInfo(fileName);
            Process pro = new Process();
            //Console.WriteLine("fileName="+ fileName);
            //Console.WriteLine("paras=" + paras);
            _logger.Debug("Open Window File Name:" + fileName);
            _logger.Debug("Open Window Paras:" + paras);
            ProcessStartInfo procStartInfo = new ProcessStartInfo(fileName, paras);
            //procStartInfo.WorkingDirectory = file.Directory.FullName;
            //procStartInfo.FileName = fileName;
            procStartInfo.UseShellExecute = true;
            pro.StartInfo = procStartInfo;
            pro.Start();
            if (!_Processs.ContainsKey(formNo))
            {
                _Processs.Add(formNo, pro);
            }
        }
        private string GenerateFilePath(string platFormCode, string brokerCode)
        {
            DirectoryInfo pathInfo = new DirectoryInfo(Application.StartupPath);
            string path = pathInfo.Parent.FullName;
            string MT4Path = path + "\\AutoTradeMT4";
            //string V4Path = path + "\\AutoTradeV4";
            string EPMPath = path + "\\AutoTradeEPM";
            string SUIPath = path + "\\AutoTradeSUI";
            string TRPath = path + "\\AutoTradeTR";
            string ZYJPath = path + "\\AutoTradeZYJ";
            string EACPath = path + "\\AutoTradeClient";
            string JSDPath = path + "\\AutoTradeJSD";

            //测试路径
            /*path = "C:\\D\\OneDrive\\07-svn\\01-gold";
            MT4Path = path + "\\AutoTradeMT4\\bin\\Debug";
            EPMPath = path + "\\AutoTradeEPM\\bin\\Debug";
            SUIPath = path + "\\AutoTradeSUI\\bin\\Debug";
            TRPath = path + "\\AutoTradeTR\\bin\\Debug";
            ZYJPath = path + "\\AutoTradeZYJ\\bin\\Debug";
            EACPath = path + "\\AutoTradeClient\\bin\\Debug";
            JSDPath = path + "\\AutoTradeJSD\\bin\\Debug";*/

            string destFile;
            switch (platFormCode)
            {
                case "MT4":
                    destFile = System.IO.Path.Combine(MT4Path, "AutoTrade.exe");
                    break;
                case "MT5":
                    destFile = System.IO.Path.Combine(MT4Path, "AutoTrade.exe");
                    break;
                case "EAC":
                    destFile = System.IO.Path.Combine(EACPath, "AutoTradeClient.exe");
                    break;
                case "MF4":
                    switch (brokerCode)
                    {
                        case "SUI":
                            destFile = System.IO.Path.Combine(SUIPath, "AutoTradeSUI.exe");
                            break;
                        case "EPM":
                            destFile = System.IO.Path.Combine(EPMPath, "AutoTradeEPM.exe");
                            break;
                        case "TR":
                            destFile = System.IO.Path.Combine(TRPath, "AutoTradeTR.exe");
                            break;
                        case "ZYJ":
                            destFile = System.IO.Path.Combine(ZYJPath, "AutoTradeZYJ.exe");
                            break;
                        case "JSD":
                            destFile = System.IO.Path.Combine(JSDPath, "AutoTradeJSD.exe");
                            break;
                        default:
                            destFile = System.IO.Path.Combine(SUIPath, "AutoTradeSUI.exe");
                            break;
                    }
                    break;
                default:
                    destFile = System.IO.Path.Combine(MT4Path, "AutoTrade.exe");
                    break;
            }
            return destFile;
        }
        private Broker GetBrokerCode(string platformCode, string brokerName)
        {
            Broker broker = null;
            List<uClient.Comm.Broker> brokerList = _Config.PlatformList[platformCode].Broker;
            foreach (Broker b in brokerList)
            {
                if (string.Equals(b.BrokerName, brokerName))
                {
                    broker = b;
                    break;
                }
            }
            if (broker == null)
            {
                string defaultBrokerCode = _Config.PlatformList[platformCode].DefaultBrokerCode;
                foreach (Broker b in brokerList)
                {
                    if (string.Equals(b.BrokerCode, defaultBrokerCode))
                    {
                        broker = b;
                        break;
                    }
                }
            }
            if (broker == null)
            {
                broker = new Broker();
                broker.BrokerCode = "test";
                broker.BrokerName = "test";
                Account _Account = new Account();
                _Account.TradePara = new TradeParametre();
                _Account.BrokerCode = broker.BrokerCode;
                if (string.Equals(platformCode, "MT4"))
                {
                    broker.PlatformNo = 2;
                }
                if (string.Equals(platformCode, "MT5"))
                {
                    broker.PlatformNo = 4;
                }
                broker.DemoAccount = _Account;
                broker.LiveAccount = _Account;
                broker.TradePara = _Account.TradePara;
                broker.DefaultAccountType = "Demo";
            }
            return broker;
        }
        private void button_MF4_Trade1_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade1", (Button)sender);
            }
            else
            {
                MF4TradeClick("MF4Trade1", (Button)sender);
            }
        }
        private void MT4TradeClick(string formNo, Button button)
        {
            string platFormCode = "MT4";
            Broker broker = GetBrokerCode(platFormCode, button.Text);
            //string formNo = platFormCode + "_" + broker.BrokerCode;
            string fileName = GenerateFilePath(platFormCode, broker.BrokerCode);
            openWindow(fileName, formNo, "【MT4】" + broker.BrokerName, platFormCode, broker.BrokerCode, broker.BrokerName);
            button.Enabled = false;
            if (!_Buttons.ContainsKey(formNo))
            {
                _Buttons.Add(formNo, button);
            }
        }
        private void MT5TradeClick(string formNo, Button button)
        {
            string platFormCode = "MT5";
            Broker broker = GetBrokerCode(platFormCode, button.Text);
            //string formNo = platFormCode + "_" + broker.BrokerCode;
            string fileName = GenerateFilePath(platFormCode, broker.BrokerCode);
            openWindow(fileName, formNo, "【MT5】" + broker.BrokerName, platFormCode, broker.BrokerCode, broker.BrokerName);
            button.Enabled = false;
            if (!_Buttons.ContainsKey(formNo))
            {
                _Buttons.Add(formNo, button);
            }
        }
        private void MF4TradeClick(string formNo, Button button)
        {
            string platFormCode = "MF4";
            Broker broker = GetBrokerCode(platFormCode, button.Text);
            //string formNo = platFormCode + "_" + broker.BrokerCode;
            string fileName = GenerateFilePath(platFormCode, broker.BrokerCode);
            openWindow(fileName, formNo, "【MF4】" + broker.BrokerName, platFormCode, broker.BrokerCode, broker.BrokerName);
            button.Enabled = false;
            if (!_Buttons.ContainsKey(formNo))
            {
                _Buttons.Add(formNo, button);
            }
        }
        private void EACTradeClick(string formNo, Button button)
        {
            Broker broker = new Broker();
            string platFormCode = "MT4";
            IDictionary<string, string> brokerInfo = DBHelper.getAutoBrokerInfo(Utils.GetLocalIP(), formNo);
            if (brokerInfo != null)
            {
                platFormCode = brokerInfo["platform"];
                broker.BrokerName = brokerInfo["broker_name"];
                broker.BrokerCode = brokerInfo["broker_code"];
                //string formNo = platFormCode + "_" + broker.BrokerCode;
                string fileName = GenerateFilePath("EAC", broker.BrokerCode);
                openWindow(fileName, formNo, "【EAC】" + broker.BrokerName, platFormCode, broker.BrokerCode, broker.BrokerName);
                button.Enabled = false;
                if (button.Text.Contains("Ready"))
                {
                    button.Text = "[" + formNo + "]Started";
                    button.BackColor = Color.Purple;
                }
                if (!_Buttons.ContainsKey(formNo))
                {
                    _Buttons.Add(formNo, button);
                }
            }
        }
        private void timer_RefreshButtonStatus_Tick(object sender, EventArgs e)
        {
            IList<string> removeKeyList = new List<string>();
            foreach (string key in _Processs.Keys)
            {
                if (_Processs[key].HasExited)
                {
                    _Buttons[key].Enabled = true;
                    removeKeyList.Add(key);
                    if (_Buttons[key].Text.Contains("Started"))
                    {
                        _Buttons[key].Text = "[" + key + "]Ready";
                        _Buttons[key].BackColor = Color.Red;
                    }

                }
            }
            if (removeKeyList != null && removeKeyList.Count > 0)
            {
                foreach (string key in removeKeyList)
                {
                    _Processs.Remove(key);
                }
            }
        }
        private void button_MT4_Trade1_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade13", (Button)sender);
            }
            else
            {
                MT4TradeClick("MT4Trade1", (Button)sender);
            }
        }
        private void button_MT4_Trade2_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade14", (Button)sender);
            }
            else
            {
                MT4TradeClick("MT4Trade2", (Button)sender);
            }
        }

        private void button_MT4_Trade3_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade15", (Button)sender);
            }
            else
            {
                MT4TradeClick("MT4Trade3", (Button)sender);
            }
        }
        private void button_MT4_Trade4_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade16", (Button)sender);
            }
            else
            {
                MT4TradeClick("MT4Trade4", (Button)sender);
            }
        }
        private void button_EAC_Trade1_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade7", (Button)sender);
            }
            else
            {
                MT5TradeClick("EACTrade1", (Button)sender);
            }
        }

        private void button_EAC_Trade2_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade8", (Button)sender);
            }
            else
            {
                MT5TradeClick("EACTrade2", (Button)sender);
            }
        }

        private void button_EAC_Trade3_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade9", (Button)sender);
            }
            else
            {
                MT5TradeClick("EACTrade3", (Button)sender);
            }
        }
        private void button_EAC_Trade4_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade10", (Button)sender);
            }
            else
            {
                MT5TradeClick("EACTrade4", (Button)sender);
            }
        }
        private void button_MF4_Trade2_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade2", (Button)sender);
            }
            else
            {
                MF4TradeClick("MF4Trade2", (Button)sender);
            }
        }

        private void button_MF4_Trade3_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade3", (Button)sender);
            }
            else
            {
                MF4TradeClick("MF4Trade3", (Button)sender);
            }
        }
        private void button_MF4_Trade4_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade4", (Button)sender);
            }
            else
            {
                MF4TradeClick("MF4Trade4", (Button)sender);
            }
        }
        private void ChangeHistory_Click(object sender, EventArgs e)
        {
            string destFile = Application.StartupPath + "\\ReadMe.txt";
            if (File.Exists(destFile))
            {
                System.Diagnostics.Process.Start(destFile);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool isAllFormClosed = true;
            foreach (string key in _Processs.Keys)
            {
                if (!_Processs[key].HasExited)
                {
                    isAllFormClosed = false;
                    break;
                }
            }
            if (!isAllFormClosed)
            {
                MessageBox.Show("请关闭所有子窗口后再关闭主窗口");
                e.Cancel = true;
            }
        }

        private void button_MF4_Trade5_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade5", (Button)sender);
            }
            else
            {
                MF4TradeClick("MF4Trade5", (Button)sender);
            }
        }

        private void button_MF4_Trade6_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade6", (Button)sender);
            }
            else
            {
                MF4TradeClick("MF4Trade6", (Button)sender);
            }
        }

        private void button_EAC_Trade5_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade11", (Button)sender);
            }
            else
            {
                MT5TradeClick("EACTrade5", (Button)sender);
            }
        }

        private void button_EAC_Trade6_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade12", (Button)sender);
            }
            else
            {
                MT5TradeClick("EACTrade6", (Button)sender);
            }
        }

        private void button_MT4_Trade5_Click(object sender, EventArgs e)
        {

            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade17", (Button)sender);
            }
            else
            {
                MT4TradeClick("MT4Trade5", (Button)sender);
            }
        }

        private void button_MT4_Trade6_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade18", (Button)sender);
            }
            else
            {
                MT4TradeClick("MT4Trade6", (Button)sender);
            }
        }

        private void button_MF4_Trade7_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade7", (Button)sender);
            }
            else
            {
                MF4TradeClick("MF4Trade7", (Button)sender);
            }
        }

        private void button_MF4_Trade8_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade8", (Button)sender);
            }
            else
            {
                MF4TradeClick("MF4Trade8", (Button)sender);
            }
        }

        private void button_MF4_Trade9_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade9", (Button)sender);
            }
            else
            {
                MF4TradeClick("MF4Trade9", (Button)sender);
            }
        }

        private void button_MF4_Trade10_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade10", (Button)sender);
            }
            else
            {
                MF4TradeClick("MF4Trade10", (Button)sender);
            }
        }

        private void button_EAC_Trade7_Click(object sender, EventArgs e)
        {
            EACTradeClick("Trade17", (Button)sender);
        }

        private void button_EAC_Trade8_Click(object sender, EventArgs e)
        {
            EACTradeClick("Trade18", (Button)sender);
        }

        private void button_EAC_Trade9_Click(object sender, EventArgs e)
        {
            EACTradeClick("Trade19", (Button)sender);
        }

        private void button_EAC_Trade10_Click(object sender, EventArgs e)
        {
            EACTradeClick("Trade20", (Button)sender);
        }

        private void button_MT4_Trade7_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade27", (Button)sender);
            }
            else
            {
                MT4TradeClick("MT4Trade7", (Button)sender);
            }
        }

        private void button_MT4_Trade8_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade28", (Button)sender);
            }
            else
            {
                MT4TradeClick("MT4Trade8", (Button)sender);
            }
        }

        private void button_MT4_Trade9_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade29", (Button)sender);
            }
            else
            {
                MT4TradeClick("MT4Trade9", (Button)sender);
            }
        }

        private void button_MT4_Trade10_Click(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                EACTradeClick("Trade30", (Button)sender);
            }
            else
            {
                MT4TradeClick("MT4Trade10", (Button)sender);
            }
        }
        /// <summary>
        /// 每隔1分钟自动扫描托管列表,更新托管账户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_RefreshAutoList_Tick(object sender, EventArgs e)
        {
            if (string.Equals(_Config.clientType, "EA"))
            {
                _logger.Info("自动刷新托管列表开始");
                foreach (KeyValuePair<String, Button> kvp in _ButtonList)
                {
                    string buttonKey = kvp.Key;
                    bool isExistFlag = false;
                    Button button = _ButtonList[buttonKey];
                    List<IDictionary<string, string>> eaList = DBHelper.getAutoList(Utils.GetLocalIP());
                    foreach (IDictionary<string, string> data in eaList)
                    {
                        //string brokerCode = data["broker_code"];
                        //string platform = string.Equals(brokerCode, "SUI") ? "MF4" : "MT4";
                        string formNo = data["form_no"];
                        if (string.Equals(buttonKey, formNo))
                        {
                            isExistFlag = true;
                            break;
                        }
                    }
                    if (isExistFlag)
                    {
                        //针对新增加的托管,修改显示内容
                        if (button.Enabled == true)
                        {
                            button.Text = "[" + buttonKey + "]Ready";
                            button.BackColor = Color.Red;
                            //自动打开窗口
                            EACTradeClick(buttonKey, button);
                            Thread.Sleep(5000);
                        }
                    }
                    else
                    {
                        //不存在表示该按钮下没有托管,或者已有托管已取消
                        if (button.Enabled == true)
                        {
                            button.Text = buttonKey;
                            button.BackColor = Color.White;
                        }
                        if (button.Enabled == false && button.Text.Contains("Started"))
                        {
                            _Processs[buttonKey].Kill();
                            _Processs.Remove(buttonKey);
                            button.Enabled = true;
                            button.Text = buttonKey;
                            button.BackColor = Color.White;
                        }
                    }
                }
                _logger.Info("自动刷新托管列表完成");
            }
        }
    }
}
