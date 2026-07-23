using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roscoff.Application.Dtos.Catalog;

namespace Roscoff.Infrastructure.Pdf;

public static class LabelTemplateGenerator
{
    private const string LOGO_ZPL = @"^GFA,4180,4180,19,,:::::::::::::::::::::::::::::::R0PFE,Q03QF8,Q07QFE,P01F8O01F,P01EQ0F8,P03CM0180038,P038M03E003C,P07N07F001C,P07N07F801C,P07O03800E,P07O01C00E,P07P0E00E,P07K03EI0600E,P07K03F800700E,P07K07FE00300E,P07L07F80100E,P07M0FC0180E,P07M07F0080E,P07L01FF8080E,P07L07A3C080E,P07L0F21F080E,P07K01E20FD80E,P07K03C204F00E,P07K038202I0E,P07K038201I0E,P07K030301800E,P07K030300C00E,P07K030380400E,P07K030180600E,P07K0301C0200E,P07K0180E0300E,P07N0F0100E,P07M087C300E,P07L03E3FE00E,P07L07F1FE00E,P07L04387800E,P07L061CJ0E,P07L020CJ0E,P07L0344J0E,P070048001F4J0E,P070094I0FCJ0E,P07I04I038J0E,P07L0CL0E,P07K01EEK0E,P070018012FK0E,P07001001198J0E,P0700600110CJ0E,P07K01306J0E,P07001800E72J0E,P07L0C3BJ0E,P07M01FJ0E,P07004I0386J0E,P07002I07EK0E,P07I0800FFK0E,P07002I0C78J0E,P07L041CJ0E,P07L040CJ0E,P07003400644J0E,P07002400234,P07I080011C,P07L01C,P07I04003F,P07007C00678,P07I0400438,P07L060C,W0304,W01E4K03E,X0FCK07F,X078K0FF8,V01EM0FFC,T0807FCI0401FFC,N018I03004FFI07C07FC,N01CI0E004CF8007FE1FC,O0E001C003C3C007FFDF8,O070038I060E007FFCF8,O0380FJ0207007FFEFE,O01C1EJ0301003FFEFFC,P0E3CI03F81003FFEFFE,P07781F07FCI03FFEFFE,P03F03FC4FFI03FFEIF,N07IFE7FE6CF8003LF,N07LFE3C3E003LF,N0383E03FE060E003LF,P03103F80303003LF,P060FJ0181003LF,P0C07CJ081003LF,O0380FEJ04I03KFE,O0700FAN03KFC,O0E0071N07KF8,O0C00608M07JF8,O08003O07JF8,S08N07JF8,S04N07F9FFC,Y03C0701FFC,Y03EJ0FFC,Y07FJ0FF8,Y04F8I0FF8,g03CI07F,P06O01CI01C,P07K018I0E,P07K03FI06,P07K07FC003,P07K03FF003,P07I04007F801,P07I04001FE018,P07M0FF008,P07004J0278080E,P07002J011E080E,P07I0CJ04F080E,P07002K023F80E,P07N018F00E,P07L03C04I0E,P070048007FC3I0E,P070094007FF8800E,P070024007FFEC00E,P07L01C7FE00E,P07M070FE00E,P070044I01C1E00E,P07I04J06J0E,P07N038I0E,P07004L0CI0E,P07009401FF83I0E,P07008403FFE1800E,P07K03IFC600E,P07K0700FF300E,P07K06001FF80E,P07K02I03F80E,P07L07C00780E,P07L07EK0E,P07L0EFK0E,P07L0C78J0E,P07L045CJ0E,P07L044CJ0E,P07L0224J0E,P07L011CJ0E,P07L018CJ0E,P07L03EK0E,P07L03FK0E,P07L0278J0E,P07L011CJ0E,P07M08CJ0E,P07L0686J0E,P07L0FC2J0E,P07L0FE2J0E,P07L04F4J0E,P07L066K0E,P07L03L0E,P07L018K0E,P07L0FEK0E,P07L0FFCJ0E,P07L0CFFJ0E,P07L041FCI0E,P07L0203EI0E,P07L0180FI0E,P07M0C07800E,P07O03801C,P07O01801C,P038Q03C,P03CQ078,P01EQ0F8,Q0F8O03F,Q07QFE,Q03QF8,R07OFE,,:::::::::::::::::::::::^FS"; 

