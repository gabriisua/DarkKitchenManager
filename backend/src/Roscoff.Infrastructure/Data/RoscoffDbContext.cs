using Microsoft.EntityFrameworkCore;
using Roscoff.Application.Interfaces;
using Roscoff.Core.Entities.Catalog;
using Roscoff.Core.Entities.Client;
using Roscoff.Core.Entities.Invoice; 
using Roscoff.Core.Interfaces;

namespace Roscoff.Infrastructure.Data;

public class RoscoffDbContext : DbContext, IRoscoffDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public RoscoffDbContext(
        DbContextOptions<RoscoffDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // --- DB SETS ---
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<DeliveryHub> DeliveryHubs => Set<DeliveryHub>(); 
    public DbSet<Staff> StaffMembers => Set<Staff>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Plate> Plates => Set<Plate>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Allergen> Allergens => Set<Allergen>();
    public DbSet<IngredientAllergen> IngredientAllergens => Set<IngredientAllergen>();
    public DbSet<PlateIngredient> PlateIngredients => Set<PlateIngredient>();
    
    // DB SETS SCONTI
    public DbSet<ClientCategoryDiscount> ClientCategoryDiscounts => Set<ClientCategoryDiscount>();
    public DbSet<ClientPlateDiscount> ClientPlateDiscounts => Set<ClientPlateDiscount>();

    // DB SETS ORDINI
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // --- NUOVO: DB SET FATTURAZIONE ---
    public DbSet<PendingInvoice> PendingInvoices => Set<PendingInvoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCustomers(modelBuilder); 
        ConfigureMenuItems(modelBuilder);
        ConfigurePlateIngredients(modelBuilder);
        ConfigureIngredientAllergens(modelBuilder);
        ConfigureDiscounts(modelBuilder);
        ConfigureOrders(modelBuilder);
        
        // --- NUOVA CONFIGURAZIONE ---
        ConfigurePendingInvoices(modelBuilder);
    }

    // --- CONFIGURAZIONI ENTITÀ ---

    private static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasOne(x => x.Customer)
                .WithMany() 
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict); 

            entity.HasOne(x => x.DeliveryHub)
                .WithMany()
                .HasForeignKey(x => x.DeliveryHubId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- NUOVE REGOLE PER IL CONTATORE SQL SERVER ---
            
            // 1. Diciamo a SQL Server che OrderSequence è autoincrementale, partendo da 1000
            entity.Property(x => x.OrderSequence)
                .UseIdentityColumn(1, 1);

            // 2. Diciamo a SQL Server di concatenare 'ORD-' e il numero di Sequence
            // stored: true salva fisicamente il valore per renderlo ricercabile
            entity.Property(x => x.OrderNumber)
                .HasComputedColumnSql("'ORD-' + CAST([OrderSequence] AS VARCHAR(20))", stored: true);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(x => x.Order)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Plate)
                .WithMany()
                .HasForeignKey(x => x.PlateId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // --- NUOVO METODO DI CONFIGURAZIONE ---
    private static void ConfigurePendingInvoices(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PendingInvoice>(entity =>
        {
            // Associa il pending invoice al suo ordine.
            // Se l'ordine viene cancellato dal sistema (Cascade), puliamo anche la coda.
            entity.HasOne(x => x.Order)
                .WithMany() 
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureDiscounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientCategoryDiscount>(entity =>
        {
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.CategoryDiscounts)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ClientPlateDiscount>(entity =>
        {
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.PlateDiscounts)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Plate)
                .WithMany()
                .HasForeignKey(x => x.PlateId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    private static void ConfigureCustomers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasMany(x => x.DeliveryHubs)
                .WithOne(x => x.Customer)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade); 
        });
    }

    private static void ConfigureMenuItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.ToTable("menu_items");

            entity.HasKey(x => new
            {
                x.MenuId,
                x.PlateId
            });

            entity.HasOne(x => x.Menu)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Plate)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.PlateId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePlateIngredients(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlateIngredient>(entity =>
        {
            entity.ToTable("plate_ingredients");

            entity.HasKey(x => new
            {
                x.PlateId,
                x.IngredientId
            });

            entity.Property(x => x.WeightInGrams)
                .HasColumnType("decimal(10,2)");

            entity.HasOne(x => x.Plate)
                .WithMany(x => x.PlateIngredients)
                .HasForeignKey(x => x.PlateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Ingredient)
                .WithMany(x => x.PlateIngredients)
                .HasForeignKey(x => x.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureIngredientAllergens(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IngredientAllergen>(entity =>
        {
            entity.ToTable("ingredient_allergens");

            entity.HasKey(x => new
            {
                x.IngredientId,
                x.AllergenId
            });

            entity.HasOne(x => x.Ingredient)
                .WithMany(x => x.IngredientAllergens)
                .HasForeignKey(x => x.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Allergen)
                .WithMany(x => x.IngredientAllergens)
                .HasForeignKey(x => x.AllergenId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserService.GetCurrentUserId();
        var now = DateTime.UtcNow;

        var entries = ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUser;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUser;

                    entry.Property(x => x.CreatedAt).IsModified = false;
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}