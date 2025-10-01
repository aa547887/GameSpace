using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;

namespace GameSpace.Areas.MiniGame.Tests
{
    public class PetLevelUpRuleValidationTests
    {
        private readonly MiniGameDbContext _context;
        private readonly PetLevelUpRuleValidationService _validationService;

        public PetLevelUpRuleValidationTests()
        {
            var options = new DbContextOptionsBuilder<MiniGameDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            
            _context = new MiniGameDbContext(options);
            
            var logger = new LoggerFactory().CreateLogger<PetLevelUpRuleValidationService>();
            _validationService = new PetLevelUpRuleValidationService(_context, logger);
        }

        [Fact]
        public async Task ValidateLevelUpRule_ValidData_ShouldReturnValid()
        {
            // Arrange
            var model = new PetLevelUpRuleCreateViewModel
            {
                Level = 1,
                ExperienceRequired = 100,
                PointsReward = 10,
                ExpReward = 5,
                IsActive = true,
                Remarks = "測試規則"
            };

            // Act
            var result = await _validationService.ValidateLevelUpRuleAsync(model);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateLevelUpRule_InvalidLevel_ShouldReturnInvalid()
        {
            // Arrange
            var model = new PetLevelUpRuleCreateViewModel
            {
                Level = 0, // 無效等級
                ExperienceRequired = 100,
                PointsReward = 10,
                ExpReward = 5,
                IsActive = true,
                Remarks = "測試規則"
            };

            // Act
            var result = await _validationService.ValidateLevelUpRuleAsync(model);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("等級必須大於 0", result.Errors);
        }

        [Fact]
        public async Task ValidateLevelUpRule_NegativeExperience_ShouldReturnInvalid()
        {
            // Arrange
            var model = new PetLevelUpRuleCreateViewModel
            {
                Level = 1,
                ExperienceRequired = -10, // 負數經驗值
                PointsReward = 10,
                ExpReward = 5,
                IsActive = true,
                Remarks = "測試規則"
            };

            // Act
            var result = await _validationService.ValidateLevelUpRuleAsync(model);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("所需經驗值不能為負數", result.Errors);
        }

        [Fact]
        public async Task ValidateLevelUpRule_NegativeRewards_ShouldReturnInvalid()
        {
            // Arrange
            var model = new PetLevelUpRuleCreateViewModel
            {
                Level = 1,
                ExperienceRequired = 100,
                PointsReward = -10, // 負數獎勵
                ExpReward = -5, // 負數獎勵
                IsActive = true,
                Remarks = "測試規則"
            };

            // Act
            var result = await _validationService.ValidateLevelUpRuleAsync(model);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("點數獎勵不能為負數", result.Errors);
            Assert.Contains("經驗獎勵不能為負數", result.Errors);
        }

        [Fact]
        public async Task ValidateLevelUpRuleLogic_ValidData_ShouldReturnValid()
        {
            // Arrange
            var level = 5;
            var experienceRequired = 500;
            var pointsReward = 50;
            var expReward = 25;

            // Act
            var result = await _validationService.ValidateLevelUpRuleLogicAsync(level, experienceRequired, pointsReward, expReward);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateLevelUpRuleLogic_LevelOutOfRange_ShouldReturnInvalid()
        {
            // Arrange
            var level = 101; // 超出範圍
            var experienceRequired = 500;
            var pointsReward = 50;
            var expReward = 25;

            // Act
            var result = await _validationService.ValidateLevelUpRuleLogicAsync(level, experienceRequired, pointsReward, expReward);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("等級必須在 1-100 之間", result.Errors);
        }

        [Fact]
        public async Task ValidateLevelUpRuleConsistency_EmptyRules_ShouldReturnWarning()
        {
            // Act
            var result = await _validationService.ValidateLevelUpRuleConsistencyAsync();

            // Assert
            Assert.True(result.IsValid);
            Assert.Contains("沒有啟用的升級規則", result.Warnings);
        }
    }
}
