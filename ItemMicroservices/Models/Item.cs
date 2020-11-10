
namespace ItemMicroservices.Models
{
    public class Item
    {
        public int MPN { get; set; }
        public string ALT1Code { get; set; }
        public string ItemCategory { get; set; }
        public string ItemDescription { get; set; }
        public string Manufacturer { get; set; }
        public string Vendor { get; set; }
        public bool BulkPack { get; set; }
        public int BulkPackQuantity { get; set; }
        public string PreferredShippingMethod { get; set; }
        public bool OverpackRequired { get; set; }
        public double Weight { get; set; }
        public string SourcingGuideline { get; set; }
        public bool? StockingStatus533 { get; set; }
        public bool? StockingStatus423 { get; set; }
        public bool? StockingStatus761 { get; set; }
        public bool? StockingStatus2911 { get; set; }
        public bool? StockingStatus2920 { get; set; }
        public bool? StockingStatus474 { get; set; }
        public bool? StockingStatus986 { get; set; }
        public bool? StockingStatus321 { get; set; }
        public bool? StockingStatus625 { get; set; }
        public bool? StockingStatus688 { get; set; }
        public bool? StockingStatus796 { get; set; }
    }
}
