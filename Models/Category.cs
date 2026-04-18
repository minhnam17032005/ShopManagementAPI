using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo_Course_Management.Models
{
    [Table("Categories")]
    public class Category : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!; // Tên danh mục

        [MaxLength(250)]
        public string? Description { get; set; } // Mô tả optional
        public bool IsActive { get; set; } = true;

        public List<Product> Products { get; set; } = new();
    }
}
