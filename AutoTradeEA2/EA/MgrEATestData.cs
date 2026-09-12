using System;
using System.Collections.Generic;
using uClient.Comm;

namespace uClient.Broker
{
    /// <summary>
    /// 构建EA测试数据
    /// </summary>
    public class MgrEATestData
    {
        /// <summary>
        /// 开仓区间
        /// </summary>
        private static int _OpenDuration = 6;
        /// <summary>
        /// 平仓区间
        /// </summary>
        private static int _CloseDuration = 5;
        /// <summary>
        /// 测试偏移量,定义为每一个偏差间隔,如42->40 偏差间隔为2
        /// </summary>
        private static int _Offset = 2;

        /// <summary>
        /// Hourly 针对Daily的偏移量,
        /// </summary>
        private static int _HourlyOffset = 0;
        /// <summary>
        /// 开仓上限
        /// </summary>
        private static int _OPEN_MAX = 40;
        /// <summary>
        /// 开仓上限
        /// </summary>
        private static int _OPEN_MIN = 30;
        /// <summary>
        /// 平仓上限
        /// </summary>
        private static int _CLOSE_MAX = 45;
        /// <summary>
        /// 平仓下限
        /// </summary>
        private static int _CLOSE_MIN = 25;
        private static bool _IS_DAILY = true;
        private static bool _IS_HOURLY = true;


