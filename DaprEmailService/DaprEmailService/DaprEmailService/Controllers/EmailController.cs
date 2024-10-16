using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DaprEmailService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        // Inject HttpClientFactory
        public EmailController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // POST: /Email/send
        [HttpPost("send")]
        public async Task<IActionResult> SendEmail()
        {
            // Dapr binding to invoke ACS
            var daprUrl = "http://localhost:3500/v1.0/bindings/azcommunicationresource";

            var emailPayload = new
            {
                operation = "post",
                data = new
                {
                    subject = "Test Email",
                    body = "Hello, this is a test email sent via Dapr and ACS."
                },
                metadata = new
                {
                    Authorization = "HMAC-SHA256 SignedHeaders=x-ms-date;host;x-ms-content-sha256&Signature=generated-signature"
                }
            };

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(emailPayload);
            var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Send request to Dapr sidecar
            var response = await _httpClient.PostAsync(daprUrl, requestContent);
            if (response.IsSuccessStatusCode)
            {
                return Ok("Email sent successfully!");
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to send email.");
            }
        }
    }
}
