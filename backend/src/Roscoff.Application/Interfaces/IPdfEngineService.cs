using System.Threading.Tasks;

namespace Roscoff.Application.Interfaces;

public interface IPdfEngineService
{
    Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent);
}