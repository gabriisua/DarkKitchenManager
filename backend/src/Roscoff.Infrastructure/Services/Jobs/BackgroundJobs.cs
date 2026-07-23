using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Invoice;

namespace Roscoff.Infrastructure.Services.Jobs;

public class InvoiceProcessingJob
{
    private readonly IRoscoffDbContext _context;
    private readonly ILogger<InvoiceProcessingJob> _logger;
    private readonly IFattureInCloudService _ficService; // <-- 1. Aggiunto il servizio

    public InvoiceProcessingJob(
        IRoscoffDbContext context,
        ILogger<InvoiceProcessingJob> logger,
        IFattureInCloudService ficService) // <-- 2. Iniettato dal costruttore
    {
        _context = context;
        _logger = logger;
        _ficService = ficService;
    }

    public async Task ProcessPendingInvoicesAsync()
    {
        _logger.LogInformation("Avvio controllo fatture in coda (Modalità Cumulativa)...");

        // 1. Peschiamo i record pendenti
        var pendingInvoices = await _context.PendingInvoices
            .Include(p => p.Order)
            .ThenInclude(o => o.Customer)
            .Include(p => p.Order)
            .ThenInclude(o => o.OrderItems)
            .Where(p => p.Status == PendingInvoiceStatus.Pending)
            .Take(50) // Alziamo il tiro a 50 visto che poi li raggruppiamo
            .ToListAsync();

        if (!pendingInvoices.Any()) return;

        // 2. Lock immediato di tutto il blocco in stato 'Processing'
        foreach (var invoice in pendingInvoices)
        {
            invoice.Status = PendingInvoiceStatus.Processing;
        }

        await _context.SaveChangesAsync();

        // 3. RAGGRUPPIAMO PER CLIENTE
        var groupsByCustomer = pendingInvoices.GroupBy(p => p.Order.CustomerId);

        foreach (var customerGroup in groupsByCustomer)
        {
            // Estraiamo tutti gli ordini di questo specifico cliente presenti nel gruppo
            var customerOrders = customerGroup.Select(p => p.Order).ToList();

            try
            {
                // Chiamata unica per il cliente con dentro TUTTI i suoi ordini
                var result = await _ficService.CreateInvoiceForCustomerAsync(customerOrders);

                // Se va a buon fine, aggiorniamo TUTTI gli ordini e TUTTE le righe della coda per questo cliente
                foreach (var invoice in customerGroup)
                {
                    invoice.Order.FicInvoiceDocumentId = result.DocumentId;
                    invoice.Order.InvoiceNumber = result.InvoiceNumber;

                    invoice.Status = PendingInvoiceStatus.Completed;
                    invoice.ProcessedAt = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Errore durante la generazione della fattura cumulativa per il cliente con ID {CustomerId}",
                    customerGroup.Key);

                // Se fallisce la chiamata, falliscono insieme tutti i record di questo cliente
                foreach (var invoice in customerGroup)
                {
                    invoice.Status = PendingInvoiceStatus.Failed;
                    invoice.ErrorMessage = ex.Message.Length > 1000 ? ex.Message.Substring(0, 996) + "..." : ex.Message;
                }
            }
        }

        // 4. Unico salvataggio finale per tutti i gruppi elaborati˙˙
        await _context.SaveChangesAsync();
        _logger.LogInformation("Ciclo di elaborazione code terminato.");
    }
}