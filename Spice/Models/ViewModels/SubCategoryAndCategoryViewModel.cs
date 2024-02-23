namespace Spice.Models.ViewModels
{
    public class SubCategoryAndCategoryViewModel
    {
        public IEnumerable<Category> Categories { get; set;}
        public SubCategory subCategory { get; set;}
        public List<string> Subcategories { get; set;}
        public string ErrorMessage { get; set;}
    }
}
