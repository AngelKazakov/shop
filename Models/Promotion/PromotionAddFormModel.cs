using RandomShop.Data;
using System.ComponentModel.DataAnnotations;

namespace RandomShop.Models.Promotion
{
    public class PromotionAddFormModel
    {
        [Required]
        [StringLength(DataConstants.Promotion.nameMaxLength, MinimumLength = DataConstants.Promotion.nameMinLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(DataConstants.Promotion.descriptionMaxLength, MinimumLength = DataConstants.Promotion.descriptionMinLength)]
        public string Description { get; set; }

        [Range(DataConstants.Promotion.discountMinRate, DataConstants.Promotion.discountMaxRate)]
        public int DiscountRate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
