// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementApi.Database.Models
{
    [Table("email")]
    public record class Email
    {
        [Column("id")]
        public long? Id { get; init; }
        [Column("version")]
        public int? Version { get; init; }
        [Column("item_id")]
        public long? ItemId { get; init; }
        [Column("address")]
        public string? Address { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        [Column("verified")]
        public bool? Verified { get; set; }
    }
}
