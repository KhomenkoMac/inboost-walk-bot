using api.Shared.CQRS;
using api.Shared.DB;
using Dapper;
using MediatR;
using System.Data;

namespace api.Walks;

public class ImeiExistsQuery : IRequest<ImeiExistsQueryResult>
{
    public string Imei { get; set; } = null!;
}

public class ImeiExistsQueryResult
{
    public bool EmeiExists { get; set; }
}

internal class ImeiExistsQueryHandler : BaseHandler<ImeiExistsQuery, ImeiExistsQueryResult>
{
    private readonly AppDbContext _context;

    public ImeiExistsQueryHandler(AppDbContext context, ILogger<ImeiExistsQuery> logger) : base(logger)
    {
        _context = context;
    }

    protected override async Task<ImeiExistsQueryResult> HandleInternal(ImeiExistsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _context.CreateConnection();
        connection.Open();

        var allWalksByIMEI = connection.Query<Walk>(
            sql: "dbo.GetByIMEI",
            param: new { request.Imei },
            commandType: CommandType.StoredProcedure).ToList();

        return await Task.FromResult(new ImeiExistsQueryResult()
        {
            EmeiExists = allWalksByIMEI.Any()
        });
    }
}
