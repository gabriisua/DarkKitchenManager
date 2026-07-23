using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Roscoff.Application.Dtos.Invoice;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Invoice;
using Roscoff.Infrastructure.Helpers;

namespace Roscoff.Infrastructure.Services;

public class FattureInCloudService : IFattureInCloudService
{
    private readonly HttpClient _httpClient;
    private readonly FattureInCloudSettings _settings;
    private readonly ILogger<FattureInCloudService> _logger;

    public FattureInCloudService(
        HttpClient httpClient,
        IOptions<FattureInCloudSettings> settings,
        ILogger<FattureInCloudService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;

        _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
    }

    private int GetFattureInCloudVatId(decimal vatRate)
    {
        string vatKey = vatRate.ToString("0.##", CultureInfo.InvariantCulture);

        if (_settings.VatMappings != null && _settings.VatMappings.TryGetValue(vatKey, out int vatId))
        {
            return vatId;
        }

        int countCaricati = _settings.VatMappings?.Count ?? 0;

        throw new InvalidOperationException(
            $"Errore mapping IVA: Il DB ha inviato {vatRate}%, convertito nella chiave '{vatKey}'. " +
            $"Questa chiave non esiste nel tuo appsettings.json. (Totale mappature caricate in memoria: {countCaricati})");
    }

    public async Task<FicDocumentResponseDto> CreateInvoiceForCustomerAsync(List<Order> orders,
        CancellationToken cancellationToken = default)
    {
        if (orders == null || !orders.Any())
            throw new ArgumentException("Nessun ordine fornito per la fatturazione cumulativa.");

        var firstOrder = orders.First();
        var customer = firstOrder.Customer;

        if (customer == null)
            throw new ArgumentException($"Dati cliente mancanti per l'ordine {firstOrder.OrderNumber}.");

        _logger.LogInformation("Generazione fattura cumulativa per il cliente {Customer} contenente {Count} ordini...",
            customer.BusinessName, orders.Count);

        var allItems = new List<object>();
        
        foreach (var order in orders)
        {
            string lineName = string.IsNullOrWhiteSpace(order.CustomerReference)
                ? order.OrderNumber
                : $"{order.OrderNumber} - {order.CustomerReference}";

            var firstItem = order.OrderItems.FirstOrDefault();
            decimal vatRate = firstItem?.VatRate ?? 10.00m; 
            int mappedVatId = GetFattureInCloudVatId(vatRate);

            allItems.Add(new
            {
                name = lineName,
                qty = 1,
                net_price = order.NetAmountCents / 100m,
                vat = new { id = mappedVatId }
            });
        }

        string notes = "Fattura riepilogativa ordini di consegna.";
        decimal totalGrossEuros = orders.Sum(o => o.TotalGrossCents) / 100m;

        int daysToAdd = customer.PaymentTermsDays ?? 0;
        string calculatedDueDate = DateTime.UtcNow.AddDays(daysToAdd).ToString("yyyy-MM-dd");

        // Cerchiamo tassativamente l'hub che rappresenta la sede legale/principale
        var mainHub = customer.DeliveryHubs?.FirstOrDefault(h => h.Name == "Sede Principale") 
                      ?? customer.DeliveryHubs?.FirstOrDefault(h => h.IsDefault)
                      ?? customer.DeliveryHubs?.FirstOrDefault();

        var payload = new
        {
            data = new
            {
                type = "invoice",
                date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                entity = new
                {
                    name = customer.BusinessName,
                    vat_number = customer.VatNumber,
                    fiscal_code = customer.FiscalCode ?? customer.VatNumber,
                    address_street = mainHub?.ShippingAddress ?? "",
                    address_city = mainHub?.City ?? "",
                    address_postal_code = mainHub?.ZipCode ?? "",
                    address_province = mainHub?.Province ?? "",
                    certified_email = customer.Pec ?? "",
                    ei_code = !string.IsNullOrWhiteSpace(customer.SdiCode) ? customer.SdiCode : "0000000"
                },
                subject = $"Fattura riepilogativa del {DateTime.UtcNow:dd/MM/yyyy}",
                notes = notes,
                items_list = allItems,
                payments_list = new[]
                {
                    new
                    {
                        amount = totalGrossEuros,
                        due_date = calculatedDueDate, 
                        status = "not_paid" 
                    }
                }
            }
        };

        var url = $"c/{_settings.CompanyId}/issued_documents";
        var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var contentError = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Errore API FattureInCloud: {response.StatusCode} - {contentError}");
        }

        var jsonResult = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: cancellationToken);
        var dataNode = jsonResult?["data"];

        int docId = dataNode?["id"]?.GetValue<int>() ??
                    throw new InvalidOperationException("ID documento non restituito da FIC");
        string invoiceNum = dataNode?["number"]?.ToString() ?? "N/D";
        string? urlPdf = dataNode?["url_pdf"]?.ToString();

        return new FicDocumentResponseDto(docId, invoiceNum, urlPdf);
    }
    
    public async Task<string> GetInvoiceUrlAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var url = $"c/{_settings.CompanyId}/issued_documents/{documentId}";
        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string contentError = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Errore FiC: {response.StatusCode} - {contentError}");
        }

        string rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var jsonResult = System.Text.Json.Nodes.JsonNode.Parse(rawJson);
        
        string? invoiceUrl = jsonResult?["data"]?["url"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(invoiceUrl))
        {
            throw new InvalidOperationException("Fatture in Cloud non ha restituito il link della fattura.");
        }

        return invoiceUrl;
    }
    
    public async Task DeleteInvoiceAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var url = $"c/{_settings.CompanyId}/issued_documents/{documentId}";
        var response = await _httpClient.DeleteAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var contentError = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Errore API FattureInCloud durante l'eliminazione del documento {documentId}: {response.StatusCode} - {contentError}");
        }
        
        _logger.LogInformation("Fattura {DocumentId} eliminata con successo da Fatture in Cloud.", documentId);
    }
}