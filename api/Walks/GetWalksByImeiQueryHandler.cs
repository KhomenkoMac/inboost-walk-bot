using api.Shared.CQRS;
using api.Shared.DB;
using Dapper;
using MediatR;
using System.Data;

namespace api.Walks;

public class GetWalksByImeiQuery : IRequest<GetWalksByImeiResult>
{
    public string Imei { get; set; } = null!;
}

public class GetWalksByImeiResult
{
    public IList<Walk> Walks { get; set; } = null!;
    public double TotalDistance { get; set; }
    public int TotalMinutes { get; set; }
    public int WalksAmount { get; set; }
}

internal class GetWalksByImeiQueryHandler : BaseHandler<GetWalksByImeiQuery, GetWalksByImeiResult>
{
    private readonly AppDbContext _context;

    public GetWalksByImeiQueryHandler(AppDbContext context, ILogger<GetWalksByImeiQuery> logger) : base(logger)
    {
        _context = context;
    }

    protected override async Task<GetWalksByImeiResult> HandleInternal(GetWalksByImeiQuery request, CancellationToken cancellationToken)
    {
        using var connection = _context.CreateConnection();
        connection.Open();

        var allWalksByIMEI = connection
        .Query<Walk>(
            sql: "dbo.GetByIMEI",
            param: new { request.Imei },
            commandType: CommandType.StoredProcedure).ToList();

        return await Task.FromResult(new GetWalksByImeiResult()
        {
            Walks = allWalksByIMEI.Take(10).ToList(),
            TotalDistance = allWalksByIMEI.Sum(x => x.PerWalkTotalDist),
            TotalMinutes = allWalksByIMEI.Sum(x => x.PerWalkMinutes),
            WalksAmount = allWalksByIMEI.Count
        });
    }
}
