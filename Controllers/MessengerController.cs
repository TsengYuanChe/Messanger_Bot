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
    public async Task<IActionResult> Webhook([FromBody] JsonElement request)
    {
        try
        {
            // 打印原始請求以排查問題
            Console.WriteLine($"Incoming Webhook: {request.GetRawText()}");

            // 嘗試將 JSON 反序列化為 WebhookRequest 模型
            var root = JsonSerializer.Deserialize<WebhookRequest>(request.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // 忽略大小寫
            });

            if (root == null || root.Entry == null || root.Entry.Count == 0)
            {
                Console.WriteLine("No entries found in the Webhook payload.");
                return BadRequest("Invalid Webhook payload.");
            }

            foreach (var entry in root.Entry)
            {
                foreach (var messaging in entry.Messaging)
                {
                    var senderId = messaging.Sender?.Id;
                    var text = messaging.Message?.Text;

                    if (!string.IsNullOrEmpty(senderId) && !string.IsNullOrEmpty(text))
                    {
                        Console.WriteLine($"Message received from {senderId}: {text}");

                        // 準備回覆
                        var replyMessage = $"PSID:{senderId}, 傳送的訊息為：{text}";

                        // 發送回覆
                        await _messengerService.SendMessage(senderId, replyMessage);
                    }
                    else
                    {
                        Console.WriteLine("Missing senderId or text in the message.");
                    }
                }
            }
            Console.WriteLine("Posting finished.");
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing Webhook: {ex.Message}");
            return StatusCode(500, "An error occurred while processing the Webhook.");
        }
    }
}

public class WebhookRequest
{
    public string? Object { get; set; } // JSON 中的 "object"
    public List<Entry>? Entry { get; set; } // JSON 中的 "entry"
}

public class Entry
{
    public string? Id { get; set; } // JSON 中的 "id"
    public long Time { get; set; } // JSON 中的 "time"
    public List<Messaging>? Messaging { get; set; } // JSON 中的 "messaging"
}

public class Messaging
{
    public Sender? Sender { get; set; } // JSON 中的 "sender"
    public Recipient? Recipient { get; set; } // JSON 中的 "recipient"
    public long Timestamp { get; set; } // JSON 中的 "timestamp"
    public Message? Message { get; set; } // JSON 中的 "message"
}

public class Sender
{
    public string? Id { get; set; } // JSON 中的 "sender.id"
}

public class Recipient
{
    public string? Id { get; set; } // JSON 中的 "recipient.id"
}

public class Message
{
    public string? Mid { get; set; } // JSON 中的 "message.mid"
    public string? Text { get; set; } // JSON 中的 "message.text"
}