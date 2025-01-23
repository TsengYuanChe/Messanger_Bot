using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/messenger")]
public class MessengerController : ControllerBase
{
    private readonly MessengerService _messengerService;

    public MessengerController(MessengerService messengerService)
    {
        _messengerService = messengerService;
    }

    // Webhook 驗證端點
    [HttpGet("webhook")]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string verifyToken,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        const string ExpectedVerifyToken = "adam880614"; // 替換為您設定的驗證權杖

        if (mode == "subscribe" && verifyToken == ExpectedVerifyToken)
        {
            Response.ContentType = "text/plain";
            Console.WriteLine("Webhook verification succeeded.");
            return Ok(challenge); // 返回 challenge 值以完成驗證
        }

        Console.WriteLine("Webhook verification failed.");
        return Unauthorized();
    }

    // 接收訊息
    [HttpPost("webhook")]
    public IActionResult Webhook([FromBody] JsonElement request)
    {
        try
        {
            // Log incoming request
            Console.WriteLine($"Incoming Webhook: {request}");

            // Parse JSON
            var root = JsonSerializer.Deserialize<WebhookRequest>(request.GetRawText());
            if (root?.Entry != null)
            {
                foreach (var entry in root.Entry)
                {
                    foreach (var messaging in entry.Messaging)
                    {
                        var senderId = messaging.Sender?.Id;
                        var text = messaging.Message?.Text;

                        if (!string.IsNullOrEmpty(senderId) && !string.IsNullOrEmpty(text))
                        {
                            Console.WriteLine($"Message received from {senderId}: {text}");
                            // You can call your service here to send a reply
                        }
                    }
                }
            }
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing incoming message: {ex.Message}");
            return StatusCode(500, "Error processing incoming message.");
        }
    }
}

public class WebhookRequest
{
    public string? Object { get; set; }
    public List<Entry>? Entry { get; set; }
}

public class Entry
{
    public long Time { get; set; }
    public string? Id { get; set; }
    public List<Messaging>? Messaging { get; set; }
}

public class Messaging
{
    public Sender? Sender { get; set; }
    public Recipient? Recipient { get; set; }
    public long Timestamp { get; set; }
    public Message? Message { get; set; }
}

public class Sender
{
    public string? Id { get; set; }
}

public class Recipient
{
    public string? Id { get; set; }
}

public class Message
{
    public string? Mid { get; set; }
    public string? Text { get; set; }
}