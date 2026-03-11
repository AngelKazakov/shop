using System.Text;
using RandomShop.Data.Models;

namespace RandomShop.Services.Email;

public class EmailTemplateService : IEmailTemplateService
{
    public string BuildOrderConfirmationHtml(Data.Models.User user, ShopOrder order)
    {
        var itemsHtml = new StringBuilder();

        foreach (var item in order.OrderLines)
        {
            string productName = item.ProductItem?.Product?.Name ?? "Product";

            itemsHtml.Append($@"
        <tr>
            <td style='padding: 12px; border-bottom: 1px solid #eeeeee; font-size: 14px;'>{productName}</td>
            <td style='padding: 12px; border-bottom: 1px solid #eeeeee; text-align: center; font-size: 14px;'>{item.Quantity}</td>
            <td style='padding: 12px; border-bottom: 1px solid #eeeeee; text-align: right; font-size: 14px;'>{item.Price:F2} лв.</td>
        </tr>");
        }

        return $@"
    <div style='background-color: #f4f7f6; padding: 20px; font-family: Helvetica, Arial, sans-serif;'>
        <div style='max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
            <div style='background-color: #1a2a3a; padding: 30px; text-align: center; color: #ffffff;'>
                <h1 style='margin: 0; font-size: 26px;'>RANDOM SHOP</h1>
            </div>
            <div style='padding: 30px;'>
                <h2 style='color: #1a2a3a; margin-top: 0;'>Order Confirmation</h2>
                <p>Hi <strong>{user.UserName}</strong>,</p>
                <p>Thank you for your purchase! Here is your receipt for <strong>Order #{order.OrderNumber}</strong>:</p>
                
                <table style='width: 100%; border-collapse: collapse; margin-top: 20px;'>
                    <thead>
                        <tr style='border-bottom: 2px solid #1a2a3a;'>
                            <th style='text-align: left; padding: 10px 0;'>Product</th>
                            <th style='text-align: center; padding: 10px 0;'>Qty</th>
                            <th style='text-align: right; padding: 10px 0;'>Price</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml.ToString()}
                    </tbody>
                </table>

                <div style='text-align: right; margin-top: 20px;'>
                    <p style='margin: 5px 0; color: #666;'>Subtotal: {order.OrderTotal:F2} лв.</p>
                    <h2 style='margin: 10px 0; color: #1a2a3a;'>Total: {order.OrderTotal:F2} лв.</h2>
                </div>
            </div>
            <div style='background-color: #f9f9f9; padding: 20px; text-align: center; font-size: 12px; color: #999;'>
                <p>© 2026 Random Shop | Varna, Bulgaria</p>
            </div>
        </div>
    </div>";
    }
}