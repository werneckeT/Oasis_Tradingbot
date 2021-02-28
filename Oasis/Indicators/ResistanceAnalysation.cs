using BinanceExchange.API.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Oasis.Indicators;
using Telegram.Bot.Types.InputFiles;

namespace Oasis
{
    public class Resistance
    {
        public Resistance(decimal Value, int ConfirmationCandles)
        {
            this.Value = Value;
            this.ConfirmationCandles = ConfirmationCandles;
        }

        public decimal Value = 0;
        public int ConfirmationCandles = 0;
    }

    public class ResistanceAnalysation
    {
        private Timer updateResistanceTimer;
        private string coinPair;
        private KlineInterval interval;
        private List<Resistance> resistances;
        private decimal lastPrice = 0;

        public List<Resistance> GetResistances()
        {
            List<Resistance> resistances = new List<Resistance>();
            decimal divergence = Helper.GetDivergence(coinPair, interval);
            var c = DataManager.GetCandleDataForPairAndInterval(coinPair, interval).candles;

            for (int i = 400; i < 500; i++)
            {
                var a = c[i];
                bool hasBeenFound = false;
                foreach (Resistance r in resistances)
                {
                    decimal deltaHigh = Math.Abs(r.Value - a.High);
                    decimal deltaLow = Math.Abs(r.Value - a.Low);

                    if (deltaHigh < divergence)
                    {
                        hasBeenFound = true;
                        r.ConfirmationCandles += 1;
                    }
                    else if (deltaLow < divergence)
                    {
                        hasBeenFound = true;
                        r.ConfirmationCandles += 1;
                    }
                }

                // Has not been found insert a new one
                if (!hasBeenFound)
                {
                    resistances.Add(new Resistance(a.Low, 1));
                }
            }

            resistances.RemoveAll(r => r.ConfirmationCandles < 3);

            foreach (var resistance in resistances.ToList())
            {
                foreach (var resistance2 in resistances.ToList())
                {
                    decimal delta = Math.Abs(resistance.Value - resistance2.Value);
                    if (delta < divergence * 10m && delta != 0)
                    {
                        if (resistance.ConfirmationCandles > resistance2.ConfirmationCandles)
                        {
                            resistances.RemoveAll(x => x.Value == resistance2.Value);
                        }
                        else if (resistance.ConfirmationCandles < resistance2.ConfirmationCandles)
                        {
                            resistances.RemoveAll(x => x.Value == resistance.Value);
                        }
                    }
                }
            }

            return resistances;
        }


        private void UpdateResistances(object obj)
        {
            resistances = GetResistances();
            decimal divergence = Helper.GetDivergence(coinPair, interval);
            decimal price = DataManager.GetPrice(coinPair);

            resistances.Sort((l, r) =>
            {
                return ((Math.Abs(l.Value - price)) < (Math.Abs(r.Value - price)) ? -1 : 1);
            });
            
            var resistance = resistances[0];
            
            if (lastPrice > resistance.Value && price < (resistance.Value - divergence))
            {
                lastPrice = price;

                List<Resistance> resistancesSorted = new List<Resistance>(resistances);

                resistancesSorted.Sort((l, r) =>
                {
                    return ((Math.Abs(l.Value - price)) < (Math.Abs(r.Value - price)) ? -1 : 1);
                });

                resistancesSorted.RemoveAll(x => x.Value == resistance.Value || x.Value > price);

                string breakoutAlert = "📉🐻\nNew Breakout SHORT: " + coinPair + "\n at " +
                                       Enum.GetName(typeof(KlineInterval), interval) + "\nBroke Resistance at " +
                                       resistance.Value + "\n";

                if (resistancesSorted.Count > 0)
                    breakoutAlert += "Next Resistance at " + resistancesSorted[0].Value + "\n";

                breakoutAlert += "📉🐻";
                //Trendline breakout
                IndicatorsManager manager = null;

                foreach (var ind in API.indicatorsManagers)
                {
                    if (ind.Key.pair == coinPair && ind.Key.interval == interval)
                        manager = ind.Value;
                }
                
                DataManager.ForceCandleUpdate(coinPair, interval);
                foreach (long group in API.config.telegram_groups)
                {
                    API.telegramClient.SendPhotoAsync(group,
                        new InputOnlineFile(new MemoryStream(ImageCreator.Graph(coinPair,interval, manager.tli.getFilteredTrendlines(), manager.sma.getAverages(), manager.vol.getVolumes(), resistances))), breakoutAlert);
                }
            }

            if (lastPrice < resistance.Value && price > (resistance.Value + divergence))
            {
                lastPrice = price;

                List<Resistance> resistancesSorted = new List<Resistance>(resistances);

                resistancesSorted.Sort((l, r) =>
                {
                    return ((Math.Abs(l.Value - price)) < (Math.Abs(r.Value - price)) ? -1 : 1);
                });


                resistancesSorted.RemoveAll(x => x.Value == resistance.Value || x.Value < price);

                var breakoutAlert = "📈🌒🚀🚀🚀🌒\nNew Breakout LONG: " + coinPair + "\n at " +
                                    Enum.GetName(typeof(KlineInterval), interval) + "\nBroke Resistance at: " +
                                    resistance.Value + "\n";

                if (resistancesSorted.Count > 0)
                    breakoutAlert += "Next Resistance at " + resistancesSorted[0].Value + "\n";
                breakoutAlert += "📈🌒🚀🚀🚀🌒";
                
                IndicatorsManager manager = null;

                foreach (var ind in API.indicatorsManagers)
                {
                    if (ind.Key.pair == coinPair && ind.Key.interval == interval)
                        manager = ind.Value;
                }
                
                DataManager.ForceCandleUpdate(coinPair, interval);
                
                //Trendline breakout
                foreach (long group in API.config.telegram_groups)
                {
                    API.telegramClient.SendPhotoAsync(group,
                        new InputOnlineFile(new MemoryStream(ImageCreator.Graph(coinPair,interval, manager.tli.getValidTrendlines(), manager.sma.getAverages(), manager.vol.getVolumes(), resistances))), breakoutAlert);
                }
            }


        }

        public ResistanceAnalysation(string pair, KlineInterval interval)
        {
            this.coinPair = pair;
            this.interval = interval;
            this.lastPrice = DataManager.GetPrice(pair);
            // Update Resistances every 5 Minutes
            this.updateResistanceTimer = new Timer(this.UpdateResistances, null, 0, 5 * 1000);
        }
    }
}
