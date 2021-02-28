using BinanceExchange.API.Client;
using BinanceExchange.API.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot;

namespace Oasis
{
    class API
    {
        public static BinanceClient binanceClient = null;
        public static TelegramBotClient telegramClient = null;

        public static Dictionary<IndicatorInfo, IndicatorsManager> indicatorsManagers =
            new Dictionary<IndicatorInfo, IndicatorsManager>();
        
        public static Config config;
        private static string configPath = Environment.CurrentDirectory + "\\config.json";
        public static void Init()
        {
            Console.WriteLine("Parsing config.json");
            
            // create file with standard values
            if (!File.Exists(configPath))
            {
                Config cfg = new Config();
                cfg.api_key = "Binance API Key here";
                cfg.secret_key = "Binance Secret Key here";
                cfg.telegram_key = "Telegram Key here";
                cfg.telegram_groups = new[] { 10000L, -10000L };
                cfg.coin_pairs = new[] { "BNBUSDT", "BTCUSDT" };
                cfg.intervals = new[] { KlineInterval.FifteenMinutes, KlineInterval.OneHour, KlineInterval.FourHours };
                cfg.default_divergence = 0.0005m;
                File.WriteAllText(configPath, JsonSerializer.Serialize<Config>(cfg));
                Helper.ExitWithMessage("No config file found. Created a new one please replace the values and try again.");
            }

            try
            {
                config = JsonSerializer.Deserialize<Config>(File.ReadAllText(Environment.CurrentDirectory + "\\config.json"));
                binanceClient = new BinanceClient(new ClientConfiguration { ApiKey = config.api_key, SecretKey = config.secret_key });
                telegramClient = new TelegramBotClient(config.telegram_key);

                foreach(var pair in config.coin_pairs)
                {
                    // Setup Data Tracking
                    DataManager.AddPriceTracking(pair);
                    Console.WriteLine("Added Price Tracker for " + pair);

                    foreach(var interval in config.intervals)
                    {
                        DataManager.AddCandleTracking(pair, interval);
                        Console.WriteLine("Added Candle Tracker for " + pair + " and Interval " + Enum.GetName(typeof(KlineInterval), interval));
                    }
                }

                DataManager.WaitForData();
                Console.WriteLine("Loaded Data Tracking");
            }
            catch(Exception ex)
            {
                Helper.ExitWithMessage("Exception: " + ex.Message);
            }
        } 
    }
}
