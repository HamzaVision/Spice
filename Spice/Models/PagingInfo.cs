namespace Spice.Models
{
	public class PagingInfo
	{
		public int totalItems { get; set; }
		public int itemsPerPage { get; set; }
		public int CurrentPage { get; set; }
		public string urlParam { get; set; }
		public int TotalPage => (int)Math.Ceiling((decimal)totalItems / itemsPerPage);
		
	}
}
