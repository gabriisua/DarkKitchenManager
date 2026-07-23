using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Roscoff.Core.Entities.Client
{
    [Table("customer_delivery_hubs")]
    public class DeliveryHub : BaseEntity<Guid>
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!; // Es. "Sede Centrale", "Ufficio Produzione"

        [Required, StringLength(255)]
        public string ShippingAddress { get; set; } = null!;

        [StringLength(100)]
        public string? AddressSuffix { get; set; } // Es. "Piano 3, Interno 12"

        [Required, StringLength(100)]
        public string City { get; set; } = null!;

        [Required, StringLength(10)]
        public string ZipCode { get; set; } = null!;

        [StringLength(50)]
        public string? Province { get; set; } 

        [StringLength(100)]
        public string? ContactPhone { get; set; } 

        [StringLength(500)]
        public string? DeliveryNotes { get; set; } // "Attenzione: Rampa ripida", "Citofonare Magazzino"

        public TimeSpan? DeliveryOpenTime { get; set; }  
        public TimeSpan? DeliveryCloseTime { get; set; } 

        public bool IsDefault { get; set; } = false; 
        public bool IsActive { get; set; } = true;

        // --- NAVIGATION PROPERTIES ---
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;
    }
}