    public static string GenerateZpl(
        PlateResponseDto plate, 
        NutritionalSummaryDto nutrition, 
        string allergensInTraces, 
        string lotNumber, 
        DateTime productionDate, 
        int totalCopies = 1,
        int pauseAfter = 0,
        DateTime? customExpiryDate = null,
        decimal? customWeight = null)
    {
        decimal totalWeightDecimal = customWeight ?? plate.Ingredients.Sum(i => i.WeightInGrams);
        var totalWeight = totalWeightDecimal.ToString("0");
        
        var finalExpiryDate = customExpiryDate ?? productionDate.AddDays(plate.DaysToExpire);
        var expiryDate = finalExpiryDate.ToString("dd/MM/yyyy");
        
        var ingredientiList = plate.Ingredients.Select(i => 
            string.IsNullOrWhiteSpace(i.SubIngredients) 
                ? i.IngredientName 
                : $"{i.IngredientName} ({i.SubIngredients})"
        );
        var ingredientiString = string.Join(", ", ingredientiList);

        var energiaKcal = nutrition.EnergyKcal.ToString("0");
        var energiaKj = (nutrition.EnergyKcal * 4.184m).ToString("0");
        var grassi = nutrition.Fats.ToString("0.0");
        var grassiSaturi = nutrition.SaturatedFats.ToString("0.0");
        var carboidrati = nutrition.Carbohydrates.ToString("0.0");
        var zuccheri = nutrition.Sugars.ToString("0.0");
        var fibre = nutrition.Fibers.ToString("0.0");
        var proteine = nutrition.Proteins.ToString("0.0");
        var sale = nutrition.Salt.ToString("0.00");

        var microonde = plate.MicrowaveMinutes.HasValue && plate.MicrowaveWattage.HasValue
            ? $"Microonde: forare pellicola, scaldare {plate.MicrowaveMinutes} min a {plate.MicrowaveWattage}W."
            : "";
        var forno = plate.PreparationInstructions ?? "Altre cotture: togliere dalla vaschetta (max 100 gradi).";
        var prepString = $"{microonde} {forno}".Trim();
        var productType = plate.ProductType ?? "Preparazione gastronomica";

        var sb = new StringBuilder();
        
        sb.Append(@"^XA");
        sb.Append(@"^CI28");
        sb.Append(@"^PW480"); 
        sb.Append(@"^LL800"); 
        sb.Append(@"^FWR");   
        
        int currentX = 450;
        int startY = 20; 

        sb.Append($@"^FO350,535{LOGO_ZPL}");

        sb.Append(@"^FO270,700^GB65,55,2^FS");
        sb.Append(@"^FO315,725^A0R,16,16^FDIT^FS");
        sb.Append(@"^FO295,705^A0R,16,16^FDE9E2U^FS");
        sb.Append(@"^FO275,725^A0R,16,16^FDCE^FS");

        sb.Append($@"^FO{currentX},{startY}^A0R,20,20^FDROSCOFF MEAL SRL - Via Messina 101, Seregno (MB)^FS");
        currentX -= 30; 

        sb.Append($@"^FO{currentX},{startY}^A0R,30,30^FD{plate.Name}^FS");
        currentX -= 35; 
        sb.Append($@"^FO{currentX},{startY}^A0R,18,18^FD{productType} - Peso: {totalWeight}g   |   Lotto: {lotNumber}^FS");
        currentX -= 25; 
        sb.Append($@"^FO{currentX},{startY}^A0R,18,18^FDDa consumare entro il: {expiryDate}^FS");
        currentX -= 30; 

        int descFont = 16;      
        int lineStep = 18;      
        int sectionGap = 8;     
        int maxChars = 65;      

        var ingLines = SplitToLines("Ingredienti: " + ingredientiString, maxChars); 
        foreach (var line in ingLines)
        {
            sb.Append($@"^FO{currentX},{startY}^A0R,{descFont},{descFont}^FD{line}^FS");
            currentX -= lineStep; 
        }
        currentX -= sectionGap; 

        var allLines = SplitToLines("Puo' contenere: " + allergensInTraces, maxChars);
        foreach (var line in allLines)
        {
            sb.Append($@"^FO{currentX},{startY}^A0R,{descFont},{descFont}^FD{line}^FS");
            currentX -= lineStep;
        }
        currentX -= sectionGap;

        string nutString = $"Valori medi 100g: Energia {energiaKj}Kj/{energiaKcal}Kcal - Grassi {grassi}g (saturi {grassiSaturi}g) - Carboidrati {carboidrati}g (zuccheri {zuccheri}g) - Fibre {fibre}g - Proteine {proteine}g - Sale {sale}g";
        var nutLines = SplitToLines(nutString, maxChars);
        foreach (var line in nutLines)
        {
            sb.Append($@"^FO{currentX},{startY}^A0R,{descFont},{descFont}^FD{line}^FS");
            currentX -= lineStep;
        }
        currentX -= sectionGap;

        var consLines = SplitToLines($"Conservazione: {plate.StorageConditions} {plate.PreservationTechnology}", maxChars);
        foreach (var line in consLines)
        {
            sb.Append($@"^FO{currentX},{startY}^A0R,{descFont},{descFont}^FD{line}^FS");
            currentX -= lineStep;
        }
        currentX -= sectionGap;

        int prepStartX = currentX; 
        
        var prepLines = SplitToLines("Preparazione: " + prepString, maxChars);
        foreach (var line in prepLines)
        {
            sb.Append($@"^FO{currentX},{startY}^A0R,{descFont},{descFont}^FD{line}^FS");
            currentX -= lineStep;
        }

        if (!string.IsNullOrWhiteSpace(plate.EanCode))
        {
            string barcodeCmd = plate.EanCode.Trim().Length <= 8 ? "^B8R" : "^BER";
            sb.Append($@"^FO{prepStartX},540^BY2{barcodeCmd},40,Y,N^FD{plate.EanCode.Trim()}^FS");
        }

        if (pauseAfter > 0)
        {
            sb.Append($@"^PQ{totalCopies},{pauseAfter}");
        }
        else
        {
            sb.Append($@"^PQ{totalCopies}");
        }

        sb.Append(@"^XZ");

        return sb.ToString();
    }

    private static List<string> SplitToLines(string text, int maxCharsPerLine)
    {
        var lines = new List<string>();
        var words = text.Split(' ');
        var currentLine = "";

        foreach (var word in words)
        {
            if ((currentLine + word).Length > maxCharsPerLine)
            {
                lines.Add(currentLine.Trim());
                currentLine = word + " ";
            }
            else
            {
                currentLine += word + " ";
            }
        }
        
        if (!string.IsNullOrWhiteSpace(currentLine))
            lines.Add(currentLine.Trim());

        return lines;
    }
}