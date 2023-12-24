using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace App.Services
{
    public class MailSettings
    {
        public required string Mail { set; get; }
        public required string DisplayName { set; get; }
        public required string Password { set; get; }
        public required string Host { set; get; }
        public int Port { set; get; }
    }

    public class SendMailService : IEmailSender
    {
        private readonly MailSettings mailSettings;

        private readonly ILogger<SendMailService> _logger;

        public SendMailService(IOptions<MailSettings> _mailsettings, ILogger<SendMailService> logger)
        {
            mailSettings = _mailsettings.Value;
            _logger = logger;
            _logger.LogInformation("Created SendMailServices");
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.Sender = new MailboxAddress(mailSettings.DisplayName, mailSettings.Mail);
            message.From.Add(new MailboxAddress(mailSettings.DisplayName, mailSettings.Mail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                smtp.Connect(mailSettings.Host, mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(mailSettings.Mail, mailSettings.Password);
                await smtp.SendAsync(message);
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory("MailSave");
                var emailSave = string.Format(@"MailSave/{0}.eml", Guid.NewGuid());
                await message.WriteToAsync(emailSave);

                _logger.LogInformation("Lỗi gửi email tới " + email);
                _logger.LogError(ex.Message);
            }

            smtp.Disconnect(true);
            _logger.LogInformation("Send email to " + email);
        }
    }
}