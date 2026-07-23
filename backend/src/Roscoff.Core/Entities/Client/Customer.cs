using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Aggiungi l'using per i Catalog (dove risiede Menu) se non c'è già
using Roscoff.Core.Entities.Catalog; 

namespace Roscoff.Core.Entities.Client
{
    [Table("customers")]
    public class Customer : BaseEntity<Guid>
    {
        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;
        
        [Required]
        public string Type { get; set; } = "Private"; // "Private" o "Business"
        
        public int? PaymentTermsDays { get; set; }

        // --- DATI FISCALI CENTRALIZZATI (Per FattureInCloud) ---
        public string? BusinessName { get; set; } 
        public string? VatNumber { get; set; }    
        public string? FiscalCode { get; set; }   
        public string? SdiCode { get; set; }      
        public string? Pec { get; set; }

        // --- LOGICA ROSCOFF ---
        // RIMOSSO: public int? AssignedMenuId { get; set; }  <--- Eliminato!
        
        public bool IsActive { get; set; } = true;
        
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public DateTime? PasswordResetAt { get; set; }

        // --- NAVIGATION PROPERTIES (EF Core) ---
        public virtual ICollection<DeliveryHub> DeliveryHubs { get; set; } = new List<DeliveryHub>();

        public virtual ICollection<ClientCategoryDiscount> CategoryDiscounts { get; set; } = new List<ClientCategoryDiscount>();
        public virtual ICollection<ClientPlateDiscount> PlateDiscounts { get; set; } = new List<ClientPlateDiscount>();
        
        // NUOVO: Navigation property verso i menù del cliente
        public virtual ICollection<Menu> DedicatedMenus { get; set; } = new List<Menu>();
    }
}