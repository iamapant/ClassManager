using DBfirst.Helper;
using DBfirst.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DBfirst.Services
{
    public class EmailService
    {
        private readonly EmailConfig _mailSettings;

        public EmailService(IOptions<EmailConfig> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string activeCode)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.DefaultSender));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = "Verification link";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
					<div style='display: flex; margin-left: 25%; height: max-content; justify-content: center; align-items: center; font-family: Arial, sans-serif;'>
						<div style='border: solid 1px lightgray; border-radius: 10px'>
							<h1 style='padding: 10px; padding-bottom: 20px; font-weight: 200; text-align: center; font-size: 25px; border-bottom: solid 1px lightgray'>Verify your account</h1>
							<div style='font-size: 14px; padding: 10px 20px'>
								<p>Verify yourself below to sign in to your StudentManagement account<br/> for <strong>{toEmail}</strong>.</p>
								<p>The link can only be used once and will expire after 10 minutes if not used.</p>
								<br/>
								<p>Use this code to finish verify your account:</p>
								<h1 style='text-align: center; font-weight: 500; font-size: 35px'>{activeCode}</h1>
								<p>If you do not require verification, please ignore this email.</p>
							</div>
						</div>
					</div>			
				"
            };
            email.Body = bodyBuilder.ToMessageBody();

            using (var smtp = new SmtpClient())
            {
                smtp.Connect(_mailSettings.Provider, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.DefaultSender, _mailSettings.Password);

                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
        }

        public async Task SendGradeAsync(string toEmail, Student student)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.DefaultSender));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = "Grade";
            var averageGrade = student.Evaluations.Any()
                ? student.Evaluations.Average(e => e.Grade)
                : 0;

            var bodyBuilder = new BodyBuilder();

            var htmlContent = @"
                <table style=""width: 30%; border-collapse: collapse; border: 1px solid black;"">
                    <thead>
                        <tr>
                            <th style=""border: 1px solid black;"" scope=""col"">Subject</th>
                            <th style=""border: 1px solid black;"" scope=""col"">Grade</th>
                        </tr>
                    </thead>
                    <tbody>";

            foreach (var evaluation in student.Evaluations)
            {
                htmlContent += $@"
                    <tr>
                        <td style=""border: 1px solid black; text-align: center"">{evaluation.AdditionExplanation}</td>
                        <td style=""border: 1px solid black; text-align: center"">{evaluation.Grade}</td>
                    </tr>";
            }

            htmlContent += $@"
                    </tbody>
                    <tfoot>
                        <tr>
                            <th style=""border: 1px solid black;"" colspan=""3"">Average: {averageGrade:F2}</th>
                        </tr>
                    </tfoot>
                </table>";

            bodyBuilder.HtmlBody = htmlContent;

            email.Body = bodyBuilder.ToMessageBody();

            using (var smtp = new SmtpClient())
            {
                smtp.Connect(_mailSettings.Provider, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.DefaultSender, _mailSettings.Password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
        }
    }
}
