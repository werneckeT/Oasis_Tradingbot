using System.Collections.Generic;
using BinanceExchange.API.Enums;
using Oasis.Indicators;

namespace Oasis
{
    class IndicatorInfo
    {
        public string pair;
        public KlineInterval interval;
    }
    public class IndicatorsManager
    {
        private string coinPair;
        private KlineInterval interval;
        public SimpleMovingAverage sma;
        public TrendlineIndicator tli;
        public ResistanceAnalysation resistanceAnalysation;
        public Volume vol;

        public IndicatorsManager(string coinPair, KlineInterval interval)
        {
            this.coinPair = coinPair;
            this.interval = interval;
            resistanceAnalysation = new ResistanceAnalysation(coinPair, interval);
            tli = new TrendlineIndicator(coinPair, interval, 100);
            sma = new SimpleMovingAverage(coinPair, interval, 9, 100);
            vol = new Volume(coinPair, interval, 100);
        }

        public void drawImage()
        {
            ImageCreator.Graph(coinPair, interval, tli.getFilteredTrendlines(), sma.getAverages(), vol.getVolumes(), resistanceAnalysation.GetResistances());
        }
    }
}