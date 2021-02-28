using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.WebSocket;

namespace Oasis.Indicators
{
    public class SimpleMovingAverage
    {
        private string coinPair;
        private KlineInterval interval;
        private int numberOfCandles;
        private int length;

        private List<decimal> averages = new List<decimal>();

        public SimpleMovingAverage(string coinPair, KlineInterval interval, int numberOfCandles, int length)
        {
            this.coinPair = coinPair;
            this.interval = interval;
            this.numberOfCandles = numberOfCandles;
            this.length = length;
            
            getAverages();
        }

        public List<decimal> getAverages()
        {
            if (averages != null)
            {
                averages.Clear();
            }
            var candleData = DataManager.GetCandleDataForPairAndInterval(coinPair, interval);
    
            if (candleData.candles.Count < numberOfCandles + length)
            {
                throw new Exception("ERROR: SMA 001");
            }
            
            for (int i = 0; i < length; i++)
            {
                decimal sum = 0m;
                for (int k = candleData.candles.Count - length - numberOfCandles + i; k < candleData.candles.Count -
                    length + i; k++)
                {
                    sum += candleData.candles[k].Close;
                }
                averages.Add(sum / numberOfCandles);
            }

            return averages;
        }
        /*
        public decimal GetMovingAverage()
        {
            var candleData = DataManager.GetCandleDataForPairAndInterval(coinPair, interval);
            decimal sum = 0;
            for (int i = candleData.candles.Count - numberOfCandles; i < candleData.candles.Count; i++)
            {
                sum += candleData.candles[i].Close;
            }
            return sum / numberOfCandles;
        }
        */
    }
}