        /// <summary>
        /// 设置初始化数据
        /// </summary>
        /// <param name="openMax">开仓上限值</param>
        /// <param name="openDuration">开仓区间(上限值->减去区间值)</param>
        /// <param name="closeDuration">平仓区间值(上限值->开仓上限值+平仓区间,下限值->开仓下限值-平仓区间)</param>
        /// <param name="offset">偏移量,参数变化范围,如openMax->openMax-offset即为开仓上限值偏移范围</param>
        /// <param name="hourlyOffset">针对Daily的偏移量</param>
        public static void SetInitData(int openMax, int openDuration, int closeDuration, int offset , int hourlyOffset)
        {
            _OpenDuration = openDuration;
            _CloseDuration = closeDuration;
            _Offset = offset;
            _HourlyOffset = hourlyOffset;
            _IS_DAILY = true;
            _IS_HOURLY = true;
            _OPEN_MAX = openMax;           
            _OPEN_MIN = _OPEN_MAX - _OpenDuration;
            _CLOSE_MAX = _OPEN_MAX + _CloseDuration;
            _CLOSE_MIN = _OPEN_MIN - _CloseDuration;
        }
        /// <summary>
        /// 根据配置参数生成策略数据
        /// </summary>
        /// <returns></returns>
        public static IList<Dictionary<string, EAEntity>> GenerateEAEntityList()
        {
            //IList<Dictionary<string, EAEntity>> EAEntityList = new List<Dictionary<string, EAEntity>>();
            int arrSize = 3;
            int[] oMax =new int[arrSize];
            for (int i = 0; i < arrSize; i++)
            {
                oMax[i] = _OPEN_MAX- _Offset*i;
            }
            int[] oDuration = new int[arrSize];
            for (int i = 0; i < arrSize; i++)
            {
                oDuration[i] = _OpenDuration + i* _Offset;
            }
            int[] cDuration = new int[arrSize];
            for (int i = 0; i < arrSize; i++)
            {
                cDuration[i] = _CloseDuration + i* _Offset;
            }
            return GenerateEAEntityList(oMax, oDuration, cDuration);
        }
        /// <summary>
        /// 40, 38, 36,34
        /// </summary>
        /// <returns></returns>
        public static IList<Dictionary<string, EAEntity>> GenerateEAEntityList1()
        {
            IList<Dictionary<string, EAEntity>> EAEntityList = new List<Dictionary<string, EAEntity>>();

            int[] oMax = { 40, 38, 36,34};//开仓上限值
            //int[] oMin = new int[oMax.Length];
            int[] oDuration = { 8, 10, 12 };//开仓区间长度
            int[] cDuration = { 6, 8, 10 };//平仓区间长度

            return GenerateEAEntityList(oMax, oDuration, cDuration);           
        }
        /// <summary>
        /// 42, 40, 38, 36
        /// </summary>
        /// <returns></returns>
        public static IList<Dictionary<string, EAEntity>> GenerateEAEntityList2()
        {
            IList<Dictionary<string, EAEntity>> EAEntityList = new List<Dictionary<string, EAEntity>>();

            int[] oMax = { 42, 40, 38, 36};//开仓最大值
            //int[] oMin = new int[oMax.Length];
            int[] oDuration = { 8, 10, 12 };//开仓区间
            int[] cDuration = { 6, 8, 10 };//平仓区间
            return GenerateEAEntityList(oMax, oDuration, cDuration);            
        }
        /// <summary>
        /// 生成EA2策略参数
        /// </summary>
        /// <param name="oMinValue"></param>
        /// <param name="cMinValue"></param>
        /// <param name="oMaxValue"></param>
        /// <param name="cMaxValue"></param>
        /// <param name="k"></param>
        /// <param name="l"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double[] GenerateEA2Para(double oMinValue,double cMinValue,double oMaxValue,double cMaxValue,int k, int l, int m)
        {
             double openGs = 0.2 - (oMinValue + cMinValue + (k + 1) * (l + 1) * (m + 1)) * 0.001;
             double closeGs = 0.85 + (oMaxValue + cMaxValue + (k + 1) * (l + 1) * (m + 1)) * 0.001;
             if (openGs <0)
             {
                 openGs = 0.006;
             }
             if (closeGs>1)
             {
                 closeGs = 0.998;
             }
             openGs = Math.Round(openGs, 3);
             closeGs = Math.Round(closeGs, 3);
             return new double[2]{ openGs,closeGs };
 
        }
        /// <summary>
        /// 通用策略生成器
        /// </summary>
        /// <param name="oMax">开仓上限值 oMax = { 42, 40, 38, 36 }</param>
        /// <param name="oDuration">开仓区间长度 oDuration = { 8, 10, 12 }</param>
        /// <param name="cDuration">平仓区间长度 cDuration = { 6, 8, 10 }</param>
        /// <returns></returns>
        public static IList<Dictionary<string, EAEntity>> GenerateEAEntityList(int[] oMax, int[] oDuration, int[] cDuration)
        {
            IList<Dictionary<string, EAEntity>> EAEntityList = new List<Dictionary<string, EAEntity>>();

            //int[] oMax = { 42, 40, 38, 36 };
            //int[] oMin = new int[oMax.Length];
            //int[] oDuration = { 8, 10, 12 };
            //int[] cDuration = { 6, 8, 10 };
            string[] openDurationList = new string[oMax.Length * oDuration.Length];
            for (int i = 0; i < oMax.Length; i++)
            {
                for (int j = 0; j < oDuration.Length; j++)
                {
                    openDurationList[i * oDuration.Length + j] = oMax[i] + "," + (oMax[i] - oDuration[j]);
                }
            }
            for (int k = 0; k < openDurationList.Length; k++)
            {
                string openDuration = openDurationList[k];
                int oMaxValue = int.Parse(openDuration.Split(',')[0]);
                int oMinValue = int.Parse(openDuration.Split(',')[1]);
                for (int l = 0; l < cDuration.Length; l++)
                {
                    int cMaxValue = oMaxValue + cDuration[l];
                    for (int m = 0; m < cDuration.Length; m++)
                    {
                        int cMinValue = oMinValue - cDuration[m];
                        //平仓下限偏移
                        EAEntity buy = new EAEntity();
                        EAEntity sell = new EAEntity();
                        EAEntity buyClose = new EAEntity();
                        EAEntity sellClose = new EAEntity();
                        buy.code = "BUY";
                        buy.name = "买多";
                        buy.active = true;
                        buyClose.code = "CLOSE_BUY";
                        buyClose.name = "买平";
                        buyClose.active = true;
                        sell.code = "SELL";
                        sell.name = "卖空";
                        sell.active = true;
                        sellClose.code = "CLOSE_SELL";
                        sellClose.name = "卖平";
                        sellClose.active = true;
                        if (_IS_DAILY)
                        {
                            buy.daily = oMaxValue;
                            buy.daily1 = oMinValue;
                            buyClose.daily = cMaxValue;
                            buyClose.daily1 = cMinValue;
                            sell.daily = oMaxValue;
                            sell.daily1 = oMinValue;
                            sellClose.daily = cMaxValue;
                            sellClose.daily1 = cMinValue;

                            double[] eaParas = GenerateEA2Para(oMinValue, cMinValue, oMaxValue, cMaxValue, k, l, m);
                            buy.gs = eaParas[0];
                            buy.gs1 = eaParas[0];
                            buyClose.gs = eaParas[1];
                            buyClose.gs1 = eaParas[1];

                            sell.gs = eaParas[0];
                            sell.gs1 = eaParas[0];
                            sellClose.gs = eaParas[1];
                            sellClose.gs1 = eaParas[1];

                        }
                        if (_IS_HOURLY)
                        {
                            buy.hourly = oMaxValue - _HourlyOffset;
                            buy.hourly1 = oMinValue - _HourlyOffset;
                            buyClose.hourly = cMaxValue - _HourlyOffset;
                            buyClose.hourly1 = cMinValue - _HourlyOffset;
                            sell.hourly = oMaxValue - _HourlyOffset;
                            sell.hourly1 = oMinValue - _HourlyOffset;
                            sellClose.hourly = cMaxValue - _HourlyOffset;
                            sellClose.hourly1 = cMinValue - _HourlyOffset;

                            double[] eaParas = GenerateEA2Para(oMinValue, cMinValue, oMaxValue, cMaxValue, k, l, m);
                            buy.gs = eaParas[0];
                            buy.gs1 = eaParas[0];
                            buyClose.gs = eaParas[1];
                            buyClose.gs1 = eaParas[1];

                            sell.gs = eaParas[0];
                            sell.gs1 = eaParas[0];
                            sellClose.gs = eaParas[1];
                            sellClose.gs1 = eaParas[1];
                        }
                        Dictionary<string, EAEntity> eAEntityDic = new Dictionary<string, EAEntity>();
                        eAEntityDic.Add("BUY", buy);
                        eAEntityDic.Add("SELL", sell);
                        eAEntityDic.Add("CLOSE_BUY", buyClose);
                        eAEntityDic.Add("CLOSE_SELL", sellClose);
                        EAEntityList.Add(eAEntityDic);
                    }
                }
            }
            return EAEntityList;
        }
        /// <summary>
        /// 生成进近1天测试数据
        /// </summary>
        public static void GenerateDailyTestData()
        {
            Random buyValumeRandom = new Random(Guid.NewGuid().GetHashCode()); // 使用GUID作为种子
            int buyValume = buyValumeRandom.Next(0,9999);
            Random sellValumeRandom = new Random(Guid.NewGuid().GetHashCode()); // 使用GUID作为种子
            int sellValume = sellValumeRandom.Next(0, 9999);
            string pt = DateTime.Now.ToString();
            DBHelper.SaveDailyData(pt, buyValume, sellValume);
        }
        /// <summary>
        /// 生成进小时测试数据
        /// </summary>
        /// <param name="time"></param>
        /// <param name="pt"></param>
        public static void GenerateHourlyTestData(string pt)
        {
            Random buyValumeRandom = new Random(Guid.NewGuid().GetHashCode()); // 使用GUID作为种子
            int buyValume = buyValumeRandom.Next(0, 9999);
            Random sellValumeRandom = new Random(Guid.NewGuid().GetHashCode()); // 使用GUID作为种子
            int sellValume = sellValumeRandom.Next(0, 9999);
            DBHelper.SaveHourlyData(sellValume.ToString(), pt, buyValume, sellValume);
        }
    }
}