using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace RandomShop.Services.Email;

public class MailSender : IEmailSender
{
    private readonly IConfiguration config;

    public MailSender(IConfiguration configuration)
    {
        this.config = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(config["SmtpSettings:FromName"], config["SmtpSettings:Username"]));

        email.To.Add(MailboxAddress.Parse(toEmail));

        email.Subject = subject;
        email.Body = new TextPart("html") { Text = htmlMessage };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            config["SmtpSettings:Host"],
            int.Parse(config["SmtpSettings:Port"]),
            SecureSocketOptions.SslOnConnect);

        await smtp.AuthenticateAsync(config["SmtpSettings:Username"], config["SmtpSettings:Password"]);

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}