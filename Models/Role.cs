using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Demo_Course_Management.Models.Enum;


namespace Demo_Course_Management.Models
{
    [Table("Roles")]
    public class Role : BaseEntity
    {
        [Required]
        public RoleType Name { get; set; } // Chỉ ADMIN, STAFF, CUSTOMER

        [MaxLength(250)]
        public string? Description { get; set; }
        public List<User> Users { get; set; } = new();

        public List<RolePermission> RolePermissions { get; set; } = new();

    }
}
