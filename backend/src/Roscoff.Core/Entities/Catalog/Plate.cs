using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Roscoff.Core.Entities.Catalog;

// 1. DEFINIAMO L'ENUM PER LE LINEE INTERNE
public enum PlateLineType
{
    Standard = 0,
    Gourmet = 1,
    Vegetale = 2,
    Fitness = 3,
    Planted = 4
}

// 2. NUOVO ENUM PER LE ICONE DIETETICHE (Cliente Esterno)
public enum DietaryIconType
{
    None = 0,
    Vegan = 1,
    Vegetarian = 2,
    Meat = 3,
    Fish = 4
}

[Table("plates")]
public class Plate : BaseEntity<int>
{
    // --- CODICE ARTICOLO (SKU) ---
    [StringLength(50)]
    public string? Code { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    public ICollection<PlateIngredient> PlateIngredients { get; set; } = new List<PlateIngredient>();

    [Required]
    public int BasePrice { get; set; } 

    [Required]
    public int DaysToExpire { get; set; } 
    
    [Required]
    public int PackagingCost { get; set; }
    
    [Required]
    public int WorkingDaysRequired { get; set; } 

    // --- CAMPO IVA ---
    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal VatRate { get; set; } = 10.00m; 

    public bool IsActive { get; set; } = true;

    // --- NUOVI CAMPI PER ETICHETTATURA E PREPARAZIONE ---
    
    [StringLength(13, MinimumLength = 8)]
    public string? EanCode { get; set; } // Gestisce EAN-8 o EAN-13

    public int? MicrowaveWattage { get; set; } // Es. 850

    [Column(TypeName = "decimal(4,1)")]
    public decimal? MicrowaveMinutes { get; set; } // Es. 2.5 per due minuti e mezzo

    [StringLength(250)]
    public string? PreparationInstructions { get; set; } // Per istruzioni alternative (es. "In forno a 180°C")

    // --- NUOVI CAMPI PER SCHEDA TECNICA E LOGISTICA ---

    [StringLength(100)]
    public string? ProductType { get; set; } = "Preparazione gastronomica";

    [StringLength(500)]
    public string? PackagingDescription { get; set; } // Es. "Vaschetta di polipropilene per alimenti..."

    [StringLength(250)]
    public string? StorageConditions { get; set; } = "Conservare in frigorifero tra 0°C e +4°C.";

    [StringLength(250)]
    public string? PreservationTechnology { get; set; } = "Confezionato in atmosfera protettiva (ATM).";

    // --- GESTIONE ICONOGRAFIE E BADGE ---
    
    // 1. Linea Commerciale Roscoff
    public PlateLineType LineType { get; set; } = PlateLineType.Standard;
    
    // 2. Icona Alimentare Principale (Cliente Esterno)
    public DietaryIconType DietaryIcon { get; set; } = DietaryIconType.None;
    

    // --- RELAZIONI ---
    public int CategoryId { get; set; }
    
    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}