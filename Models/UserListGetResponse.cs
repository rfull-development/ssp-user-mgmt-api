﻿// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.Text.Json.Serialization;

namespace UserManagementApi.Models
{
    public record class UserListGetResponse
    {
        [JsonPropertyName("totalCount")]
        [JsonRequired]
        public required long TotalCount { get; init; }
        [JsonPropertyName("count")]
        [JsonRequired]
        public required long Count { get; init; }
        [JsonPropertyName("users")]
        [JsonRequired]
        public required List<User> Users { get; init; }
    }
}
