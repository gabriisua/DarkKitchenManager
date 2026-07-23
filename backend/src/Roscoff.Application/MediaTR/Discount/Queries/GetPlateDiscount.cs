using MediatR;
using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Client;

namespace Roscoff.Application.MediaTR.Client.Queries;

public record GetPlateDiscountsQuery(Guid CustomerId) : IRequest<List<ClientPlateDiscount>>;

public class GetPlateDiscountsHandler : IRequestHandler<GetPlateDiscountsQuery, List<ClientPlateDiscount>>
{
    private readonly IRoscoffDbContext _context;
    public GetPlateDiscountsHandler(IRoscoffDbContext context) => _context = context;

    public async Task<List<ClientPlateDiscount>> Handle(GetPlateDiscountsQuery request, CancellationToken cancellationToken)
    {
        return await _context.ClientPlateDiscounts
            .AsNoTracking()
            .Include(d => d.Plate)
            .Where(d => d.CustomerId == request.CustomerId)
            .ToListAsync(cancellationToken);
    }
}