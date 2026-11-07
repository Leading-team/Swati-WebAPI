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
    Task SendSubscriptionPaymentEmailAsync(
    SubscriptionPaymentEmailRequest request,
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
    public async Task SendSubscriptionPaymentEmailAsync(
    SubscriptionPaymentEmailRequest request,
    CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            _settings.FromName,
            _settings.FromEmail));

        message.To.Add(MailboxAddress.Parse(request.Email));

        message.Subject = "Subscription Payment Successful - Swati Magazine";

        message.Body = new BodyBuilder
        {
            HtmlBody = $@"
        <div style='font-family:Arial,sans-serif;
                    max-width:650px;margin:auto;
                    border:1px solid #ddd;padding:25px;'>

            <h2 style='color:#2e7d32;'>
                Payment Successful!
            </h2>

            <p>Dear {System.Net.WebUtility.HtmlEncode(request.SubscriberName)},</p>

            <p>
                Thank you for subscribing to Swati Magazine.
                Your payment has been successfully received.
            </p>

            <h3>Subscription Details</h3>

            <table style='width:100%;border-collapse:collapse;'>
                <tr>
                    <td style='padding:8px;'>Plan</td>
                    <td style='padding:8px;'>{System.Net.WebUtility.HtmlEncode(request.PlanName)}</td>
                </tr>
                <tr>
                    <td style='padding:8px;'>Edition</td>
                    <td style='padding:8px;'>{System.Net.WebUtility.HtmlEncode(request.Edition)}</td>
                </tr>
                <tr>
                    <td style='padding:8px;'>Subscription Term</td>
                    <td style='padding:8px;'>{System.Net.WebUtility.HtmlEncode(request.SubscriptionTerm)}</td>
                </tr>
                <tr>
                    <td style='padding:8px;'>Subscription Amount</td>
                    <td style='padding:8px;'>₹{request.SubscriptionAmount:N2}</td>
                </tr>
                <tr>
                    <td style='padding:8px;'>Courier Charges</td>
                    <td style='padding:8px;'>₹{request.CourierCharges:N2}</td>
                </tr>
               
                <tr style='font-weight:bold;'>
                    <td style='padding:8px;'>Total Amount Paid</td>
                    <td style='padding:8px;'>₹{request.TotalAmount:N2}</td>
                </tr>
                <tr>
                    <td style='padding:8px;'>Payment ID</td>
                    <td style='padding:8px;'>{System.Net.WebUtility.HtmlEncode(request.PaymentId)}</td>
                </tr>
                <tr>
                    <td style='padding:8px;'>Order ID</td>
                    <td style='padding:8px;'>{System.Net.WebUtility.HtmlEncode(request.OrderId)}</td>
                </tr>
                <tr>
                    <td style='padding:8px;'>Subscription Start Date</td>
                    <td style='padding:8px;'>{request.StartDate?.ToString("dd-MMM-yyyy") ?? "-"}</td>
                </tr>
                <tr>
                    <td style='padding:8px;'>Subscription End Date</td>
                    <td style='padding:8px;'>{request.EndDate?.ToString("dd-MMM-yyyy") ?? "-"}</td>
                </tr>
            </table>

            <p>
                Thank you for choosing Swati Magazine.
            </p>

            <p>
                Regards,<br/>
                Swati Magazine Team
            </p>
        </div>"
        }.ToMessageBody();

        using var client = new MailKit.Net.Smtp.SmtpClient();

        await client.ConnectAsync(
            _settings.Host,
            _settings.Port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        try
        {
            await client.AuthenticateAsync(
                _settings.Username,
                _settings.Password,
                cancellationToken);

            await client.SendAsync(message, cancellationToken);
        }
        finally
        {
            await client.DisconnectAsync(true, cancellationToken);
        }
    }

}