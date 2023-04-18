using api.Walks;
using MediatR;
using System.Text;
using Viber.Bot;

namespace api.ViberBot;

public interface IViberService
{
    Task HandleMessage(CallbackData update, CancellationToken cancellationToken);
}

public class ViberService : IViberService
{
    private readonly IViberBotClient _viberBotClient;
    private readonly IMediator _mediator;
    private readonly IUsersStorage _usersInfo;

    public ViberService(IViberBotClient viberBotClient, IMediator mediator, IUsersStorage usersNavigation)
    {
        _viberBotClient = viberBotClient;
        _mediator = mediator;
        _usersInfo = usersNavigation;
    }

    public async Task HandleMessage(CallbackData update, CancellationToken cancellationToken = default)
    {
        if (update.Event is EventType.Subscribed)
        {
            if (_usersInfo.UserInAnyStage(update.User.Id))
            {
                _ = await _viberBotClient.SendTextMessageAsync(CreateSubscriptionMessage(update, "Wellcome back!"));
                _usersInfo.SetStage(update.User.Id, ChatUIState.MainMenu);
            }
            else
            {
                _usersInfo.SetStage(update.User.Id, ChatUIState.Authorization);

                string responseMessage = "Wellcome!\nTo start enter your IMEI";
                _ = await _viberBotClient.SendTextMessageAsync(CreateSubscriptionMessage(update, responseMessage));
                return;
            }
        }

        if (!_usersInfo.UserInAnyStage(update.Sender.Id))
        {
            _usersInfo.SetStage(update.Sender.Id, ChatUIState.Authorization);

            string responseMessage = "Enter your IMEI";
            _ = await _viberBotClient.SendTextMessageAsync(CreateTextMessage(update, responseMessage));
            return;
        }

        Viber.Bot.TextMessage message;

        if (update.Message.Type == MessageType.Text)
        {
            message = update.Message as Viber.Bot.TextMessage;
        }
        else
        {
            return;
        }

        if (_usersInfo.GetState(update.Sender.Id) == ChatUIState.Authorization)
        {
            if (!IsImei(message.Text))
            {
                var responseMessage = "Invalid IMEI, try again";
                await _viberBotClient.SendTextMessageAsync(CreateTextMessage(update, responseMessage));
                return;
            }
            else
            {
                if (await TryAddImei(update, message.Text, cancellationToken))
                {
                    _usersInfo.SetStage(update.Sender.Id, ChatUIState.MainMenu);
                    await ShowMainMenu(update, cancellationToken);
                }
                return;
            }
        }

        if (_usersInfo.GetState(update.Sender.Id) == ChatUIState.MainMenu)
        {
            if (message.Text.Equals("/top10walks", StringComparison.Ordinal))
            {
                _ = _usersInfo.TryGetImei(update.Sender.Id, out string imei);
                GetWalksByImeiQuery query = new() { Imei = imei };
                GetWalksByImeiResult result = await _mediator.Send(query, cancellationToken);


                _usersInfo.SetStage(update.Sender.Id, ChatUIState.Top10Walks);

                if (result == null)
                {
                    _ = await _viberBotClient.SendKeyboardMessageAsync(CreateKeyboardTop10WalksMessage(update, "No any walk yet"));
                    return;
                }

                var messageBuffer = new StringBuilder();
                
                var i = 0;
                foreach (var walk in result.Walks)
                {
                    messageBuffer.AppendLine($"Top {++i}");
                    messageBuffer.AppendLine($"Distance: {walk.PerWalkTotalDist} km");
                    messageBuffer.AppendLine($"Duration: {walk.PerWalkMinutes} min");
                    messageBuffer.AppendLine();
                }

                _ = await _viberBotClient.SendKeyboardMessageAsync(CreateKeyboardTop10WalksMessage(update, messageBuffer.ToString()));
                return;
            }

            if (message.Text.Equals("/changeIMEI", StringComparison.Ordinal))
            {
                _usersInfo.SetStage(update.Sender.Id, ChatUIState.ChangeImei);
                _ = await _viberBotClient.SendKeyboardMessageAsync(CreateKeyboardChangeImeiMessage(update, "Eneter new IMEI"));
                return;
            }
        }

        if (_usersInfo.GetState(update.Sender.Id) == ChatUIState.ChangeImei)
        {
            if (message.Text.Equals("/main_menu", StringComparison.Ordinal))
            {
                _usersInfo.SetStage(update.Sender.Id, ChatUIState.MainMenu);
                await ShowMainMenu(update, cancellationToken);
                return;
            }

            if (!IsImei(message.Text))
            {
                _ = await _viberBotClient.SendKeyboardMessageAsync(CreateKeyboardChangeImeiMessage(update, "Invalid IMEI"));
                return;
            }
            else
            {
                if (await TrySetImei(update, message.Text, cancellationToken))
                {
                    _ = await _viberBotClient.SendKeyboardMessageAsync(CreateKeyboardChangeImeiMessage(update, "IMEI was successfully changed"));
                    return;
                }
            }
        }

        if (_usersInfo.GetState(update.Sender.Id) == ChatUIState.Top10Walks)
        {
            if (message.Text.Equals("/main_menu", StringComparison.Ordinal))
            {
                _usersInfo.SetStage(update.Sender.Id, ChatUIState.MainMenu);
                await ShowMainMenu(update, cancellationToken);
                return;
            }
        }

        return;
    }


