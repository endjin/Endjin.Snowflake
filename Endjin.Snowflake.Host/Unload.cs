// <copyright file="Unload.cs" company="Endjin">
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
    /// The function class that implements the unload HTTP operation.
    /// </summary>
    public static class Unload
    {
        /// <summary>
        /// The function's run method.
        /// </summary>
        /// <param name="req">The request to process.</param>
        /// <param name="logger">A logger instance.</param>
        /// <param name="context">The function's execution context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation including the HTTP response message.</returns>
        [FunctionName("Unload")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/unload")]
            HttpRequestMessage req,
            ILogger logger,
            ExecutionContext context)
        {
            ServiceProvider serviceProvider = Initializer.Initialize(context);

            string body = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
            UnloadCommand command = JsonConvert.DeserializeObject<UnloadCommand>(body);

            if (!command.Validate(out string message))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, message);
            }

            IConfiguration config = serviceProvider.GetService<IConfiguration>();
            var client = new SnowflakeClient(config["ConnectionString"]);
            try
            {
                client.Unload(command.Stage, command.Query, command.FilePrefix, command.Warehouse, command.Database, command.Schema, command.SingleFile, command.Overwrite);
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
