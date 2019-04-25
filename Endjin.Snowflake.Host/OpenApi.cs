// <copyright file="OpenApi.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.Snowflake.Host
{
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;

    /// <summary>
    /// The function class that returns the OpenApi definition.
    /// </summary>
    public static class OpenApi
    {
        /// <summary>
        /// The function's run method.
        /// </summary>
        /// <param name="req">The request to process.</param>
        /// <param name="version">The version of the OpenApi definition.</param>
        /// <param name="extension">The extension.</param>
        /// <param name="context">The function's <see cref="ExecutionContext"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation including the HTTP response message.</returns>
        [FunctionName("Swagger")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "openapi/{version}.{extension}")] HttpRequest req,
            string version,
            string extension,
            ExecutionContext context)
        {
            string filePath = Path.Combine(context.FunctionAppDirectory, $"yaml\\{version}\\snowflake.{extension}");
            if (!File.Exists(filePath))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(filePath, FileMode.Open);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue($"application/{extension}");
            return response;
        }
    }
}
