using NLog;
using System.Text;



namespace AutoHorseRace.Utils
{
    /// <summary>
    /// DB 操作
    /// </summary>
    public class DBHelper
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 获取系统配置参数
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> getSysConfig()
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT sc.name,sc.value FROM unieap_sys_config sc where sc.activate_flag = '{0}'", "Y");
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            Dictionary<string, string> data = new Dictionary<string, string>();
            if (datas != null && datas.Count > 0)
            {
                foreach (var dataItem in datas)
                {
                    data.Add(dataItem["name"], dataItem["value"]);
                }
            }
            return data;
        }
    }
}
