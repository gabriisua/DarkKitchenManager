using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Roscoff.Core.Entities.Client
{
    // --- 1. THE ROLES DEFINITION ---
    // We place this here (or in a separate file in your Core/Constants folder)
    // to completely eliminate "magic strings" from your application.
    public static class StaffRoles
    {
        public const string Manager = "MANAGER";
        public const string Administrator = "ADMINISTRATOR";
        public const string Operator = "OPERATOR";
        public const string Logistic = "LOGISTIC";

        // AGGIUNGI QUESTA RIGA:
        public static readonly string[] AllRoles = { Manager, Administrator, Operator, Logistic };
    }

    // --- 2. THE ENTITY ---
    [Table("staff")]
    public class Staff : BaseEntity<Guid>
    {
        [Required, StringLength(50)]
        public string Username { get; set; } = null!;
        
        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } = null!;
        
        [Required]
        public string Role { get; set; } = StaffRoles.Logistic;

        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
        
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public DateTime? PasswordResetAt { get; set; }
        
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}