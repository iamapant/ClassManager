
namespace DBfirst.Helper
{
    public interface IEmailHelper
    {
        Task SendEmailAsync(EmailRequest emailRequest);
    }
}