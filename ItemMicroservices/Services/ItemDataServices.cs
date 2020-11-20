using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using ItemMicroservices.Models;
using Microsoft.Extensions.Logging;
using Polly;

namespace ItemMicroservices.Services
{
    public class ItemDataServices
    {
        public ILogger _logger { get; set; }
        public string errorLogsUrl { get; set; }
        public string connString = Environment.GetEnvironmentVariable("SQL_CONN");

        public ItemDataServices(ILogger log, string logUrl)
        {
            _logger = log;
            errorLogsUrl = logUrl;
        }

        public ItemDataServices() { }


        /// <summary>
        ///     Provides all item data in the Ferguson items table for each MPN. Stocking statuses are cast as bools instead of strings.
        /// </summary>
        /// <param name="MPNs">Master Product Numbers to be included in the query.</param>
        /// <returns>A dictionary where MPN is the key and value is the item data from the table.</returns>
        public ItemAndStockingData RequestItemDataByMPN(List<string> MPNs)
        {
            try
            {
                var itemDict = GetItemData(MPNs);

                var stockingDict = GetStockingStatuses(MPNs);

                return new ItemAndStockingData()
                {
                    itemDataDict = itemDict,
                    stockingStatusDict = stockingDict
                };
            }
            catch(SqlException ex)
            {
                var title = "Sql Exception in RequestItemDataByMPN";
                _logger.LogError(ex, title);
                var teamsMessage = new TeamsMessage(title, $"Error message: {ex.Message}. Stacktrace: {ex.StackTrace}", "red", errorLogsUrl);
                teamsMessage.LogToTeams(teamsMessage);
                throw;
            }
        }


        /// <summary>
        ///     Runs a SELECT query on the ItemData table for all MPNs.
        /// </summary>
        /// <param name="MPNs">Master Product Numbers to include in the query.</param>
        /// <returns>Dictionary of MPN : ItemData</returns>
        public Dictionary<string, Item> GetItemData(List<string> MPNs)
        {
            var retryPolicy = Policy.Handle<SqlException>().Retry(5, (ex, count) =>
            {
                var title = "Error in GetItemData";
                _logger.LogWarning(ex, $"{title}. Retrying...");

                if (count == 5)
                {
                    var teamsMessage = new TeamsMessage(title, $"Error: {ex.Message}. Stacktrace: {ex.StackTrace}", "red", errorLogsUrl);
                    teamsMessage.LogToTeams(teamsMessage);
                    _logger.LogError(ex, title);
                }
            });

            return retryPolicy.Execute(() =>
            {
                using (var conn = new SqlConnection(connString))
                {
                    var query = @"
                        SELECT MPN, ItemCategory, Manufacturer, BulkPack, BulkPackQuantity, PreferredShippingMethod, Weight, SourcingGuideline, Vendor, ItemDescription, Overpack, ALTCODE
                        FROM feiazprdspsrcengdb1.Data.ItemData
                        WHERE MPN in @MPNs";

                    var itemDict = conn.Query<Item>(query, new { MPNs }, commandTimeout: 150)
                        .ToDictionary(item => item.MPN, item => item);

                    AddMissingMPNsToDict(itemDict, MPNs);

                    return itemDict;
                }
            });
        }


        /// <summary>
        ///     Gets the stocking status by branch number for each MPN and initializes a stocking status dictionary.
        /// </summary>
        /// <param name="MPNs">Master Product Numbers</param>
        /// <returns>Dictionary where MPN is the key and a dictionary of branch number/boolean is the value. If value is true for any branch, it is a stocking location.</returns>
        public Dictionary<string, Dictionary<string, bool>> GetStockingStatuses(List<string> MPNs)
        {
            var retryPolicy = Policy.Handle<SqlException>().Retry(5, (ex, count) =>
            {
                var title = "Error in GetStockingStatuses";
                _logger.LogWarning(ex, $"{title}. Retrying...");

                if (count == 5)
                {
                    var teamsMessage = new TeamsMessage(title, $"Error: {ex.Message}. Stacktrace: {ex.StackTrace}", "red", errorLogsUrl);
                    teamsMessage.LogToTeams(teamsMessage);
                    _logger.LogError(ex, title);
                }
            });

            return retryPolicy.Execute(() =>
            {
                using (var conn = new SqlConnection(connString))
                {
                    var query = @"
                        SELECT MPN, BranchNumber, StockingStatus
                        FROM feiazprdspsrcengdb1.Data.StockingStatus 
                        WHERE MPN in @MPNs";

                    var results = conn.Query<Stocking>(query, new { MPNs }, commandTimeout: 150);

                    var stockingDict = results
                        .GroupBy(stocking => stocking.MPN)
                        .ToDictionary(stockingGroup => stockingGroup.Key,
                                      stockingGroup => stockingGroup.ToDictionary(stocking => stocking.BranchNumber, 
                                                                                  stocking => stocking.StockingStatus == "Stocking"));

                    AddMissingMPNsToDict(stockingDict, MPNs);

                    return stockingDict;
                }
            });
        }


        /// <summary>
        ///     Checks if any MPNs do not have entries in the dictionary. If one is missing, a dictionary entry will be added with MPN as the key and null as the value.
        /// </summary>
        /// <param name="dict">Dictionary to add missing keys to.</param>
        /// <param name="MPNs">List of Master Product Numbers to add to dictionary if missing.</param>
        public void AddMissingMPNsToDict(Dictionary<string, Item> dict, List<string> MPNs)
        {
            var missingMPNs = MPNs.Where(x => dict.All(y => y.Key != x.ToString())).ToList();

            // If any MPNs were not returned in the query, it means they are invalid. Add them to the dict as null.
            if (missingMPNs.Any())
            {
                missingMPNs.ForEach(mpn => dict.Add(mpn.ToString(), null));
            }
        }


        /// <summary>
        ///     Checks if any MPNs do not have entries in the dictionary. If one is missing, a dictionary entry will be added with MPN as the key and null as the value.
        /// </summary>
        /// <param name="dict">Dictionary to add missing keys to.</param>
        /// <param name="MPNs">List of Master Product Numbers to add to dictionary if missing.</param>
        public void AddMissingMPNsToDict(Dictionary<string, Dictionary<string, bool>> dict, List<string> MPNs)
        {
            var missingMPNs = MPNs.Where(x => dict.All(y => y.Key != x.ToString())).ToList();

            // If any MPNs were not returned in the query, it means they are invalid. Add them to the dict as null.
            if (missingMPNs.Any())
            {
                missingMPNs.ForEach(mpn => dict.Add(mpn.ToString(), null));
            }
        }
    }
}
