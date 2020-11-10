using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using ItemMicroservices.Models;
using Microsoft.Extensions.Logging;

namespace ItemMicroservices.Services
{
    public class ItemDataServices
    {
        public ILogger _logger { get; set; }
        public string errorLogsUrl { get; set; }

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
        public Dictionary<int, Item> RequestItemDataByMPN(List<int> MPNs)
        {
            try
            {
                var connString = Environment.GetEnvironmentVariable("SQL_CONN");

                using (var conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // TODO: update DB name once it is created
                    var query = @"
                    SELECT MPN, ItemCategory, Manufacturer, BulkPack, BulkPackQuantity, PreferredShippingMethod, Weight, SourcingGuideline, Vendor, ItemDescription, OverpackRequired, ALT1Code, 
                        CASE WHEN [StockingStatus533] = 'Stocking' THEN 1 WHEN [StockingStatus533] = 'Non-Stocking' THEN 0 ELSE null END [StockingStatus533],
                        CASE WHEN [StockingStatus423] = 'Stocking' THEN 1 WHEN [StockingStatus423] = 'Non-Stocking' THEN 0 ELSE null END [StockingStatus423],
                        CASE WHEN [StockingStatus761] = 'Stocking' THEN 1 WHEN [StockingStatus761] = 'Non-Stocking' THEN 0 ELSE null END [StockingStatus761],
                        CASE WHEN [StockingStatus2911] = 'Stocking' THEN 1 WHEN [StockingStatus2911] = 'Non-Stocking' THEN 0 ELSE null END [StockingStatus2911],
                        CASE WHEN [StockingStatus2920] = 'Stocking' THEN 1 WHEN [StockingStatus2920] = 'Non-Stocking' THEN 0 ELSE null END [StockingStatus2920],
                        CASE WHEN [StockingStatus474] = 'Stocking' THEN 1 WHEN [StockingStatus474] = 'Non-Stocking' THEN 0 ELSE null END [StockingStatus474],
                        CASE WHEN [StockingStatus986] = 'Stocking' THEN 1 WHEN [StockingStatus986] = 'Non-Stocking' THEN 0 ELSE null END [StockingStatus986],
                        CASE WHEN [StockingStatus321] = 'Stocking' THEN 1 WHEN [StockingStatus321] = 'Non-Stocking' THEN 0 ELSE null END [StockingStatus321],
                        CASE WHEN [StockingStatus625] = 'Stocking' THEN 1 WHEN [StockingStatus625] = 'Non-Stocking' THEN 0 ELSE null END [StockingStatus625],
                        CASE WHEN [StockingStatus688] = 'Stocking' THEN 1 WHEN [StockingStatus688] = 'Non-Stocking' THEN 0 ELSE null END [StockingStatus688],
                        CASE WHEN [StockingStatus796] = 'Stocking' THEN 1 WHEN [StockingStatus796] = 'Non-Stocking' THEN 0 ELSE null END [StockingStatus796]
                    FROM FergusonIntegration.ferguson.Items 
                    WHERE MPN in @MPNs";

                    var mpnsWithItemData = conn.Query<Item>(query, new { MPNs }, commandTimeout: 6)
                        .ToDictionary(item => item.MPN, item => item);

                    var missingMPNs = MPNs.Where(x => mpnsWithItemData.All(y => y.Key != x)).ToList();

                    // If any MPNs were not returned in the query, it means they are invalid. Add them to the dict as null.
                    if (missingMPNs.Any())
                    {
                        missingMPNs.ForEach(mpn => mpnsWithItemData.Add(mpn, null));
                    }

                    conn.Close();

                    return mpnsWithItemData;
                }
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
    }
}
