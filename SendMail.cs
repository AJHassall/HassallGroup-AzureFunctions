using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.Communication.Email;

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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {

            FormBody body = await JsonSerializer.DeserializeAsync<FormBody>(req.Body);

            bool isHuman = await _reCaptchaValidationService.VerifyReCaptchaAsync(body.Token);

            if (isHuman)
            {
                return new BadRequestObjectResult("reCaptcha failed");
            }

            string connectionString = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
            string recipientAddress = Environment.GetEnvironmentVariable("RECIPIENT_ADDRESS");
            string senderAddress = Environment.GetEnvironmentVariable("SENDER_ADDRESS");

            var emailClient = new EmailClient(connectionString);


            EmailSendOperation emailSendOperation = emailClient.Send(
                WaitUntil.Completed,
                senderAddress: senderAddress,
                recipientAddress: recipientAddress,
                subject: "body.Data.Subject",
                htmlContent: 
                $"""
                    <html>
                        <p>Client Name: {"body.Data.Name"}.</p>
                        <p>Subject:     {"body.Data.Subject"}</p>
                        <p>Message:     {"body.Data.Message"}</p>
                    </html>
                """,
                plainTextContent: $"""
                        Client Name: {"body.Data.Name"}.
                        Subject:     {"body.Data.Subject"}
                        Message:     {"body.Data.Message"}
                """);

            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }

    public class FormBody{
        //       data: { name, email, subject, message },
         //      token,

        public Data Data;
        public string Token;
    }
        public class Data{
        //       data: { name, email, subject, message },
         //      token,

        public string Name;
        public string Subject; 
        public string Message; 
    }
}
