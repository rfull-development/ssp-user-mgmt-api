﻿// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
namespace UserManagementApi.Databases.Interfaces
{
    public interface IDatabaseClientFactory
    {
        IUserDatabaseClient CreateUserDatabaseClient();
    }
}
