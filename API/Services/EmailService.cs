using System.Transactions;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;

public class EmailService
 {

    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger) 
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(EmailSendDto emailSend) 
    {

        MailjetClient client = new MailjetClient(_configuration["MailJet:ApiKey"], _configuration["MailJet:Secret"]);

        var email = new TransactionalEmailBuilder()
            .WithFrom(new SendContact(_configuration["Email:From"], _configuration["Email:ApplicationName"]))
            .WithSubject(emailSend.Subject)
            .WithHtmlPart(emailSend.Body)
            .WithTo(new SendContact(emailSend.To))
            .Build();

        var response = await client.SendTransactionalEmailAsync(email);

        if (response.Messages != null)
        {
            if (response.Messages[0].Status == "success")
            {
                return true;
            }
        }

        this._logger.LogInformation($"Email response status {response.Messages![0].Status}");
        return false;
    }
}