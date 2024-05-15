using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CryptoPortfolioService_WebRole.Models
{
    public class CryptoCurrencyListItem
    {
        public string Symbol1 { get; set; }
        public string Symbol2 { get; set; }
        public double MinLotSize { get; set; }
        public double MinLotSize2 { get; set; }
        public double MaxLotSize { get; set; }
        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
    }
}