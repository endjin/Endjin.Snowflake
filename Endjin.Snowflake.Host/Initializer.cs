// <copyright file="Initializer.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.Snowflake.Host
{
    using System.Threading;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A class responsible for initializing the function host environment.
    /// </summary>
    internal static class Initializer
    {
        private static readonly object SyncRoot = new object();

        private static ServiceProvider serviceProvider;

        /// <summary>
        /// Initializes the function host environment.
        /// </summary>
        /// <param name="context">The function's <see cref="Microsoft.Azure.WebJobs.ExecutionContext"/>.</param>
        /// <returns>An initialized <see cref="serviceProvider"/>.</returns>
        internal static ServiceProvider Initialize(Microsoft.Azure.WebJobs.ExecutionContext context)
        {
            if (serviceProvider == null)
            {
                Monitor.Enter(SyncRoot);
                try
                {
                    if (serviceProvider == null)
                    {
                        serviceProvider = BuildContainer(context);
                    }
                }
                finally
                {
                    Monitor.Exit(SyncRoot);
                }
            }

            return serviceProvider;
        }

        private static ServiceProvider BuildContainer(Microsoft.Azure.WebJobs.ExecutionContext context)
        {
            var services = new ServiceCollection();

            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(config);

            return services.BuildServiceProvider();
        }
        }
}
