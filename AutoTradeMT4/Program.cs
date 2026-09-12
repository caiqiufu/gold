using System;
using System.Windows.Forms;
using uClient.Comm;

namespace uClient.Broker
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
                }else
                {
                    paras = new string[5] { "MT4Trade1", "【MT4】MT4Trade1 ", "MT4", "", "" };
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
