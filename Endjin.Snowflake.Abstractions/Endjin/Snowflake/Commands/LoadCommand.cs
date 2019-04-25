// <copyright file="LoadCommand.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.Snowflake.Commands
{
    /// <summary>
    /// A class representing a command to load Snowflake from a given stage.
    /// </summary>
    public class LoadCommand
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
        /// Gets or sets the name of the Snowflake stage that contains the data to load.
        /// The stage is typically an Azure or AWS external stage.
        /// </summary>
        /// <remarks>
        /// The stage must already exist within the Snowflake account.
        /// If <see cref="Database"/> and <see cref="Schema"/> are omitted then stage should be fully qualified.
        /// </remarks>
        public string Stage { get; set; }

        /// <summary>
        /// Gets or sets the name of the target table in the Snowflake account that data will be loaded into.
        /// If <see cref="Database"/> and <see cref="Schema"/> are omitted then the target table should be fully qualified.
        /// </summary>
        public string TargetTable { get; set; }

        /// <summary>
        /// Gets or sets a collection of files or file paths to include during the load operation.
        /// When not specified then the contents of the stage will be loaded.
        /// </summary>
        public string[] Files { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Snowflake should load all files even if they have
        /// already been previously loaded.
        /// </summary>
        public bool Force { get; set; }

        /// <summary>
        /// Validates the command returning true if the command is valid.
        /// </summary>
        /// <param name="message">Validation error message if the command is not valid, otherwise null.</param>
        /// <returns>True if the command is valid.</returns>
        public bool Validate(out string message)
        {
            if (string.IsNullOrWhiteSpace(this.TargetTable))
            {
                message = $"{nameof(this.TargetTable)} is a required property";
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
