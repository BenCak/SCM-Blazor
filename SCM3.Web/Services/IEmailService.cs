namespace SCM3.Web.Services;

// HTML email via the internal GA-ASI SMTP server, sent through MailKit (root CLAUDE.md
// §9, §10). Every triggered action sends one of these alongside its in-app notification.
public interface IEmailService
{
    Task SendAsync(string toAddress, string subject, string htmlBody, string? plainTextBody = null);
}
