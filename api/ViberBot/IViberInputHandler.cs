using Viber.Bot;

namespace api.ViberBot;

public interface IViberInputHandler
{
    Task HandleMessage(CallbackData update, CancellationToken cancellationToken);
}
