namespace Spice.Models.ViewModels
{
    public class MenuItemViewModel
    {
        public IEnumerable<Category> CategoryList { get; set; }
        public MenuItem MenuItem { get; set; }
        public IEnumerable<SubCategory> SubCategoryList { get; set; }
    }
}
