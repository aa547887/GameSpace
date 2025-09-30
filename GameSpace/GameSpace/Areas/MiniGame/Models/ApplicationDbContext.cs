using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Data
{
    public class ApplicationDbContext : GameSpacedatabaseContext
    {
        public ApplicationDbContext(DbContextOptions<GameSpacedatabaseContext> options) : base(options)
        {
        }

        // MiniGame Area 特有的 DbSets (如果需要的話)
        // 大部分模型已經在 GameSpacedatabaseContext 中定義了
        
        // 如果需要添加 MiniGame Area 特有的模型，可以在這裡添加
        // 例如：
        // public DbSet<MiniGameAreaSpecificModel> MiniGameAreaSpecificModels { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // MiniGame Area 特有的模型配置可以在這裡添加
            // 大部分配置已經在 GameSpacedatabaseContext 中處理了
        }
    }
}
