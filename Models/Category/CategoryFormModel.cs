using RandomShop.Data;
using System.ComponentModel.DataAnnotations;

namespace RandomShop.Models.Category
{
    public class CategoryFormModel
    {
        [Required]
        [StringLength(DataConstants.productCategoryMaxLength, MinimumLength = DataConstants.productCategoryMinLength)]
        public string Name { get; set; }

        //public int? ParentCategoryId { get; set; }

        public int? SelectedCategoryFromDropDown { get; set; }
    }
}
