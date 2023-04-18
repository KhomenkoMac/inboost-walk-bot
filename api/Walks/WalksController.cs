using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace api.Walks;

[ApiController]
[Route("[controller]")]
public class WalksController : ControllerBase
{
    private readonly ILogger<WalksController> _logger;
    private readonly IMediator _mediator;

    public WalksController(ILogger<WalksController> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("{imei}")]
    public async Task<IList<Walk>> Get([FromRoute] string imei, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetWalksByImeiQuery() { Imei = imei }, CancellationToken.None);
        return response.Walks;
    }
}
