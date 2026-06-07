using System.Text.RegularExpressions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SCM3.Web.Services;

public partial class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendAsync(string toAddress, string subject, string htmlBody, string? plainTextBody = null)
    {
        var smtp = configuration.GetSection("Smtp");
        var host = smtp["Host"];

        // user-secrets only — SMTP host is never committed (root CLAUDE.md §10, §17.3).
        // Demo mode runs without it, so log and skip rather than throwing.
        if (string.IsNullOrWhiteSpace(host))
        {
            logger.LogWarning("Smtp:Host is not configured — skipping email to {ToAddress} (\"{Subject}\")", toAddress, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtp["FromName"] ?? "SCM3 Portal", smtp["FromAddress"] ?? "scm3-portal@ga-asi.com"));
        message.To.Add(MailboxAddress.Parse(toAddress));
        message.Subject = subject;
        message.Body = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = plainTextBody ?? StripHtmlRegex().Replace(htmlBody, string.Empty)
        }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(host, smtp.GetValue("Port", 587), SecureSocketOptions.StartTls);

        var username = smtp["Username"];
        if (!string.IsNullOrWhiteSpace(username))
        {
            await client.AuthenticateAsync(username, smtp["Password"] ?? string.Empty);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(quit: true);
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex StripHtmlRegex();
}
