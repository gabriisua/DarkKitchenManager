using MediatR;
using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Client;

namespace Roscoff.Application.MediaTR.Discount.Commands;

public class SetPlateDiscountCommand : IRequest<bool>
{
    public Guid CustomerId { get; set; }
    public int PlateId { get; set; }
    public int OverridePrice { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public class SetPlateDiscountHandler : IRequestHandler<SetPlateDiscountCommand, bool>
{
    private readonly IRoscoffDbContext _context;
    public SetPlateDiscountHandler(IRoscoffDbContext context) => _context = context;

    public async Task<bool> Handle(SetPlateDiscountCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.ClientPlateDiscounts
            .FirstOrDefaultAsync(d => d.CustomerId == request.CustomerId && d.PlateId == request.PlateId, cancellationToken);

        if (existing != null)
        {
            existing.OverridePrice = request.OverridePrice;
            existing.ValidFrom = request.ValidFrom;
            existing.ValidTo = request.ValidTo;
            _context.ClientPlateDiscounts.Update(existing);
        }
        else
        {
            var discount = new ClientPlateDiscount
            {
                CustomerId = request.CustomerId,
                PlateId = request.PlateId,
                OverridePrice = request.OverridePrice,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo
            };
            _context.ClientPlateDiscounts.Add(discount);
        }

        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}