using PuppeteerSharp;
using PuppeteerSharp.Media;
using Roscoff.Application.Interfaces;

namespace Roscoff.Infrastructure.Services;

public class PdfEngineService : IPdfEngineService, IAsyncDisposable
{
    private IBrowser? _browser;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    // Questo metodo garantisce che Chrome venga avviato una sola volta in tutta l'app
    private async Task<IBrowser> GetBrowserAsync()
    {
        if (_browser != null && !_browser.IsClosed)
            return _browser;

        await _semaphore.WaitAsync();
        try
        {
            if (_browser == null || _browser.IsClosed)
            {
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                _browser = await Puppeteer.LaunchAsync(new LaunchOptions 
                { 
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" } 
                });
            }
            return _browser;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent)
    {
        // 1. Prendi il browser (già avviato e caldo)
        var browser = await GetBrowserAsync();
        
        // 2. Apri solo una nuova scheda
        await using var page = await browser.NewPageAsync();

        // 3. Usa 'Load' invece di 'Networkidle0'. 
        // 'Load' aspetta che la pagina sia caricata, senza aspettare i 500ms di silenzio di rete extra.
        await page.SetContentAsync(htmlContent, new NavigationOptions 
        { 
            WaitUntil = new[] { WaitUntilNavigation.Load } 
        });

        var pdfBytes = await page.PdfDataAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions 
            { 
                Top = "10mm", Bottom = "10mm", Left = "10mm", Right = "10mm" 
            }
        });

        // 4. La scheda (page) viene chiusa automaticamente dal 'await using', liberando la RAM.
        return pdfBytes;
    }

    // Pulisce l'istanza di Chromium quando l'applicazione viene spenta
    public async ValueTask DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            await _browser.DisposeAsync();
        }
    }
}