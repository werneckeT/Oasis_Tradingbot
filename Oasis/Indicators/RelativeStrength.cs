using System.Collections.Generic;
using System.Linq;
using BinanceExchange.API.Enums;
using log4net.Util;

namespace Oasis.Indicators
{
    public class RelativeStrength
    {
        private string coinPair;
        private int numberOfCandles;
        private KlineInterval interval;

        public RelativeStrength(string coinPair, int numberOfCandles, KlineInterval interval)
        {
            this.coinPair = coinPair;
            this.numberOfCandles = numberOfCandles;
            this.interval = interval;
        }

        public decimal getStrengthSimple()
        {
            var data = DataManager.GetCandleDataForPairAndInterval(coinPair, interval);
            List<decimal> positiveMoves = new List<decimal>();
            List<decimal> negativeMoves = new List<decimal>();
            for (int i = data.candles.Count - numberOfCandles; i < data.candles.Count; i++)
            {
                if (data.candles[i - 1].Close < data.candles[i].Close)
                {
                    positiveMoves.Add(data.candles[i].Close - data.candles[i - 1].Close);
                    negativeMoves.Add(0m);
                }else if (data.candles[i - 1].Close == data.candles[i].Close)
                {
                    positiveMoves.Add(0m);
                    negativeMoves.Add(0m);
                }
                else
                {
                    positiveMoves.Add(0m);
                    negativeMoves.Add(data.candles[i - 1].Close - data.candles[i].Close);
                }
            }

            var averageUpMove = positiveMoves.Sum() / positiveMoves.Count;
            var averageMoveDown = negativeMoves.Sum() / negativeMoves.Count;

            var rs = averageUpMove / averageMoveDown;

            return 100 - (100/(1 + rs));
        }

        public decimal getStrengthExponential()
        {
            return 0m;
        }
        public decimal getStrengthSmooth()
        {
            var smoothingFactor = 1 / numberOfCandles;
            return 0m;
        }
    }
}