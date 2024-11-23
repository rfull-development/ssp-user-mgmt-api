// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Adapters;
using UserManagementApi.Databases.Clients.Postgres;
using UserManagementApi.Databases.Exceptions;

namespace UserManagementApi.Handlers
{
    public static class UserHandler
    {
        public static void AddUserHandler(this WebApplication app)
        {
            var users = app.MapGroup("/users");
            users.MapGet("/", GetUserListAsync);
            users.MapPost("/", CreateUserAsync);
            users.MapGet("/{id}", GetUserAsync);
            users.MapPatch("/{id}", UpdateUserAsync);
            users.MapDelete("/{id}", DeleteUserAsync);
        }

        private static async Task<IResult> GetUserListAsync(
            [FromQuery(Name = "start-id")] string? startId,
            [FromQuery(Name = "limit")] int limit = 20
            )
        {
            List<Models.User>? users;
            long totalCount;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateUserDatabaseClient();
                UserAdapter adapter = new(databaseClient);
                users = await adapter.GetListAsync(startId, limit);
                totalCount = await adapter.GetTotalCountAsync();
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            Models.UserListGetResponse response = new()
            {
                TotalCount = totalCount,
                Count = users.Count,
                Users = users
            };
            return Results.Ok(response);
        }

        private static async Task<IResult> CreateUserAsync()
        {
            string id;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateUserDatabaseClient();
                UserAdapter adapter = new(databaseClient);
                id = await adapter.CreateAsync();
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseConflictException)
            {
                return Results.Conflict();
            }
            Models.UserCreateResponse response = new()
            {
                Id = id
            };
            return Results.Created($"/users/{response.Id}", response);
        }

        private static async Task<IResult> GetUserAsync(
            [FromRoute(Name = "id")] string id
            )
        {
            Models.User? user;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateUserDatabaseClient();
                UserAdapter adapter = new(databaseClient);
                user = await adapter.GetAsync(id);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            if (user is null)
            {
                return Results.NotFound();
            }
            Models.UserGetResponse response = new()
            {
                Id = user.Id,
                Name = user.Name,
            };
            return Results.Ok(response);
        }

        private static async Task<IResult> UpdateUserAsync(
            [FromRoute(Name = "id")] string id,
            [FromBody] Models.UserUpdateRequest updateRequest
            )
        {
            bool success;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateUserDatabaseClient();
                UserAdapter adapter = new(databaseClient);
                Models.User user = new()
                {
                    Id = id,
                    Name = updateRequest.Name,
                };
                success = await adapter.SetAsync(user);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseConflictException)
            {
                return Results.Conflict();
            }
            if (!success)
            {
                return Results.NotFound();
            }
            return Results.Accepted();
        }

        private static async Task<IResult> DeleteUserAsync(
            [FromRoute(Name = "id")] string id
            )
        {
            bool success;
            try
            {
                DatabaseClientFactory databaseClientFactory = new();
                using var databaseClient = databaseClientFactory.CreateUserDatabaseClient();
                UserAdapter adapter = new(databaseClient);
                success = await adapter.DeleteAsync(id);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest();
            }
            catch (DatabaseParameterException)
            {
                return Results.BadRequest();
            }
            if (!success)
            {
                return Results.NotFound();
            }
            return Results.Accepted();
        }
    }
}
