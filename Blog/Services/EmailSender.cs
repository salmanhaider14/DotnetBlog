
using Blog.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Blog.Services;

public class EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
 ILogger<EmailSender> logger, HttpClient httpClient) : IEmailSender<ApplicationUser>
{
    private readonly ILogger _logger = logger;
    public AuthMessageSenderOptions Options { get; } = optionsAccessor.Value;
    private readonly HttpClient _httpClient = httpClient;


    public Task SendConfirmationLinkAsync(ApplicationUser user, string email,
       string confirmationLink) => SendEmailAsync(email, "Confirm your email",
       "Please confirm your account by " +
       $"<a href='{confirmationLink}'>clicking here</a>.");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email,
    string resetLink) => SendEmailAsync(email, "Reset your password",
    $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email,
        string resetCode) => SendEmailAsync(email, "Reset your password",
        $"Please reset your password using the following code: {resetCode}");



    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(Options.SendGridKey))
        {
            throw new Exception("Null MailGunKey");
        }
        await Execute(Options.SendGridKey, Options.SendGridDomain, subject, message, toEmail);
    }

    public async Task Execute(string apiKey, string domain, string subject, string message, string toEmail)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(domain, "Password Recovery"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));
        msg.SetClickTracking(false, false);
        var response = await client.SendEmailAsync(msg);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Email to {toEmail} queued successfully!");
        }
        else
        {
            var error = await response.Body.ReadAsStringAsync();
            _logger.LogError($"Failure sending email to {toEmail}: {error}");
        }
    }
}
