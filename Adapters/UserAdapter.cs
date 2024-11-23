// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using UserManagementApi.Databases.Interfaces;
using UserManagementApi.Models;

namespace UserManagementApi.Adapters
{
    public class UserAdapter(IUserDatabaseClient client)
    {
        private readonly IUserDatabaseClient _client = client;

        public async Task<string> CreateAsync()
        {
            Guid dbGuid = await _client.CreateAsync();
            string id = dbGuid.ToString();
            return id;
        }

        public async Task<User?> GetAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            Databases.Models.Item? dbItem = await _client.GetAsync(dbGuid);
            if (dbItem is null)
            {
                return null;
            }
            if ((dbItem.Id is not long dbId) ||
                (dbItem.Guid is null))
            {
                throw new InvalidDataException();
            }

            UserName? name = await GetNameAsync(dbId);
            User user = new()
            {
                Id = id,
                Name = name,
            };
            return user;
        }

        public async Task<bool> SetAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            if (!Guid.TryParse(user.Id, out Guid dbGuid))
            {
                throw new ArgumentNullException(user.Id);
            }

            Databases.Models.Item? dbItem = await _client.GetAsync(dbGuid);
            if (dbItem is null)
            {
                return false;
            }
            if ((dbItem.Id is not long dbId) ||
                (dbItem.Guid is null))
            {
                throw new InvalidDataException();
            }

            bool success = true;
            if (user.Name is not null)
            {
                success = await SetNameAsync(dbId, user.Name);
            }
            return success;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid dbGuid))
            {
                throw new ArgumentException(null, nameof(id));
            }

            int rows = await _client.DeleteAsync(dbGuid);
            bool success = rows > 0;
            return success;
        }

        private async Task<UserName?> GetNameAsync(long dbId)
        {
            if (dbId < 1)
            {
                throw new InvalidDataException();
            }

            Databases.Models.Name? dbName = await _client.GetNameAsync(dbId);
            if (dbName is null)
            {
                return null;
            }

            UserName name = new()
            {
                First = dbName.First,
                Middle = dbName.Middle,
                Last = dbName.Last,
                Display = dbName.Display,
            };
            return name;
        }

        public async Task<bool> SetNameAsync(long dbId, UserName name)
        {
            ArgumentNullException.ThrowIfNull(name);

            Databases.Models.Name? dbName = await _client.GetNameAsync(dbId);
            int rows;
            if (dbName is not null)
            {
                dbName.First = name.First;
                dbName.Middle = name.Middle;
                dbName.Last = name.Last;
                dbName.Display = name.Display;
                rows = await _client.SetNameAsync(dbName);
            }
            else
            {
                dbName = new()
                {
                    ItemId = dbId,
                    First = name.First,
                    Middle = name.Middle,
                    Last = name.Last,
                    Display = name.Display
                };
                rows = await _client.CreateNameAsync(dbName);
            }
            bool success = rows > 0;
            return success;
        }

        public async Task<List<User>> GetListAsync(string? id, int limit)
        {
            Guid dbGuid;
            if (id != null)
            {
                if (!Guid.TryParse(id, out dbGuid))
                {
                    throw new ArgumentException(null, nameof(id));
                }
            }
            else
            {
                dbGuid = Guid.Empty;
            }

            List<Databases.Models.ListItem> dbItems = await _client.GetListAsync(dbGuid, limit);
            List<User> users = [];
            foreach (var dbItem in dbItems)
            {
                if ((dbItem.Id is not long dbItemId) ||
                    (dbItem.Guid is not Guid dbItemGuid))
                {
                    throw new InvalidDataException();
                }

                UserName? name = await GetNameAsync(dbItemId);
                User user = new()
                {
                    Id = dbItemGuid.ToString(),
                    Name = name,
                };
                users.Add(user);
            }
            return users;
        }

        public async Task<long> GetTotalCountAsync()
        {
            long count = await _client.GetTotalCountAsync();
            return count;
        }
    }
}
