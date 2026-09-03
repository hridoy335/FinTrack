using System.Net;
using System.Net.Mail;
using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace FinTrackCore.Infrastructure.Services;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _smtpSettings;

    public SmtpEmailSender(IOptions<SmtpSettings> smtpOptions)
    {
        _smtpSettings = smtpOptions.Value;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_smtpSettings.Host)
            || string.IsNullOrWhiteSpace(_smtpSettings.FromEmail))
        {
            throw new InvalidOperationException("Smtp settings are missing.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
        {
            EnableSsl = _smtpSettings.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_smtpSettings.UserName))
        {
            client.Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password);
        }

        await client.SendMailAsync(message, ct);
    }
}
