using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace RAI.Lab3.WebApp.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _host = configuration["Smtp:Host"]
            ?? throw new InvalidOperationException("SMTP host is not configured");
        _port = int.Parse(configuration["Smtp:Port"]
            ?? throw new InvalidOperationException("SMTP port is not configured"));
        _username = configuration["Smtp:Username"]
            ?? throw new InvalidOperationException("SMTP username is not configured");
        _password = configuration["Smtp:Password"]
            ?? throw new InvalidOperationException("SMTP password is not configured");
        _fromEmail = configuration["Smtp:FromEmail"]
            ?? throw new InvalidOperationException("SMTP from email is not configured");
        _fromName = configuration["Smtp:FromName"] ?? "RAI Lab3 Application";
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // Connect to the SMTP server
            await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);

            // Authenticate
            await client.AuthenticateAsync(_username, _password);

            // Send the email
            await client.SendAsync(message);

            // Disconnect
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", email);
            throw;
        }
    }
}
