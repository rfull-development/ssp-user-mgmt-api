// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using UserManagementApi.Databases.Models;

namespace UserManagementApi.Databases.Interfaces
{
    public interface IUserDatabaseClient : IDisposable
    {
        public Task<Guid> CreateAsync();
        public Task<Item?> GetAsync(Guid guid);
        public Task<int> DeleteAsync(Guid guid);
        public Task<int> CreateNameAsync(Name name);
        public Task<Name?> GetNameAsync(long itemId);
        public Task<int> SetNameAsync(Name name);
        public Task<List<ListItem>> GetListAsync(Guid guid, int limit);
        public Task<long> GetTotalCountAsync();
    }
}
