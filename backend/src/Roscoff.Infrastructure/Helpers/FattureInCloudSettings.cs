namespace Roscoff.Infrastructure.Helpers;

public class FattureInCloudSettings
{
    public string BaseUrl { get; set; } = "https://api-v2.fattureincloud.it";
    public string AccessToken { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    
    public Dictionary<string, int> VatMappings { get; set; } = new();
}