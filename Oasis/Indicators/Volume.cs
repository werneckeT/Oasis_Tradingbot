using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BinanceExchange.API.Enums;

namespace Oasis.Indicators
{
    public class Volume
    {
        private string coinPair;
        private KlineInterval interval;
        private int length;

        private List<decimal> volumeList = new List<decimal>();

        public Volume(string coinPair, KlineInterval interval, int length)
        {
            this.coinPair = coinPair;
            this.interval = interval;
            this.length = length;
        }

        public List<decimal> getVolumes()
        {
            if (volumeList.Count > 0)
            {
             volumeList.Clear();   
            }
            var data = DataManager.GetCandleDataForPairAndInterval(coinPair, interval);
            for (var i = data.candles.Count - length; i < data.candles.Count; i++)
            {
                volumeList.Add(data.candles[i].Volume);
            }
            return volumeList;
        }
    }
}