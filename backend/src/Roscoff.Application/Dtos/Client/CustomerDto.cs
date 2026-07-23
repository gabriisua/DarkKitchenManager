using System.ComponentModel.DataAnnotations;
using Roscoff.Application.Dtos.Common;

namespace Roscoff.Application.Dtos.Client;

// ==========================================
// --- DTO PER GLI HUB DI CONSEGNA ---
// ==========================================

public record DeliveryHubDto(
    Guid Id, 
    string Name, 
    string ShippingAddress, 
    string? AddressSuffix, 
    string City, 
    string ZipCode, 
    string? Province,
    string? ContactPhone, 
    string? DeliveryNotes,
    TimeSpan? DeliveryOpenTime, 
    TimeSpan? DeliveryCloseTime, 
    bool IsDefault, 
    bool IsActive
);

public class DeliveryHubCreateDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    [Required, StringLength(255)]
    public string ShippingAddress { get; set; } = null!;

    public string? AddressSuffix { get; set; }

    [Required, StringLength(100)]
    public string City { get; set; } = null!;

    [Required, StringLength(10)]
    public string ZipCode { get; set; } = null!;

    public string? Province { get; set; }
    public string? ContactPhone { get; set; }
    public string? DeliveryNotes { get; set; }
    public TimeSpan? DeliveryOpenTime { get; set; }
    public TimeSpan? DeliveryCloseTime { get; set; }
    
    public bool IsDefault { get; set; } = false;
}

public class DeliveryHubUpdateDto
{
    public string? Name { get; set; }
    public string? ShippingAddress { get; set; }
    public string? AddressSuffix { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? Province { get; set; }
    public string? ContactPhone { get; set; }
    public string? DeliveryNotes { get; set; }
    public TimeSpan? DeliveryOpenTime { get; set; }
    public TimeSpan? DeliveryCloseTime { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
}

// ==========================================
// --- DTO PER IL CUSTOMER ---
// ==========================================

public class CustomerCreateDto
{
    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = null!;

    [Required, MinLength(8)]
    public string Password { get; set; } = null!; 

    [Required]
    public string Type { get; set; } = "Private";

    // --- Dati Fiscali ---
    public string? BusinessName { get; set; }
    public string? VatNumber { get; set; }
    public string? FiscalCode { get; set; }
    public string? SdiCode { get; set; }
    public string? Pec { get; set; }
    public int? PaymentTermsDays { get; set; }

    // --- Dati Logistici (Verranno usati per creare il primo Hub di default) ---
    [Required, StringLength(255)]
    public string ShippingAddress { get; set; } = null!;

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(10)]
    public string? ZipCode { get; set; }

    [StringLength(100)]
    public string? ContactPhone { get; set; }
}

public record CustomerDetailsDto(
    Guid Id,
    string Email,
    string Type,
    string? BusinessName,
    string? VatNumber,
    string? FiscalCode,
    string? SdiCode,
    string? Pec,
    bool IsActive,
    int? PaymentTermsDays,
    IEnumerable<DeliveryHubDto> DeliveryHubs
);

public class CustomerUpdateDto
{
    public string? Type { get; set; }
    
    // Dati Fiscali
    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }
    public string? BusinessName { get; set; }
    public string? VatNumber { get; set; }
    public string? FiscalCode { get; set; }
    public string? SdiCode { get; set; }
    public string? Pec { get; set; }
    public int? PaymentTermsDays { get; set; }

    // Dati Logistici (Il Service aggiornerà l'Hub di default con questi dati)
    public string? ShippingAddress { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? ContactPhone { get; set; }
}

public class CustomerQueryParameters : BasePaginationRequestDto
{
    // Filtri specifici per il cliente
    public string? Type { get; set; }       // "Private" o "Business"
    public bool? IsActive { get; set; }     // null = tutti, true = attivi, false = disattivati
}

public record CustomerReadDto(
    Guid Id,
    string Email,
    string Type,
    string? BusinessName,
    string? ContactPhone, // Il Service pesca questo dall'Hub di default
    bool IsActive
);