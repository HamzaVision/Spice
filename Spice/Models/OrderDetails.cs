using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spice.Models
{
	public class OrderDetails
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int OrderId { get; set; }

		[ForeignKey("OrderId")]
		public virtual OrderHeader OrderHeader { get; set; }
		public int MenuItemId { get; set; }
		[ForeignKey("MenuItemId")]
		public virtual MenuItem MenuItem { get; set; }

		public int count { get; set; }
		public string Description { get; set; }
		public string Name { get; set; }
		[Required]
		public int Price { get; set; }
	}
}
