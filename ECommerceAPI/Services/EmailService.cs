using System.Text.RegularExpressions;
using ECommerceAPI.Options;
using ECommerceAPI.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;

namespace ECommerceAPI.Services;

public class EmailService : IEmailService
{
    private readonly GmailSmtpOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<GmailSmtpOptions> options,
        ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Recipient email is required.", nameof(to));

        if (string.IsNullOrWhiteSpace(_options.Username))
            throw new InvalidOperationException("Gmail SMTP username is not configured.");

        if (string.IsNullOrWhiteSpace(_options.Password))
            throw new InvalidOperationException("Gmail SMTP password is not configured.");

        var fromAddress = string.IsNullOrWhiteSpace(_options.FromEmail)
            ? _options.Username
            : _options.FromEmail;

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_options.FromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.ReplyTo.Add(new MailboxAddress(_options.FromName, fromAddress));
        message.Subject = subject;

        // These matter a lot for spam scoring — MailKit does not set them
        // reliably on its own, and their absence is a strong spam signal.
        message.MessageId = MimeUtils.GenerateMessageId(
            (fromAddress.Split('@').ElementAtOrDefault(1)) ?? "gmail.com");
        message.Date = DateTimeOffset.UtcNow;
        message.Headers.Add("X-Mailer", "ECommerceAPI");
        message.Priority = MessagePriority.Normal;

        // Every send needs a real plain-text alternative. HTML-only mail
        // is one of the most common spam-filter triggers.
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = ConvertToPlainText(htmlBody)
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();

        try
        {
            _logger.LogInformation(
                "Connecting to Gmail SMTP {Host}:{Port}",
                _options.Host,
                _options.Port);

            await smtp.ConnectAsync(
                _options.Host,
                _options.Port,
                SecureSocketOptions.StartTls,
                cancellationToken);

            _logger.LogInformation(
                "Authenticating with Gmail SMTP as {Username}",
                _options.Username);

            await smtp.AuthenticateAsync(
                _options.Username,
                _options.Password,
                cancellationToken);

            _logger.LogInformation(
                "Sending email to {Email}. Subject: {Subject}",
                to,
                subject);

            var response = await smtp.SendAsync(message, cancellationToken);

            await smtp.DisconnectAsync(true, cancellationToken);

            // Log the SMTP response so you can confirm Gmail actually
            // accepted the message (vs. it silently vanishing later).
            _logger.LogInformation(
                "Email accepted by Gmail SMTP. To: {Email}, Subject: {Subject}, MessageId: {MessageId}, Response: {Response}",
                to,
                subject,
                message.MessageId,
                response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email to {Email}. Subject: {Subject}",
                to,
                subject);

            if (smtp.IsConnected)
            {
                try
                {
                    await smtp.DisconnectAsync(true);
                }
                catch
                {
                    // Ignore disconnect errors after the original failure.
                }
            }

            throw;
        }
    }

    /// <summary>
    /// Produces a reasonable plain-text fallback from an HTML body.
    /// Filters weigh text/html ratio and penalize HTML-only messages.
    /// </summary>
    private static string ConvertToPlainText(string html)
    {
        // Preserve link destinations as visible text before stripping tags,
        // since filters trust mail more when link text matches the href.
        var withLinks = Regex.Replace(
            html,
            "<a[^>]*href=[\"']([^\"']+)[\"'][^>]*>(.*?)</a>",
            "$2 ( $1 )",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        var noTags = Regex.Replace(withLinks, "<.*?>", " ", RegexOptions.Singleline);
        var decoded = System.Net.WebUtility.HtmlDecode(noTags);
        var collapsed = Regex.Replace(decoded, @"[ \t]+", " ");
        collapsed = Regex.Replace(collapsed, @"\s*\n\s*", "\n").Trim();

        return collapsed;
    }
}