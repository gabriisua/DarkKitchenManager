using System;
using System.Collections.Generic;
using System.Text;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Interfaces;
using Roscoff.Infrastructure.Pdf;

namespace Roscoff.Infrastructure.Services;

public class PrinterService : IPrinterService
{
    private readonly IZplPrintService _zplPrintService;

    // =========================================================================
    // CONFIGURAZIONE IP DELLE STAMPANTI
    // =========================================================================
    // Stampante principale (es. per Standard e Crio)
    private readonly string _printerIpStandard = ""; 
    // Stampante secondaria (es. per rotoli lunghi Cortilia/Foorban)
    private readonly string _printerIpSecondaria = ""; 

    public PrinterService(IZplPrintService zplPrintService)
    {
        _zplPrintService = zplPrintService;
    }

    // =========================================================================
    // MOTORE 1: ETICHETTE STANDARD
    // =========================================================================
    public void PrintMultipleLabels(List<PrintJobRequestDto> printJobs)
    {
        var masterStringBuilder = new StringBuilder();

        for (int i = 0; i < printJobs.Count; i++)
        {
            var job = printJobs[i];

            string singleLabelZpl = LabelTemplateGenerator.GenerateZpl(
                plate: job.Plate,
                nutrition: job.Nutrition,
                allergensInTraces: job.Allergens,
                lotNumber: job.LotNumber,
                productionDate: job.ProductionDate,
                totalCopies: job.Copies,
                pauseAfter: job.PauseAfter,
                customExpiryDate: job.CustomExpiryDate,
                customWeight: job.CustomWeight
            );

            masterStringBuilder.Append(singleLabelZpl);

            if (i < printJobs.Count - 1)
            {
                var currentPlateName = job.Plate.Name.ToUpper();
                var nextPlateName = printJobs[i + 1].Plate.Name.ToUpper();

                string separatorLabel = $@"
^XA
^PW480
^LL800
^FWR
^FO350,50^A0R,40,40^FDFINE LOTTO:^FS
^FO280,50^A0R,60,60^FD{currentPlateName}^FS
^FO150,50^A0R,30,30^FDCambio rotolo o scatola.^FS
^FO80,50^A0R,40,40^FDPROSSIMO JOB:^FS
^FO20,50^A0R,50,50^FD{nextPlateName}^FS
^PQ1,1
^XZ";
                masterStringBuilder.Append(separatorLabel);
            }
        }

        string finalZplData = masterStringBuilder.ToString();
        SendToPrinter(finalZplData, _printerIpStandard);
    }

    // =========================================================================
    // MOTORE 2: ETICHETTE CORTILIA (Formato Lungo 25.4 cm, Verticale)
    // =========================================================================
    public void PrintCortiliaMultipleLabels(List<PrintJobRequestDto> printJobs)
    {
        var masterStringBuilder = new StringBuilder();

        for (int i = 0; i < printJobs.Count; i++)
        {
            var job = printJobs[i];

            string singleLabelZpl = CortiliaLabelGenerator.GenerateZpl(
                plate: job.Plate,
                nutrition: job.Nutrition,
                allergensInTraces: job.Allergens,
                lotNumber: job.LotNumber,
                productionDate: job.ProductionDate,
                totalCopies: job.Copies,
                pauseAfter: job.PauseAfter,
                customExpiryDate: job.CustomExpiryDate,
                customWeight: job.CustomWeight
            );

            masterStringBuilder.Append(singleLabelZpl);

            if (i < printJobs.Count - 1)
            {
                var currentPlateName = job.Plate.Name.ToUpper();
                var nextPlateName = printJobs[i + 1].Plate.Name.ToUpper();

                string separatorLabel = $@"
^XA
^PW480
^LL2032
^FWN
^FO20,300^A0N,50,50^FDFINE LOTTO:^FS
^FO20,380^A0N,60,60^FD{currentPlateName}^FS
^FO20,550^A0N,30,30^FDCambio scatola o rotolo.^FS
^FO20,850^A0N,50,50^FDPROSSIMO JOB:^FS
^FO20,930^A0N,60,60^FD{nextPlateName}^FS
^PQ1,1
^XZ";
                masterStringBuilder.Append(separatorLabel);
            }
        }

        string finalZplData = masterStringBuilder.ToString();
        SendToPrinter(finalZplData, _printerIpSecondaria);
    }

