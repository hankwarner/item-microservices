﻿using AzureFunctions.Extensions.Swashbuckle;
using ItemMicroservices;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using System.Reflection;

[assembly: WebJobsStartup(typeof(SwashBuckleStartup))]
[assembly: FunctionsStartup(typeof(Startup))]
namespace ItemMicroservices
{
    internal class SwashBuckleStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            //Register the extension
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
        }
    }


    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}