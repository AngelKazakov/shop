using RandomShop.Models.Email;

namespace RandomShop.Services.Email;

public interface IEmailTemplateService
{
    public string BuildOrderConfirmationHtml(OrderConfirmationEmailModel model);
}
