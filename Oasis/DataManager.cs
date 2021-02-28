using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Response;
using Oasis.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Oasis
{
    class DataManager
    {
        private static List<CandleData> candleDatas = new List<CandleData>();
        private static Dictionary<string, decimal> priceTrackers = new Dictionary<string, decimal>();
        
        // Update priceTrackers every minute
        private static Timer priceUpdateTimer = new Timer((obj) =>
        {
            try
            {
                var allPrices = API.binanceClient.GetAllPrices().GetAwaiter().GetResult();

                foreach (var priceTracker in priceTrackers.Keys.ToList())
                {
                    var response = allPrices.Find(x => x.Symbol == priceTracker);

                    if (response == null)
                        throw new Exception("Unable to get Price for Symbol " + priceTracker);
                    priceTrackers[priceTracker] = response.Price;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while receiving prices: " + ex.ToString());
            }
        }, null, 0, 5000);

        public static void ForceCandleUpdate(string pair, KlineInterval interval)
        {
            CandleData data = candleDatas.Find(x => x.coinPair == pair && x.interval == interval);
            if (data == null)
                throw new Exception("Tried to update Candle Data for Pair and or Interval that isn't existing.");
            data.Update(null);
        }
        
        public static void AddCandleTracking(string pair, KlineInterval interval)
        {
            if (candleDatas.Find(x => x.coinPair == pair && x.interval == interval) != null)
                throw new Exception("Tried to add candle tracking but tracker already exists for " + pair + ".");

            candleDatas.Add(new CandleData(pair, interval));
        }
        public static void AddPriceTracking(string pair)
        {
            if (priceTrackers.ContainsKey(pair))
                throw new Exception("Tried to add price tracking but tracker already exists for " + pair + ".");

            priceTrackers.Add(pair, 0);
        }

        public static void WaitForData()
        {
            bool found = false;

            while (!found)
            {
                bool isFilled = true;

                foreach (var v in priceTrackers)
                {
                    if (v.Value == 0m)
                        isFilled = false;
                }

                if (isFilled)
                    found = true;

                Thread.Sleep(10);
            }

            bool initializedCandles = false;
            bool isFilled2 = true;
            while (!initializedCandles)
            {
                foreach (var v in candleDatas)
                {
                    if (v.candles.Count <= 0)
                        isFilled2 = false;
                }

                if (isFilled2)
                    initializedCandles = true;

                Thread.Sleep(10);
            }
        }

        public static decimal GetPrice(string pair)
        {
            if (!priceTrackers.ContainsKey(pair))
                throw new Exception("Tried to get price for untracked pair: " + pair + ".");

            return priceTrackers[pair];
        }

        public static CandleData GetCandleDataForPairAndInterval(string pair, KlineInterval interval)
        {
            CandleData data = candleDatas.Find(x => x.coinPair == pair && x.interval == interval);
            
            if (data == null)
                throw new Exception("Tried to get Candle Data for Pair and or Interval that isn't existing.");

            return data;
        }

    }
}
