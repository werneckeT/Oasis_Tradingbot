using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BinanceExchange.API.Enums;

namespace Oasis.Indicators
{
    public class TrendPoint
    {
        public int candle;
        public decimal value;

        public TrendPoint(decimal value, int candle)
        {
            this.candle = candle;
            this.value = value;
        }
    }
    
    
    public class Trendline
    {
        public decimal movement;
        public decimal n;
        public int startpoint;
        private KlineInterval interval;
        public List<TrendPoint> linePoints;
        public int candleDataSize;

        public Trendline(decimal m, decimal n, int startpoint, KlineInterval interval, List<TrendPoint> linePoints, int candleDataSize)
        {
            this.movement = m;
            this.n = n;
            this.startpoint = startpoint;
            this.interval = interval;
            this.linePoints = linePoints;
            this.candleDataSize = candleDataSize;
        }
    }
    public class TrendlineIndicator
    {
        private string coinPair;
        private KlineInterval interval;
        private int candleNumber;
        static List<Trendline> alertedTrends = new List<Trendline>();

        private Timer analyseTimer;

        private List<Trendline> validTrendlines;
        public TrendlineIndicator(string coin, KlineInterval interval, int numberOfCandles)
        {
            this.coinPair = coin;
            this.interval = interval;
            this.candleNumber = numberOfCandles;
            analyseTimer = new Timer((obj) => { AnalyseCoin(); }, null, 0, 1000 * 10);
        }

        public List<Trendline> getValidTrendlines()
        {
            if (validTrendlines == null)
            {
                this.AnalyseCoin();
            }
            return validTrendlines.ToList();
        }

        public List<Trendline> getFilteredTrendlines()
        {
            /*TODO
             Check if Trendline is up or down -> positiv movement = bottom line, negativ movement = top line
             
             */
            List<Trendline> filteredTrendlines = new List<Trendline>();
            var validTrendlines = getValidTrendlines();

            for (int i = 0; i < validTrendlines.Count; i++)
            {
                var useable = true;
                if (filteredTrendlines.Count > 0)
                {
                    for (int j = 0; j < filteredTrendlines.Count; j++)
                    {
                        if (filteredTrendlines[j].movement < 0)
                        {
                            if (validTrendlines[i].movement < 0)
                            {
                                if (validTrendlines[i].linePoints[validTrendlines[i].linePoints.Count - 1].value ==
                                    filteredTrendlines[j].linePoints[filteredTrendlines[j].linePoints.Count - 1].value)
                                {
                                    useable = false;
                                } 
                            }
                        }
                        else
                        {
                            if (validTrendlines[i].movement > 0)
                            {
                                if (validTrendlines[i].linePoints[validTrendlines[i].linePoints.Count - 1].value ==
                                    filteredTrendlines[j].linePoints[filteredTrendlines[j].linePoints.Count - 1].value)
                                {
                                    useable = false;
                                } 
                            }
                        }
                    }
                }

                if (useable)
                {
                    filteredTrendlines.Add(validTrendlines[i]);
                }
            }
            return filteredTrendlines;
        }

        private void AnalyseCoin()
        {
            var data = DataManager.GetCandleDataForPairAndInterval(coinPair, interval);
            var divergence = Helper.GetDivergence(coinPair, interval);
            var currentPrice = DataManager.GetPrice(coinPair);
            var validTrendlinesTop = GetTopLines();
            var validTrendlinesBottom = GetBottomLines();
            
            //TODO: Add Trend usage; Add Candle difference check
            validTrendlines = validTrendlinesTop.Concat(validTrendlinesBottom).ToList();
            //ImageCreator.Graph(coinPair, interval, validTrendlinesTop.Concat(validTrendlinesBottom).ToList());
            //ImageCreator.Graph(coinPair, interval, validTrendlinesTop);
            
            foreach (var tl in validTrendlinesTop)
            {
                if (alertedTrends.Find(x => x.movement == tl.movement) == null)
                {
                    var resistancePrice = tl.movement * tl.candleDataSize + tl.n;
                    if (resistancePrice < currentPrice - divergence)
                    {
                        var breakoutAlert = "📈🌒🚀🚀🚀🌒\nNew Breakout LONG: " + coinPair + "\n at " + Enum.GetName(typeof(KlineInterval), interval) + "\nResistance Price: " + resistancePrice + "\nCurrentPrice: " + currentPrice + "\n📈🌒🚀🚀🚀🌒";
                        //Trendline breakout
                        foreach (var group in API.config.telegram_groups)
                        {
                            //API.telegramClient.SendTextMessageAsync(group, breakoutAlert);
                        }

                        alertedTrends.Add(tl);
                    }
                }
            }


            foreach(var tl in validTrendlinesBottom)
            {
                if(alertedTrends.Find(x => x.movement == tl.movement) == null)
                {
                    var resistancePrice = tl.movement * tl.candleDataSize + tl.n;
                    if(resistancePrice > currentPrice + divergence)
                    {
                        string breakoutAlert = "📉🐻\nNew Breakout SHORT: " + coinPair + "\n at " + Enum.GetName(typeof(KlineInterval), interval) + "\nResistance Price: " + resistancePrice + "\nCurrentPrice: " + currentPrice + "\n📉🐻";
                        //Trendline breakout
                        foreach (long group in API.config.telegram_groups)
                        {
                            //API.telegramClient.SendTextMessageAsync(group, breakoutAlert);
                        }

                        alertedTrends.Add(tl);
                    }
                }
            }
        }

