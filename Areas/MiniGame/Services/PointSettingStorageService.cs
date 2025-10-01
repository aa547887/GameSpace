using Areas.MiniGame.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Areas.MiniGame.Services
{
    /// <summary>
    /// 點數設定儲存邏輯服務實作
    /// </summary>
    public class PointSettingStorageService : IPointSettingStorageService
    {
        private readonly string _connectionString;
        private readonly ILogger<PointSettingStorageService> _logger;

        public PointSettingStorageService(IConfiguration configuration, ILogger<PointSettingStorageService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                "Server=DESKTOP-8HQIS1S\\SQLEXPRESS;Database=GameSpacedatabase;Integrated Security=true;TrustServerCertificate=true;";
            _logger = logger;
        }

        /// <summary>
        /// 儲存點數設定
        /// </summary>
        public async Task<PointSettingStorageResult> SavePointSettingAsync(PointSettingStorageModel model)
        {
            try
            {
                // 先驗證設定
                var validationResult = await ValidatePointSettingAsync(model);
                if (!validationResult.IsValid)
                {
                    return new PointSettingStorageResult
                    {
                        Success = false,
                        Message = $"驗證失敗：{string.Join(", ", validationResult.ValidationErrors)}",
                        AffectedRows = 0
                    };
                }

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var result = model.Operation switch
                {
                    "Create" => await CreatePointSettingAsync(connection, model),
                    "Update" => await UpdatePointSettingAsync(connection, model),
                    "Delete" => await DeletePointSettingAsync(connection, model),
                    "Toggle" => await TogglePointSettingAsync(connection, model),
                    _ => new PointSettingStorageResult { Success = false, Message = "不支援的操作類型" }
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "儲存點數設定時發生錯誤，模型：{@Model}", model);
                return new PointSettingStorageResult
                {
                    Success = false,
                    Message = $"儲存時發生錯誤：{ex.Message}",
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// 批量儲存點數設定
        /// </summary>
        public async Task<PointSettingBatchStorageResult> BatchSavePointSettingsAsync(List<PointSettingStorageModel> models)
        {
            var result = new PointSettingBatchStorageResult
            {
                TotalCount = models.Count
            };

            foreach (var model in models)
            {
                try
                {
                    var saveResult = await SavePointSettingAsync(model);
                    result.Results.Add(saveResult);

                    if (saveResult.Success)
                    {
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.ErrorCount++;
                        result.ErrorMessages.Add($"ID {model.Id}: {saveResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.ErrorMessages.Add($"ID {model.Id}: 處理時發生錯誤 - {ex.Message}");
                    _logger.LogError(ex, "批量儲存點數設定時發生錯誤，模型：{@Model}", model);
                }
            }

            return result;
        }

        /// <summary>
        /// 驗證點數設定
        /// </summary>
        public async Task<PointSettingValidationResult> ValidatePointSettingAsync(PointSettingStorageModel model)
        {
            var result = new PointSettingValidationResult { IsValid = true };

            // 基本驗證
            if (model.PetLevel <= 0)
            {
                result.ValidationErrors.Add("寵物等級必須大於 0");
                result.IsValid = false;
            }

            if (model.RequiredPoints < 0)
            {
                result.ValidationErrors.Add("所需點數不能為負數");
                result.IsValid = false;
            }

            if (string.IsNullOrEmpty(model.SettingType))
            {
                result.ValidationErrors.Add("設定類型不能為空");
                result.IsValid = false;
            }

            if (model.UserId <= 0)
            {
                result.ValidationErrors.Add("使用者ID必須大於 0");
                result.IsValid = false;
            }

            // 檢查寵物等級是否已存在（僅在新增時檢查）
            if (model.Operation == "Create" && result.IsValid)
            {
                try
                {
                    using var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();

                    var tableName = model.SettingType == "SkinColor" ? "PetSkinColorPointSettings" : "PetBackgroundPointSettings";
                    var query = $"SELECT COUNT(*) FROM {tableName} WHERE PetLevel = @PetLevel";
                    
                    using var command = new SqlCommand(query, connection);
                    command.Parameters.Add(new SqlParameter("@PetLevel", model.PetLevel));
                    
                    var count = (int)await command.ExecuteScalarAsync();
                    if (count > 0)
                    {
                        result.ValidationErrors.Add($"寵物等級 {model.PetLevel} 的{GetSettingTypeName(model.SettingType)}設定已存在");
                        result.IsValid = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "驗證寵物等級是否存在時發生錯誤");
                    result.ValidationErrors.Add("驗證時發生錯誤，請稍後再試");
                    result.IsValid = false;
                }
            }

            // 警告檢查
            if (model.RequiredPoints > 10000)
            {
                result.Warnings.Add("所需點數過高，可能影響使用者體驗");
            }

            if (model.PetLevel > 100)
            {
                result.Warnings.Add("寵物等級過高，請確認是否正確");
            }

            return result;
        }

        /// <summary>
        /// 取得點數設定統計
        /// </summary>
        public async Task<PointSettingStatisticsResult> GetPointSettingStatisticsAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var result = new PointSettingStatisticsResult();

                // 換色設定統計
                var skinColorQuery = @"
                    SELECT 
                        COUNT(*) as TotalCount,
                        SUM(CASE WHEN IsEnabled = 1 THEN 1 ELSE 0 END) as ActiveCount,
                        AVG(CAST(RequiredPoints as DECIMAL)) as AveragePoints,
                        MAX(RequiredPoints) as MaxPoints,
                        MIN(RequiredPoints) as MinPoints
                    FROM PetSkinColorPointSettings";

                using var skinColorCommand = new SqlCommand(skinColorQuery, connection);
                using var skinColorReader = await skinColorCommand.ExecuteReaderAsync();
                if (await skinColorReader.ReadAsync())
                {
                    result.TotalSkinColorSettings = skinColorReader.GetInt32("TotalCount");
                    result.ActiveSkinColorSettings = skinColorReader.GetInt32("ActiveCount");
                    result.AverageSkinColorPoints = skinColorReader.IsDBNull("AveragePoints") ? 0 : skinColorReader.GetDecimal("AveragePoints");
                    result.MaxSkinColorPoints = skinColorReader.IsDBNull("MaxPoints") ? 0 : skinColorReader.GetInt32("MaxPoints");
                    result.MinSkinColorPoints = skinColorReader.IsDBNull("MinPoints") ? 0 : skinColorReader.GetInt32("MinPoints");
                }
                await skinColorReader.CloseAsync();

                // 換背景設定統計
                var backgroundQuery = @"
                    SELECT 
                        COUNT(*) as TotalCount,
                        SUM(CASE WHEN IsEnabled = 1 THEN 1 ELSE 0 END) as ActiveCount,
                        AVG(CAST(RequiredPoints as DECIMAL)) as AveragePoints,
                        MAX(RequiredPoints) as MaxPoints,
                        MIN(RequiredPoints) as MinPoints
                    FROM PetBackgroundPointSettings";

                using var backgroundCommand = new SqlCommand(backgroundQuery, connection);
                using var backgroundReader = await backgroundCommand.ExecuteReaderAsync();
                if (await backgroundReader.ReadAsync())
                {
                    result.TotalBackgroundSettings = backgroundReader.GetInt32("TotalCount");
                    result.ActiveBackgroundSettings = backgroundReader.GetInt32("ActiveCount");
                    result.AverageBackgroundPoints = backgroundReader.IsDBNull("AveragePoints") ? 0 : backgroundReader.GetDecimal("AveragePoints");
                    result.MaxBackgroundPoints = backgroundReader.IsDBNull("MaxPoints") ? 0 : backgroundReader.GetInt32("MaxPoints");
                    result.MinBackgroundPoints = backgroundReader.IsDBNull("MinPoints") ? 0 : backgroundReader.GetInt32("MinPoints");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得點數設定統計時發生錯誤");
                throw;
            }
        }

        #region 私有方法

        private async Task<PointSettingStorageResult> CreatePointSettingAsync(SqlConnection connection, PointSettingStorageModel model)
        {
            var tableName = model.SettingType == "SkinColor" ? "PetSkinColorPointSettings" : "PetBackgroundPointSettings";
            var query = $@"
                INSERT INTO {tableName} 
                (PetLevel, RequiredPoints, IsEnabled, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, Remarks)
                VALUES 
                (@PetLevel, @RequiredPoints, @IsEnabled, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt, @Remarks)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@PetLevel", model.PetLevel));
            command.Parameters.Add(new SqlParameter("@RequiredPoints", model.RequiredPoints));
            command.Parameters.Add(new SqlParameter("@IsEnabled", model.IsEnabled));
            command.Parameters.Add(new SqlParameter("@CreatedBy", model.UserId));
            command.Parameters.Add(new SqlParameter("@CreatedAt", DateTime.UtcNow));
            command.Parameters.Add(new SqlParameter("@UpdatedBy", model.UserId));
            command.Parameters.Add(new SqlParameter("@UpdatedAt", DateTime.UtcNow));
            command.Parameters.Add(new SqlParameter("@Remarks", (object?)model.Remarks ?? DBNull.Value));

            var affectedRows = await command.ExecuteNonQueryAsync();
            return new PointSettingStorageResult
            {
                Success = affectedRows > 0,
                Message = affectedRows > 0 ? "新增成功" : "新增失敗",
                AffectedRows = affectedRows
            };
        }

        private async Task<PointSettingStorageResult> UpdatePointSettingAsync(SqlConnection connection, PointSettingStorageModel model)
        {
            var tableName = model.SettingType == "SkinColor" ? "PetSkinColorPointSettings" : "PetBackgroundPointSettings";
            var query = $@"
                UPDATE {tableName} 
                SET PetLevel = @PetLevel, 
                    RequiredPoints = @RequiredPoints, 
                    IsEnabled = @IsEnabled, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt,
                    Remarks = @Remarks
                WHERE Id = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@Id", model.Id));
            command.Parameters.Add(new SqlParameter("@PetLevel", model.PetLevel));
            command.Parameters.Add(new SqlParameter("@RequiredPoints", model.RequiredPoints));
            command.Parameters.Add(new SqlParameter("@IsEnabled", model.IsEnabled));
            command.Parameters.Add(new SqlParameter("@UpdatedBy", model.UserId));
            command.Parameters.Add(new SqlParameter("@UpdatedAt", DateTime.UtcNow));
            command.Parameters.Add(new SqlParameter("@Remarks", (object?)model.Remarks ?? DBNull.Value));

            var affectedRows = await command.ExecuteNonQueryAsync();
            return new PointSettingStorageResult
            {
                Success = affectedRows > 0,
                Message = affectedRows > 0 ? "更新成功" : "更新失敗，找不到指定的設定",
                AffectedRows = affectedRows
            };
        }

        private async Task<PointSettingStorageResult> DeletePointSettingAsync(SqlConnection connection, PointSettingStorageModel model)
        {
            var tableName = model.SettingType == "SkinColor" ? "PetSkinColorPointSettings" : "PetBackgroundPointSettings";
            var query = $"DELETE FROM {tableName} WHERE Id = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@Id", model.Id));

            var affectedRows = await command.ExecuteNonQueryAsync();
            return new PointSettingStorageResult
            {
                Success = affectedRows > 0,
                Message = affectedRows > 0 ? "刪除成功" : "刪除失敗，找不到指定的設定",
                AffectedRows = affectedRows
            };
        }

        private async Task<PointSettingStorageResult> TogglePointSettingAsync(SqlConnection connection, PointSettingStorageModel model)
        {
            var tableName = model.SettingType == "SkinColor" ? "PetSkinColorPointSettings" : "PetBackgroundPointSettings";
            var query = $@"
                UPDATE {tableName} 
                SET IsEnabled = ~IsEnabled, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@Id", model.Id));
            command.Parameters.Add(new SqlParameter("@UpdatedBy", model.UserId));
            command.Parameters.Add(new SqlParameter("@UpdatedAt", DateTime.UtcNow));

            var affectedRows = await command.ExecuteNonQueryAsync();
            return new PointSettingStorageResult
            {
                Success = affectedRows > 0,
                Message = affectedRows > 0 ? "狀態切換成功" : "狀態切換失敗，找不到指定的設定",
                AffectedRows = affectedRows
            };
        }

        private string GetSettingTypeName(string settingType)
        {
            return settingType == "SkinColor" ? "換色" : "換背景";
        }

        #endregion
    }
}
