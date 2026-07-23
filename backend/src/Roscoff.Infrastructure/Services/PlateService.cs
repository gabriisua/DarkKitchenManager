using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Dtos.Catalog;
using Roscoff.Application.Dtos.Common;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Catalog;
using Roscoff.Infrastructure.Data;
using Roscoff.Infrastructure.Pdf;

namespace Roscoff.Infrastructure.Services;

public class PlateService : IPlateService
{
    private readonly RoscoffDbContext _context;
    private readonly INutritionService _nutritionService;
    private readonly IPdfEngineService _pdfEngine;

    public PlateService(RoscoffDbContext context, INutritionService nutritionService, IPdfEngineService pdfEngine)
    {
        _context = context;
        _nutritionService = nutritionService;
        _pdfEngine = pdfEngine;
    }

    public async Task<PaginatedResponseDto<PlateResponseDto>> GetAllAsync(PlateQueryParameters filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Plates
            .Include(p => p.Category)
            .Include(p => p.PlateIngredients)
                .ThenInclude(pi => pi.Ingredient)
            .AsSplitQuery()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search;
            query = query.Where(p => p.Name.Contains(searchTerm) ||
                                     (p.Code != null && p.Code.Contains(searchTerm)) || 
                                     (p.Description != null && p.Description.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(p => p.Name == filter.Name);

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(p => p.IsActive == filter.IsActive.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.BasePrice >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.BasePrice <= filter.MaxPrice.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(p => p.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(p => p.CreatedAt <= filter.DateTo.Value);
            
        if (filter.LineType.HasValue)
            query = query.Where(p => p.LineType == filter.LineType.Value);

        if (filter.DietaryIcon.HasValue)
            query = query.Where(p => p.DietaryIcon == filter.DietaryIcon.Value);

        bool isDesc = filter.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        query = filter.SortColumn?.ToLower() switch
        {
            "name"       => isDesc ? query.OrderByDescending(p => p.Name)        : query.OrderBy(p => p.Name),
            "baseprice"  => isDesc ? query.OrderByDescending(p => p.BasePrice)   : query.OrderBy(p => p.BasePrice),
            "category"   => isDesc ? query.OrderByDescending(p => p.Category!.Name) : query.OrderBy(p => p.Category!.Name),
            _            => isDesc ? query.OrderByDescending(p => p.CreatedAt)   : query.OrderBy(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        int validPage = filter.Page > 0 ? filter.Page : 1;
        int validPageSize = filter.PageSize > 0 ? filter.PageSize : 10;

        var items = await query
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .Select(p => new PlateResponseDto(
                p.Id,
                p.Code, 
                p.Name,
                p.Description,
                p.Category != null ? p.Category.Name : "N/D",
                p.BasePrice,
                p.PackagingCost,
                p.VatRate,
                p.EanCode,
                p.MicrowaveWattage,
                p.MicrowaveMinutes,
                p.PreparationInstructions,
                p.DaysToExpire,
                p.ProductType,
                p.PackagingDescription,
                p.StorageConditions,
                p.PreservationTechnology,
                p.LineType, 
                p.DietaryIcon,
                p.PlateIngredients.Select(pi => new PlateIngredientResponseDto(
                    pi.IngredientId,
                    pi.Ingredient != null ? pi.Ingredient.Name : "Sconosciuto",
                    pi.Ingredient != null ? pi.Ingredient.SubIngredients : null, 
                    pi.WeightInGrams,
                    pi.Ingredient != null ? pi.Ingredient.CostPer1000g : 0
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedResponseDto<PlateResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize
        };
    }

    public async Task<PlateResponseDto> CreatePlateAsync(RequestPlateDto dto)
    {
        var plate = new Plate
        {
            Code = dto.Code, 
            Name = dto.Name,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            BasePrice = dto.BasePrice,
            VatRate = dto.VatRate,
            PackagingCost = dto.PackagingCost,
            DaysToExpire = dto.DaysToExpire, 
            WorkingDaysRequired = 1,
            IsActive = true,
            EanCode = dto.EanCode,
            MicrowaveWattage = dto.MicrowaveWattage,
            MicrowaveMinutes = dto.MicrowaveMinutes,
            PreparationInstructions = dto.PreparationInstructions,
            ProductType = !string.IsNullOrWhiteSpace(dto.ProductType) ? dto.ProductType : "Preparazione gastronomica",
            PackagingDescription = dto.PackagingDescription,
            StorageConditions = !string.IsNullOrWhiteSpace(dto.StorageConditions) ? dto.StorageConditions : "Conservare in frigorifero tra 0°C e +4°C.",
            PreservationTechnology = !string.IsNullOrWhiteSpace(dto.PreservationTechnology) ? dto.PreservationTechnology : "Confezionato in atmosfera protettiva (ATM).",
            LineType = dto.LineType,
            DietaryIcon = dto.DietaryIcon,
            PlateIngredients = dto.Ingredients.Select(i => new PlateIngredient
            {
                IngredientId = i.IngredientId,
                WeightInGrams = i.WeightInGrams
            }).ToList()
        };

        _context.Plates.Add(plate);
        await _context.SaveChangesAsync();

        var createdPlateDto = await GetPlateWithIngredientsAsync(plate.Id);
        return createdPlateDto!;
    }

    public async Task<PlateResponseDto?> GetPlateWithIngredientsAsync(int id)
    {
        var plate = await _context.Plates
            .Include(p => p.Category)
            .Include(p => p.PlateIngredients)
                .ThenInclude(pi => pi.Ingredient)
                    .ThenInclude(i => i!.IngredientAllergens)
                        .ThenInclude(ia => ia.Allergen)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plate == null) return null;

        return new PlateResponseDto(
            plate.Id,
            plate.Code, 
            plate.Name,
            plate.Description,
            plate.Category?.Name ?? "N/D",
            plate.BasePrice,
            plate.PackagingCost,
            plate.VatRate,
            plate.EanCode,
            plate.MicrowaveWattage,
            plate.MicrowaveMinutes,
            plate.PreparationInstructions,
            plate.DaysToExpire,
            plate.ProductType,
            plate.PackagingDescription,
            plate.StorageConditions,
            plate.PreservationTechnology,
            plate.LineType,
            plate.DietaryIcon,
            plate.PlateIngredients.Select(pi => new PlateIngredientResponseDto(
                pi.IngredientId,
                pi.Ingredient?.Name ?? "Sconosciuto",
                pi.Ingredient?.SubIngredients, 
                pi.WeightInGrams,
                pi.Ingredient?.CostPer1000g ?? 0
            )).ToList()
        );
    }
    
    public async Task<PlateResponseDto?> UpdatePlateAsync(int id, UpdatePlateDto dto)
    {
        var plate = await _context.Plates
            .Include(p => p.PlateIngredients)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plate == null) return null;

        plate.Code = dto.Code; 
        plate.Name = dto.Name ?? plate.Name;
        plate.Description = dto.Description ?? plate.Description;
        plate.CategoryId = dto.CategoryId > 0 ? dto.CategoryId : plate.CategoryId;
        plate.BasePrice = dto.BasePrice;
        plate.VatRate = dto.VatRate;
        plate.PackagingCost = dto.PackagingCost;
        plate.DaysToExpire = dto.DaysToExpire;
        plate.EanCode = dto.EanCode;
        plate.MicrowaveWattage = dto.MicrowaveWattage;
        plate.MicrowaveMinutes = dto.MicrowaveMinutes;
        plate.PreparationInstructions = dto.PreparationInstructions;
        plate.ProductType = dto.ProductType ?? plate.ProductType;
        plate.PackagingDescription = dto.PackagingDescription;
        plate.StorageConditions = dto.StorageConditions ?? plate.StorageConditions;
        plate.PreservationTechnology = dto.PreservationTechnology ?? plate.PreservationTechnology;
        
        plate.LineType = dto.LineType;
        plate.DietaryIcon = dto.DietaryIcon;

        var newIngredientIds = dto.Ingredients?.Select(i => i.IngredientId).ToList() ?? new List<int>();

        var ingredientsToRemove = plate.PlateIngredients
            .Where(pi => !newIngredientIds.Contains(pi.IngredientId))
            .ToList();

        foreach (var toRemove in ingredientsToRemove)
        {
            plate.PlateIngredients.Remove(toRemove);
        }

        if (dto.Ingredients != null)
        {
            foreach (var incomingIngredient in dto.Ingredients)
            {
                var existingPlateIngredient = plate.PlateIngredients
                    .FirstOrDefault(pi => pi.IngredientId == incomingIngredient.IngredientId);

                if (existingPlateIngredient != null)
                {
                    existingPlateIngredient.WeightInGrams = incomingIngredient.WeightInGrams;
                }
                else
                {
                    plate.PlateIngredients.Add(new PlateIngredient
                    {
                        IngredientId = incomingIngredient.IngredientId,
                        WeightInGrams = incomingIngredient.WeightInGrams,
                        PlateId = plate.Id
                    });
                }
            }
        }

        await _context.SaveChangesAsync();

        return await GetPlateWithIngredientsAsync(plate.Id);
    }

    public async Task<bool> SoftDeletePlateAsync(int id)
    {
        var plate = await _context.Plates.FindAsync(id);

        if (plate == null) return false;

        plate.IsActive = false;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<byte[]?> GenerateTechnicalSheetPdfAsync(int id)
    {
        var plateDto = await GetPlateWithIngredientsAsync(id);

        if (plateDto == null) return null;

        var ingredientsList = plateDto.Ingredients
            .OrderByDescending(pi => pi.WeightInGrams)
            .Select(pi => 
            {
                var sub = !string.IsNullOrWhiteSpace(pi.SubIngredients) ? $" ({pi.SubIngredients})" : "";
                return $"{pi.IngredientName}{sub}";
            })
            .ToList();

        string rawIngredientsText = string.Join(", ", ingredientsList) + ".";
        string formattedIngredientsHtml = Regex.Replace(rawIngredientsText, @"\b[A-Z]{4,}\b", "<strong><u>$0</u></strong>");
        
        var nutrition = await _nutritionService.CalculatePlateNutritionAsync(id);
        
        string allergensInTraces = nutrition.Allergens != null && nutrition.Allergens.Any() 
            ? string.Join(", ", nutrition.Allergens.Select(a => a.Name)) 
            : "Nessuno";

        string finalHtml = TechnicalSheetTemplateGenerator.GenerateHtml(plateDto, formattedIngredientsHtml, allergensInTraces, nutrition);

        return await _pdfEngine.GeneratePdfFromHtmlAsync(finalHtml); 
    }
}