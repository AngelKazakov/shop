namespace RandomShop.Areas.Admin.Models;

public class AdminProductDeleteResult
{
    public int RequestedCount { get; set; }

    public int DeletedCount { get; set; }

    public int SkippedCount { get; set; }

    public List<int> DeletedIds { get; set; } = new();

    public List<int> SkippedIds { get; set; } = new();

    public List<string> Errors { get; set; } = new();
}