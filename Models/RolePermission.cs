using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo_Course_Management.Models
{
    [Table("RolePermissions")]
    public class RolePermission
    {
        [Required]
        public int RoleId { get; set; } // FK → Role

        public Role Role { get; set; } = null!;

        [Required]
        public int PermissionId { get; set; } // FK → Permission

        public Permission Permission { get; set; } = null!;
    }
}
