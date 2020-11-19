
using System.Collections.Generic;

namespace ItemMicroservices.Models
{
    public class Item
    {
        public string MPN { get; set; }
        public string AltCode { get; set; }
        public string ItemCategory { get; set; }
        public string ItemDescription { get; set; }
        public string Manufacturer { get; set; }
        public string Vendor { get; set; }
        public bool BulkPack { get; set; }
        public int BulkPackQuantity { get; set; }
        public string PreferredShippingMethod { get; set; }
        public bool Overpack { get; set; }
        public double Weight { get; set; }
        public string SourcingGuideline { get; set; }
    }
}
