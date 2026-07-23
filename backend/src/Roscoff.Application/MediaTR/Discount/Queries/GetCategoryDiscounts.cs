using MediatR;
using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Interfaces; 
using Roscoff.Core.Entities.Client;

namespace Roscoff.Application.MediaTR.Client.Queries;

public record GetCategoryDiscountsQuery(Guid CustomerId) : IRequest<List<ClientCategoryDiscount>>;

public class GetCategoryDiscountsHandler : IRequestHandler<GetCategoryDiscountsQuery, List<ClientCategoryDiscount>>
{
    private readonly IRoscoffDbContext _context; // <-- Inietta l'interfaccia

    public GetCategoryDiscountsHandler(IRoscoffDbContext context) => _context = context;

    public async Task<List<ClientCategoryDiscount>> Handle(GetCategoryDiscountsQuery request, CancellationToken cancellationToken)
    {
        return await _context.ClientCategoryDiscounts
            .AsNoTracking()
            .Include(d => d.Category)
            .Where(d => d.CustomerId == request.CustomerId)
            .ToListAsync(cancellationToken);
    }
}