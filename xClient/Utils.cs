using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using System.Threading.Tasks;
using NLog.Fluent;

namespace xClient
{
    class Utils
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        //public static StreamWriter sw = null;
        //public static FileStream fs;
        public static void AddMsgToTXT(string path,string fileName, string msg)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string destFile = System.IO.Path.Combine(path, fileName);
            if (!File.Exists(destFile))
            {
                //报错xxxxxxxx...because it is being used by another process.创建完后关闭文件即可。
                File.Create(destFile).Close();
            }


            //FileStream fs = new FileStream(destFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
            FileStream fs = File.OpenWrite(destFile);
            try {
                // 将待写的入数据从字符串转换为字节数组
                Encoding encoder = Encoding.UTF8;
                byte[] bytes = encoder.GetBytes(msg + "\r\n");
                //设定书写的开始位置为文件的末尾  
                fs.Position = fs.Length;
                //将待写入内容追加到文件末尾  
                fs.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
            }
            finally
            {
                fs.Close();
            }
            //sw.WriteLine(msg);
            //sw.Flush();
        }
        public static string GenerateDirectoryPath(string baseDirectory)
        {
            System.DateTime currentTime = System.DateTime.Now;
            string directory = currentTime.ToString("yyyyMMdd");
            string path = AppDomain.CurrentDomain.BaseDirectory+"\\"+ baseDirectory + "\\" + directory;
            return path;
        }
        public static void AddTradeMsg(string msg,string AccountName)
        {
            string path = GenerateDirectoryPath("TradeInfo");
            System.DateTime currentTime = System.DateTime.Now;
            string fileName = currentTime.ToString("yyyy-MM-dd")+"_"+ AccountName;
            AddMsgToTXT(path, fileName+".txt",msg);
        }
        public static Boolean AutoTrade(StrategySymbol Strategy,double Change,double Duration,string OptType)
        {
            if ("Buy".Equals(OptType))
            {
                double ExecuteValue = Change * Convert.ToDouble(Strategy.BuyClose.ChangeW) + (Duration / 10) * Convert.ToDouble(Strategy.BuyClose.DurationW);
                if (ExecuteValue >= Convert.ToDouble(Strategy.TradePara.BuyClose)&& Duration>=Convert.ToDouble(Strategy.BuyClose.Duration))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }else
            {
                double ExecuteValue = Change * Convert.ToDouble(Strategy.SellClose.ChangeW) + (Duration / 10) * Convert.ToDouble(Strategy.SellClose.DurationW);
                if (ExecuteValue >= Convert.ToDouble(Strategy.TradePara.SellClose) && Duration >= Convert.ToDouble(Strategy.BuyClose.Duration))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }            
        }
    }
}
