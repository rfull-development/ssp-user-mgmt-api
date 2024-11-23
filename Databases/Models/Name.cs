// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementApi.Databases.Models
{
    [Table("name")]
    public record class Name
    {
        [Column("item_id")]
        public long? ItemId { get; init; }
        [Column("version")]
        public int? Version { get; init; }
        [Column("first")]
        public string? First { get; set; }
        [Column("middle")]
        public string? Middle { get; set; }
        [Column("last")]
        public string? Last { get; set; }
        [Column("display")]
        public string? Display { get; set; }
    }
}
