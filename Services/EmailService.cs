using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SWWebAPI.Models.Entities;
using System.Net.Mail;

public interface IEmailService
{
    Task SendContactEmailAsync(
        ContactUsRequest request,
        CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendContactEmailAsync(
        ContactUsRequest request,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            _settings.FromName,
            _settings.FromEmail));

        //message.To.Add(MailboxAddress.Parse(_settings.ToEmail));
        message.To.Add(MailboxAddress.Parse(request.Email));

        // Customer email will be available as Reply-To.
        message.ReplyTo.Add(
            MailboxAddress.Parse(request.Email));

        message.Subject = $"Contact Us - {request.EnquiryType}";
          // < p >< b > Subject:</ b > { System.Net.WebUtility.HtmlEncode(request.EnquiryType ?? "")}</ p >
        message.Body = new BodyBuilder
        {
            HtmlBody = $@"
                <h2>New Enquiry – Swati Magazine</h2>
                <p><b>Name:</b> {System.Net.WebUtility.HtmlEncode(request.FullName)}</p>
                <p><b>Email:</b> {System.Net.WebUtility.HtmlEncode(request.Email)}</p>
                <p><b>Phone:</b> {System.Net.WebUtility.HtmlEncode(request.Phone ?? "")}</p>
                <p><b>Message:</b></p>
                <p>{System.Net.WebUtility.HtmlEncode(request.Message).Replace("\n", "<br/>")}</p>"
        }.ToMessageBody();

        var client = new MailKit.Net.Smtp.SmtpClient();

        await client.ConnectAsync(
            _settings.Host,
            _settings.Port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await client.AuthenticateAsync(
            _settings.Username,
            _settings.Password,
            cancellationToken);

        var response = await client.SendAsync(message, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);
    }
}