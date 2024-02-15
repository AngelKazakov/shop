using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class Country
    {
        public int Id { get; set; }

        [Required]
        [StringLength(DataConstants.countryNameMaxLength)]
        public string Name { get; set; }

        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
