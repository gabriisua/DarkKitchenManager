using Roscoff.Application.Dtos.Catalog;
namespace Roscoff.Application.Interfaces; 

public interface IPrinterService
{
    void PrintMultipleLabels(List<PrintJobRequestDto> printJobs);
    void PrintCortiliaMultipleLabels(List<PrintJobRequestDto> printJobs); 
    void PrintFoorbanMultipleLabels(List<PrintJobRequestDto> printJobs);
    void PrintCrioMultipleLabels(List<PrintJobRequestDto> printJobs);
}