using LiveCharts;
using LiveCharts.Defaults;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using uClient.Comm;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Series = System.Windows.Forms.DataVisualization.Charting.Series;

namespace uClient.Broker
{
    public class EAChart
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();
      
        /// <summary>
        /// 蜡烛图
        /// </summary>
        public LiveCharts.WinForms.CartesianChart cartesianChart_Candle;
        /// <summary>
        /// 进一天/汇总
        /// </summary>
        public Chart chart_daily;
        public Chart chart_price;
        //黄金变化率采用该图表
        public Chart chart_latest;
        public Chart chart_onetrade;


        /// <summary>
        /// 最新黄金汇总数据
        /// </summary>
        public double latestGoldSumValue;
        /// <summary>
        /// 最新黄金汇总数据列表
        /// </summary>
        public List<double> latestGoldSumValueList = new List<double>();
        /// <summary>
        /// 最新白银汇总数据
        /// </summary>
        public double latestSilverSumValue;
        /// <summary>
        /// 最新黄金分时数据
        /// </summary>
        public double latestGoldHourlyValue;
        /// <summary>
        /// 最新黄金分时数据列表
        /// </summary>
        public List<double> latestGoldHourlyValueList = new List<double>();

        /// <summary>
        /// 最新黄金近1天数据
        /// </summary>
        public double latestGoldDailyValue;
        /// <summary>
        /// 最新黄金近1天数据列表
        /// </summary>
        public List<double> latestGoldDailyValueList = new List<double>();




        /// <summary>
        /// 最新黄金白银变化率数据
        /// </summary>
        public double latestGSValue;
        /// <summary>
        /// 最新黄金白银变化率斜率,判断买入卖出反向
        /// 1: 黄金斜率比白银大, 黄金卖出,白银买入
        /// -1: 黄金斜率比白银小,黄金买入,白银卖出
        /// </summary>
        public string lastGSSlopeValue;
        /// <summary>
        /// 最新黄金白银开仓比例,以1手黄金为基准单位
        /// </summary>
        public string lastGSLotRatioValue;

        /// <summary>
        /// 最新黄金白银变化率数据列表
        /// </summary>
        public List<double> latestGSValueList = new List<double>();

        /// <summary>
        /// 最新黄金分时图表数据
        /// </summary>
        public List<IDictionary<string, string>> goldHourlyChartData;

        /// <summary>
        /// 最新黄金近1天图表数据
        /// </summary>
        public List<IDictionary<string, string>> goldDailyChartData;

        /// <summary>
        /// 最新黄金汇总图表数据
        /// </summary>
        public List<IDictionary<string, string>> goldSumChartData;

        /// <summary>
        /// EA配置数据
        /// </summary>
        public EAStrategyConfig _StrategyConfig;
        /// <summary>
        /// 价格区间长度
        /// </summary>
        public int _ChunkSize = 5;
        /// <summary>
        /// K线周期
        /// </summary>
        public string _KLine_Period = "ONE_HOUR";
        /// <summary>
        /// K线类型
        /// </summary>
        public string _KLine_Symbol = "XAUUSD";

        /// <summary>
        /// 斜率,控制开单
        /// </summary>
        public double _Slope = 0.2;

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="strategyConfig"></param>
        /// <param name="cartesianChart_Candle">蜡烛图</param>
        /// <param name="chart_daily">进一天数据/总览数据</param>
        /// <param name="chart_latest"></param>
        public EAChart(EAStrategyConfig strategyConfig, LiveCharts.WinForms.CartesianChart cartesianChart_Candle, Chart chart_daily, Chart chart_latest)
        {
            this._StrategyConfig = strategyConfig;
            this.cartesianChart_Candle = cartesianChart_Candle;
            this.chart_daily = chart_daily;
            //this.chart_price = chart_price;
            this.chart_latest = chart_latest;
            //this.chart_onetrade = chart_onetrade;
        }

