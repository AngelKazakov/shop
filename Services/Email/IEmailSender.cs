namespace RandomShop.Services.Email;

public interface IEmailSender
{
    public Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
}