using Microsoft.Extensions.Options;
using Viber.Bot;

namespace api.ViberBot;

/// <summary>
/// Using background service to 
/// </summary>
public class WebhookInitializer : BackgroundService
{
    private readonly IOptionsMonitor<BotConfiguration> _botConfigs;
    private readonly IViberBotClient _viberBotClient;
    private readonly ILogger<WebhookInitializer> _logger;

    public WebhookInitializer(IOptionsMonitor<BotConfiguration> botConfigs, IViberBotClient viberBotClient)
    {
        _botConfigs = botConfigs;
        _viberBotClient = viberBotClient;
    }

    //public override async Task StartAsync(CancellationToken cancellationToken)
    //{
    //}

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(2000);
        try
        {
            await _viberBotClient.SetWebhookAsync($"{_botConfigs.CurrentValue.Webhook}/bot");
        }
        catch (Exception e)
        {
            //_logger.LogError("Failed webhook");
        }
    }
}