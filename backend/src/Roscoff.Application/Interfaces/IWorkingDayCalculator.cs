namespace Roscoff.Application.Interfaces;

public interface IWorkingDayCalculator
{
    /// <summary>
    /// Calcola la prima data utile di consegna basandosi sui giorni di lavorazione richiesti,
    /// escludendo i weekend e calcolando il superamento del cut-off orario.
    /// </summary>
    /// <param name="orderDateTime">Data e ora di ricezione dell'ordine (UTC)</param>
    /// <param name="daysRequired">Giorni lavorativi richiesti dal piatto più complesso nell'ordine</param>
    /// <returns>La data di consegna calcolata (a mezzanotte del giorno di consegna)</returns>
    DateTime CalculateDeliveryDate(DateTime orderDateTime, int daysRequired);
}