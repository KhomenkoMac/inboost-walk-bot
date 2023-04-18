using api.Shared.DB;
using api.ViberBot;
using api.Walks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Viber.Bot;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection("ViberBot"));

builder.Services.AddControllers().AddNewtonsoftJson();

/// Add services to the container. 
// 3rd party libraries
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetWalksByImeiQuery).Assembly));

// user services
builder.Services.AddTransient<IViberService, ViberService>();
builder.Services.AddSingleton<IUsersStorage, UsersStorage>();
builder.Services.AddSingleton<AppDbContext>();

// system services
builder.Services.AddHttpClient("viberclient")
    .AddTypedClient<IViberBotClient>((_, serviceProvider) =>
    {
        IOptionsMonitor<BotConfiguration> configuration = serviceProvider.GetRequiredService<IOptionsMonitor<BotConfiguration>>();

        return new ViberBotClient(configuration.CurrentValue.Token);
    });

// workers
builder.Services.AddHostedService<WebhookInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//app.UseHttpsRedirection();

//http://localhost:8000/my-walks/359339077003915
//app.MapGet("/walks/{imei}", async ([FromRoute] string imei, IMediator _mediator) =>
//{
//    return await _mediator.Send(new GetWalksByImeiQuery() { Imei = imei }, CancellationToken.None);
//});
app.MapControllers();

app.Run();

#region Configuration
public class BotConfiguration
{
    public string Token { get; set; } = null!;
    public string Webhook { get; set; } = null!;
}

#endregion
