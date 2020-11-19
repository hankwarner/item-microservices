//using Xunit;
//using ItemMicroservices.Services;
//using ItemMicroservices.Models;
//using System.Collections.Generic;

//namespace ItemMicroservices.Test
//{
//    public class ItemDataServicesTests
//    {
//        private static ItemDataServices itemServices = new ItemDataServices();
//        private Item testItem1 = new Item()
//        {
//            MPN = 37572,
//            ItemCategory = "Garbage Disposals",
//            Manufacturer = "InSinkErator",
//            BulkPack = false,
//            BulkPackQuantity = 0,
//            PreferredShippingMethod = "Ground",
//            Weight = 13.96,
//            SourcingGuideline = "FEI",
//            StockingStatus533 = true,
//            StockingStatus423 = true,
//            StockingStatus761 = true,
//            StockingStatus2911 = true,
//            StockingStatus2920 = true,
//            StockingStatus474 = true,
//            StockingStatus986 = true,
//            StockingStatus321 = true,
//            StockingStatus688 = true,
//            StockingStatus796 = true,
//            Vendor = "IN-SINKERATOR",
//            ItemDescription = "1/2 HP Disposer Badger",
//            OverpackRequired = false
//        };
//        private Item testItem2 = new Item()
//        {
//            MPN = 5484,
//            ItemCategory = "Drop-In Bathtubs",
//            Manufacturer = "KOHLER",
//            BulkPack = false,
//            BulkPackQuantity = 0,
//            PreferredShippingMethod = "LTL",
//            Weight = 88.00,
//            SourcingGuideline = "FEI",
//            StockingStatus533 = true,
//            StockingStatus423 = true,
//            StockingStatus761 = true,
//            StockingStatus2911 = true,
//            StockingStatus2920 = true,
//            StockingStatus474 = true,
//            StockingStatus986 = true,
//            StockingStatus321 = true,
//            StockingStatus688 = true,
//            StockingStatus796 = true,
//            Vendor = "KOHLER COMPANY",
//            ItemDescription = "@ 48 X 32 Acrylic Bath Greek White",
//            OverpackRequired = true
//        };


//        [Fact]
//        public void Test_RequestItemDataByMPN()
//        {
//            var mpns = new List<int>() { testItem1.MPN, testItem2.MPN };

//            var itemDict = itemServices.RequestItemDataByMPN(mpns);

//            Assert.Equal(itemDict[testItem1.MPN], testItem1);
//            Assert.Equal(itemDict[testItem2.MPN], testItem2);
//        }


//        [Fact]
//        public void Test_RequestItemDataByMPN_InvalidMPN()
//        {
//            var mpns = new List<int>() { 7777777 };

//            var itemDict = itemServices.RequestItemDataByMPN(mpns);

//            Assert.Null(itemDict[7777777]);
//        }
//    }
//}
