using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace C_C_Proj_WebStore.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Shoe Model")]
        public string ShoeModel { get; set; }

        [Required]
        [DisplayName("Shoe Brand")]
        public string Brand { get; set; }

        [Required]
        [DisplayName("Age Group")]
        public string AgeGroup { get; set; }

        [Required]
        [DisplayName("Stock Count")]
        public int StockCount { get; set; }

        [Required]
        [DisplayName("List Price")]
        public double ListPrice { get; set; }

        [Required]
        [DisplayName("Price for 1-50")]
        public double Price { get; set; }

        [Required]
        [DisplayName("Price for 50+")]
        public double Price50 { get; set; }

        [Required]
        [DisplayName("Price for 100+")]
        public double Price100 { get; set; }

        [Required]
        [DisplayName("Shoe Description")]
        public string Description { get; set; }

        [Required]
        [DisplayName("Shoe Size")]
        public double Size { get; set; }

        [Required]
        [DisplayName("Shoe Color")]
        public string Color { get; set; }

        [Required]
        [DisplayName("Gender")]
        public string Gender { get; set; }

        [ValidateNever]
        [DisplayName("Stock Status")]
        public string? StockStatus { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }

        [Required]
        [DisplayName("Category")]
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

        [ValidateNever]
        public List<ProductImage> ProductImages {  get; set; }
    }
}
