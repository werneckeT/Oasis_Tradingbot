using BinanceExchange.API.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oasis
{
    class Config
    {
        public String api_key { get; set; }
        public String secret_key { get; set; }
        public String telegram_key { get; set; }
        public long[] telegram_groups { get; set; }
        public String[] coin_pairs { get; set; }
        public KlineInterval[] intervals { get; set; }
        
        public decimal default_divergence { get; set; }
    }
}
