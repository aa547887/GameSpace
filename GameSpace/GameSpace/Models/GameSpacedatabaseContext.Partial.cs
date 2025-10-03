using Microsoft.EntityFrameworkCore;

namespace GameSpace.Models
{
    public partial class GameSpacedatabaseContext
    {
        // Partial class for custom extensions to GameSpacedatabaseContext
        // DbSets are defined in the main GameSpacedatabaseContext.cs file

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // Custom model configurations can be added here if needed
            // Entity configurations are already defined in the main context file
        }
    }
}

