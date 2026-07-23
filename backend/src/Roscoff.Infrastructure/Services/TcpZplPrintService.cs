using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Roscoff.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Roscoff.Infrastructure.Services;

public class TcpZplPrintService : IZplPrintService
{
    private readonly ILogger<TcpZplPrintService> _logger;

    public TcpZplPrintService(ILogger<TcpZplPrintService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> PrintLabelAsync(string ipAddress, string zplContent)
    {
        const int STANDARD_RAW_PRINT_PORT = 9100;

        try
        {
            _logger.LogInformation($"Tentativo di invio stampa ZPL all'IP {ipAddress}...");

            using var client = new TcpClient();
            
            // Impostiamo un timeout rapido per non bloccare il server se la stampante è spenta (es. 2 secondi)
            var connectTask = client.ConnectAsync(ipAddress, STANDARD_RAW_PRINT_PORT);
            if (await Task.WhenAny(connectTask, Task.Delay(2000)) != connectTask)
            {
                _logger.LogWarning($"Timeout: Impossibile raggiungere la stampante TSC all'IP {ipAddress}");
                return false;
            }

            // Se la connessione ha successo, prendiamo lo stream e spariamo i byte
            using var stream = client.GetStream();
            
            // UTF8 gestisce bene gli accenti italiani se la stampante è configurata con la codifica internazionale (comando ^CI28)
            byte[] data = Encoding.UTF8.GetBytes(zplContent);
            
            await stream.WriteAsync(data, 0, data.Length);
            await stream.FlushAsync();

            _logger.LogInformation("Etichetta inviata con successo alla TSC!");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Errore TCP durante l'invio alla stampante TSC {ipAddress}");
            return false;
        }
    }
}