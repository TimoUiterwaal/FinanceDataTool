using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceDataTool
{
    internal class Portfolio
    {
        public required String Symbol { get; set; }
        public double? Change { get; set; }
        public double? PercentageChange { get; set; }
        public double? HighPrice { get; set; }
        public double? LowPrice { get; set; }
        public double? OpenPrice { get; set; }
        public double? PreviousClose { get; set; }
        public double? CurrentPrice { get; set; }
        public long? Timestamp { get; set; }


    }
}
