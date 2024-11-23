// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using Dapper;
using Npgsql;
using UserManagementApi.Databases.Exceptions;
using UserManagementApi.Databases.Extensions;
using UserManagementApi.Databases.Interfaces;
using UserManagementApi.Databases.Models;

namespace UserManagementApi.Databases.Clients.Postgres
{
    public class UserDatabaseClient(string connectionString, string schema) : DatabaseClient(schema), IUserDatabaseClient, IDisposable
    {
        private readonly NpgsqlConnection _connection = new(connectionString);

        static UserDatabaseClient()
        {
            SqlMapper.SetTypeMap(typeof(Item), new CustomPropertyTypeMap(typeof(Item), CustomMap));
            SqlMapper.SetTypeMap(typeof(Name), new CustomPropertyTypeMap(typeof(Name), CustomMap));
            SqlMapper.SetTypeMap(typeof(ListItem), new CustomPropertyTypeMap(typeof(ListItem), CustomMap));
        }

        public async Task<Guid> CreateAsync()
        {
            await _connection.OpenIfClosedAsync();
            using var transaction = await _connection.BeginTransactionAsync();

            Guid guid;
            try
            {
                string table = GetTableName<Item>();
                string query = $"""
                INSERT INTO
                    {table}
                DEFAULT VALUES
                RETURNING
                    "guid";
                """;
                guid = await _connection.ExecuteScalarAsync<Guid?>(query) ?? Guid.Empty;
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to create item.", e);
            }
            if (guid == Guid.Empty)
            {
                throw new DatabaseConflictException();
            }
            return guid;
        }

        public async Task<Item?> GetAsync(Guid guid)
        {
            await _connection.OpenIfClosedAsync();

            Item? item;
            try
            {
                string table = GetTableName<Item>();
                string columns = GenerateColumnListQuery<Item>();
                string query = $"""
                SELECT
                    {columns}
                FROM
                    {table}
                WHERE
                    "guid" = @Guid;
                """;
                item = await _connection.QueryFirstOrDefaultAsync<Item>(query, new
                {
                    Guid = guid
                });
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get item.", e);
            }
            return item;
        }

        public async Task<int> DeleteAsync(Guid guid)
        {
            await _connection.OpenIfClosedAsync();
            var transaction = await _connection.BeginTransactionAsync();

            int rows;
            try
            {
                string table = GetTableName<Item>();
                string query = $"""
                DELETE FROM {table}
                WHERE
                    "guid" = @Guid;
                """;
                rows = await _connection.ExecuteAsync(query, new
                {
                    Guid = guid
                });
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to delete item.", e);
            }
            return rows;
        }

        public async Task<int> CreateNameAsync(Name name)
        {
            if ((name is null) ||
                (name.ItemId is null))
            {
                throw new DatabaseParameterException();
            }

            await _connection.OpenIfClosedAsync();
            var transaction = await _connection.BeginTransactionAsync();

            int rows;
            try
            {
                string table = GetTableName<Name>();
                string columns = GenerateColumnListQuery(name, [
                    nameof(Name.Version)
                    ]);
                string values = GenerateInsertValueListQuery(name, [
                    nameof(Name.Version)
                    ]);
                string query = $"""
                INSERT INTO
                    {table} ({columns})
                VALUES
                    ({values});
                """;
                rows = await _connection.ExecuteAsync(query, name);
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to create name.", e);
            }
            return rows;
        }

        public async Task<Name?> GetNameAsync(long itemId)
        {
            if (itemId < 1)
            {
                throw new DatabaseParameterException(nameof(itemId));
            }

            await _connection.OpenIfClosedAsync();

            Name? name;
            try
            {
                string table = GetTableName<Name>();
                string columns = GenerateColumnListQuery<Name>();
                string query = $"""
                SELECT
                    {columns}
                FROM
                    {table}
                WHERE
                    "item_id" = @ItemId;
                """;
                name = await _connection.QueryFirstOrDefaultAsync<Name>(query, new
                {
                    ItemId = itemId
                });
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get name.", e);
            }
            return name;
        }

        public async Task<int> SetNameAsync(Name name)
        {
            if ((name is null) ||
                (name.ItemId is null))
            {
                throw new DatabaseParameterException();
            }

            await _connection.OpenIfClosedAsync();
            var transaction = await _connection.BeginTransactionAsync();

            int rows;
            try
            {
                string table = GetTableName<Name>();
                string updateSet = GenerateUpdateSetListQuery(name, [
                    nameof(Name.ItemId),
                    nameof(Name.Version)
                    ]);
                string query = $"""
                UPDATE {table}
                SET
                    {updateSet},
                    "version" = "version" + 1
                WHERE
                    "item_id" = @ItemId
                    AND "version" = @Version;
                """;
                rows = await _connection.ExecuteAsync(query, name);
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new DatabaseException("Failed to set name.", e);
            }
            if (rows < 1)
            {
                throw new DatabaseConflictException();
            }
            return rows;
        }

        public async Task<List<ListItem>> GetListAsync(Guid guid, int limit)
        {
            limit = Math.Min(Math.Max(limit, 1), 128);

            await _connection.OpenIfClosedAsync();

            List<ListItem> items;
            try
            {
                string cte = GetTableName<Item>();
                string table = GetTableName<ListItem>();
                string columns = GenerateColumnListQuery<ListItem>();
                string condition;
                if (guid != Guid.Empty)
                {
                    condition = """
                        "id" >= (
                            SELECT
                                "id"
                            FROM
                                id_cte
                        )
                    """;
                }
                else
                {
                    condition = "TRUE";
                }
                string query = $"""
                WITH
                    id_cte AS (
                        SELECT
                            "id"
                        FROM
                            {cte}
                        WHERE
                            "guid" = @Guid
                    )
                SELECT
                    {columns}
                FROM
                    {table}
                WHERE
                    {condition}
                LIMIT
                    @Limit;
                """;
                IEnumerable<ListItem> results = await _connection.QueryAsync<ListItem>(query, new
                {
                    Guid = guid,
                    Limit = limit
                });
                items = results.ToList();
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get user list.", e);
            }
            return items;
        }

        public async Task<long> GetTotalCountAsync()
        {
            await _connection.OpenIfClosedAsync();

            long count;
            try
            {
                string query = $"""
                SELECT
                    n_live_tup
                FROM
                    pg_catalog.pg_stat_user_tables
                WHERE
                    relname = 'item';
                """;
                count = await _connection.ExecuteScalarAsync<long?>(query) ?? 0;
            }
            catch (Exception e)
            {
                throw new DatabaseException("Failed to get total count.", e);
            }
            return count;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection.Dispose();
            }
        }
    }
}
