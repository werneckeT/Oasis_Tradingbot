using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Request;
using BinanceExchange.API.Models.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Oasis.Indicators
{
    class CandleData
    {
        public string coinPair;
        public KlineInterval interval;
        public List<KlineCandleStickResponse> candles;
        public Timer updateTimer;

        public CandleData(string coinPair, KlineInterval interval)
        {
            this.coinPair = coinPair;
            this.interval = interval;
            int minutes = Helper.IntervalToMinuteMatrix[interval];
            Update(0);
            this.updateTimer = new Timer(this.Update, null, (int)(60 * 1000 * MinToNextCall(interval)), 60 * 1000 * minutes);
        }

        public void Update(object obj)
        {
            try
            {
                candles = API.binanceClient.GetKlinesCandlesticks(new GetKlinesCandlesticksRequest 
                {
                    Symbol = coinPair,
                    Interval = interval
                }).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while receiving candles: " + ex.ToString());
            }
        }

        double MinToNextCall(KlineInterval interval)
        {
            if(interval == KlineInterval.OneMinute)
            {
                DateTime time = DateTime.UtcNow;
                time.AddSeconds(-time.Second);
                time.AddMinutes(1);

                TimeSpan delta = time - DateTime.UtcNow;
                return delta.TotalMinutes;
            }
            
            if (interval == KlineInterval.FifteenMinutes)
            {
                DateTime time = DateTime.UtcNow;
                time.AddSeconds(-time.Second);
                time.AddMinutes(15 - (time.Minute % 15));

                TimeSpan delta = time - DateTime.UtcNow;
                return delta.TotalMinutes;
            }
            
            if (interval == KlineInterval.OneHour)
            {
                DateTime time = DateTime.UtcNow;
                time.AddSeconds(-time.Second);
                time.AddMinutes(-time.Minute);
                time.AddHours(1);

                TimeSpan delta = time - DateTime.UtcNow;
                return delta.TotalMinutes;
            }
            
            if (interval == KlineInterval.FourHours)
            {
                DateTime time = DateTime.UtcNow;
                time.AddSeconds(-time.Second);
                time.AddMinutes(-time.Minute);
                time.AddHours(4 - (time.Hour % 4));

                TimeSpan delta = time - DateTime.UtcNow;
                return delta.TotalMinutes;
            }
            
            if (interval == KlineInterval.OneDay)
            {
                DateTime time = DateTime.UtcNow;
                time.AddSeconds(-time.Second);
                time.AddMinutes(-time.Minute);
                time.AddHours(-time.Hour);
                time.AddDays(1);

                TimeSpan delta = time - DateTime.UtcNow;
                return delta.TotalMinutes;
            }
            
            if (interval == KlineInterval.TwelveHours)
            {
                DateTime time = DateTime.UtcNow;
                time.AddSeconds(-time.Second);
                time.AddMinutes(-time.Minute);
                time.AddHours(12 - (time.Hour % 12));

                TimeSpan delta = time - DateTime.UtcNow;
                return delta.TotalMinutes;
            }
            
            throw new NotImplementedException("Interval Conversion not implemented");
        }
    }
}
