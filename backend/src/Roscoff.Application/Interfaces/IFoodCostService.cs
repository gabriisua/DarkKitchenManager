namespace Roscoff.Application.Interfaces;

public interface IFoodCostService
{
    /// <summary>
    /// Calcola il costo totale di produzione di un piatto in centesimi di euro (int).
    /// </summary>
    Task<int> CalculatePlateFoodCostAsync(int plateId);
}