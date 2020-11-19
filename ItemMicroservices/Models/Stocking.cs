using System;
using System.Collections.Generic;
using System.Text;

namespace ItemMicroservices.Models
{
    public class Stocking
    {
        public string MPN { get; set; }

        public string BranchNumber { get; set; }

        public string StockingStatus { get; set; }
    }
}
