using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spice.Models
{
    public class ShoppingCart
    {
        public ShoppingCart()
        {
            count= 1;
        }
        [Required]
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        [NotMapped]
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
        public int MenuItemId { get; set; }
        [NotMapped]
        [ForeignKey("MenuItemId")]
        public virtual MenuItem MenuItem { get; set; }
        [Range(1,int.MaxValue,ErrorMessage ="Please enter a valid Count")]
        public int count { get; set; }
    }
}
