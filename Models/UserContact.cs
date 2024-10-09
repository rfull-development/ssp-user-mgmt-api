// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.Text.Json.Serialization;

namespace UserManagementApi.Models
{
    public record class UserContact
    {
        [JsonPropertyName("emails")]
        public List<Email>? Emails { get; set; }
    }
}
