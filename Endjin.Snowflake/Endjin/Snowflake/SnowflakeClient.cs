// <copyright file="SnowflakeClient.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.Snowflake
{
    using System.Data;
    using global::Snowflake.Data.Client;

    /// <summary>
    /// A client for submitting queries to Snowflake.
    /// </summary>
    public class SnowflakeClient
    {
        private readonly string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowflakeClient"/> class.
        /// </summary>
        /// <param name="connectionString">The Snowflake connection string to use.</param>
        public SnowflakeClient(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Executes a sequence of Snowflake statements that are not expected to return a result set.
        /// </summary>
        /// <param name="statements">The query statements to execute.</param>
        /// <returns>The number of rows affected. When more than one statement is supplied the method will return the number of rows affected for the last statement only.</returns>
        public int ExecuteNonQuery(params string[] statements)
        {
            using (IDbConnection conn = new SnowflakeDbConnection())
            {
                conn.ConnectionString = this.connectionString;
                conn.Open();

                IDbCommand cmd = conn.CreateCommand();

                int affectedRows = 0;
                foreach (string command in statements)
                {
                    cmd.CommandText = command;
                    affectedRows = cmd.ExecuteNonQuery();
                }

                return affectedRows;
            }
        }
    }
}
