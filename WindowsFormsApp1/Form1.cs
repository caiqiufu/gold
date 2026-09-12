using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Xml;
using System.Threading;
using V4;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        bool isLogin = false;
        V4Client _Client = new V4Client();

        void getPrice()
        {
            try {
                while (true)
                {
                    string res = _Client.getPrice();
                    Console.WriteLine("getPrice," + res);
                    if (res != "" && res.Contains("19:"))
                    {
                        _Client.bid = Convert.ToDouble(res.Substring(3, 7));
                        _Client.ask = Convert.ToDouble(res.Substring(11, 7));
                        Action action = () =>
                        {
                            textBoxPriceInfo.Text = DateTime.Now + " :" + res;
                        };
                        Invoke(action);                     
                    }
                }
            } catch (Exception e) {
                Console.WriteLine("getPrice error,"+e.Message);
            } 
        }
        DateTime startTimes = DateTime.Now;
        void getTrading()
        {
            if (isLogin) 
            {
                string res = _Client.getTrading2();
                Console.WriteLine(res);
                textBoxTradingInfo.Text = DateTime.Now + " :" + res;
            }        
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Button2_Click(object sender, EventArgs e)
        {           
            if (!isLogin)
            {
                bool res0 = _Client.Login1("83665", "qazwsx123", false, "203.160.75.182", 4523);
                isLogin = true;
                textBoxLogInfo.Items.Add(DateTime.Now+":"+"登录成功");
                Action a = getPrice;
                a.BeginInvoke(null, null);
            }
        }
        public static XmlDocument xml = new XmlDocument();
        /// <summary>
        /// AccountId|InstrumentId|QuotePolicyId
        /// </summary>
        /// <returns></returns>
        public static string[] myGetInitData(string res)
        {
            string[] paras = new string[10];
            xml.LoadXml(res);
            XmlNode accountNode = xml.SelectSingleNode("InitializeData/Accounts/Account");
            if (accountNode!=null)
            {
                paras[0] = accountNode.Attributes["Id"].Value;
                XmlNodeList instrumentNodes = xml.SelectNodes("InitializeData/Instruments/Instrument");
                if (instrumentNodes != null && instrumentNodes.Count > 0)
                {
                    foreach (XmlNode node in instrumentNodes)
                    {
                        if (node.Attributes["OriginCode"].Value.Equals("XAUUSD"))
                        {
                            paras[1] = node.Attributes["Id"].Value;
                        }
                    }
                }
                XmlNodeList quotationNodes = xml.SelectNodes("InitializeData/Quotations/Quotation");
                if (quotationNodes != null && quotationNodes.Count > 0)
                {
                    foreach (XmlNode node in quotationNodes)
                    {
                        if (node.Attributes["InstrumentId"].Value.Equals(paras[1]))
                        {
                            paras[2] = node.Attributes["QuotePolicyId"].Value;
                        }
                    }
                }               
            }
            return paras;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string res = _Client.OpenBuyOrder1();
            textBoxLogInfo.Items.Add(DateTime.Now + ":" + "手动买入" + res);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string res = _Client.OpenSellOrder1();
            textBoxLogInfo.Items.Add(DateTime.Now + ":" + "手动卖出" + res);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string res = _Client.CloseAllOrders();
            textBoxLogInfo.Items.Add(DateTime.Now + ":" + "平仓" + res);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            getTrading();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBoxLogInfo.Items.Add(DateTime.Now + ":" + "开启自动下单");
            //异步操作
            Task.Run(()=> {
                if (isLogin)
                {
                    int j = 1;
                    while (true)
                    {
                        if (j%50==0)
                        {
                            string res = _Client.OpenBuyOrder1();                           
                            Action action = () =>
                            {
                                 textBoxLogInfo.Items.Add(DateTime.Now + ":" + "自动买入" + res);
                            };
                            Invoke(action);
                        }
                        if (j % 200 == 0)
                        {
                            string res = _Client.OpenSellOrder1();                            
                            Action action = () =>
                            {
                                textBoxLogInfo.Items.Add(DateTime.Now + ":" + "自动卖出" + res);
                            };
                            Invoke(action);
                        }
                        if (j % 100 == 0)
                        {
                            string res= _Client.CloseAllOrders();                          
                            Action action = () =>
                            {
                                textBoxLogInfo.Items.Add(DateTime.Now + ":" + "自动平仓" + res);
                            };
                            Invoke(action);
                        }
                        j++;
                        Action action1 = () =>
                        {
                            textBoxLogInfo.Items.Add(DateTime.Now + ":j=" + j);
                        };
                        Invoke(action1);
                    }
                }
            });
            
        }
        public void LoginQuote() {          
            bool res0 = _Client.LoginQuota("203.160.75.182", 4529);
            Console.WriteLine("LoginQuote=" + res0);
            Console.WriteLine("LoginQuote Thread:" + Thread.CurrentThread.ManagedThreadId.ToString());

        }
        private void button6_Click(object sender, EventArgs e)
        {
            string res = _Client.Recover1();
            Console.WriteLine("Recover1=" + res);
            Console.WriteLine("Recover1 Thread:" + Thread.CurrentThread.ManagedThreadId.ToString());
        }
        private void textBoxLogInfo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
