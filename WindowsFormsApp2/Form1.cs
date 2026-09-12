using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        private string _configFile = System.Windows.Forms.Application.StartupPath + "\\MyConfig.json";
        private Config _config;
        private Broker _Broker;
        private Account _Account;
        private string _BrokerName;
        private string _AccountType;
        private string _UserCode;
        private string _Password;
        private string _Symbol;
        private string _IP;
        private string _Port;
        public Form1()
        {
            InitializeComponent();
            InitialCombox();
            InitialConfig();
        }
        private void InitialCombox()
        {
            comboBox1.Text = "请选择品种";
            comboBox1.Items.Add("GOLD");
            comboBox1.Items.Add("LLG");
            comboBox1.SelectedIndex = 0;
        }
        private void InitialConfig()
        {
            _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(_configFile));
            //平台商
            if (_config.Broker != null && _config.Broker.Length > 0)
            {
                foreach (Broker bro in _config.Broker)
                {
                    comboBox_Broke.Items.Add(bro.BrokerName);
                }
            }
            comboBox_AccType.Items.Add("Demo");
            comboBox_AccType.Items.Add("Live");
            _Broker = GetBroker(_config.DefaultBroker);
            _Account = _Broker.DefaultAccount == "Demo" ? _Broker.DemoAccount : _Broker.LiveAccount;
            InitialPara();
            ///////////////////////////////
            comboBox_Broke.SelectedItem = _BrokerName;
        }
        private void InitialPara()
        {
            _BrokerName = _Broker.BrokerName;
            _AccountType = _Broker.DefaultAccount;
            _UserCode = _Account.UserCode;
            _Password = _Account.Password;
            _Symbol = _Broker.DefaultSymbol;
            _IP = _Account.IP;
            _Port = _Account.Port;
        }
        /**
         * 平台商选择
         */
        private void comboBox_Broke_SelectedIndexChanged(object sender, EventArgs e)
        {
            string BrokerName = comboBox_Broke.SelectedItem.ToString();
            _config.DefaultBroker = BrokerName;
            _Broker = GetBroker(BrokerName);
            _Account = GetAccount(BrokerName, _Broker.DefaultAccount);
            InitialPara();
            comboBox_AccType.SelectedItem = _AccountType;
            SetValues();
        }
        /**
         * 模拟、实盘选择
         */
        private void comboBox_AccType_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Account = GetAccount(_BrokerName, comboBox_AccType.SelectedItem.ToString());
            _Broker.DefaultAccount = comboBox_AccType.SelectedItem.ToString();
            SetValues();
        }


        /**
         * 从配置文件中获取平台商信息
         */
        private Broker GetBroker(string BrokerName)
        {
            Broker[] BroList = _config.Broker;
            if (BroList != null && BroList.Length > 0)
            {
                foreach (Broker bro in BroList)
                {
                    if (bro.BrokerName.Equals(BrokerName))
                    {
                        return bro;
                    }
                }
            }
            return null;

        }
        private Account GetAccount(string BrokerName, string TypeName)
        {
            Broker Bro = GetBroker(BrokerName);
            if (TypeName.Equals("Demo"))
            {
                return Bro.DemoAccount;
            }
            if (TypeName.Equals("Live"))
            {
                return Bro.LiveAccount;
            }
            return null;
        }
        private void SetValues()
        {
            textBox_UserCode.Text = _UserCode;
            textBox_Password.Text = _Password;
            textBox_IP.Text = _IP;
            textBox_Port.Text = _Port;
            comboBox_Symbol.Items.Clear();
            if (_Account.Symbol != null && _Account.Symbol.Length > 0)
            {
                foreach (string ss in _Account.Symbol)
                {
                    comboBox_Symbol.Items.Add(ss);
                }
            }
            comboBox_Symbol.SelectedItem = _Symbol;
        }
        private void SaveConfig()
        {
            try
            {
                _Account.UserCode = textBox_UserCode.Text;
                _Account.Password = textBox_Password.Text;
                _Account.IP = textBox_IP.Text;
                _Account.Port = textBox_Port.Text;
                if (comboBox_Symbol.Items == null)
                {
                    _Broker.DefaultSymbol = comboBox_Symbol.Text;
                }
                else
                {
                    _Broker.DefaultSymbol = comboBox_Symbol.SelectedText;
                }
                
                File.WriteAllText(_configFile, JsonConvert.SerializeObject(_config));
            }
            catch (Exception ex)
            {
            }
        }
        private void AddConfig()
        {
            try
            {
                Broker NewBroker = new Broker();
                NewBroker.BrokerName = "TestAdd";
                NewBroker.DefaultAccount = "Demo";
                NewBroker.DefaultSymbol = "";
                Account NewDemoAccount = new Account();
                NewDemoAccount.IP = textBox_IP.Text;
                NewDemoAccount.Port = textBox_Port.Text;
                NewDemoAccount.UserCode = "";
                NewDemoAccount.Password = "";
                NewDemoAccount.Symbol = new string[0];
                NewBroker.DemoAccount = NewDemoAccount;
                Account NewLiveAccount = new Account();
                NewLiveAccount.IP = textBox_IP.Text;
                NewLiveAccount.Port = textBox_Port.Text;
                NewLiveAccount.UserCode = "";
                NewLiveAccount.Password = "";
                NewLiveAccount.Symbol =new string [0];
                NewBroker.LiveAccount = NewLiveAccount;
                Broker[] NewBrokerList = new Broker[_config.Broker.Length + 1];
                for (int i = 0; i < _config.Broker.Length; i++)
                {
                    NewBrokerList[i] = _config.Broker[i];
                }
                NewBrokerList[_config.Broker.Length] = NewBroker;
                _config.Broker = NewBrokerList;
                File.WriteAllText(_configFile, JsonConvert.SerializeObject(_config));
            }
            catch (Exception ex)
            {
            }
        }

        private List<double> _bidDiff = new List<double>() { 0, 0, 0, 0, 0 };
        private void button1_Click(object sender, EventArgs e)
        {
            if (_Button_ChangeColor_Timer!=null)
            {
                _Button_ChangeColor_Timer.Enabled = false;
                _Button_ChangeColor_Timer.Stop();
                _Button_ChangeColor_Timer = null;
                button1.BackColor = (Color.Transparent);
            }
            _bidDiff.Insert(0, System.Convert.ToDouble(textBox1.Text));
            //_bidDiff.RemoveAt(5);
            label1.Text = System.Convert.ToString(_bidDiff[0]);
            label2.Text = System.Convert.ToString(_bidDiff[1]);
            label3.Text = System.Convert.ToString(_bidDiff[2]);
            label4.Text = System.Convert.ToString(_bidDiff[3]);
            label5.Text = System.Convert.ToString(_bidDiff[4]);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                MessageBox.Show(comboBox1.SelectedItem.ToString());
            }
            else 
            {
                MessageBox.Show("comboBox1 value is null");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            comboBox1.Text = "";
            comboBox1.Items.Add("MY_GOLD1");
            comboBox1.Items.Add("MY_GOLD1");
            comboBox1.SelectedIndex = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AddConfig();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            timer_Tick_Color();
        }
        private System.Timers.Timer _Button_ChangeColor_Timer;
        private void timer_Tick_Color()
        {
            if (_Button_ChangeColor_Timer==null) {
                _Button_ChangeColor_Timer = new System.Timers.Timer(500);
                _Button_ChangeColor_Timer.Enabled = true;
                _Button_ChangeColor_Timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Tick);
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {

            if (button1.BackColor == Color.Transparent)
            {
                button1.BackColor = Color.Red;
            }
            else
            {
                button1.BackColor = (Color.Transparent);
            }
        }
        Socket client;
        private void button5_Click(object sender, EventArgs e)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse("75.2.68.79");
            IPEndPoint point = new IPEndPoint(ip, 443);
            client.Connect(point);
            if (client.Connected)
            {
                ShowMsg("连接成功");
            }
            else 
            {
                ShowMsg("连接失败");
            }

            ShowMsg("服务器" + client.RemoteEndPoint.ToString());
            ShowMsg("客户端:" + client.LocalEndPoint.ToString());
            Thread th = new Thread(ReceiveMsg);
            th.IsBackground = true;
            th.Start();
            if (bgWorker.IsBusy!= true)
            {
                ShowMsg("bgWorker.IsBusy=false");
                //bgWorker.RunWorkerAsync();
            }
            else
            {
                ShowMsg("bgWorker.IsBusy=true");
            }
        }
        void ShowMsg(string msg)
        {
            //listBox1.Items.Add(msg + "\r\n");
            Console.WriteLine(msg);
        }
        void ReceiveMsg(object o)
        {
            while (true)
            {
                if (client.Connected)
                {
                    ShowMsg("连接正常");
                }
                else
                {
                    ShowMsg("连接失败");
                }
                byte[] buffer = new byte[1024 * 1024];
                ShowMsg("ReceiveBufferSize:" + client.ReceiveBufferSize);
                int n = client.Receive(buffer);
                string _data = Encoding.UTF8.GetString(buffer);
                Console.WriteLine("_data=" + _data);
                string words = Encoding.UTF8.GetString(buffer, 0, n);
                Console.WriteLine("words1="+ words);
                //ShowMsg(client.RemoteEndPoint.ToString() + ":" + words);
            }
        }
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //ShowMsg("worker_DoWork...");
            Console.WriteLine("worker_DoWork...");
            while (true)
            {
                Console.WriteLine("while...");
                byte[] buffer = new byte[1024 * 1024];
                int n = client.Receive(buffer);
                string words = Encoding.UTF8.GetString(buffer, 0, n);
                Console.WriteLine("words="+ words);
                //ShowMsg(client.RemoteEndPoint.ToString() + ":" + words);
                Console.WriteLine(client.RemoteEndPoint.ToString() + ":" + words);
            }
        }

        private void buttonSendSMS_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            double price = 0;
            double slippage = 0;
            double lot = 0.1;
            double dou = 0;
            string cha = "hello";
            string str = "world";
            int intt = 100;
            IntPtr ret = CppDll.test(ref price, slippage, lot);
            string res = Marshal.PtrToStringAnsi(ret);
            Console.WriteLine("res="+res);
            Console.WriteLine("price=" + price);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            String age1 = "38";
            IntPtr ptr3 = Marshal.StringToHGlobalAnsi(age1);
            //bool r1 = CppDll.Fun2("20");
            //Console.WriteLine("r1=" + r1);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //int r3 = CppDll.Fun3(1,2);
            //Console.WriteLine("r3=" + r3);
        }
        string sessionId = "";
        private void button9_Click(object sender, EventArgs e)
        {
            //Console.WriteLine("qiufu input");
            //int r4 = CppDll.Fun4("qiufu");
            //Console.WriteLine("r4=" + r4);
            //IntPtr temp = CppDll.getValue();
            //string res = Marshal.PtrToStringAnsi(temp).ToString();
            //temp = IntPtr.Zero;
            //Console.WriteLine("r4=" + res);

        }

        private void button10_Click(object sender, EventArgs e)
        {
            //byte[] userName = Encoding.ASCII.GetBytes("Chai");
            //byte[] password = Encoding.ASCII.GetBytes("123456");
            //byte[] accountType = Encoding.ASCII.GetBytes("Demo");
            //byte[] ip = Encoding.ASCII.GetBytes("203.160.75.182");
            //byte[] port = Encoding.ASCII.GetBytes("4523");



            string strIP = "203.160.75.182";
            string strUser = "35037";
            string strPass = "ab168168";
            CppDll.init(strIP, 4523);
            CppDll.SetInitParas("ba756498-d979-4109-9905-43d4abc3916c", "55f6a2b3-51a8-466a-99d2-45897bbf13fb", "4c1dba2d-6007-4df3-93cd-6cf61afd0332");
            //CppDll.myConnect();
            //CppDll.SetPathName("DEM");//设置登录模拟还是真实 YSG是真实 DEM是模拟
            CppDll.Connect();
            IntPtr ret = CppDll.Login(strUser, strPass,"DEM");//登录函数
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            Console.WriteLine("Login=" + res);
            /*
            IntPtr ret2 = CppDll.GetInitParas();
            string res2 = Marshal.PtrToStringAnsi(ret2).ToString();
            Console.WriteLine("GetInitParas=" + res2);
            IntPtr ret4 = CppDll.GetInitData();
            res = Marshal.PtrToStringAnsi(ret4).ToString();
            Console.WriteLine("GetInitData=" + res);
            IntPtr ret5 = CppDll.GetSettingData();
            res = Marshal.PtrToStringAnsi(ret5).ToString();
            Console.WriteLine("GetSettingData=" + res);             
             */

        }

        private void button11_Click(object sender, EventArgs e)
        {
            IntPtr temp = CppDll.GetInitData();
            string res = Marshal.PtrToStringAnsi(temp).ToString();
            Console.WriteLine("InitData=" + res);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            IntPtr temp = CppDll.GetSettingData();
            string res = Marshal.PtrToStringAnsi(temp).ToString();
            Console.WriteLine("GetSettingData=" + res);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            IntPtr temp = CppDll.GetTradingData();
            string res = Marshal.PtrToStringAnsi(temp).ToString();
            Console.WriteLine("GetTradingData=" + res);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            byte[] instrumentId = Encoding.ASCII.GetBytes("cb925195-5e79-43d2-96c4-3b8a330dada0");
            byte[] quotePolicyId = Encoding.ASCII.GetBytes("4c1dba2d-6007-4df3-93cd-6cf61afd0332");
            //IntPtr temp = CppDll.GetQuotation(instrumentId,quotePolicyId);
            //string res = Marshal.PtrToStringAnsi(temp).ToString();
            //Console.WriteLine("GetQuotation=" + res);
            IntPtr ret8 = CppDll.GetQuote();//查询行情报价
            string res = Marshal.PtrToStringAnsi(ret8).ToString();
            Console.WriteLine("GetChartQuotaion=" + res);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            IntPtr ret9 = CppDll.OpenBuyOrder("1987.34","0.1","0.1");
            string res = Marshal.PtrToStringAnsi(ret9).ToString();
            Console.WriteLine("OpenBuyOrder=" + res);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            IntPtr ret9 = CppDll.OpenSellOrder("1987.34", "0.1", "0.1");
            string res = Marshal.PtrToStringAnsi(ret9).ToString();
            Console.WriteLine("OpenSellOrder=" + res);
        }

        private void button15_Click_1(object sender, EventArgs e)
        {
            IntPtr ret9 = CppDll.CloseOrder("1987.34", "0.1",  "1987.34", "0.1", "fd377bd1-c71b-40ee-a3c7-783f04fd81d7", "2021-03-16 12:53:46", "True");
            string res = Marshal.PtrToStringAnsi(ret9).ToString();
            Console.WriteLine("OpenSellOrder=" + res);
        }

        private void DisConnect_Click(object sender, EventArgs e)
        {
            bool tes = CppDll.DisConnect();
            Console.WriteLine("DisConnect=" + tes);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            IntPtr ret8 = CppDll.GetQuote();
            string res = Marshal.PtrToStringAnsi(ret8).ToString();
            Console.WriteLine(res);
        }
    }
    //C#调用C++的DLL搜集整理的所有数据类型转换方式,可能会有重复或者多种方案,自己多测试
    //c++:HANDLE(void *) ---- c#:System.IntPtr 
    //c++:Byte(unsigned char) ---- c#:System.Byte 
    //c++:SHORT(short) ---- c#:System.Int16 
    //c++:WORD(unsigned short) ---- c#:System.UInt16 
    //c++:INT(int) ---- c#:System.Int16
    //c++:INT(int) ---- c#:System.Int32 
    //c++:UINT(unsigned int) ---- c#:System.UInt16
    //c++:UINT(unsigned int) ---- c#:System.UInt32
    //c++:LONG(long) ---- c#:System.Int32 
    //c++:ULONG(unsigned long) ---- c#:System.UInt32 
    //c++:DWORD(unsigned long) ---- c#:System.UInt32 
    //c++:DECIMAL ---- c#:System.Decimal 
    //c++:BOOL(long) ---- c#:System.Boolean 
    //c++:CHAR(char) ---- c#:System.Char 
    //c++:LPSTR(char *) ---- c#:System.String 
    //c++:LPWSTR(wchar_t *) ---- c#:System.String 
    //c++:LPCSTR(const char *) ---- c#:System.String 
    //c++:LPCWSTR(const wchar_t *) ---- c#:System.String 
    //c++:PCAHR(char *) ---- c#:System.String 
    //c++:BSTR ---- c#:System.String 
    //c++:FLOAT(float) ---- c#:System.Single 
    //c++:DOUBLE(double) ---- c#:System.Double 
    //c++:VARIANT ---- c#:System.Object 
    //c++:PBYTE(byte *) ---- c#:System.Byte[] 


    //c++:BSTR ---- c#:StringBuilder
    //c++:LPCTSTR ---- c#:StringBuilder
    //c++:LPCTSTR ---- c#:string
    //c++:LPTSTR ---- c#:[MarshalAs(UnmanagedType.LPTStr)] string 
    //c++:LPTSTR 输出变量名 ---- c#:StringBuilder 输出变量名
    //c++:LPCWSTR ---- c#:IntPtr
    //c++:BOOL ---- c#:bool  
    //c++:HMODULE ---- c#:IntPtr  
    //c++:HINSTANCE ---- c#:IntPtr 
    //c++:结构体 ---- c#:public struct 结构体{}; 
    //c++:结构体 **变量名 ---- c#:out 变量名 //C#中提前申明一个结构体实例化后的变量名
    //c++:结构体 &变量名 ---- c#:ref 结构体 变量名



    //c++:WORD ---- c#:ushort
    //c++:DWORD ---- c#:uint
    //c++:DWORD ---- c#:int


    //c++:UCHAR ---- c#:int
    //c++:UCHAR ---- c#:byte
    //c++:UCHAR* ---- c#:string
    //c++:UCHAR* ---- c#:IntPtr


    //c++:GUID ---- c#:Guid
    //c++:Handle ---- c#:IntPtr
    //c++:HWND ---- c#:IntPtr
    //c++:DWORD ---- c#:int
    //c++:COLORREF ---- c#:uint




    //c++:unsigned char ---- c#:byte
    //c++:unsigned char * ---- c#:ref byte
    //c++:unsigned char * ---- c#:[MarshalAs(UnmanagedType.LPArray)] byte[]
    //c++:unsigned char * ---- c#:[MarshalAs(UnmanagedType.LPArray)] Intptr


    //c++:unsigned char & ---- c#:ref byte
    //c++:unsigned char 变量名 ---- c#:byte 变量名
    //c++:unsigned short 变量名 ---- c#:ushort 变量名
    //c++:unsigned int 变量名 ---- c#:uint 变量名
    //c++:unsigned long 变量名 ---- c#:ulong 变量名


    //c++:char 变量名 ---- c#:byte 变量名 //C++中一个字符用一个字节表示,C#中一个字符用两个字节表示
    //c++:char 数组名[数组大小] ---- c#:MarshalAs(UnmanagedType.ByValTStr, SizeConst = 数组大小)] public string 数组名; ushort


    //c++:char * ---- c#:string //传入参数
    //c++:char * ---- c#:StringBuilder//传出参数
    //c++:char *变量名 ---- c#:ref string 变量名
    //c++:char *输入变量名 ---- c#:string 输入变量名
    //c++:char *输出变量名 ---- c#:[MarshalAs(UnmanagedType.LPStr)] StringBuilder 输出变量名


    //c++:char ** ---- c#:string
    //c++:char **变量名 ---- c#:ref string 变量名
    //c++:const char * ---- c#:string
    //c++:char[] ---- c#:string
    //c++:char 变量名[数组大小] ---- c#:[MarshalAs(UnmanagedType.ByValTStr,SizeConst=数组大小)] public string 变量名; 


    //c++:struct 结构体名 *变量名 ---- c#:ref 结构体名 变量名
    //c++:委托 变量名 ---- c#:委托 变量名


    //c++:int ---- c#:int
    //c++:int ---- c#:ref int
    //c++:int & ---- c#:ref int
    //c++:int * ---- c#:ref int //C#中调用前需定义int 变量名 = 0;


    //c++:*int ---- c#:IntPtr
    //c++:int32 PIPTR * ---- c#:int32[]
    //c++:float PIPTR * ---- c#:float[]



    //c++:double** 数组名 ---- c#:ref double 数组名
    //c++:double*[] 数组名 ---- c#:ref double 数组名
    //c++:long ---- c#:int
    //c++:ulong ---- c#:int

    //c++:UINT8 * ---- c#:ref byte //C#中调用前需定义byte 变量名 = new byte();  




    //c++:handle ---- c#:IntPtr
    //c++:hwnd ---- c#:IntPtr


    //c++:void * ---- c#:IntPtr  
    //c++:void * user_obj_param ---- c#:IntPtr user_obj_param
    //c++:void * 对象名称 ---- c#:([MarshalAs(UnmanagedType.AsAny)]Object 对象名称





    //c++:char, INT8, SBYTE, CHAR ---- c#:System.SByte  
    //c++:short, short int, INT16, SHORT ---- c#:System.Int16  
    //c++:int, long, long int, INT32, LONG32, BOOL , INT ---- c#:System.Int32  
    //c++:__int64, INT64, LONGLONG ---- c#:System.Int64  
    //c++:unsigned char, UINT8, UCHAR , BYTE ---- c#:System.Byte  
    //c++:unsigned short, UINT16, USHORT, WORD, ATOM, WCHAR , __wchar_t ---- c#:System.UInt16  
    //c++:unsigned, unsigned int, UINT32, ULONG32, DWORD32, ULONG, DWORD, UINT ---- c#:System.UInt32  
    //c++:unsigned __int64, UINT64, DWORDLONG, ULONGLONG ---- c#:System.UInt64  
    //c++:float, FLOAT ---- c#:System.Single  
    //c++:double, long double, DOUBLE ---- c#:System.Double  


    //Win32 Types ---- CLR Type  



    //Struct需要在C#里重新定义一个Struct
    //CallBack回调函数需要封装在一个委托里，delegate static extern int FunCallBack(string str);


    //unsigned char** ppImage替换成IntPtr ppImage
    //int& nWidth替换成ref int nWidth
    //int*, int&, 则都可用 ref int 对应
    //双针指类型参数，可以用 ref IntPtr
    //函数指针使用c++: typedef double (*fun_type1)(double); 对应 c#:public delegate double fun_type1(double);
    //char* 的操作c++: char*; 对应 c#:StringBuilder;
    //c#中使用指针:在需要使用指针的地方 加 unsafe




    //unsigned char对应public byte
    /*
    * typedef void (*CALLBACKFUN1W)(wchar_t*, void* pArg);
    * typedef void (*CALLBACKFUN1A)(char*, void* pArg);
    * bool BIOPRINT_SENSOR_API dllFun1(CALLBACKFUN1 pCallbackFun1, void* pArg);
    * 调用方式为
    * [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    * public delegate void CallbackFunc1([MarshalAs(UnmanagedType.LPWStr)] StringBuilder strName, IntPtr pArg);
    * 
    * 
    */
}