    // =========================================================================
    // MOTORE 3: ETICHETTE FOORBAN (Formato Lungo 25.4 cm, Verticale)
    // =========================================================================
    public void PrintFoorbanMultipleLabels(List<PrintJobRequestDto> printJobs)
    {
        var masterStringBuilder = new StringBuilder();

        for (int i = 0; i < printJobs.Count; i++)
        {
            var job = printJobs[i];

            string singleLabelZpl = FoorbanLabelGenerator.GenerateZpl(
                plate: job.Plate,
                nutrition: job.Nutrition,
                allergensInTraces: job.Allergens,
                lotNumber: job.LotNumber,
                productionDate: job.ProductionDate,
                totalCopies: job.Copies,
                pauseAfter: job.PauseAfter,
                customExpiryDate: job.CustomExpiryDate,
                isWow: job.IsWow,
                isXl: job.IsXl,
                customWeight: job.CustomWeight
            );

            masterStringBuilder.Append(singleLabelZpl);

            if (i < printJobs.Count - 1)
            {
                var currentPlateName = job.Plate.Name.ToUpper();
                var nextPlateName = printJobs[i + 1].Plate.Name.ToUpper();

                string separatorLabel = $@"
^XA
^PW480
^LL2032
^FWN
^FO20,300^A0N,50,50^FDFINE LOTTO:^FS
^FO20,380^A0N,60,60^FD{currentPlateName}^FS
^FO20,550^A0N,30,30^FDCambio scatola o rotolo.^FS
^FO20,850^A0N,50,50^FDPROSSIMO JOB:^FS
^FO20,930^A0N,60,60^FD{nextPlateName}^FS
^PQ1,1
^XZ";
                masterStringBuilder.Append(separatorLabel);
            }
        }

        string finalZplData = masterStringBuilder.ToString();
        SendToPrinter(finalZplData, _printerIpSecondaria);
    }

    // =========================================================================
    // MOTORE 4: ETICHETTE CRIOGENICHE (Formato 15x6.5 cm)
    // =========================================================================
    public void PrintCrioMultipleLabels(List<PrintJobRequestDto> printJobs)
    {
        var masterStringBuilder = new StringBuilder();

        for (int i = 0; i < printJobs.Count; i++)
        {
            var job = printJobs[i];

            string singleLabelZpl = CrioLabelGenerator.GenerateZpl(
                plate: job.Plate,
                nutrition: job.Nutrition,
                allergensInTraces: job.Allergens,
                lotNumber: job.LotNumber,
                productionDate: job.ProductionDate,
                isThawed: job.IsThawed,                     
                thawingDate: job.ThawingDate,               
                targetLanguage: job.TargetLanguage ?? "IT", 
                totalCopies: job.Copies,
                pauseAfter: job.PauseAfter,
                customExpiryDate: job.CustomExpiryDate,     
                customWeight: job.CustomWeight
            );

            masterStringBuilder.Append(singleLabelZpl);

            if (i < printJobs.Count - 1)
            {
                var currentPlateName = job.Plate.Name.ToUpper();
                var nextPlateName = printJobs[i + 1].Plate.Name.ToUpper();

                // Separatore adattato alle dimensioni (520 x 1200)
                string separatorLabel = $@"
^XA
^PW520
^LL1200
^FWR
^FO450,50^A0R,40,40^FDFINE LOTTO:^FS
^FO380,50^A0R,60,60^FD{currentPlateName}^FS
^FO250,50^A0R,30,30^FDCambio rotolo o scatola.^FS
^FO150,50^A0R,40,40^FDPROSSIMO JOB:^FS
^FO50,50^A0R,50,50^FD{nextPlateName}^FS
^PQ1,1
^XZ";
                masterStringBuilder.Append(separatorLabel);
            }
        }

        string finalZplData = masterStringBuilder.ToString();
        SendToPrinter(finalZplData, _printerIpStandard);
    }

    // =========================================================================
    // LAYER DI COMUNICAZIONE (Delegato a IZplPrintService)
    // =========================================================================
    private void SendToPrinter(string zplData, string ipAddress)
    {
        // Chiamata asincrona al servizio TCP, risolta in modo sincrono (.GetAwaiter().GetResult())
        // per mantenere la compatibilità con l'interfaccia IPrinterService attuale.
        bool success = _zplPrintService.PrintLabelAsync(ipAddress, zplData).GetAwaiter().GetResult();
        
        if (!success)
        {
            throw new Exception($"Impossibile comunicare con la stampante all'IP {ipAddress}. Verifica che sia accesa e in rete.");
        }
    }
}