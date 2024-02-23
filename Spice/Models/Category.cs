using MessagePack;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace Spice.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        
        [Display(Name="Category Name")]
        [Required]
        public string Name { get; set; }
    }
}
