using Roscoff.Application.Interfaces;


namespace Roscoff.Infrastructure.Helpers;

public class WorkingDayCalculator : IWorkingDayCalculator
{
    // L'orario limite in cui la cucina smette di accettare ordini per il giorno in corso
    private const int CutOffHour = 16; 

    // Identificatore del fuso orario italiano
    // (Su Linux/Docker potrebbe chiamarsi "Europe/Rome", su Windows "W. Europe Standard Time")
    private readonly string _timeZoneId;

    public WorkingDayCalculator()
    {
        // Fallback per cross-platform compatibility
        _timeZoneId = OperatingSystem.IsWindows() ? "W. Europe Standard Time" : "Europe/Rome";
    }

    public DateTime CalculateDeliveryDate(DateTime orderDateTimeUtc, int daysRequired)
    {
        // 1. Convertiamo l'ora UTC del server nell'ora locale italiana reale
        var italyTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId);
        var localOrderTime = TimeZoneInfo.ConvertTimeFromUtc(orderDateTimeUtc, italyTimeZone);

        // Partiamo dalla data in cui è stato piazzato l'ordine
        var processingDate = localOrderTime.Date;

        // 2. Regola del Cut-Off e del Weekend iniziale
        // Se ordino DOPO le 16:00, o se ordino di Sabato/Domenica, 
        // è come se avessi ordinato la mattina del primo giorno lavorativo utile.
        if (localOrderTime.Hour >= CutOffHour || IsWeekend(processingDate))
        {
            processingDate = AddWorkingDays(processingDate, 1);
        }

        // 3. Aggiungiamo i giorni di lavorazione richiesti dal piatto
        var deliveryDate = AddWorkingDays(processingDate, daysRequired);

        // 4. Restituiamo la data a mezzanotte (la consegna vale per tutto l'arco della giornata)
        return deliveryDate;
    }

    /// <summary>
    /// Aggiunge N giorni lavorativi a una data, saltando sistematicamente Sabato e Domenica.
    /// </summary>
    private DateTime AddWorkingDays(DateTime startDate, int daysToAdd)
    {
        var resultDate = startDate;
        var remainingDays = daysToAdd;

        while (remainingDays > 0)
        {
            resultDate = resultDate.AddDays(1);

            if (!IsWeekend(resultDate))
            {
                remainingDays--;
            }
        }

        return resultDate;
    }

    private bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
}