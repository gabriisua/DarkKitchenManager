using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; 

namespace Roscoff.Core.Entities.Catalog;

[Table("plate_ingredients")]
[PrimaryKey(nameof(PlateId), nameof(IngredientId))] // Chiave primaria composta per EF Core
public class PlateIngredient
{
    public int PlateId { get; set; }
    public int IngredientId { get; set; }

    [Required]
    [Column(TypeName = "decimal(8, 2)")]
    public decimal WeightInGrams { get; set; } // Peso Lordo inserito nella ricetta

    // Relazioni
    [ForeignKey(nameof(PlateId))]
    public Plate? Plate { get; set; }

    [ForeignKey(nameof(IngredientId))]
    public Ingredient? Ingredient { get; set; }
}