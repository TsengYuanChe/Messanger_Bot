using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 添加 HttpClient 和 MessengerService
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MessengerService>();

// 加入控制器
builder.Services.AddControllers();

var app = builder.Build();

// 配置路由
app.MapControllers();

app.Run();