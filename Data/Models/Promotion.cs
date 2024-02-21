using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class Promotion
    {
        public int Id { get; set; }

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

        public ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();
    }
}
