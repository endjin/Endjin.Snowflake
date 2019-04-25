// <copyright file="Load.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.Snowflake.Host
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Endjin.Snowflake.Commands;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// The function class that implements the load HTTP operation.
    /// </summary>
    public static class Load
    {
        /// <summary>
        /// The function's run method.
        /// </summary>
        /// <param name="req">The request to process.</param>
        /// <param name="logger">A logger instance.</param>
        /// <param name="context">The function's execution context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation including the HTTP response message.</returns>
        [FunctionName("Load")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/load")]
            HttpRequestMessage req,
            ILogger logger,
            ExecutionContext context)
        {
            ServiceProvider serviceProvider = Initializer.Initialize(context);

            string body = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
            LoadCommand command = JsonConvert.DeserializeObject<LoadCommand>(body);

            if (!command.Validate(out string message))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, message);
            }

            IConfiguration config = serviceProvider.GetService<IConfiguration>();
            var client = new SnowflakeClient(config["ConnectionString"]);
            try
            {
                client.Load(command.Stage, command.TargetTable, command.Files, command.Warehouse, command.Database, command.Schema, command.Force);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing request");
                return req.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
