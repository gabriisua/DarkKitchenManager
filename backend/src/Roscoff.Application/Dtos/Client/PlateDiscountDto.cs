namespace Roscoff.Application.Dtos.Client;

public class PlateDiscountDto
{
    public Guid CustomerId { get; set; }
    public string BusinessName { get; set; } = string.Empty; // Nome o Email del cliente
    public int PlateId { get; set; }
    public string PlateName { get; set; } = string.Empty;
    public int OverridePrice { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    public bool IsActive => (!ValidFrom.HasValue || ValidFrom <= DateTime.UtcNow) && 
                            (!ValidTo.HasValue || ValidTo >= DateTime.UtcNow);
}

public class CategoryDiscountDto
{
    public Guid CustomerId { get; set; }
    public string BusinessName { get; set; } = string.Empty; // Nome o Email del cliente
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    public bool IsActive => (!ValidFrom.HasValue || ValidFrom <= DateTime.UtcNow) && 
                            (!ValidTo.HasValue || ValidTo >= DateTime.UtcNow);
}