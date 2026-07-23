namespace Roscoff.Core.Enums;

public enum OrderStatus
{
    Pending = 1,       // In attesa di validazione/accettazione
    Confirmed = 2,     // Accettato, in coda di produzione
    InProduction = 3,  // In cucina
    Shipped = 4,       // Affidato alla logistica / In consegna
    Delivered = 5,     // Consegnato
    Cancelled = 6      // Annullato
}
