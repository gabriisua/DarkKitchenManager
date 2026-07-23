using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Dtos.Client;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Client;
using Roscoff.Infrastructure.Data;

namespace Roscoff.Infrastructure.Services;

public class StaffService : IStaffService
{
    private readonly RoscoffDbContext _context;
    private readonly IPasswordService _passwordService;

    public StaffService(RoscoffDbContext context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public async Task<PaginatedResponseDto<StaffReadDto>> GetAllAsync(StaffQueryParameters filter, CancellationToken cancellationToken = default)
    {
        var query = _context.StaffMembers.AsQueryable();

        // --- 1. FILTRI ---
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search;
            query = query.Where(s => s.Username.Contains(searchTerm) || 
                                     (s.Email != null && s.Email.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            query = query.Where(s => s.Email == filter.Email);
        }

        if (!string.IsNullOrWhiteSpace(filter.Role))
        {
            query = query.Where(s => s.Role == filter.Role);
        }

        if (filter.DateFrom.HasValue)
        {
            query = query.Where(s => s.CreatedAt >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(s => s.CreatedAt <= filter.DateTo.Value);
        }

        // --- 2. ORDINAMENTO ---
        bool isDesc = filter.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        query = filter.SortColumn?.ToLower() switch
        {
            "username" => isDesc ? query.OrderByDescending(s => s.Username) : query.OrderBy(s => s.Username),
            "email" => isDesc ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email),
            "role" => isDesc ? query.OrderByDescending(s => s.Role) : query.OrderBy(s => s.Role),
            "lastlogin" => isDesc ? query.OrderByDescending(s => s.LastLogin) : query.OrderBy(s => s.LastLogin),
            _ => isDesc ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        int validPage = filter.Page > 0 ? filter.Page : 1;
        int validPageSize = filter.PageSize > 0 ? filter.PageSize : 10;

        var items = await query
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .Select(s => new StaffReadDto(s.Id, s.Username, s.Email, s.Role, s.IsActive, s.LastLogin))
            .ToListAsync(cancellationToken);

        return new PaginatedResponseDto<StaffReadDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize
        };
    }

    public async Task<StaffReadDto> CreateAsync(StaffCreateDto dto, CancellationToken cancellationToken = default)
    {
        // --- NUOVO CONTROLLO: Validazione Ruolo ---
        if (!StaffRoles.AllRoles.Contains(dto.Role))
            throw new ArgumentException($"Il ruolo '{dto.Role}' non è un ruolo valido.");

        // Controllo unicità 
        bool exists = await _context.StaffMembers
            .AnyAsync(s => s.Username == dto.Username || s.Email == dto.Email, cancellationToken);
            
        if (exists)
            throw new InvalidOperationException("Un utente con questo Username o Email esiste già.");

        var staff = new Staff
        {
            Username = dto.Username,
            Email = dto.Email, 
            PasswordHash = _passwordService.HashPassword(dto.Password), 
            Role = dto.Role,
            IsActive = true
        };

        _context.StaffMembers.Add(staff);
        await _context.SaveChangesAsync(cancellationToken);

        return new StaffReadDto(staff.Id, staff.Username, staff.Email, staff.Role, staff.IsActive, staff.LastLogin);
    }

    public async Task<bool> UpdateAsync(Guid id, StaffUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var staff = await _context.StaffMembers.FindAsync(new object[] { id }, cancellationToken);
        if (staff == null) return false;

        // --- NUOVO CONTROLLO: Validazione Ruolo ---
        if (!StaffRoles.AllRoles.Contains(dto.Role))
            throw new ArgumentException($"Il ruolo '{dto.Role}' non è un ruolo valido.");

        if (staff.Email != dto.Email)
        {
            bool emailExists = await _context.StaffMembers.AnyAsync(s => s.Email == dto.Email && s.Id != id, cancellationToken);
            if (emailExists)
                throw new InvalidOperationException("L'email specificata è già in uso da un altro utente.");
        }

        staff.Email = dto.Email;
        staff.Role = dto.Role;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            staff.PasswordHash = _passwordService.HashPassword(dto.Password); 
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var staff = await _context.StaffMembers.FindAsync(new object[] { id }, cancellationToken);
        if (staff == null) return false;

        staff.IsActive = false; 
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<StaffReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var s = await _context.StaffMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            
        if (s == null) return null;
        
        return new StaffReadDto(s.Id, s.Username, s.Email, s.Role, s.IsActive, s.LastLogin);
    }
}