using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models;

public class EmailLog
{
    public int Id { get; set; }

    [Required]
    public string Recipient { get; set; } = null!;

    [Required]
    public string Subject { get; set; } = null!;

    [Required]
    public string Body { get; set; } = null!;

    public DateTime SendAt { get; set; } = DateTime.UtcNow;

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public int ShopOrderId { get; set; }

    public ShopOrder ShopOrder { get; set; }
}