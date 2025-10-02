using System.Net;
using System.Net.Mail;
using DBfirst.Helper;

namespace DBfirst.Helper
{
    public class SendMail
    {
        /*public static bool SendEmail(string to, string subject, string body, string attatchFile)
        {
            try
            {
                MailMessage msg = new MailMessage(EmailHelper.emailSender, to, subject, body);
                using (var client = new SmtpClient(EmailHelper.hostEmail, EmailHelper.portEmail))
                {
                    client.EnableSsl = true;

                    if(!string.IsNullOrEmpty(attatchFile))
                    {
                        Attachment attachment  = new Attachment(attatchFile);
                        msg.Attachments.Add(attachment);
                    }

                    NetworkCredential credential = new NetworkCredential(EmailHelper.emailSender, EmailHelper.passwordSender);
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = credential;
                    client.Send(msg);
                }
            }
            catch (SmtpException smtpEx)
            {
                // Log detailed information
                Console.WriteLine($"SMTP Exception: {smtpEx.Message} - Status Code: {smtpEx.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
            }

            return true;
        }*/
    
    }
}
