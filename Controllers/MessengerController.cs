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
    public IActionResult VerifyWebhook([FromQuery] string hub_mode, [FromQuery] string hub_verify_token, [FromQuery] string hub_challenge)
    {
        const string verifyToken = "my_messenger_bot_token"; // 與 appsettings.json 的 VerifyToken 保持一致

        if (hub_mode == "subscribe" && hub_verify_token == verifyToken)
        {
            Console.WriteLine("Webhook verification succeeded.");
            return Ok(hub_challenge); // 返回 hub.challenge 完成驗證
        }

        Console.WriteLine("Webhook verification failed.");
        return Unauthorized("Invalid verify token.");
    }

    // 接收訊息
    [HttpPost("webhook")]
    public IActionResult ReceiveMessage([FromBody] dynamic request)
    {
        Console.WriteLine($"Incoming Webhook: {request}");
        
        // 簡單解析訊息並回應
        try
        {
            var message = request.entry[0].messaging[0].message.text.ToString();
            var senderId = request.entry[0].messaging[0].sender.id.ToString();

            Console.WriteLine($"Message received from {senderId}: {message}");

            // 呼叫服務回應訊息
            _messengerService.SendMessage(senderId, $"收到您的訊息：{message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing incoming message: {ex.Message}");
        }

        return Ok();
    }
}