        public void initChart()
        {
           
            //近1天数据
            chart_daily.Titles.Add("近1天");
            chart_daily.Titles[0].Alignment = ContentAlignment.TopCenter;
            chart_daily.Titles[0].ForeColor = Color.Black;
            chart_daily.Titles[0].Font = new Font("微软细黑", 8f, FontStyle.Regular);
            chart_daily.ChartAreas[0].AxisX.IsStartedFromZero = true;
            chart_daily.ChartAreas[0].AxisX.Interval = 1;
            chart_daily.ChartAreas[0].AxisY.IsStartedFromZero = true;
            chart_daily.ChartAreas[0].AxisY.Maximum = 100;
            chart_daily.ChartAreas[0].AxisX.LabelStyle.Angle = 30;

            //网格线设置为虚线
            chart_daily.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chart_daily.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            //图例放上面
            chart_daily.Legends[0].Docking = Docking.Top;
            chart_daily.Series.Clear();

            Series seriesDailyGold = new Series("近1天");
            chart_daily.Series.Add(seriesDailyGold);
            seriesDailyGold.ChartType = SeriesChartType.Spline;
            seriesDailyGold.Color = Color.Blue;
            //seriesDailyGold.IsValueShownAsLabel = true;
            seriesDailyGold.LabelForeColor = Color.Blue;

            Series seriesSumGold = new Series("总览");
            chart_daily.Series.Add(seriesSumGold);
            seriesSumGold.ChartType = SeriesChartType.Spline;
            seriesSumGold.Color = Color.Peru;
            //seriesDailyGold.IsValueShownAsLabel = true;
            seriesSumGold.LabelForeColor = Color.Peru;

            //实时交易行情
            //chart_latest.Titles.Add("实时行情");
            chart_latest.Titles.Add("黄金白银变化率");
            chart_latest.Titles[0].Alignment = ContentAlignment.TopCenter;
            chart_latest.Titles[0].ForeColor = Color.Black;
            chart_latest.Titles[0].Font = new Font("微软细黑", 10f, FontStyle.Regular);
            chart_latest.ChartAreas[0].AxisY.IsStartedFromZero = true;
            chart_latest.ChartAreas[0].AxisY.Maximum = 1;

            //网格线设置为虚线
            chart_latest.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chart_latest.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            //图例放上面
            chart_latest.Legends[0].Docking = Docking.Top;

            chart_latest.Series.Clear();

            //黄金白银变化率
            Series seriesGS = new Series("GS变化率");
            chart_latest.Series.Add(seriesGS);
            seriesGS.ChartType = SeriesChartType.Spline;
            seriesGS.Color = Color.BurlyWood;
            seriesGS.LabelForeColor = Color.BurlyWood;

            //蜡烛图
            //OhlcSeries seriesCandle = new OhlcSeries("Candle");
            //cartesianChart_Candle.Series.Clear();
            //cartesianChart_Candle.Series.Add(seriesCandle);
        }

        public void loadChartData()
        {
            //generateHourlyChart(null);
            generateDailyChart(null);
            generateGSChart(_ChunkSize,null);
            generateCandleChart(null);
        }
        /// <summary>
        /// 加载回测数据
        /// </summary>
        /// <param name="time"></param>
        public void loadTestChartData(DateTime time)
        {
            if (string.Equals(_StrategyConfig.analysisDataType, "D"))
            {
                generateDailyChart(time);
            }
            if (string.Equals(_StrategyConfig.analysisDataType, "H"))
            {
                generateCandleChart(time);
            }
            if (string.Equals(_StrategyConfig.analysisDataType, "K"))
            {
                generateCandleChart(time);
            }
        }
        public void clearChartData()
        {
            cartesianChart_Candle.Series.Clear();
            chart_daily.Series[0].Points.Clear();
            chart_daily.Series[1].Points.Clear();
            chart_latest.Series[0].Points.Clear();
        }


