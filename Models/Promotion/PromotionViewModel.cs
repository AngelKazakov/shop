using RandomShop.Data;
using System.ComponentModel.DataAnnotations;

namespace RandomShop.Models.Promotion
{
    public class PromotionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int DiscountRate { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);
    }
}
