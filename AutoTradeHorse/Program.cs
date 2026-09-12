using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using uClient.Broker;
using uClient.Comm;

namespace DataLoad
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
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
                paras = new string[5] { "数据采集平台", "【MT4】MT5Trade1 ", "MT4", "", "" };
                //paras = new string[5] { "MF4Trade1", "【MF4】SUI", "MF4", "SUI", "SUI" };
            }
            MainFormPara mainFormPara = new MainFormPara();
            mainFormPara.FormNo = paras[0];
            mainFormPara.FormName = paras[1];
            mainFormPara.FormType = "";
            mainFormPara.PlatformCode = paras[2];
            mainFormPara.BrokerCode = paras[3];
            mainFormPara.BrokerName = paras[4];
            Application.Run(new AutoTradeForm(mainFormPara));
        }
    }
}
