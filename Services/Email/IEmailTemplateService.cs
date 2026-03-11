using RandomShop.Data.Models;

namespace RandomShop.Services.Email;

public interface IEmailTemplateService
{
    public string BuildOrderConfirmationHtml(Data.Models.User user,ShopOrder order);
}