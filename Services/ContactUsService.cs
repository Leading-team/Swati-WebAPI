using Microsoft.Data.SqlClient;
using SWWebAPI.Models.Entities;
using System.Data;

namespace SWWebAPI.Services
{
    public class ContactUsService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public ContactUsService(
            IConfiguration configuration,
            IEmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<long> SubmitContactAsync(
            ContactUsRequest request,
            CancellationToken cancellationToken)
        {
            long contactId;
            try
            {
                var connectionString =
                    _configuration.GetConnectionString("DefaultConnection");

                // 1. Save Contact Us request
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    const string sql = @"
                INSERT INTO contactus
                (FullName, Email, Phone, enquiry_type, Message,language)
                OUTPUT INSERTED.contact_id
                VALUES
                (@FullName, @Email, @Phone, @Subject, @Message, @Language)";

                    using var command = new SqlCommand(sql, connection);

                    command.Parameters.Add("@FullName", SqlDbType.NVarChar, 150)
                        .Value = request.FullName;

                    command.Parameters.Add("@Email", SqlDbType.NVarChar, 200)
                        .Value = request.Email;

                    command.Parameters.Add("@Phone", SqlDbType.NVarChar, 20)
                        .Value = (object?)request.Phone ?? DBNull.Value;

                    command.Parameters.Add("@Subject", SqlDbType.NVarChar, 250)
                        .Value = (object?)request.EnquiryType ?? DBNull.Value;

                    command.Parameters.Add("@Message", SqlDbType.NVarChar, -1)
                        .Value = request.Message;

                    command.Parameters.Add("@Language", SqlDbType.NVarChar, 10)
                        .Value = (object?)request.Language ?? DBNull.Value;

                    contactId = Convert.ToInt64(
                        await command.ExecuteScalarAsync(cancellationToken));
                }

                // 2. Send email notification
                try
                {
                    await _emailService.SendContactEmailAsync(
                        request, cancellationToken);

                    // 3. Update email status
                    await UpdateEmailStatusAsync(
                        contactId, true, null, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Contact request is already saved.
                    await UpdateEmailStatusAsync(
                        contactId, false, ex.Message, cancellationToken);
                }

                return contactId;
            }
            catch (OperationCanceledException)
      when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                //_logger.LogError(
                //    ex,
                //    "Error occurred while submitting Contact Us request.");

                throw;
            }
        }

        private async Task UpdateEmailStatusAsync(
            long contactId,
            bool emailSent,
            string? error,
            CancellationToken cancellationToken)
        {
            var connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = @"
            UPDATE ContactUs
            SET IsemailSent = @IsEmailSent,
                email_sentdate = @EmailSentDate,
                email_error = @EmailError
            WHERE contact_id = @ContactId";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@ContactId", SqlDbType.BigInt)
                .Value = contactId;

            command.Parameters.Add("@IsEmailSent", SqlDbType.Bit)
                .Value = emailSent;

            command.Parameters.Add("@EmailSentDate", SqlDbType.DateTime2)
                .Value = emailSent ? DateTime.UtcNow : DBNull.Value;

            command.Parameters.Add("@EmailError", SqlDbType.NVarChar, 1000)
                .Value = (object?)error ?? DBNull.Value;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
