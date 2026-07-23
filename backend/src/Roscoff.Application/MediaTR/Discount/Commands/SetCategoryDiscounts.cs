using MediatR;
using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Client;

// Fai molta attenzione a questo namespace, deve combaciare con gli using del Controller!
namespace Roscoff.Application.MediaTR.Discount.Commands;

// L'aggiunta di ": IRequest<bool>" risolve l'errore del casting da void a bool e l'errore IRequest
public class SetCategoryDiscountCommand : IRequest<bool>
{
    public Guid CustomerId { get; set; }
    public int CategoryId { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public class SetCategoryDiscountHandler : IRequestHandler<SetCategoryDiscountCommand, bool>
{
    private readonly IRoscoffDbContext _context;

    public SetCategoryDiscountHandler(IRoscoffDbContext context) => _context = context;

    public async Task<bool> Handle(SetCategoryDiscountCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.ClientCategoryDiscounts
            .FirstOrDefaultAsync(d => d.CustomerId == request.CustomerId && d.CategoryId == request.CategoryId, cancellationToken);

        if (existing != null)
        {
            existing.DiscountPercentage = request.DiscountPercentage;
            existing.ValidFrom = request.ValidFrom;
            existing.ValidTo = request.ValidTo;
            _context.ClientCategoryDiscounts.Update(existing);
        }
        else
        {
            var discount = new ClientCategoryDiscount
            {
                CustomerId = request.CustomerId,
                CategoryId = request.CategoryId,
                DiscountPercentage = request.DiscountPercentage,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo
            };
            _context.ClientCategoryDiscounts.Add(discount);
        }

        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}