    private static Viber.Bot.TextMessage CreateTextMessage(CallbackData update, string text)
    {
        return new()
        {
            Receiver = update.Sender.Id,
            Sender = new UserBase()
            {
                Name = UserBaseConstants.Name,
                Avatar = UserBaseConstants.Avatar
            },
            Text = text
        };
    }

    private static Viber.Bot.TextMessage CreateSubscriptionMessage(CallbackData update, string text)
    {
        return new()
        {
            Receiver = update.User.Id,
            Sender = new UserBase()
            {
                Name = UserBaseConstants.Name,
                Avatar = UserBaseConstants.Avatar
            },
            Text = text
        };
    }

    private static Viber.Bot.KeyboardMessage CreateKeyboardMainMenuMessage(CallbackData update, string text)
    {
        KeyboardButton getTop10Button = new()
        {
            Text = "Top 10 walks",
            ActionType = KeyboardActionType.Reply,
            ActionBody = "/top10walks",
            Columns = 3
        };

        KeyboardButton changeImeiButton = new()
        {
            Text = "Change IMEI",
            ActionType = KeyboardActionType.Reply,
            ActionBody = "/changeIMEI",
            Columns = 3
        };

        Keyboard keyboard = new()
        {
            Buttons = new List<KeyboardButton>() { getTop10Button, changeImeiButton },
            ButtonsGroupColumns = 6
        };

        return new()
        {
            Receiver = update.Sender.Id,
            Sender = new UserBase()
            {
                Name = UserBaseConstants.Name,
                Avatar = UserBaseConstants.Avatar
            },
            Text = text,
            Keyboard = keyboard
        };
    }

    private static Viber.Bot.KeyboardMessage CreateKeyboardTop10WalksMessage(CallbackData update, string text)
    {
        KeyboardButton backButton = new()
        {
            Text = "Back",
            ActionType = KeyboardActionType.Reply,
            ActionBody = "/main_menu"
        };

        Keyboard keyboard = new()
        {
            Buttons = new List<KeyboardButton>() { backButton }
        };

        return new()
        {
            Receiver = update.Sender.Id,
            Sender = new UserBase()
            {
                Name = UserBaseConstants.Name,
                Avatar = UserBaseConstants.Avatar
            },
            Text = text,
            Keyboard = keyboard
        };
    }

    private static Viber.Bot.KeyboardMessage CreateKeyboardChangeImeiMessage(CallbackData update, string text)
    {
        KeyboardButton backButton = new()
        {
            Text = "Back",
            ActionType = KeyboardActionType.Reply,
            ActionBody = "/main_menu"
        };

        Keyboard keyboard = new()
        {
            Buttons = new List<KeyboardButton>() { backButton }
        };

        return new()
        {
            Receiver = update.Sender.Id,
            Sender = new UserBase()
            {
                Name = UserBaseConstants.Name,
                Avatar = UserBaseConstants.Avatar
            },
            Text = text,
            Keyboard = keyboard
        };
    }

    private async Task<bool> TryAddImei(CallbackData update, string imei, CancellationToken cancellationToken)
    {
        ImeiExistsQuery imeiExistsQuery = new() { Imei = imei };
        ImeiExistsQueryResult imeiExistsQueryResult = await _mediator.Send(imeiExistsQuery, cancellationToken);

        string responseMessage;
        if (!imeiExistsQueryResult.EmeiExists)
        {
            responseMessage = "There is no such IMEI";
            _ = await _viberBotClient.SendTextMessageAsync(CreateTextMessage(update, responseMessage));
            return false;
        }

        return _usersInfo.TryAddImei(update.Sender.Id, imei);
    }

    private async Task<bool> TrySetImei(CallbackData update, string imei, CancellationToken cancellationToken)
    {
        ImeiExistsQuery imeiExistsQuery = new() { Imei = imei };
        ImeiExistsQueryResult imeiExistsQueryResult = await _mediator.Send(imeiExistsQuery, cancellationToken);

        string responseMessage;
        if (!imeiExistsQueryResult.EmeiExists)
        {
            responseMessage = "There is no such IMEI";
            _ = await _viberBotClient.SendKeyboardMessageAsync(CreateKeyboardChangeImeiMessage(update, responseMessage));
            return false;
        }

        _usersInfo.SetImei(update.Sender.Id, imei);
        return true;
    }

    private async Task ShowMainMenu(CallbackData update, CancellationToken cancellationToken)
    {
        _ = _usersInfo.TryGetImei(update.Sender.Id, out string imei);
        var getWalksResult = await _mediator.Send(new GetWalksByImeiQuery 
        { 
            Imei = imei 
        }, cancellationToken);

        if (!getWalksResult.Walks.Any())
        {
            _ = await _viberBotClient.SendTextMessageAsync(CreateTextMessage(update, "No any walk yet"));
            return;
        }

        string responseMessage = 
            $"Walks count: {getWalksResult.WalksAmount}" + Environment.NewLine +
            $"Total distance: {Math.Round(getWalksResult.TotalDistance, 2)} kilometers\n" + Environment.NewLine +
            $"Total duration: {getWalksResult.TotalMinutes} minutes";
        _ = await _viberBotClient.SendKeyboardMessageAsync(CreateKeyboardMainMenuMessage(update, responseMessage));
    }

    private static bool IsImei(string imei)
    {
        if (imei.Length < 15) return false;
        return imei.All(x => char.IsDigit(x));
    }
}
