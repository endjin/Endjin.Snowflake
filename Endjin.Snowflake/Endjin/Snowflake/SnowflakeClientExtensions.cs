// <copyright file="SnowflakeClientExtensions.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.Snowflake
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A set of extension methods for executing Snowflake commands.
    /// </summary>
    public static class SnowflakeClientExtensions
    {
        /// <summary>
        /// Instructs Snowflake to load data from a stage into a target table.
        /// </summary>
        /// <param name="client">
        /// The <see cref="SnowflakeClient"/>.
        /// </param>
        /// <param name="stage">
        /// The name of the Snowflake stage that contains the data to load.
        /// The stage is typically an Azure or AWS external stage.
        /// If <paramref name="database"/> and <paramref name="schema"/> are omitted then the stage name should be fully qualified.
        /// </param>
        /// <param name="targetTable">
        /// The name of the target table.
        /// If <paramref name="database"/> and <paramref name="schema"/> are omitted then the target table should be fully qualified.
        /// </param>
        /// <param name="files">
        /// The collection of files or file paths to include during the load operation.
        /// When not specified then the contents of the stage will be loaded.</param>
        /// <param name="warehouse">
        /// The name of the data warehouse that the command will run against.
        /// If the data warehouse is not supplied then the warehouse will be determined by the connection string used to execute the command.
        /// </param>
        /// <param name="database">
        /// The name of the database context to apply to the command.
        /// When not specified the stage and target table must be fully qualified.
        /// </param>
        /// <param name="schema">
        /// The schema context to apply to the load command.
        /// When not specified the stage and target table must be schema qualified.
        /// </param>
        /// <param name="force">
        /// A value indicating whether Snowflake should load all files even if they have
        /// already been previously loaded.
        /// </param>
        /// <returns>The number of rows affected.</returns>
        public static int Load(this SnowflakeClient client, string stage, string targetTable, string[] files = null, string warehouse = null, string database = null, string schema = null, bool force = false)
        {
            IList<string> commands = DefineSnowflakeQueryContext(warehouse, database, schema);
            commands.Add(LoadSnowflakeCommand(stage, targetTable, files, force));
            return client.ExecuteNonQuery(commands.ToArray());
        }

        /// <summary>
        /// Instructs Snowflake to unload data from a query into a stage.
        /// </summary>
        /// <param name="client">
        /// The <see cref="SnowflakeClient"/>.
        /// </param>
        /// <param name="stage">
        /// The name of the Snowflake stage that contains the data to load.
        /// The stage is typically an Azure or AWS external stage.
        /// If <paramref name="database"/> and <paramref name="schema"/> are omitted then the stage name should be fully qualified.
        /// </param>
        /// <param name="query">
        /// A query to generate the results to unload.
        /// If <paramref name="database"/> and <paramref name="schema"/> are omitted then the objects referenced in the query should be fully qualified.
        /// </param>
        /// <param name="filePrefix">
        /// A file prefix for the resulting files.
        /// </param>
        /// <param name="warehouse">
        /// The name of the data warehouse that the command will run against.
        /// If the data warehouse is not supplied then the warehouse will be determined by the connection string used to execute the command.
        /// </param>
        /// <param name="database">
        /// The name of the database context to apply to the command.
        /// When not specified the stage and target table must be fully qualified.
        /// </param>
        /// <param name="schema">
        /// The schema context to apply to the load command.
        /// When not specified the stage and target table must be schema qualified.
        /// </param>
        /// <param name="singleFile">
        /// a value indicating whether Snowflake should generate a single file or multiple files.
        /// </param>
        /// <param name="overwrite">
        /// A value indicating whether Snowflake should overwrite existing files if they already exist in the stage.
        /// </param>
        public static void Unload(this SnowflakeClient client, string stage, string query, string filePrefix, string warehouse = null, string database = null, string schema = null, bool singleFile = false, bool overwrite = false)
        {
            IList<string> commands = DefineSnowflakeQueryContext(warehouse, database, schema);
            commands.Add(UnloadSnowflakeCommand(stage, query, filePrefix, singleFile, overwrite));
            client.ExecuteNonQuery(commands.ToArray());
        }

        private static string UnloadSnowflakeCommand(string stage, string query, string filePrefix, bool singleFile, bool overwrite)
        {
            filePrefix = filePrefix != null ? "/" + filePrefix?.TrimStart('/') : string.Empty;
            StringBuilder sb = new StringBuilder($"COPY INTO '@{stage}{filePrefix}' FROM ({query})")
                .Append($" SINGLE={singleFile}")
                .Append($" OVERWRITE={overwrite};");
            return sb.ToString();
        }

        private static string LoadSnowflakeCommand(string stage, string targetTable, string[] files, bool force)
        {
            var sb = new StringBuilder($"COPY INTO {targetTable} FROM '@{stage}'");

            if (files?.Length > 0)
            {
                string filesOptions = string.Join(",", files.Select(o => $"'{o.Trim('\'')}'").ToArray());
                sb.Append($" FILES=({filesOptions})");
            }

            sb.Append($" FORCE={force};");

            return sb.ToString();
        }

        private static IList<string> DefineSnowflakeQueryContext(string warehouse, string database, string schema)
        {
            var context = new List<string>();

            if (!string.IsNullOrWhiteSpace(warehouse))
            {
                context.Add($"USE WAREHOUSE \"{warehouse.ToUpper()}\";");
            }

            if (!string.IsNullOrEmpty(database))
            {
                context.Add($"USE DATABASE \"{database.ToUpper()}\";");
                if (!string.IsNullOrEmpty(schema))
                {
                    context.Add($"USE SCHEMA \"{schema.ToUpper()}\";");
                }
            }

            return context;
        }
    }
}
