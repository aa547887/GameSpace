using Microsoft.VisualStudio.TestTools.UnitTesting;
using Areas.MiniGame.Services;
using Areas.MiniGame.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Areas.MiniGame.Tests.Services
{
    [TestClass]
    public class PetSkinColorPointSettingServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ILogger<PetSkinColorPointSettingService>> _mockLogger;
        private PetSkinColorPointSettingService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<PetSkinColorPointSettingService>>();
            
            // 設定連線字串
            _mockConfiguration.Setup(x => x.GetConnectionString("GameSpace"))
                .Returns("Server=DESKTOP-8HQIS1S\\SQLEXPRESS;Database=GameSpacedatabase;Trusted_Connection=true;");
            
            _service = new PetSkinColorPointSettingService(_mockConfiguration.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetAllAsync_ShouldReturnSettings()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;

            // Act
            var result = await _service.GetAllAsync(page, pageSize);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Settings);
        }

        [TestMethod]
        public async Task GetByIdAsync_WithValidId_ShouldReturnSetting()
        {
            // Arrange
            int id = 1;

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CreateAsync_WithValidModel_ShouldReturnTrue()
        {
            // Arrange
            var model = new PetSkinColorPointSettingCreateViewModel
            {
                PetLevel = 1,
                RequiredPoints = 100,
                IsEnabled = true
            };

            // Act
            var result = await _service.CreateAsync(model);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task UpdateAsync_WithValidModel_ShouldReturnTrue()
        {
            // Arrange
            var model = new PetSkinColorPointSettingEditViewModel
            {
                Id = 1,
                PetLevel = 1,
                RequiredPoints = 150,
                IsEnabled = true
            };

            // Act
            var result = await _service.UpdateAsync(model);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DeleteAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            int id = 1;

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
