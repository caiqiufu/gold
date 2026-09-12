using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetMQ;
using NetMQ.Sockets;

namespace NetworkTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        PublisherSocket _publisher = new PublisherSocket();
        SubscriberSocket _subscriber = new SubscriberSocket();
        string _ip1 = "";
        Ping _ping = new Ping();
        Boolean _StratTestFlag = false;
        Boolean _StratReceiveFlag = false;
        private void buttonBind_Click(object sender, EventArgs e)
        {
            string BindIP = textBoxBindIP.Text;
            if (buttonBind.Text == "Bind")
            {                
                _publisher.Bind(BindIP);
            }
            else 
            {
                _publisher.Unbind(BindIP);


            }
            buttonBind.Text = buttonBind.Text=="Bind"?"UnBind":"Bind";
        }
        private void SendMsg() 
        {
            _ip1 = textBoxIP1.Text;
            PingReply pingReply = _ping.Send(_ip1);
            long currentTicks = DateTime.Now.Ticks;
            string Msg = pingReply.RoundtripTime + "|" + currentTicks;
            _publisher.SendFrame(Msg);
            string str = DateTime.Now.ToString("h:mm:ss.fff");
            listBoxSendMsg.Items.Add(str+" "+Msg);
            if (listBoxSendMsg.Items.Count>50)
            {
                listBoxSendMsg.Items.Clear();
            }
        }
        private void ReceiveMsg(string results)
        {        
            if (listBoxSendMsg.InvokeRequired)
            {
                listBoxSendMsg.Invoke(new Action<string>(ReceiveMsg), new object[] { results });
            }
            else
            {
                string[] split = results.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                long PingTime = Convert.ToInt32(split[0]);
                long TimeStample = long.Parse(split[1]);
                //Console.WriteLine("TimeStample="+TimeStample);
                long currentTicks = DateTime.Now.Ticks;
                long timeDiffHour = Convert.ToInt32((textBox1.Text==null|| textBox1.Text=="")?"0": textBox1.Text);
                //Console.WriteLine("currentTicks="+currentTicks);
                long TimeDuration = (currentTicks- timeDiffHour * 3600 * 10000000 - TimeStample) / 10000;
                string IP = textBoxBindIP.Text.Substring(6);
                PingReply pingReply = _ping.Send(IP.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);

                //long TimeDuration = (currentTicks - TimeStample) / 10000;
                listBoxSendMsg.Items.Add("服务端到数据源Ping:"+PingTime + " " + "接收时间间隔:"+TimeDuration+"  "+ "客户端到服务端Ping:"+pingReply.RoundtripTime);
                if (listBoxSendMsg.Items.Count > 50)
                {
                    listBoxSendMsg.Items.Clear();
                }
            }
        }
        private void timerSendMsg_Tick(object sender, EventArgs e)
        {
            if (_StratTestFlag)
            {
                SendMsg();
            }
        }

        private void buttonStartTest_Click(object sender, EventArgs e)
        {
            _StratTestFlag = true;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            _StratTestFlag = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _StratReceiveFlag = true;
            _subscriber.Subscribe("");
            if (backgroundWorker1.IsBusy != true)
            {
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (backgroundWorker1.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    Console.WriteLine(_StratReceiveFlag);
                    if (_StratReceiveFlag)
                    {
                        Console.WriteLine("1111");
                        string results = _subscriber.ReceiveFrameString();
                        Console.WriteLine(results+"2222");
                        //string results = "123|111122334445555666";
                        if (!string.IsNullOrEmpty(results))
                        {
                            ReceiveMsg(results);
                        }
                    }
                }
            }
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            _subscriber.Options.TcpKeepalive = true;
            _subscriber.Options.TcpKeepaliveIdle = new TimeSpan(5, 0, 0);
            _subscriber.Options.TcpKeepaliveInterval = new TimeSpan(0, 0, 1);
            string BindIP = textBox2.Text;
            if (button2.Text == "Bind")
            {
                _subscriber.Connect(BindIP);
            }
            else
            {
                _subscriber.Disconnect(BindIP);
            }
            button2.Text = button2.Text == "Bind" ? "UnBind" : "Bind";    
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _StratReceiveFlag = false;
        }
    }
}
