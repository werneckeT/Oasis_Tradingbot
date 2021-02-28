using System;
using Oasis.Indicators;
using System.Threading;
using BinanceExchange.API.Enums;

namespace Oasis
{
    class Program
    {
        static void Main(string[] args)
        {
            // Basic Setup
            Console.Title = "Oasis Trading Bot";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Booting Oasis");
            API.Init();

            //var rs = new RelativeStrength("BTCUSDT", 14, KlineInterval.OneHour);
            //Console.Write(rs.getStrengthSimple());

            foreach (var pair in API.config.coin_pairs)
            {

                foreach (var interval in API.config.intervals)
                {
                    //new TrendlineIndicator(pair, interval, 100);
                    IndicatorsManager IDCM = new IndicatorsManager(pair, interval);
                    IndicatorInfo info = new IndicatorInfo();
                    info.pair = pair;
                    info.interval = interval;
                    API.indicatorsManagers.Add(info, IDCM);
                    IDCM.drawImage();
                }
            }
            
            ImageCreator.CreateChart();
            
            while(true)
                Console.ReadLine();
        }
    }
}
