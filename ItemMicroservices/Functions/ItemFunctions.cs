using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ItemMicroservices.Models;
using ItemMicroservices.Services;
using System.Linq;
using System.Web;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ItemMicroservices
{
    public class ItemFunctions
    {
        public static string errorLogsUrl = Environment.GetEnvironmentVariable("ERROR_LOGS_URL");


        /// <summary>
        ///     Gets all item data and stocking statuses for the requested MPNs.
        /// </summary>
        /// <returns>A dictionary of item data and stocking statuses with MPN as the key.</returns>
        [FunctionName("GetItemAndStockingDataByMPN")]
        [QueryStringParameter("mpn", "Master Product Number", Required = true, DataType = typeof(string))]
        [ProducesResponseType(200, Type = typeof(ItemAndStockingData))]
        [ProducesResponseType(400, Type = typeof(BadRequestObjectResult))]
        [ProducesResponseType(500, Type = typeof(StatusCodeResult))]
        public static async Task<ActionResult<ItemAndStockingData>> GetItemAndStockingDataByMPN(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "item")] HttpRequest req,
            ILogger log)
        {
            try
            {
                // Call item data service
                var itemDataResponse = await GetItemDataByMPN(req, log);
                var itemDataResult = (ObjectResult)itemDataResponse.Result;

                if (itemDataResult.StatusCode == 400)
                {
                    var msg = itemDataResult.Value.ToString();
                    log.LogWarning(msg);

                    return new BadRequestObjectResult(msg) { Value = msg };
                }

                // Call stocking status service
                var stockingDataResponse = await GetStockingStatusesByMPN(req, log);
                var stockingDataResult = (ObjectResult)stockingDataResponse.Result;

                if (stockingDataResult.StatusCode == 400)
                {
                    var msg = stockingDataResult.Value.ToString();
                    log.LogWarning(msg);

                    return new BadRequestObjectResult(msg){ Value = msg };
                }

                var response = new ItemAndStockingData()
                {
                    itemDataDict = (Dictionary<string, Item>)itemDataResult.Value,
                    stockingStatusDict = (Dictionary<string, Dictionary<string, bool>>)stockingDataResult.Value
                };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                var title = "Exception in GetItemAndStockingDataByMPN";
                log.LogError(ex, title);
                var teamsMessage = new TeamsMessage(title, $"Error message: {ex.Message}. Stacktrace: {ex.StackTrace}", "red", errorLogsUrl);
                teamsMessage.LogToTeams(teamsMessage);

                return new StatusCodeResult(500);
            }
        }


        /// <summary>
        ///     Gets values from the ItemData table such as weight, src guideline, pref ship method, vendor for the requested MPNs.
        /// </summary>
        /// <returns>A dictionary of item data with MPN as the key.</returns>
        [FunctionName("GetItemDataByMPN")]
        [QueryStringParameter("mpn", "Master Product Number", Required = true, DataType = typeof(string))]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, Item>))]
        [ProducesResponseType(400, Type = typeof(BadRequestObjectResult))]
        [ProducesResponseType(500, Type = typeof(StatusCodeResult))]
        public static async Task<ActionResult<Dictionary<string, Item>>> GetItemDataByMPN(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "item/data")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var query = HttpUtility.ParseQueryString(req.QueryString.ToString());
                var mpnsArr = query.Get("mpn")?.Split(",");
                log.LogInformation(@"MPNs:", mpnsArr);

                if(mpnsArr == null)
                {
                    var msg = "No MPNs provided.";
                    log.LogWarning(msg);
                    return new BadRequestObjectResult(msg) { Value = "Please provide at least one MPN as a query parameter." };
                }

                var itemService = new ItemDataServices(log, errorLogsUrl);
                var itemDict = itemService.GetItemData(mpnsArr.ToList());

                return new OkObjectResult(itemDict);
            }
            catch(Exception ex)
            {
                var title = "Exception in GetItemDataByMPN";
                log.LogError(ex, title);
                var teamsMessage = new TeamsMessage(title, $"Error message: {ex.Message}. Stacktrace: {ex.StackTrace}", "red", errorLogsUrl);
                teamsMessage.LogToTeams(teamsMessage);

                return new StatusCodeResult(500);
            }
        }


        /// <summary>
        ///     Gets all stocking statuses by branch number for each MPN requested.
        /// </summary>
        /// <returns>A dictionary where MPN is the key, and the values are dictionaries where key is branch number and value is true if location is stocking.</returns>
        [FunctionName("GetStockingStatusesByMPN")]
        [QueryStringParameter("mpn", "Master Product Number", Required = true, DataType = typeof(string))]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, Dictionary<string, bool>>))]
        [ProducesResponseType(400, Type = typeof(BadRequestObjectResult))]
        [ProducesResponseType(500, Type = typeof(StatusCodeResult))]
        public static async Task<ActionResult<Dictionary<string, Dictionary<string, bool>>>> GetStockingStatusesByMPN(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "item/stocking")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var query = HttpUtility.ParseQueryString(req.QueryString.ToString());
                var mpnsArr = query.Get("mpn")?.Split(",");
                log.LogInformation(@"MPNs:", mpnsArr);

                if (mpnsArr == null)
                {
                    var msg = "No MPNs provided.";
                    log.LogWarning(msg);
                    return new BadRequestObjectResult(msg) { Value = "Please provide at least one MPN as a query parameter." };
                }

                var itemService = new ItemDataServices(log, errorLogsUrl);
                var stockingDict = itemService.GetStockingStatuses(mpnsArr.ToList());

                return new OkObjectResult(stockingDict);
            }
            catch (Exception ex)
            {
                var title = "Exception in GetStockingStatusesByMPN";
                log.LogError(ex, title);
                var teamsMessage = new TeamsMessage(title, $"Error message: {ex.Message}. Stacktrace: {ex.StackTrace}", "red", errorLogsUrl);
                teamsMessage.LogToTeams(teamsMessage);

                return new StatusCodeResult(500);
            }
        }
    }
}
