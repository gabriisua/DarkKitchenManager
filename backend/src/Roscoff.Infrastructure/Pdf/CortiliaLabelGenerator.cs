using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Core.Entities.Catalog;

namespace Roscoff.Infrastructure.Pdf;

public static class CortiliaLabelGenerator
{
    private const string CORTILIA_LOGO_ZPL = @""; 
    
    private const string ICON_GOURMET = @"^GFA,1710,1710,15,,::::::::::::::::::T07F8,S0JFE,R0LFC,Q03F8I07F,Q0FCK0FC,P03EL01F,P0F8M07C,O01EN01E,O03CO0F8,O07P03C,N01EP01E,N03CQ0F,N038Q078,N07R03C,N0ER01C,M01CS0E,M038S07,M038J01FFEK038,M07K03IF8J038,M0EK07IFCJ01C,M0EK0JFCK0C,L01CJ01JFEK0E,L01CJ01JFEK06,L018J01KFK07,L038J03KFK07,L03K03KFK03,L03I07E3KF1F80038,L07001PFE0018,L06003QF0018,L06003QF801C,L06007QF801C,L0E007QFC01C,L0E007QFC00C,L0E00RFC00C,L0C00RFC00C,:L0C007QFC00C,L0C007QF800C,L0C003QF800C,L0C003QFI0C,L0C001PFEI0C,L0EI07OF8I0C,L0EJ03MF8J0C,L0EJ03MF8I01C,L06J07MF8I01C,L06J07MF8I018,:L07J07MF8I018,L03J07MFCI038,L03J0NFCI03,L038I0NFCI07,L018I0NFCI07,L01CI0NFCI06,M0CI0NFEI0E,M0E001NFE001C,M06001NFE001C,M07001FFJ01FE0038,M03801CM06003,M038S07,M01CS0E,N0ER01C,N07R03C,N038Q078,N01CQ0F,O0EP01E,O078O03C,O03CO0F,O01FN01E,P078M07C,P03FL01F,Q0FCK0FC,Q03FCI0FF,R07KF8,S0JFC,T03F,,:::::::::::::::::::^FS";
    private const string ICON_VEGETALE = @"^GFA,1635,1635,15,,:::::::::::::::::::T0FFE,R01JFE,R0FF003FC,Q03FJ03F,Q0F8K07C,P01EL01E,P078M078,P0FN01C,O01CO0E,O038O07,O07P038,O0EP01C,N01CQ0E,N038Q07,N03R038,N07R018,N0ER01C,N0CS0C,M01CS0E,M018K03IF8I06,M018J01KFE007,M03K07LF003,M03J01MF0038,M07J03LFE0018,M06J07LFE0018,M06J0MFC0018,M06I01MFC001C,M0EI03MF8I0C,:M0CI07IF9IF8I0C,M0CI0IFE7IFJ0C,M0CI0IF8JFJ0C,M0CI0FFE3JFJ0C,M0C001FF8JFEJ0C,M0C001FF1JFEJ0C,M0C001FC3JFEJ0C,M0C001F8KFCJ0C,M0E001F1KFCJ0C,M0E001E3KF8J0C,M06I0CLF8I01C,M06I01LFJ018,M06I03KFEJ018,M07I07KFEJ018,M03I0LFCJ038,M03001LF8J03,M038039KFK07,M018030JFEK06,M01C0607IFCK0E,N0CI01IFL0C,N0EJ03F8K01C,N07R018,N03R038,N038Q07,N01CQ0E,O0EP01C,O07P038,O038O07,O01CO0E,P0FN01C,P078M078,P01EL01F,Q0F8K07C,Q03FJ01F,R0FF003FC,R01JFE,S01FFE,,::::::::::::::::::::::^FS";
    private const string ICON_FITNESS = @"^GFA,1665,1665,15,,:::::::::::::::::::::::S07IF,R03JFE,R0FCI0FC,Q03CJ01F,Q0FL07C,P03CL01E,P078M07,P0EN03C,O01CO0E,O038O07,O07P038,O0E0018L018,N01C007CM0C,N01801FCM0E,N03003FEM06,N07003FEM03,N06007FEM038,N0E007FCM018,N0C00FFCM01C,N0C00FF8N0C,M01800FEO0C,M01800F8O06,M01801F8O06,M03001F8O06,:M03001F8O03,M03001F8J0F8I03,M03001FCI03FEI03,M07003FCI0IF8003,M06003FE001IFC003,M06003FE001IFE003,M06003FFI07FFE003,M06003FF01JFE003,M03003FF07JFE003,M03003FF1KFE003,M03003FF3KFE003,M03003NFE003,M03003NFE006,M03001NFE006,M01801NFC006,M01801NFC00E,M01800NF800C,N0C00NF800C,N0C007KFDF0018,N06007KF3E0018,N06003JFDFC003,N03001LF8003,N03801KFEI06,N01800KFJ0E,O0C007F8K01C,O0E0018L038,O07P03,O038O0E,O01CN01C,P0EN038,P078M07,P01EL01E,Q0FL078,Q03EJ01F,R0FC001F8,R01JFE,S03FFE,,::::::::::::::::::::::::^FS";
    private const string ICON_PLANTED = @"^GFA,1380,1380,15,,::::::::::::::S01FFE,R01JFE,R0FE001F8,Q03EJ03E,Q0F8K078,P01EL01E,P078M0F,P0EN038,O01CN01C,O038O0E,O07P03,O0EP038,N01CP01C,N018Q0E,N03R06,N07R03,N06O03F038,N0EN03FF018,N0CN0FFE01C,M01CJ07FC1FFE00C,M018J03FF3FFE00C,M018J03KFE006,M03K03IFEFE006,M03K03FFEDFE006,M03K01IF3FC007,M03K01FDF7FC003,M06K01FEIFC003,M06K01FF7FF8003,M06L0FFB7FI03,M06L0FFDFEI03,M06L07FE7CI03,M06L03FEK03,M060107FC1F9K03,M0601BIF8M03,M0601BIFEK0403,M06019JFCI03E03,M06019KF001FE03,M0700DFFCFFC0FFC03,M0300CFFC1FC7FF003,M0300CIF81CFFE006,M03006JF81FF8006,M018067LFEI06,M0180603KF8I0C,M01803007IFEJ0C,N0C02001IF8I01C,N0CK07FEJ018,N06L0F8J038,N07R03,N03R07,N038Q0E,N01CQ0C,O0EP018,O06P038,O038O07,O01CO0E,P0EN03C,P07N078,P03CL01E,Q0FL07C,Q07CJ01F,Q01F8I0FC,R03KF,S07IF,U08,,::::::::::::^FS";

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

