namespace RandomShop.Models.Category
{
    public class MainCategoryViewModel
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public ICollection<SubCategoryModel> SubCategories { get; set; } = new List<SubCategoryModel>();
    }
}
