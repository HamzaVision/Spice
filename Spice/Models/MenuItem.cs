using MessagePack;
using Microsoft.Build.Framework;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace Spice.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string Name { get; set; }
    
        public string description { get; set; }
        public string spicyness;
        public enum Espicy { NA=0,NotSpicy=1,Spicy=2,VerySpicy=3}
        public string Image { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }

        [DisplayName("SubCategory")]
        public int SubCategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("SubCategoryId")]
        public virtual SubCategory SubCategory { get; set; }
    
        [Range(1,int.MaxValue,ErrorMessage="Price Should be Greater then ${1}")]
        public double Price { get; set; }
    }
}
