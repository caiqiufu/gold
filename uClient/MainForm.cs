using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using uClient.Comm;

namespace uClient
{
    partial class MainForm : Form
    {
        private NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        public MainForm()
        {
            InitializeComponent();
        }
        public string testpa = "";
        public ArrayList _Forms = new ArrayList();
        Dictionary<String, Button> _Buttons = new Dictionary<String, Button>();
        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="formNo"></param>
        /// <param name="formName"></param>
        /// <param name="platFormName"></param>
        private void openWindow(string formNo,string formName,string platFormCode)
        {
            MainFormPara mainFormPara = new MainFormPara();
            mainFormPara.FormNo = formNo;
            mainFormPara.FormName = formName;
            mainFormPara.FormType = "";
            mainFormPara.PlatformCode = platFormCode;
            mainFormPara.BrokerName = "";
            if (_Forms.Count < 10)
            {
                TradeForm form2 = new TradeForm(mainFormPara);
                form2.Show();
                _Forms.Add(form2);
            }
            else
            {
                MessageBox.Show("最多只能打开10个窗口");
            }
        }
        public void SetButtonEnabled(string buttonName,bool status)
        {
            if (_Buttons.Keys.Contains(buttonName))
            {
                _Buttons[buttonName].Enabled = status;
                _Buttons.Remove(buttonName);
            }           
            else {
                _logger.Error("[" + buttonName + "]不存在");
            }
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            String version = Application.ProductVersion;
            this.Text ="AutoTrade[" +version+ "]";
        }

        private void button_MFT1_Click(object sender, EventArgs e)
        {
            openWindow("MF4Trade1", "【MF4】 " + button_MFT1.Text, "MF4");
            button_MFT1.Enabled = false;
            _Buttons.Add("MF4Trade1", button_MFT1);
        }

        private void button_MFT2_Click(object sender, EventArgs e)
        {
            openWindow("MF4Trade2", "【MF4】 " + button_MFT2.Text, "MF4");
            button_MFT2.Enabled = false;
            _Buttons.Add("MF4Trade2", button_MFT2);
        }

        private void button_MFT3_Click(object sender, EventArgs e)
        {
            openWindow("MF4Trade3", "【MF4】 " + button_MFT3.Text, "MF4");
            button_MFT3.Enabled = false;
            _Buttons.Add("MF4Trade2", button_MFT2);
        }

        private void button_V4T1_Click(object sender, EventArgs e)
        {
            openWindow("V4Trade1", "【V4】 " + button_V4T1.Text, "V4");
            button_V4T1.Enabled = false;
            _Buttons.Add("V4Trade1", button_V4T1);
        }

        private void button_V4T2_Click(object sender, EventArgs e)
        {
            openWindow("V4Trade2", "【V4】 " + button_V4T2.Text, "V4");
            button_V4T2.Enabled = false;
            _Buttons.Add("V4Trade2", button_V4T2);
        }

        private void button_V4T3_Click(object sender, EventArgs e)
        {
            openWindow("V4Trade3", "【V4】 " + button_V4T3.Text, "V4");
            button_V4T3.Enabled = false;
            _Buttons.Add("V4Trade3", button_V4T3);
        }

        private void button_MT4T1_Click(object sender, EventArgs e)
        {
            openWindow("MT4Trade1", "【MT4】 " + button_MT4T1.Text, "MT4");
            button_MT4T1.Enabled = false;
            _Buttons.Add("MT4Trade1", button_MT4T1);
        }
        private void button_MT4T2_Click(object sender, EventArgs e)
        {
            openWindow("MT4Trade2", "【MT4】 " + button_MT4T2.Text, "MT4");
            button_MT4T2.Enabled = false;
            _Buttons.Add("MT4Trade2", button_MT4T2);
        }

        private void button_MT4T3_Click(object sender, EventArgs e)
        {
            openWindow("MT4Trade3", "【MT4】 " + button_MT4T2.Text, "MT4");
            button_MT4T3.Enabled = false;
            _Buttons.Add("MT4Trade3", button_MT4T3);
        }
    }
}
