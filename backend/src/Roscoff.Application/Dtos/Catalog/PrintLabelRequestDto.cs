namespace Roscoff.Application.Dtos.Catalog;

public class PrintLabelRequestDto
{
    public int Copies { get; set; } = 1;
    public int PauseAfter { get; set; } = 0; 
    public string? LotNumber { get; set; } 
    public DateTime? CustomExpiryDate { get; set; }
    public decimal? CustomWeight { get; set; } 
    public bool IsWow { get; set; }
    public bool IsXl { get; set; }
    
    // --- NUOVI CAMPI CRIOGENICO ---
    public bool IsThawed { get; set; }
    public DateTime? ThawingDate { get; set; }
    public string? TargetLanguage { get; set; }
}

public class PrintBatchItemDto
{
    public int PlateId { get; set; }         
    public int Copies { get; set; } 
    public int PauseAfter { get; set; } 
    public string? LotNumber { get; set; } 
    public DateTime? CustomExpiryDate { get; set; }
    public decimal? CustomWeight { get; set; }
    public bool IsWow { get; set; }
    public bool IsXl { get; set; }
    
    // --- NUOVI CAMPI CRIOGENICO ---
    public bool IsThawed { get; set; }
    public DateTime? ThawingDate { get; set; }
    public string? TargetLanguage { get; set; }
}

public class PrintJobRequestDto
{
    public PlateResponseDto Plate { get; set; } = null!;
    public NutritionalSummaryDto Nutrition { get; set; } = null!;
    public string Allergens { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public DateTime ProductionDate { get; set; }
    public int Copies { get; set; }
    public int PauseAfter { get; set; }
    public DateTime? CustomExpiryDate { get; set; }
    public decimal? CustomWeight { get; set; }
    public bool IsWow { get; set; }
    public bool IsXl { get; set; }
    
    // --- NUOVI CAMPI CRIOGENICO ---
    public bool IsThawed { get; set; }
    public DateTime? ThawingDate { get; set; }
    public string? TargetLanguage { get; set; }
}