using DS;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using V4;

namespace V4Quote
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        private PublisherSocket publisher;
        private string _configFile = System.Windows.Forms.Application.StartupPath + "\\MyDSConfig.json";
        Config _config;
        string _DSType = "V4";
        string _BrokerType = "YSG";
        string _BindAddress = "";
        string _AccountName = "";
        string _Password = "";
        string _AccountType = "Live";
        V4Client _Client;
        private void buttonBind_Click(object sender, EventArgs e)
        {
            if (buttonBind.Text.ToLower() == "bind")
            {
                if (textBoxIp.Text != null)
                {
                    try
                    {
                        Trace.WriteLine("bind to :" + textBoxIp.Text);
                        publisher = new PublisherSocket();
                        publisher.Bind(textBoxIp.Text);

                        buttonBind.Text = "UnBind";//172.31.47.203
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Bind Error:" + ex.Message);
                        Trace.WriteLine(ex.Message);
                    }
                }
                LogInfo("绑定地址成功");
            }
            else
            {
                try
                {
                    if (publisher != null)
                    {
                        Trace.WriteLine("Unbind to :" + textBoxIp.Text);
                        publisher.Unbind(textBoxIp.Text);
                        buttonBind.Text = "Bind";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("UnBind Error:" + ex.Message);
                    Trace.WriteLine(ex.Message);
                }
                LogInfo("解绑地址成功");
            }
        }
        private void InitialPara()
        {
            if (_config.AccountList.ContainsKey(_BrokerType + "_" + _AccountType))
            {
                Account acc = _config.AccountList[_BrokerType + "_" + _AccountType];
                _AccountName = acc.AccountName;
                _Password = acc.Password;
                _AccountType = acc.AccountType;
            }else
            {
                _AccountName = "";
                _Password = "";
            }
            this._Client = new V4Client(this);
            textBoxIp.Text = _BindAddress;
            textBoxUserCode.Text = _AccountName;
            textBoxPassword.Text = _Password;
            comboBoxAccountType.Text = _AccountType;

        }
        private void button1_Click(object sender, EventArgs e)
        {
            SaveConfig();
            LogInfo("保存成功");
            MessageBox.Show("保存成功");
        }
        private void SaveConfig()
        {
            if ("OEC".Equals(_DSType))
            {
                _config.BindAddressOEC = _BindAddress;
            }
            if ("T4".Equals(_DSType))
            {
                _config.BindAddressT4 = _BindAddress;
            }
            if ("V4".Equals(_DSType))
            {
                _config.BindAddressV4 = _BindAddress;
            }
            _config.DSType = _DSType;
            _config.BrokerType = _BrokerType;
            string brokerAccKey = _BrokerType + "_" + _AccountType;
            _config.AccountList.Remove(brokerAccKey);
            Account acc = new Account();
            acc.AccountName = _AccountName;
            acc.Password = _Password;
            acc.AccountType = _AccountType;
            _config.AccountList.Add(brokerAccKey, acc);
            System.IO.File.WriteAllText(_configFile, JsonConvert.SerializeObject(_config));
        }

        private void textBoxIp_TextChanged(object sender, EventArgs e)
        {
            _BindAddress = textBoxIp.Text;
        }
        private void LoadConfig()
        {
            if (System.IO.File.Exists(_configFile))
            {
                _config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText(_configFile));
                if (_config != null)
                {
                    //_DSType = _config.DSType;
                    if ("OEC".Equals(_DSType))
                    {
                        _BindAddress = _config.BindAddressOEC;
                    }
                    if ("T4".Equals(_DSType))
                    {
                        _BindAddress = _config.BindAddressT4;
                    }
                    if ("V4".Equals(_DSType))
                    {
                        _BindAddress = _config.BindAddressV4;
                    }
                    _BrokerType = _config.BrokerType;
                }
                InitialPara();
                LogInfo("加载完成");
            }
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            LoadConfig();
        }

        private void textBoxUserCode_TextChanged(object sender, EventArgs e)
        {
            _AccountName = textBoxUserCode.Text;
        }

        private void textBoxPassword_TextChanged(object sender, EventArgs e)
        {
            _Password = textBoxPassword.Text;
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            if (_AccountName == null || _AccountName == "")
            {
                LogInfo("账号不能为空");
                MessageBox.Show("账号不能为空");
                return;
            }
            if (_Password == null || _Password == "")
            {
                LogInfo("密码不能为空");
                MessageBox.Show("密码不能为空");
                return;
            }
            string brokerAccKey = _BrokerType + "_" + _AccountType;
            string ip = "";
            int port = 4523;
            switch(brokerAccKey)
            {
                case "YSG_Live":
                    ip = "203.160.75.182";
                    port = 4523;
                    break;
                case "YSG_Demo":
                    ip = "203.160.75.182";
                    port = 4523;
                    break;
                case "WFB_Live":
                    ip = "75.2.85.181";
                    port = 4523;
                    break;
                case "WFB_Demo":
                    ip = "18.166.151.60";
                    port = 4523;
                    break;
            }
            string accountType = "";
            if (string.Equals(_BrokerType, "YSG"))
            {
                accountType = string.Equals(_AccountType, "Live") ? "YSG" : "DEM";
            }
            if (string.Equals(_BrokerType, "WFB"))
            {
                accountType = string.Equals(_AccountType, "Live") ? "WFB" : "WFB";
            }
            _ConnectFlag = _Client.Login1(_AccountName, _Password, accountType, ip, port);
            if (_ConnectFlag)
            {
                LogInfo("登录成功");
                if (bgwQuote.IsBusy != true)
                {
                    bgwQuote.RunWorkerAsync();
                }
            }
        }
        private void sendPrice(string symbol,decimal Bid, decimal Ask, string strLastVolTotal)
        {

            if (publisher != null)
            {
                publisher.SendFrame($"{symbol} {Bid} {Ask} {strLastVolTotal}");
            }
        }
        public bool _ConnectFlag { set; get; }
        private void buttonLogout_Click(object sender, EventArgs e)
        {
            if (isConnect())
            {
                _ConnectFlag = false;
                _Client.Logout();
            }
        }
        public bool isConnect()
        {
            return _ConnectFlag;
        }

        private void comboBoxAccountType_SelectedIndexChanged(object sender, EventArgs e)
        {
            _AccountType = comboBoxAccountType.Text;
            InitialPara();
        }
        private string _QuoteStr = "";
        private void bgwQuote_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            DateTime startTime = DateTime.Now;
            while (true)
            {
                if (_ConnectFlag)
                {
                    try
                    {
                        if ((DateTime.Now - startTime).TotalMinutes > 15)
                        {
                            startTime = DateTime.Now;
                            _Client.LoginQuote1();
                        }
                        DateTime st = DateTime.Now;
                        string[] quotes = _Client.GetQuote1();
                        if(quotes!=null)
                        {
                            DateTime et = DateTime.Now;
                            sendPrice(quotes[0], Convert.ToDecimal(quotes[1]), Convert.ToDecimal(quotes[2]), "");
                            _QuoteStr = "报价:" + "Bid:" + quotes[1] + ",Ask:" + quotes[2] + " " + "Total Time:" + (et - st).TotalMilliseconds;
                            if (bgwLog.IsBusy != true)
                            {
                                bgwLog.RunWorkerAsync();
                            }
                            //LogInfo("报价:" + "Bid:" + quotes[0] + ",Ask:" + quotes[1] + " " + DateTime.Now);
                        }

                    }
                    catch (Exception e1)
                    {
                        _QuoteStr = "获取报价异常:" + e1.Message;
                        if (bgwLog.IsBusy != true)
                        {
                            bgwLog.RunWorkerAsync();
                        }
                        //LogInfo("获取报价异常:" + e1.Message);
                    }
                }              
            }
        }
        /// <summary>
        /// 操作日志
        /// </summary>
        /// <param name="msg"></param>
        public void LogInfo(string msg)
        {
            _LstLog.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss.fff") + " " + msg);
            if (_LstLog.Items.Count > 10)
            {
                _LstLog.Items.RemoveAt(_LstLog.Items.Count - 1);
            }
        }

        private void bgwLog_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            LogInfo(_QuoteStr);
        }

        private void comboBox_Broker_SelectedIndexChanged(object sender, EventArgs e)
        {
            _BrokerType = comboBox_Broker.Text;
            InitialPara();
        }
    }
}
