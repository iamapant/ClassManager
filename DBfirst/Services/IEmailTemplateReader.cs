
namespace DBfirst.Services
{
    public interface IEmailTemplateReader
    {
        Task<string> GetTemplate(string templateName);
    }
}