        /// <summary>
        /// 近一天数据
        /// </summary>
        public void generateDailyChart(DateTime? time)
        {
            //近一天数据
            Series seriesDailyGold = chart_daily.Series[0];
            //汇总数据
            Series seriesSumGold = chart_daily.Series[1];

            seriesDailyGold.Points.Clear();
            seriesSumGold.Points.Clear();
            latestGoldDailyValueList.Clear();
            latestGoldSumValueList.Clear();
            if (time == null)
            {
                goldDailyChartData = DSHelper.getDailyData(_StrategyConfig.DSType);
            }
            else
            {
                goldDailyChartData = DSHelper.getTestDailyData(_StrategyConfig.DSType, time.Value);
            }
            if (goldDailyChartData != null && goldDailyChartData.Count > 0)
            {
                int i = 1;
                foreach (IDictionary<string, string> data in goldDailyChartData)
                {

                    double buyValume = Convert.ToDouble(data["buy_valume"]);
                    double sellValume = Convert.ToDouble(data["sell_valume"]);
                    double p;
                    if (buyValume == 0 && sellValume == 0)
                    {
                        p = 50;
                    }
                    else
                    {
                        p = 100 * buyValume / (buyValume + sellValume);
                        p = double.Parse(Math.Round(p, 2).ToString());
                    }
                    latestGoldDailyValue = p;
                    latestGoldDailyValueList.Add(p);
                    seriesDailyGold.Points.AddXY(data["create_time"].Split(' ')[1], p);
                    i++;
                }
            }
            goldSumChartData = DSHelper.getSumData(_StrategyConfig.DSType);

            if (goldSumChartData != null && goldSumChartData.Count > 0)
            {
                int j = 1;
                foreach (IDictionary<string, string> data in goldSumChartData)
                {
                    double buyValume = 0;
                    double sellValume = 0;
                    if (data["buy_valume"] != "0" && data["sell_valume"] != "0")
                    {
                        buyValume = Convert.ToDouble(data["buy_valume"]);
                        sellValume = Convert.ToDouble(data["sell_valume"]);
                    }
                    double p = 100 * buyValume / (buyValume + sellValume);
                    p = double.Parse(Math.Round(p, 2).ToString());
                    latestGoldSumValue = p;
                    latestGoldSumValueList.Add(p);
                    seriesSumGold.Points.AddXY(data["create_time"].Split(' ')[1], p);
                    j++;
                }
            }
        }
        /// <summary>
        /// 黄金和白银变化趋势曲线
        /// </summary>
        /// <param name="chunkSize">价格区间长度</param>
        /// <param name="time"></param>
        public void generateGSChart(int chunkSize, DateTime? time)
        {
            Series seriesGSGold = chart_latest.Series[0];

            seriesGSGold.Points.Clear();
            latestGSValueList.Clear();
            //数据排序升序(从远到近)
            List<IDictionary<string, string>> priceData = new List<IDictionary<string, string>>();
            List<IDictionary<string, string>> silverChartData = new List<IDictionary<string, string>>();
            if (time == null)
            {
                priceData = DSHelper.getPrice((chunkSize*10).ToString(), "dukascopy", "gold");
            }
            else
            {
                priceData = DSHelper.getTestPrice((chunkSize * 10).ToString(), "dukascopy", "gold", time.Value);
            }            
            List<double> goldPrices = new List<double>();
            List<double> silverPrices = new List<double>();
            List<double> correlations = new List<double>();
            List<string> slopes = new List<string>();
            List<string> lotRatios = new List<string>();
            if (priceData != null && priceData.Count > 0)
            {                
                foreach (IDictionary<string, string> data in priceData)
                {
                    goldPrices.Add(Convert.ToDouble(data["gold_price"]));
                    silverPrices.Add(Convert.ToDouble(data["silver_price"]));
                }
            }
            //Console.WriteLine("Price List:" + string.Join(",", goldPrices));
            if (goldPrices.Count>100 && silverPrices.Count>100) 
            {
                int smoothingWindow = 3;  // 平滑窗口
                List<double> smoothedGold = ChangeRateUtils.MovingAverageRate(goldPrices, smoothingWindow);
                List<double> smoothedSilver = ChangeRateUtils.MovingAverageRate(silverPrices, smoothingWindow);
                List<List<double>> goldResultMovingAverage = SplitList(smoothedGold, chunkSize);
                List<List<double>> silverResultMovingAverage = SplitList(smoothedSilver, chunkSize);
                //List<List<double>> goldResultMovingAverage = SplitList(goldPrices, chunkSize);
                //List<List<double>> silverResultMovingAverage = SplitList(silverPrices, chunkSize);
                List<List<double>> goldResult = SplitList(goldPrices, chunkSize);
                //Console.WriteLine("List:"+ Utils.ListOfListsToString(goldResult));
                List<List<double>> silverResult = SplitList(silverPrices, chunkSize);
                if (goldResult.Count > 0 && silverResult.Count >0)
                {
                    for (int i = 0; i< goldResult.Count; i++)
                    {
                        //var goldRateOfChange = SlopeUtils.CalculateRateOfChange(goldResult[i]);
                        //var silverRateOfChange = SlopeUtils.CalculateRateOfChange(silverResult[i]);
                        //double correlation = PearsonCorrelationUtils.PearsonCorrelation(goldRateOfChange, silverRateOfChange);
                        double correlation = CalculateSpearmanCorrelationUtils.CalculateSpearmanCorrelation(goldResultMovingAverage[i], silverResultMovingAverage[i]);
                        string slope = Utils.DetermineTrendWithRegression(chunkSize ,_Slope, goldResult[i], silverResult[i]);                       
                        double lotRatio = TradeRatioCalculatorUtils.GetGSLotRatio(goldResult[i], silverResult[i]);
                        lotRatio = TradeRatioCalculatorUtils.GetGSLotRatio(goldResult[i], silverResult[i]);
                        double adjustedRatio = Math.Round(lotRatio, 2);
                        if (!double.IsNaN(correlation))
                        {
                            correlations.Add(System.Math.Round(correlation, 3));
                            slopes.Add(slope);
                            lotRatios.Add(adjustedRatio.ToString());
                            //_logger.Info("time:"+ time.ToString()+",slope:" + slope + ",correlation:" + correlation);
                        }
                    }                   
                }          
            }
            if (correlations.Count>0)
            {            
                //最新数据是最后一条数据
                for (int i = 0 ;i < correlations.Count; i++)
                {                    
                    latestGSValueList.Add(correlations[i]);
                    seriesGSGold.Points.AddXY(i+1, correlations[i]);
                }
                latestGSValue = correlations[correlations.Count-1];
                lastGSSlopeValue = slopes[correlations.Count - 1];
                lastGSLotRatioValue = lotRatios[correlations.Count - 1];
            }           
        }
        public void generateCandleChart(DateTime? time)
        {
            // 假设已有 List<PricePoint> 输入
            //List<PricePoint> priceData = GetSamplePriceData();
            //var candles = Utils.AggregatePriceData(priceData, intervalMinutes: 5);
            List<Candle> candles;
            if (time == null)
            {
                candles = DSHelper.getKline(_KLine_Period, _KLine_Symbol);
            }
            else 
            {
                candles = DSHelper.getTestKline(_KLine_Period, _KLine_Symbol, time.Value);
            }
            if (candles!=null && candles.Count>0) {
                var ohlcValues = new ChartValues<OhlcPoint>();
                var timeList = new List<DateTime>();

                foreach (var c in candles)
                {
                    ohlcValues.Add(new OhlcPoint((double)c.Open, (double)c.High, (double)c.Low, (double)c.Close));
                    timeList.Add(c.Time);
                }

                cartesianChart_Candle.Series = new LiveCharts.SeriesCollection
                {
                    new LiveCharts.Wpf.OhlcSeries
                    {
                        Values = ohlcValues,
                        Title = "价格"
                    }
                };

                cartesianChart_Candle.AxisX.Clear();
                cartesianChart_Candle.AxisY.Clear();
                cartesianChart_Candle.AxisX.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "时间",
                    LabelsRotation = 45,
                    LabelFormatter = value =>
                    {
                        int index = (int)value;
                        if (index >= 0 && index < timeList.Count)
                            return timeList[index].ToString("HH:mm");
                        return "";
                    }
                });

                //cartesianChart_Candle.AxisY.Clear();
                /*cartesianChart_Candle.AxisY.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "价格"
                });*/
            }
        }
        public static List<List<double>> SplitList(List<double> inputList, int chunkSize)
        {
            List<List<double>> chunks = new List<List<double>>();
            for (int i = 0; i + chunkSize <= inputList.Count; i += chunkSize)
            {
                var chunk = inputList.GetRange(i, chunkSize);
                chunks.Add(chunk);
            }
            return chunks;
        }
        public static List<List<T>> ChunkListFromEnd<T>(List<T> source, int chunkSize)
        {
            if (chunkSize <= 0)
                throw new ArgumentException("chunkSize must be greater than 0");

            List<List<T>> result = new List<List<T>>();

            // 计算第一个块开始的索引，确保最后一块是完整的
            int start = source.Count - (source.Count % chunkSize == 0 ? chunkSize : source.Count % chunkSize) - chunkSize;

            for (int i = source.Count - chunkSize; i >= 0; i -= chunkSize)
            {
                result.Add(source.GetRange(i, chunkSize));
            }

            // 因为我们是从后往前加的，需要反转一下最终结果顺序（使得顺序是从尾到头）
            result.Reverse();
            return result;
        }
    }
}
