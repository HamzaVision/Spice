namespace Spice.Models.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<MenuItem> MenuItems{ get; set; }
        public IEnumerable<Category> categories { get; set; }
        public IEnumerable<Coupon> coupons { get; set; }
    }
}
