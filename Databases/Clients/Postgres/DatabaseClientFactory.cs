﻿// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using Npgsql;
using UserManagementApi.Databases.Interfaces;
using UserManagementApi.Databases.Models;

namespace UserManagementApi.Databases.Clients.Postgres
{
    public class DatabaseClientFactory : IDatabaseClientFactory
    {
        private static readonly string _schema;
        private static readonly string _connectionString;

        static DatabaseClientFactory()
        {
            var config = ConfigLoader.Config;
            _connectionString = CreateConnectionString(config);
            _schema = config.Database.Schema;
        }

        private static string CreateConnectionString(Config config)
        {
            NpgsqlConnectionStringBuilder builder = new()
            {
                Host = config.Host,
                Port = config.Port,
                SslMode = config.SslMode switch
                {
                    "Require" => SslMode.Require,
                    "Prefer" => SslMode.Prefer,
                    "Disable" => SslMode.Disable,
                    "Allow" => SslMode.Allow,
                    _ => throw new InvalidOperationException(),
                },
                Username = config.Account.Username,
                Password = config.Account.Password,
                Database = config.Database.Name
            };
            if (config.Pooling is Config.PoolingSection pooling)
            {
                builder.Pooling = true;
                builder.MinPoolSize = pooling.MinSize;
                builder.MaxPoolSize = pooling.MaxSize;
            }
            string connectionString = builder.ToString();
            return connectionString;
        }

        public DatabaseClientFactory()
        {
        }

        public IUserDatabaseClient CreateUserDatabaseClient()
        {
            var client = new UserDatabaseClient(_connectionString, _schema);
            return client;
        }
    }
}
