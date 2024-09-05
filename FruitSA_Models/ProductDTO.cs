using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FruitSA_Models
{
    public class ProductDTO
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Please Enter a Product Name")]
        public string? ProductName { get; set; }

        [Required(ErrorMessage = "Please Enter a Product Code")]
        public string? ProductCode { get; set; }  // Generated in the 'Create' action

        [ValidateNever]
        [Display(Name = "Product Image")]
        public string? ImagePath { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please provide a Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
        public double Price { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public CategoryDTO CategoryDTO { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Username { get; set; } = string.Empty;
        public DateTime? UpdateAt { get; set; }
    }
}
