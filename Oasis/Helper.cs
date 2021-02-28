using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BinanceExchange.API.Enums;

namespace Oasis
{
    class Helper
    {
        public static Dictionary<KlineInterval, int> IntervalToMinuteMatrix = new Dictionary<KlineInterval, int>()
        {
            {KlineInterval.OneMinute, 1},
            {KlineInterval.ThreeMinutes, 3},
            {KlineInterval.FiveMinutes, 5},
            {KlineInterval.FifteenMinutes, 15},
            {KlineInterval.ThirtyMinutes, 30},
            {KlineInterval.OneHour, 60},
            {KlineInterval.TwoHours, 120},
            {KlineInterval.FourHours, 240},
            {KlineInterval.EightHours, 480},
            {KlineInterval.TwelveHours, 720},
            {KlineInterval.OneDay, 1440},
            {KlineInterval.ThreeDays, 4320},
            {KlineInterval.OneWeek, 10080},
            {KlineInterval.OneMonth, 43200},
        };

        public static void ExitWithMessage(string message)
        {
            Console.WriteLine(message);
            Console.ReadLine();
            Environment.Exit(-1);
        }

        private static Dictionary<KlineInterval, decimal> IntervalDivergenceMultiplicator =
            new Dictionary<KlineInterval, decimal>()
            {
                {KlineInterval.OneMinute, 1},
                {KlineInterval.ThreeMinutes, 1.2m},
                {KlineInterval.FiveMinutes, 1.5m},
                {KlineInterval.FifteenMinutes, 2},
                {KlineInterval.ThirtyMinutes, 2.5m},
                {KlineInterval.OneHour, 2.85m},
                {KlineInterval.TwoHours, 3m},
                {KlineInterval.FourHours, 3.25m},
                {KlineInterval.EightHours, 3.5m},
                {KlineInterval.TwelveHours, 3.75m},
                {KlineInterval.OneDay, 4m},
                {KlineInterval.ThreeDays, 4.5m},
                {KlineInterval.OneWeek, 5m},
                {KlineInterval.OneMonth, 5m},
            };

        public static decimal GetDivergence(string coinpair, KlineInterval interval)
        {
            var defaultDivergence = API.config.default_divergence;
            var currentPrice = DataManager.GetPrice(coinpair);
            return currentPrice * defaultDivergence * IntervalDivergenceMultiplicator[interval];
        }
    }
}
