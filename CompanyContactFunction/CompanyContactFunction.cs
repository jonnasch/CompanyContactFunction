using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CompanyContactFunction
{
    public static class CompanyContactFunction
    {
        [FunctionName("CompanyContactFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

           
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string name = data?.name;
            string email = data?.email;
            string message = data?.contactMessage; 
            var succsess = await SendMail(name, email, message);
            
            if (succsess)
            {
                return new OkObjectResult("Mail skickat");

            }
            else { return new BadRequestResult(); }

        }

        public static async Task<bool> SendMail(string name, string email, string message)
        {
            var apiKey = Environment.GetEnvironmentVariable("API_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("info@schultzberg.nu", "Hemsidan");
            var subject = "Ny kontaktbegäran";
            var to = new EmailAddress("jonna.schultzberg@live.se", "Jonna");
            var plainTextContent = ComposeMessage(name, email, message);
            var htmlContent = ComposeHtmlMessage(name, email, message);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response.IsSuccessStatusCode;
        }

        private static string ComposeMessage(string name, string email, string message)
        {
            return $"Tog emot ett meddelande från {name} ({email}): \n\n {message}";
        }
        private static string ComposeHtmlMessage(string name, string email, string message)
        {
            return $"Tog emot ett meddelande från <strong>{name} ({email}):</strong> <br><br> {message}";
        }
    }

}
