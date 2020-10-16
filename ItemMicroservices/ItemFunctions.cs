using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ItemMicroservices.Models;
using ItemMicroservices.Services;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Web;
using AzureFunctions.Extensions.Swashbuckle.Attribute;

namespace ItemMicroservices
{
    public class ItemFunctions
    {
        public static string errorLogsUrl = Environment.GetEnvironmentVariable("ERROR_LOGS_URL");


        [FunctionName("GetItemDataByMPN")]
        [QueryStringParameter("mpn", "Master Product Number", Required = true)]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Dictionary<int, Item>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(BadRequestObjectResult))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(NotFoundObjectResult))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(StatusCodeResult))]
        public static IActionResult GetItemDataByMPN(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "items")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var query = HttpUtility.ParseQueryString(req.QueryString.ToString());
                var mpnsArr = query.Get("mpn")?.Split(",");
                log.LogInformation(@"MPNs:", mpnsArr);

                if(mpnsArr == null)
                {
                    log.LogWarning("No MPNs provided.");
                    return new BadRequestObjectResult("Missing MPN")
                    {
                        Value = "Please provide at least one MPN as a query parameter.",
                        StatusCode = 400
                    };
                }

                // Filter out values that are not valid ints
                var validMPNs = mpnsArr
                    .Select(n => int.TryParse(n, out int mpn) ? mpn : (int?)null)
                    .Where(n => n.HasValue)
                    .Select(n => n.Value).ToList();

                if (validMPNs.Count == 0)
                {
                    log.LogWarning("MPN(s) are invalid.");
                    return new NotFoundObjectResult("Invalid MPN(s)")
                    {
                        Value = "The MPN(s) provided are not valid integers.",
                        StatusCode = 404
                    };
                }

                var itemService = new ItemDataServices(log, errorLogsUrl);

                var itemDataDict = itemService.RequestItemDataByMPN(validMPNs);

                return new OkObjectResult(itemDataDict);
            }
            catch(Exception ex)
            {
                var title = "Exception in QuoteShipment";
                log.LogError(ex, title);
                var teamsMessage = new TeamsMessage(title, $"Error message: {ex.Message}. Stacktrace: {ex.StackTrace}", "red", errorLogsUrl);
                teamsMessage.LogToTeams(teamsMessage);

                return new StatusCodeResult(500);
            }
        }
    }
}
