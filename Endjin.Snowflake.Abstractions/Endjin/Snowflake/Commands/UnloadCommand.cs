// <copyright file="UnloadCommand.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.Snowflake.Commands
{
    /// <summary>
    /// A class representing a command to unload data from Snowflake into a stage.
    /// </summary>
    public class UnloadCommand
    {
        /// <summary>
        /// Gets or sets the name of the database context to apply to the command.
        /// When not specified the stage and target table must be fully qualified.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the schema context to apply to the load command.
        /// When not specified the stage and target table must be schema qualified.
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Gets or sets the name of the data warehouse that the command will run against.
        /// If the data warehouse is not supplied then the warehouse will be determined by the connection string used to execute the command.
        /// </summary>
        public string Warehouse { get; set; }

        /// <summary>
        /// Gets or sets the name of the Snowflake stage that data should be unloaded to.
        /// The stage is typically an Azure or AWS external stage.
        /// </summary>
        /// <remarks>
        /// The stage must already exist within the Snowflake account.
        /// If <see cref="Database"/> and <see cref="Schema"/> are omitted then stage should be fully qualified.
        /// </remarks>
        public string Stage { get; set; }

        /// <summary>
        /// Gets or sets a query to generate the results to unload.
        /// If <see cref="Database"/> and <see cref="Schema"/> are omitted then the objects referenced in the query should be fully qualified.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets a file prefix for the resulting files.
        /// </summary>
        public string FilePrefix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Snowflake should generate a single file or multiple files.
        /// </summary>
        public bool SingleFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Snowflake should overwrite existing files if they already exist in the stage.
        /// </summary>
        public bool Overwrite { get; set; }

        /// <summary>
        /// Validates the command returning true if the command is valid.
        /// </summary>
        /// <param name="message">Validation error message if the command is not valid, otherwise null.</param>
        /// <returns>True if the command is valid.</returns>
        public bool Validate(out string message)
        {
            if (string.IsNullOrWhiteSpace(this.Query))
            {
                message = $"{nameof(this.Query)} is a required property";
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.Stage))
            {
                message = $"{nameof(this.Stage)} is a required property";
                return false;
            }

            message = null;
            return true;
        }
    }
}
