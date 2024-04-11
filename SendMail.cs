using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HassallGroup.Function
{
    public class SendMail
    {
        private readonly ILogger<SendMail> _logger;
        private readonly ReCaptchaValidationService _reCaptchaValidationService;

        public SendMail(ILogger<SendMail> logger, ReCaptchaValidationService reCaptchaValidationService)
        {
            _logger = logger;
            _reCaptchaValidationService = reCaptchaValidationService;
        }

        [Function("SendMail")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {

            body = await JsonSerializer.DeserializeAsync<FormBody>(req.Body);

            bool isHuman = _reCaptchaValidationService.SendMail(body.token);

            if (!isHuman)
            {
                return new BadObjectResult("reCaptcha failed");
            }

            string connectionString = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
            string recipientAddress = Environment.GetEnvironmentVariable("RECIPIENT_ADDRESS");
            string senderAddress = Environment.GetEnvironmentVariable("SENDER_ADDRESS");

            var emailClient = new EmailClient(connectionString);


            EmailSendOperation emailSendOperation = emailClient.Send(
                WaitUntil.Completed,
                senderAddress: senderAddress,
                recipientAddress: recipientAddress,
                subject: body.Data.subject,
                htmlContent: 
                $"""
                    <html>
                        <p>Client Name: {name}.</p>
                        <p>Subject:     {subject}</p>
                        <p>Message:     {message}</p>
                    </html>
                """,
                plainTextContent: $"""
                        Client Name: {name}.
                        Subject:     {subject}
                        Message:     {message}
                """);

            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }

    public class FormBody{
        //       data: { name, email, subject, message },
         //      token,

        public object Data;
        public string Token;
    }
}
