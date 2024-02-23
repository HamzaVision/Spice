namespace Spice.Models.ViewModels
{
	public class OrderDetailsAndPagingViewModel
	{
		public IList<OrderDetailsViewModel> OrderDetailsList { get; set; }
		public PagingInfo pagingObj { get; set; }
	}
}
