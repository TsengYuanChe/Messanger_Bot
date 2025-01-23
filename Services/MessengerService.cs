using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class MessengerService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;

    public MessengerService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        _accessToken = configuration["Messenger_AccessToken"] 
            ?? throw new ArgumentNullException("Messenger_AccessToken 未設置於 appsettings.json 或環境變數中");
    }

    public async Task SendMessage(string recipientId, string message)
    {
        var url = $"https://graph.facebook.com/v17.0/me/messages?access_token={_accessToken}";
        var payload = new
        {
            recipient = new { id = recipientId },
            message = new { text = message }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Console.WriteLine("Reply sent successfully!");
    }
}