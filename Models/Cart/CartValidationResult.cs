namespace RandomShop.Models.Cart;

public class CartValidationResult
{
    public bool IsValid => !Errors.Any();

    public List<string> Errors { get; set; } = new List<string>();
}