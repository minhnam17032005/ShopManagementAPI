using System.ComponentModel.DataAnnotations;

namespace Demo_Course_Management.DTOs.request
{
    public class UpdateStockDTO
    {
        [Required]
        public int Stock { get; set; }
    }
}
