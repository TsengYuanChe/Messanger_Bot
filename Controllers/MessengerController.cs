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