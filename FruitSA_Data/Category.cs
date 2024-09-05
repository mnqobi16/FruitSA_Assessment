using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FruitSA_Models
{
    public class Category
    {

        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]{3}\d{3}$", ErrorMessage = "Category Code must be in the format ABC123.")]
        public string CategoryCode { get; set; } // Format: ABC123
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Username { get; set; } = string.Empty;
        public DateTime? UpdateAt { get; set; }
    }
}
