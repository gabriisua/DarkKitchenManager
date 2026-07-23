using System.Threading.Tasks;

namespace Roscoff.Application.Interfaces;

public interface IZplPrintService
{
    /// <summary>
    /// Invia una stringa ZPL cruda a una stampante di rete sulla porta 9100.
    /// </summary>
    Task<bool> PrintLabelAsync(string ipAddress, string zplContent);
}