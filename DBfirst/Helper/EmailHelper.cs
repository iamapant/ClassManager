using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace DBfirst.Helper
{
    public class EmailHelper : IEmailHelper
    {
        EmailConfig _emailConfig;
        public EmailHelper(IOptions<EmailConfig> emailConfig)
        {
            _emailConfig = emailConfig.Value;
        }
        public async Task SendEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient(_emailConfig.Provider, _emailConfig.Port);
                smtpClient.Credentials = new NetworkCredential(_emailConfig.DefaultSender, _emailConfig.Password);
                smtpClient.EnableSsl = true;

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_emailConfig.DefaultSender);
                mailMessage.To.Add(emailRequest.To);
                mailMessage.Subject = emailRequest.Subject;
                mailMessage.Body = emailRequest.Content;

                if (emailRequest.AttachmentFilePaths.Length > 0)
                {
                    foreach (var path in emailRequest.AttachmentFilePaths)
                    {
                        Attachment attachment = new Attachment(path);
                        mailMessage.Attachments.Add(attachment);
                    }
                }
                await smtpClient.SendMailAsync(mailMessage);

                mailMessage.Dispose();
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
