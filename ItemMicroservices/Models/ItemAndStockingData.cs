using System;
using System.Collections.Generic;
using System.Text;

namespace ItemMicroservices.Models
{
    public class ItemAndStockingData
    {
        public Dictionary<string, Item> itemDataDict { get; set; } = new Dictionary<string, Item>();

        public Dictionary<string, Dictionary<string, bool>> stockingStatusDict { get; set; } = new Dictionary<string, Dictionary<string, bool>>();
    }
}
