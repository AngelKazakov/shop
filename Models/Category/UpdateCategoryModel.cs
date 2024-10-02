namespace RandomShop.Models.Category
{
    public class UpdateCategoryModel
    {
        public int Id { get; set; }

        public string CurrentName { get; set; }

        public string NewName { get; set; }

        public int CurrentParrentCategoryId { get; set; }

        public int? NewParrentCategoryId { get; set; }

        public ICollection<MainCategoryViewModel> MainCategories { get; set; } = new List<MainCategoryViewModel>();
    }
}
