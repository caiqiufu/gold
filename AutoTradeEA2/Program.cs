using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Forms;
using uClient.Comm;

namespace uClient.Broker
{
    internal static class Program
    {
        /// <summary>
        /// 设置窗体显示
        /// </summary>
        /// <param name="hWnd">窗体句柄</param>
        /// <param name="fAltTab">是否显示</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        //static void Main()
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new EAForm());
        //}


        /// <summary>
        /// 应用程序的主入口点,可以启动多个实例
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string[] paras = null;
            if (args != null && args.Length > 0)
            {
                string str = args[0].Trim();
                paras = str.Split(',');
            }
            else
            {
                //MF4Trade1,【MF4】SUI,MF4,SUI,SUI
                paras = new string[5] { "策略分析平台", "【MT4】MT4Trade1 ", "MT4", "", "" };
                paras = new string[5] { "策略分析平台", "【MT5】MT5Trade1 ", "MT5", "", "" };
                //paras = new string[5] { "MF4Trade1", "【MF4】SUI", "MF4", "SUI", "SUI" };
            }
            MainFormPara mainFormPara = new MainFormPara();
            mainFormPara.FormNo = paras[0];
            mainFormPara.FormName = paras[1];
            mainFormPara.FormType = "";
            mainFormPara.PlatformCode = paras[2];
            mainFormPara.BrokerCode = paras[3];
            mainFormPara.BrokerName = paras[4];
            Application.Run(new EAForm(mainFormPara));
        }
        /// <summary>
        /// 获取前主窗体序列化文件对象
        /// </summary>
        /// <returns></returns>
        public static IntPtr GetMainFormSerialize()
        {
            try
            {
                //文件路径
                string filePath = "MainFormSerialize";
                //判断文件是否存在
                if (File.Exists(filePath))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        IntPtr hWnd = (IntPtr)bf.Deserialize(fs);
                        return hWnd;
                    }
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
            return new IntPtr();
        }
    }
}
