using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Dtos.Client;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Client;
using Roscoff.Infrastructure.Data;

namespace Roscoff.Infrastructure.Services;

public class DeliveryHubService : IDeliveryHubService
{
    private readonly RoscoffDbContext _context;

    public DeliveryHubService(RoscoffDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<DeliveryHubDto>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.DeliveryHubs
            .Where(h => h.CustomerId == customerId && h.IsActive) // Filtra per cliente e solo hub attivi
            .AsNoTracking() // Ottimizzazione per query di sola lettura
            .Select(h => new DeliveryHubDto(
                h.Id, 
                h.Name, 
                h.ShippingAddress, 
                h.AddressSuffix, 
                h.City, 
                h.ZipCode, 
                h.Province,
                h.ContactPhone, 
                h.DeliveryNotes,
                h.DeliveryOpenTime, 
                h.DeliveryCloseTime, 
                h.IsDefault, 
                h.IsActive
            ))
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, Guid? HubId)> CreateAsync(Guid customerId, DeliveryHubCreateDto request)
    {
        // 1. Verifica che il cliente esista
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId);
        if (!customerExists)
            return (false, "Cliente non trovato.", null);

        // 2. Logica dell'Hub di Default
        // Se il nuovo hub viene impostato come default, dobbiamo rimuovere il default da tutti gli altri
        if (request.IsDefault)
        {
            var existingDefaults = await _context.DeliveryHubs
                .Where(h => h.CustomerId == customerId && h.IsDefault)
                .ToListAsync();

            foreach (var existingHub in existingDefaults)
            {
                existingHub.IsDefault = false;
            }
        }

        // 3. Creazione del nuovo Hub
        var newHub = new DeliveryHub
        {
            CustomerId = customerId,
            Name = request.Name,
            ShippingAddress = request.ShippingAddress,
            AddressSuffix = request.AddressSuffix,
            City = request.City,
            ZipCode = request.ZipCode,
            Province = request.Province,
            ContactPhone = request.ContactPhone,
            DeliveryNotes = request.DeliveryNotes,
            DeliveryOpenTime = request.DeliveryOpenTime,
            DeliveryCloseTime = request.DeliveryCloseTime,
            IsDefault = request.IsDefault,
            IsActive = true
        };

        _context.DeliveryHubs.Add(newHub);
        await _context.SaveChangesAsync();

        return (true, "Hub di consegna creato con successo.", newHub.Id);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Guid customerId, Guid hubId, DeliveryHubUpdateDto request)
    {
        // 1. Recupero l'hub assicurandomi che appartenga al cliente corretto
        var hub = await _context.DeliveryHubs
            .FirstOrDefaultAsync(h => h.Id == hubId && h.CustomerId == customerId);
        
        if (hub == null)
            return (false, "Hub non trovato o non appartenente a questo cliente.");

        // 2. Gestione logica dell'Hub di Default in fase di aggiornamento
        if (request.IsDefault.HasValue && request.IsDefault.Value && !hub.IsDefault)
        {
            var otherDefaults = await _context.DeliveryHubs
                .Where(h => h.CustomerId == customerId && h.Id != hubId && h.IsDefault)
                .ToListAsync();

            foreach (var otherHub in otherDefaults)
            {
                otherHub.IsDefault = false;
            }
        }

        // 3. Aggiornamento dei campi (solo se il nuovo valore non è nullo)
        hub.Name = request.Name ?? hub.Name;
        hub.ShippingAddress = request.ShippingAddress ?? hub.ShippingAddress;
        hub.AddressSuffix = request.AddressSuffix ?? hub.AddressSuffix;
        hub.City = request.City ?? hub.City;
        hub.ZipCode = request.ZipCode ?? hub.ZipCode;
        hub.Province = request.Province ?? hub.Province;
        hub.ContactPhone = request.ContactPhone ?? hub.ContactPhone;
        hub.DeliveryNotes = request.DeliveryNotes ?? hub.DeliveryNotes;
        hub.DeliveryOpenTime = request.DeliveryOpenTime ?? hub.DeliveryOpenTime;
        hub.DeliveryCloseTime = request.DeliveryCloseTime ?? hub.DeliveryCloseTime;
        
        if (request.IsDefault.HasValue) hub.IsDefault = request.IsDefault.Value;
        if (request.IsActive.HasValue) hub.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync();

        return (true, "Hub aggiornato con successo.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid customerId, Guid hubId)
    {
        var hub = await _context.DeliveryHubs
            .FirstOrDefaultAsync(h => h.Id == hubId && h.CustomerId == customerId);
        
        if (hub == null)
            return (false, "Hub non trovato.");

        // Soft delete: invece di eliminarlo fisicamente, lo disattiviamo
        // In questo modo non rompi lo storico degli ordini passati o dei vecchi DDT
        hub.IsActive = false;
        hub.IsDefault = false; // Togliamo il default se lo era

        await _context.SaveChangesAsync();

        return (true, "Hub eliminato con successo.");
    }
}