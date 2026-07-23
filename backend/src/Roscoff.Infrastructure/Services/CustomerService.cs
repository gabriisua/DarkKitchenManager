using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Dtos.Client;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Client;
using Roscoff.Infrastructure.Data;

namespace Roscoff.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly RoscoffDbContext _context;

    public CustomerService(RoscoffDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponseDto<CustomerReadDto>> GetAllAsync(CustomerQueryParameters parameters)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search;
            query = query.Where(c => 
                c.Email.Contains(search) || 
                (c.BusinessName != null && c.BusinessName.Contains(search)) ||
                c.DeliveryHubs.Any(h => h.ShippingAddress.Contains(search))
            );
        }

        if (!string.IsNullOrWhiteSpace(parameters.Type))
            query = query.Where(c => c.Type == parameters.Type);

        if (parameters.IsActive.HasValue)
            query = query.Where(c => c.IsActive == parameters.IsActive.Value);

        if (parameters.DateFrom.HasValue)
            query = query.Where(c => c.CreatedAt >= parameters.DateFrom.Value);

        if (parameters.DateTo.HasValue)
            query = query.Where(c => c.CreatedAt <= parameters.DateTo.Value);

        bool isDesc = string.Equals(parameters.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = parameters.SortColumn?.ToLower() switch
        {
            "email"       => isDesc ? query.OrderByDescending(c => c.Email)        : query.OrderBy(c => c.Email),
            "businessname"=> isDesc ? query.OrderByDescending(c => c.BusinessName) : query.OrderBy(c => c.BusinessName),
            "type"        => isDesc ? query.OrderByDescending(c => c.Type)         : query.OrderBy(c => c.Type),
            _             => isDesc ? query.OrderByDescending(c => c.CreatedAt)    : query.OrderBy(c => c.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        int validPage = parameters.Page > 0 ? parameters.Page : 1;
        int validPageSize = parameters.PageSize > 0 ? parameters.PageSize : 10;

        var items = await query
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .Select(c => new CustomerReadDto(
                c.Id,
                c.Email,
                c.Type,
                c.BusinessName,
                c.DeliveryHubs.Where(h => h.IsDefault).Select(h => h.ContactPhone).FirstOrDefault(), 
                c.IsActive
            ))
            .AsNoTracking() 
            .ToListAsync();

        return new PaginatedResponseDto<CustomerReadDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize
        };
    }

    public async Task<CustomerDetailsDto?> GetByIdAsync(Guid id)
    {
        var c = await _context.Customers
            .Include(x => x.DeliveryHubs)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (c == null) return null;

        var hubsDto = c.DeliveryHubs.Select(h => new DeliveryHubDto(
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
        )).ToList();

        return new CustomerDetailsDto(
            c.Id, 
            c.Email, 
            c.Type, 
            c.BusinessName, 
            c.VatNumber, 
            c.FiscalCode, 
            c.SdiCode, 
            c.Pec,
            c.IsActive, 
            c.PaymentTermsDays,
            hubsDto 
        );
    }

    public async Task<(bool Success, string Message, Guid? CustomerId)> CreateAsync(CustomerCreateDto request)
    {
        if (await _context.Customers.AnyAsync(c => c.Email == request.Email))
            return (false, "Esiste già un cliente con questa email.", null);

        var customer = new Customer
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Type = request.Type, 
            BusinessName = request.BusinessName,
            VatNumber = request.VatNumber,
            FiscalCode = request.FiscalCode,
            SdiCode = request.SdiCode,
            Pec = request.Pec,
            IsActive = true,
            PaymentTermsDays = request.PaymentTermsDays,
            
            DeliveryHubs = new List<DeliveryHub>
            {
                new DeliveryHub
                {
                    Name = "Sede Principale",
                    ShippingAddress = request.ShippingAddress,
                    City = request.City ?? string.Empty, 
                    ZipCode = request.ZipCode ?? string.Empty,
                    ContactPhone = request.ContactPhone,
                    IsDefault = true,
                    IsActive = true
                }
            }
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return (true, "Cliente e sede principale creati con successo.", customer.Id);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Guid id, CustomerUpdateDto request)
    {
        var customer = await _context.Customers
            .Include(c => c.DeliveryHubs)
            .FirstOrDefaultAsync(c => c.Id == id);
        
        if (customer == null) 
            return (false, "Cliente non trovato.");

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != customer.Email)
        {
            if (await _context.Customers.AnyAsync(c => c.Email == request.Email && c.Id != id))
            {
                return (false, "L'email specificata è già in uso da un altro cliente.");
            }
            
            customer.Email = request.Email;
        }

        customer.Type = request.Type ?? customer.Type;
        customer.BusinessName = request.BusinessName ?? customer.BusinessName;
        customer.VatNumber = request.VatNumber ?? customer.VatNumber;
        customer.FiscalCode = request.FiscalCode ?? customer.FiscalCode;
        customer.SdiCode = request.SdiCode ?? customer.SdiCode;
        customer.Pec = request.Pec ?? customer.Pec;
        customer.PaymentTermsDays = request.PaymentTermsDays ?? customer.PaymentTermsDays;

        var defaultHub = customer.DeliveryHubs.FirstOrDefault(h => h.IsDefault);
        if (defaultHub != null)
        {
            defaultHub.ShippingAddress = request.ShippingAddress ?? defaultHub.ShippingAddress;
            defaultHub.City = request.City ?? defaultHub.City;
            defaultHub.ZipCode = request.ZipCode ?? defaultHub.ZipCode;
            defaultHub.ContactPhone = request.ContactPhone ?? defaultHub.ContactPhone;
        }

        await _context.SaveChangesAsync();
        
        return (true, "Cliente aggiornato con successo.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        
        if (customer == null) 
            return (false, "Cliente non trovato.");

        customer.IsActive = false;
        
        await _context.SaveChangesAsync();
        
        return (true, "Cliente disattivato con successo.");
    }
}