        private List<Trendline> GetTopLines()
        {
            var data = DataManager.GetCandleDataForPairAndInterval(coinPair, interval);
            var divergence = Helper.GetDivergence(coinPair, interval);
            List<Trendline> trendlines = new List<Trendline>();

            for(var i = data.candles.Count - this.candleNumber; i < data.candles.Count - 3; i++)
            {
                for(var j = i + 1; j < data.candles.Count - 2; j++)
                {
                    var m = GetMovement(i, j, data.candles[i].High, data.candles[j].High);
                    var n = GetN(i, j, data.candles[i].High, data.candles[j].High);

                    Trendline tl = new Trendline(m, n, i, interval, new List<TrendPoint>(), data.candles.Count);
                    tl.linePoints.Add(new TrendPoint(data.candles[i].High,i));
                    tl.linePoints.Add(new TrendPoint(data.candles[j].High,j));

                    for (int k = j + 1; k < data.candles.Count - 1; k++)
                    {
                        if(Math.Abs((m*k + n) - data.candles[k].High) <= divergence)
                        {
                            tl.linePoints.Add(new TrendPoint(data.candles[k].High,k));
                        }
                    }
                    if(tl.linePoints.Count > 2)
                    {
                        trendlines.Add(tl);
                    }
                }
            }
            return RemoveBadTrendlinesTop(data, trendlines, divergence);
        }

        private List<Trendline> GetBottomLines()
        {
            CandleData data = DataManager.GetCandleDataForPairAndInterval(coinPair, interval);
            decimal divergence = Helper.GetDivergence(coinPair, interval);
            List<Trendline> trendlines = new List<Trendline>();

            for(int i = data.candles.Count - this.candleNumber; i < data.candles.Count - 3; i++)
            {
                for (int j = i + 1; j < data.candles.Count - 2; j++)
                {
                    decimal movement = GetMovement(i, j, data.candles[i].Low, data.candles[j].Low);
                    decimal n = GetN(i, j, data.candles[i].Low, data.candles[j].Low);

                    Trendline tl = new Trendline(movement, n, i, interval, new List<TrendPoint>(), data.candles.Count);
                    tl.linePoints.Add(new TrendPoint(data.candles[i].Low,i));
                    tl.linePoints.Add(new TrendPoint(data.candles[j].Low,j));

                    for(int k = j + 1; k < data.candles.Count - 1; k++)
                    {
                        if(Math.Abs((movement * k + n) - data.candles[k].Low) <= divergence )
                        {
                            tl.linePoints.Add(new TrendPoint(data.candles[k].Low,k));
                        }
                    }
                    if(tl.linePoints.Count > 2)
                    {
                        trendlines.Add(tl);
                    }
                }
            }
            return RemoveBadTrendlinesBottom(data, trendlines, divergence);
        }

        private static List<Trendline> RemoveBadTrendlinesTop(CandleData data, List<Trendline> trendlist, decimal divergence)
        {
            List<Trendline> usefullTrendlines = new List<Trendline>();

            foreach(Trendline tl in trendlist.ToArray())
            {
                //-1 da der letzte Candle der nochnicht abgeschlossene ist
                bool usefull = true;
                for(int i = tl.startpoint; i < data.candles.Count - 1; i++)
                {
                    if(data.candles[i].High - divergence > tl.movement * i + tl.n)
                    {
                        usefull = false;
                        break;
                    }
                }
                if(usefull)
                {
                    if (tl.linePoints[0].value > tl.linePoints[tl.linePoints.Count - 1].value)
                    {
                        usefullTrendlines.Add(tl);
                    }
                }
            }
            return usefullTrendlines;
        }

        private static List<Trendline> RemoveBadTrendlinesBottom(CandleData data, List<Trendline> trendlist, decimal divergence)
        {
            var usefullTrendlines = new List<Trendline>();

            foreach(var tl in trendlist.ToArray())
            {
                var usefull = true;
                for (var i = tl.startpoint; i < data.candles.Count - 1; i++)
                {
                    if (data.candles[i].Low + divergence < tl.movement * i + tl.n)
                    {
                        usefull = false;
                        break;
                    }
                }
                if (usefull)
                {
                    if (tl.linePoints[0].value < tl.linePoints[tl.linePoints.Count - 1].value)
                    {
                        usefullTrendlines.Add(tl);
                    }
                }
            }
            return usefullTrendlines;
        }

        private static decimal GetMovement(decimal x1, decimal x2, decimal y1, decimal y2)
        {
            if( x2 == x1)
            {
                return 0;
            }
            return (y2 - y1) / (x2 - x1);
        }

        private static decimal GetN(decimal x1, decimal x2, decimal y1, decimal y2)
        {
            if (x2 == x1)
            {
                return 0;
            }
            return (x2 * y1 - x1 * y2) / (x2 - x1);
        }

        /**
         * Returns the Trend from the Trendline start to the current Price in percent
         */
        private static decimal GetTrenddirection(string coinPair, Trendline tl)
        {
            return tl.linePoints.Count > 0 ? (100 * DataManager.GetPrice(coinPair)) / tl.linePoints[0].value : 0;
        }
    }
}
