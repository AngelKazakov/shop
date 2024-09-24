namespace RandomShop.Models.Category
{
    public class CategoryFormViewModel
    {
        public CategoryFormModel CategoryFormModel { get; set; }

        public CategoryViewModel? CategoryViewModel { get; set; }

        public MainCategoryViewModel? MainCategoryViewModel { get; set; }

        public ICollection<MainCategoryViewModel> MainCategories { get; set; } = new List<MainCategoryViewModel>();
    }
}