        var sb = new StringBuilder();
        
        sb.Append(@"^XA");
        sb.Append(@"^CI28");
        sb.Append(@"^POI");    
        sb.Append(@"^PW480");  
        sb.Append(@"^LL2032"); 
        sb.Append(@"^FWN");    
        
        sb.Append(@"^FO155,70^A0N,14,14^FB280,5,0,R^FD Prodotto per Cortilia S.p.A. \&Conservare in frigorifero a\&temperatura compresa tra 0 C e 4 C.\&Prodotto nello stabilimento:\&IT E9E2U CE^FS");
        
        string heatingTime = (!plate.MicrowaveMinutes.HasValue || plate.MicrowaveMinutes.Value <= 0) 
            ? @"0/30\&Sec" 
            : @"2/3\&Min";

        sb.Append($@"^FO35,160^A0N,18,18^FB280,2,0,R^FD{heatingTime}^FS");

        string selectedIconZpl = "";
        string lineName = "";

        switch (plate.LineType)
        {
            case PlateLineType.Gourmet:
                selectedIconZpl = ICON_GOURMET;
                lineName = "GOURMET";
                break;
            case PlateLineType.Vegetale:
                selectedIconZpl = ICON_VEGETALE;
                lineName = "VEGETALE";
                break;
            case PlateLineType.Fitness:
                selectedIconZpl = ICON_FITNESS;
                lineName = "FITNESS";
                break;
            case PlateLineType.Planted:
                selectedIconZpl = ICON_PLANTED;
                lineName = "PLANTED";
                break;
        }

        if (plate.LineType != PlateLineType.Standard)
        {
            sb.Append($@"^FO45,85{selectedIconZpl}");
            sb.Append($@"^FO45,192^A0N,16,16^FB120,2,0,C^FD{lineName}^FS");
        }

        sb.Append($@"^FO20,560^A0N,35,35^FB440,3,0,C^FD{plate.Name}^FS");

        sb.Append($@"^FO20,880^A0N,35,35^FB440,3,0,C^FD{plate.Name}^FS");

        int currentX = 35;      
        int currentY = 1130;    
        int descFont = 20;      
        int lineStep = 24;      
        int sectionGap = 16;    
        int maxChars = 38;      

        var ingLines = SplitToLines("Ingredienti: " + ingredientiString, maxChars); 
        foreach (var line in ingLines)
        {
            sb.Append($@"^FO{currentX},{currentY}^A0N,{descFont},{descFont}^FD{line}^FS");
            currentY += lineStep; 
        }
        currentY += sectionGap; 

        var allLines = SplitToLines("Allergeni: Contiene " + allergensInTraces, maxChars);
        foreach (var line in allLines)
        {
            sb.Append($@"^FO{currentX},{currentY}^A0N,{descFont},{descFont}^FD{line}^FS");
            currentY += lineStep;
        }
        currentY += sectionGap;

        var tracceLines = SplitToLines("Tracce: " + allergensInTraces, maxChars);
        foreach (var line in tracceLines)
        {
            sb.Append($@"^FO{currentX},{currentY}^A0N,{descFont},{descFont}^FD{line}^FS");
            currentY += lineStep;
        }
        currentY += sectionGap;

        var nutLines = SplitToLines($"Valori nutrizionali per 100g: Energia {energiaKj}Kj/{energiaKcal}Kcal - Grassi {grassi}g (saturi {grassiSaturi}g) - Carboidrati {carboidrati}g (zuccheri {zuccheri}g) - Fibre {fibre}g - Proteine {proteine}g - Sale {sale}g", maxChars);
        foreach (var line in nutLines)
        {
            sb.Append($@"^FO{currentX},{currentY}^A0N,{descFont},{descFont}^FD{line}^FS");
            currentY += lineStep;
        }

        int footerY = 1800; 
        
        sb.Append($@"^FO{currentX},{footerY}^A0N,22,22^FD{totalWeight}gr^FS");
        sb.Append($@"^FO{currentX},{footerY + 30}^A0N,22,22^FDLotto: {lotNumber}^FS");
        sb.Append($@"^FO{currentX},{footerY + 60}^A0N,22,22^FDScadenza: {expiryDate}^FS");

        if (!string.IsNullOrWhiteSpace(plate.EanCode))
        {
            string barcodeCmd = plate.EanCode.Trim().Length <= 8 ? "^B8N" : "^BEN";
            sb.Append($@"^FO100,1940^BY3{barcodeCmd},80,Y,N^FD{plate.EanCode.Trim()}^FS");
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