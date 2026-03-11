using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

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
        string host = GetRequiredSetting("SmtpSettings:Host");
        string username = GetRequiredSetting("SmtpSettings:Username");
        string password = GetRequiredSetting("SmtpSettings:Password");
        string fromName = GetRequiredSetting("SmtpSettings:FromName");
        int port = GetRequiredPort();
        SecureSocketOptions securityMode = GetSecurityMode();

        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(fromName, username));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart("html") { Text = htmlMessage };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(host, port, securityMode);
        await smtp.AuthenticateAsync(username, password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    private string GetRequiredSetting(string key)
    {
        string? value = this.config[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required SMTP setting '{key}'.");
        }

        return value;
    }

    private int GetRequiredPort()
    {
        string portValue = GetRequiredSetting("SmtpSettings:Port");

        if (!int.TryParse(portValue, out int port) || port <= 0)
        {
            throw new InvalidOperationException(
                $"Invalid SMTP setting 'SmtpSettings:Port'. Value '{portValue}' is not a valid port.");
        }

        return port;
    }

    private SecureSocketOptions GetSecurityMode()
    {
        string? rawValue = this.config["SmtpSettings:SecurityMode"];

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return SecureSocketOptions.SslOnConnect;
        }

        if (Enum.TryParse<SecureSocketOptions>(rawValue, true, out var securityMode))
        {
            return securityMode;
        }

        throw new InvalidOperationException(
            $"Invalid SMTP setting 'SmtpSettings:SecurityMode'. Value '{rawValue}' is not supported.");
    }
}
