using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace uClient
{
    public class GcXauFusionEngine
    {
        // =========================
        // 1. 数据结构
        // =========================
        public class MarketTick
        {
            public DateTime Time { get; set; }
            public double Price { get; set; }
        }

        // =========================
        // ⭐ 2. 信号类型（新增）
        // =========================
        public enum TradeSignal
        {
            None,
            Buy,
            Sell,
            Exit
        }

        private TradeSignal currentPosition = TradeSignal.None;
        private DateTime lastTradeTime = DateTime.MinValue;

        // =========================
        // 3. 内部缓存
        // =========================
        private MarketTick latestGC;
        private MarketTick latestXAU;

        private readonly object lockObj = new object();

        // =========================
        // 4. Z-Score buffer
        // =========================
        private readonly int windowSize = 100;
        private readonly Queue<double> deviationBuffer = new Queue<double>();

        private double mean;
        private double std;

        // =========================
        // 5. 参数
        // =========================
        private double carry = 10;
        private bool running = false;

        // =========================
        // ⭐ 6. 信号事件（升级）
        // =========================
        public event Action<TradeSignal, double, double, double, double, double> OnSignal;
        // (signal, gc, xau, fair, deviation, zscore)

        // =========================
        // 7. 数据输入
        // =========================
        public void UpdateGC(double price)
        {
            lock (lockObj)
            {
                latestGC = new MarketTick
                {
                    Time = DateTime.UtcNow,
                    Price = price
                };
            }
        }

        public void UpdateXAU(double price)
        {
            lock (lockObj)
            {
                latestXAU = new MarketTick
                {
                    Time = DateTime.UtcNow,
                    Price = price
                };
            }
        }

        // =========================
        // 8. 启动引擎
        // =========================
        public void Start()
        {
            running = true;
            Task.Run(ProcessLoop);
        }

        public void Stop()
        {
            running = false;
        }

        // =========================
        // 9. 主循环
        // =========================
        private void ProcessLoop()
        {
            while (running)
            {
                MarketTick gc;
                MarketTick xau;

                lock (lockObj)
                {
                    gc = latestGC;
                    xau = latestXAU;
                }

                if (gc == null || xau == null)
                {
                    Thread.Sleep(10);
                    continue;
                }

                // 时间同步
                if (Math.Abs((gc.Time - xau.Time).TotalMilliseconds) > 200)
                {
                    Thread.Sleep(5);
                    continue;
                }

                // Fair Price
                double gcSpot = gc.Price - carry;
                double fairPrice = 0.7 * gcSpot + 0.3 * xau.Price;

                // Deviation
                double deviation = xau.Price - fairPrice;

                // ZScore
                double z = AddZScore(deviation);

                // =========================
                // ⭐ 信号检测（新增核心）
                // =========================
                var signal = DetectSignal(z);

                if (signal != TradeSignal.None)
                {
                    OnSignal?.Invoke(signal, gc.Price, xau.Price, fairPrice, deviation, z);

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {signal} | Z={z:F2} Dev={deviation:F2}");
                }

                Thread.Sleep(10);
            }
        }

        // =========================
        // ⭐ 10. 信号检测逻辑
        // =========================
        private TradeSignal DetectSignal(double zScore)
        {
            double entryThreshold = 2.0;
            double exitThreshold = 0.5;

            // 防止过于频繁交易（10秒）
            if ((DateTime.Now - lastTradeTime).TotalSeconds < 10)
                return TradeSignal.None;

            // ===== 开仓 =====
            if (currentPosition == TradeSignal.None)
            {
                if (zScore > entryThreshold)
                {
                    currentPosition = TradeSignal.Sell;
                    lastTradeTime = DateTime.Now;
                    return TradeSignal.Sell;
                }

                if (zScore < -entryThreshold)
                {
                    currentPosition = TradeSignal.Buy;
                    lastTradeTime = DateTime.Now;
                    return TradeSignal.Buy;
                }
            }

            // ===== 平仓 =====
            if (currentPosition == TradeSignal.Buy && zScore > -exitThreshold)
            {
                currentPosition = TradeSignal.None;
                lastTradeTime = DateTime.Now;
                return TradeSignal.Exit;
            }

            if (currentPosition == TradeSignal.Sell && zScore < exitThreshold)
            {
                currentPosition = TradeSignal.None;
                lastTradeTime = DateTime.Now;
                return TradeSignal.Exit;
            }

            return TradeSignal.None;
        }

        // =========================
        // 11. Z-Score
        // =========================
        private double AddZScore(double value)
        {
            deviationBuffer.Enqueue(value);

            if (deviationBuffer.Count > windowSize)
                deviationBuffer.Dequeue();

            mean = deviationBuffer.Average();

            double variance = deviationBuffer
                .Select(x => Math.Pow(x - mean, 2))
                .Average();

            std = Math.Sqrt(variance);

            if (std == 0)
                return 0;

            return (value - mean) / std;
        }
    }
}
