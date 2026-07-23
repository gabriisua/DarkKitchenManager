using System.ComponentModel.DataAnnotations;
using Roscoff.Core.Interfaces;

namespace Roscoff.Core.Entities;

public abstract class BaseEntity<TId> : IAuditableEntity
{
    [Key]
    public TId Id { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public string? CreatedBy { get; set; } 
    public string? UpdatedBy { get; set; }
}