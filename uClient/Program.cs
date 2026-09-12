using System;
using System.Windows.Forms;
using uClient.Comm;

namespace uClient
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                string[] paras = null;
                if (args != null && args.Length > 0)
                {
                    string str = args[0].Trim();
                    paras = str.Split(',');
                }
                else
                {
                    //MF4Trade1,【MF4】SUI,MF4,SUI,SUI
                    //MT5Trade1,【MT5】mingtakfn,MT5,fnmarkets,mingtakfn
                    paras = new string[5] { "MT5Trade1", "【MT5】MT5Trade1 ", "MT5", "", "" };
                    //paras = new string[5] { "MT4Trade1", "【MT4】MT4Trade1", "MT4", "MT4", "MT4" };
                    //paras = new string[5] { "MF4Trade1", "【MF4】SUI", "MF4", "SUI", "SUI" };
                }
                MainFormPara mainFormPara = new MainFormPara();
                mainFormPara.FormNo = paras[0];
                mainFormPara.FormName = paras[1];
                mainFormPara.FormType = "";
                mainFormPara.PlatformCode = paras[2];
                mainFormPara.BrokerCode = paras[3];
                mainFormPara.BrokerName = paras[4];
                Application.Run(new TradeForm(mainFormPara));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
