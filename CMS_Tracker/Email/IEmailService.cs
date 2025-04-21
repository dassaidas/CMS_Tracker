namespace CMS_Tracker.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string templateName, Dictionary<string, string> placeholders);
    }
}
