using MessagePack;
using Microsoft.Build.Execution;
using System.ComponentModel.DataAnnotations;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace Spice.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string CouponType { get; set; }
        public enum ECouponType {percent = 0, Dollar=1 }

        [Required]
        public double discount { get; set; }

        [Required]
        public double MinimumAmount { get; set; }

        public byte[] Picture { get; set; }

        public bool IsActive { get; set; }
    }
}
