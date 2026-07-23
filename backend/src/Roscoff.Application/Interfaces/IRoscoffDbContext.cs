using Microsoft.EntityFrameworkCore;
using Roscoff.Core.Entities.Client;
using Roscoff.Core.Entities.Invoice; // Assicurati che punti alla cartella delle tue entità

namespace Roscoff.Application.Interfaces;

public interface IRoscoffDbContext
{
    DbSet<ClientCategoryDiscount> ClientCategoryDiscounts { get; }
    DbSet<ClientPlateDiscount> ClientPlateDiscounts { get; }
    
    DbSet<Order> Orders { get; }
    DbSet<PendingInvoice> PendingInvoices { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}