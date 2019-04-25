using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Endjin.Snowflake.Demo
{
    static class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ServiceProvider serviceProvider = ConfigureServices(serviceCollection);
            ILogger<SnowflakeClient> logger = serviceProvider.GetService<ILogger<SnowflakeClient>>();

            try
            {
                Console.WriteLine("Example of how to use the Snowflake client to load and unload data");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Ensure you have run Snowflake/Setup.ps1 before continuing");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Enter your Snowflake connection string:");
                Console.WriteLine("HINT.. its usually in the format 'ACCOUNT=<account-name>;HOST=<account-name>.<account-region>.azure.snowflakecomputing.com;USER=<user>;PASSWORD=<password>;ROLE=<role>'");
                var client = new SnowflakeClient(Console.ReadLine());

                Console.WriteLine();
                Console.WriteLine("Enter your database name:");
                string database = Console.ReadLine();

                Console.WriteLine();
                Console.WriteLine("Enter name of the warehouse you want to use:");
                string dataWarehouse = Console.ReadLine();

                Console.WriteLine();
                Console.WriteLine("Enter the input path of the data to load (relative to storage container:");
                string inputPath = Console.ReadLine();

                Console.WriteLine();
                Console.WriteLine("Enter the output path of the data to unload to (relative to storage container:");
                string outputPath = Console.ReadLine();

                Console.WriteLine();
                Console.WriteLine("Loading data from Azure into Snowflake SALES.LINEITEM...");
                string schema = "SALES";
                client.Load("azure_adf_stage", "LINEITEM", new[] { inputPath }, dataWarehouse, database, schema, true);
                Console.WriteLine("Load complete");

                Console.WriteLine();
                Console.WriteLine("Unloading aggregate data from Snowflake to Azure...");
                client.Unload("azure_adf_stage", "select * from SupplierAgg", $"{outputPath}.gzip", dataWarehouse, database, schema, true, true);
                Console.WriteLine("Unload complete");

                serviceProvider.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Ooops... something went wrong...");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.WriteLine("Please check your inputs and try again");                
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("\r\nPress any key to continue");
            Console.ReadKey();
        }

        private static ServiceProvider ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
            return services.BuildServiceProvider();
        }
    }
}
