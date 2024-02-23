using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spice.Models
{
	public class OrderHeader
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public string ApplicationUserId { get; set; }

		[ForeignKey("ApplicationUserId")]
		public virtual ApplicationUser ApplicationUser { get; set; }
		[Required]
		public DateTime OrderDate { get; set; }
		[Required]
		public double OriginalTotal { get; set; }
		[Required]
		[Display(Name = "Order Total")]
		[DisplayFormat(DataFormatString = "{0:C}")]
		public double FinalTotal { get; set; }
		[Required]
		[Display(Name = "PickUp TIme")]
		public DateTime PickUpTime { get; set; }
		[Required]
		[NotMapped]
		public DateTime PickUpDate { get; set; }

		[Display(Name ="PickUp Name")]
		public string PickUpName{ get; set; }

		[Display(Name = "Coupon Code")]
		public string CouponCode { get; set; }
		public string CouponCodeDiscount { get; set; }
		public string Status { get; set; }
		public string PaymentStatus { get; set; }
		public string Comments { get; set; }

		[Display(Name = "Phone Number")]
		public string PickUpNumber { get; set; }
		public string TransactionId { get; set; }
	}
}
