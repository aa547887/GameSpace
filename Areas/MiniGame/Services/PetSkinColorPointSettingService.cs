using Areas.MiniGame.Models;
using System.Data.SqlClient;
using System.Data;

namespace Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換色所需點數設定服務實作
    /// </summary>
    public class PetSkinColorPointSettingService : IPetSkinColorPointSettingService
    {
        private readonly string _connectionString;
        private readonly ILogger<PetSkinColorPointSettingService> _logger;

        public PetSkinColorPointSettingService(IConfiguration configuration, ILogger<PetSkinColorPointSettingService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                "Server=DESKTOP-8HQIS1S\\SQLEXPRESS;Database=GameSpacedatabase;Integrated Security=true;TrustServerCertificate=true;";
            _logger = logger;
        }

        /// <summary>
        /// 取得所有設定
        /// </summary>
        public async Task<PetSkinColorPointSettingListViewModel> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var result = new PetSkinColorPointSettingListViewModel
            {
                CurrentPage = page,
                PageSize = pageSize
            };

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // 取得總筆數
                var countQuery = @"
                    SELECT COUNT(*) 
                    FROM PetSkinColorPointSettings 
                    WHERE IsEnabled = 1";

                using var countCommand = new SqlCommand(countQuery, connection);
                result.TotalCount = (int)await countCommand.ExecuteScalarAsync();

                // 取得分頁資料
                var query = @"
                    SELECT p.Id, p.PetLevel, p.RequiredPoints, p.IsEnabled, 
                           p.CreatedAt, p.UpdatedAt,
                           c.Manager_Name as CreatedByName, u.Manager_Name as UpdatedByName
                    FROM PetSkinColorPointSettings p
                    LEFT JOIN ManagerData c ON p.CreatedBy = c.Manager_Id
                    LEFT JOIN ManagerData u ON p.UpdatedBy = u.Manager_Id
                    WHERE p.IsEnabled = 1
                    ORDER BY p.PetLevel
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                command.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Settings.Add(new PetSkinColorPointSettingViewModel
                    {
                        Id = reader.GetInt32("Id"),
                        PetLevel = reader.GetInt32("PetLevel"),
                        RequiredPoints = reader.GetInt32("RequiredPoints"),
                        IsEnabled = reader.GetBoolean("IsEnabled"),
                        CreatedAt = reader.GetDateTime("CreatedAt"),
                        UpdatedAt = reader.GetDateTime("UpdatedAt"),
                        CreatedByName = reader.IsDBNull("CreatedByName") ? "" : reader.GetString("CreatedByName"),
                        UpdatedByName = reader.IsDBNull("UpdatedByName") ? "" : reader.GetString("UpdatedByName")
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物換色所需點數設定列表時發生錯誤");
                throw;
            }

            return result;
        }

        /// <summary>
        /// 根據ID取得設定
        /// </summary>
        public async Task<PetSkinColorPointSettingViewModel?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT p.Id, p.PetLevel, p.RequiredPoints, p.IsEnabled, 
                           p.CreatedAt, p.UpdatedAt,
                           c.Manager_Name as CreatedByName, u.Manager_Name as UpdatedByName
                    FROM PetSkinColorPointSettings p
                    LEFT JOIN ManagerData c ON p.CreatedBy = c.Manager_Id
                    LEFT JOIN ManagerData u ON p.UpdatedBy = u.Manager_Id
                    WHERE p.Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new PetSkinColorPointSettingViewModel
                    {
                        Id = reader.GetInt32("Id"),
                        PetLevel = reader.GetInt32("PetLevel"),
                        RequiredPoints = reader.GetInt32("RequiredPoints"),
                        IsEnabled = reader.GetBoolean("IsEnabled"),
                        CreatedAt = reader.GetDateTime("CreatedAt"),
                        UpdatedAt = reader.GetDateTime("UpdatedAt"),
                        CreatedByName = reader.IsDBNull("CreatedByName") ? "" : reader.GetString("CreatedByName"),
                        UpdatedByName = reader.IsDBNull("UpdatedByName") ? "" : reader.GetString("UpdatedByName")
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物換色所需點數設定時發生錯誤，ID: {Id}", id);
                throw;
            }

            return null;
        }

        /// <summary>
        /// 根據寵物等級取得設定
        /// </summary>
        public async Task<PetSkinColorPointSettingViewModel?> GetByPetLevelAsync(int petLevel)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT p.Id, p.PetLevel, p.RequiredPoints, p.IsEnabled, 
                           p.CreatedAt, p.UpdatedAt,
                           c.Manager_Name as CreatedByName, u.Manager_Name as UpdatedByName
                    FROM PetSkinColorPointSettings p
                    LEFT JOIN ManagerData c ON p.CreatedBy = c.Manager_Id
                    LEFT JOIN ManagerData u ON p.UpdatedBy = u.Manager_Id
                    WHERE p.PetLevel = @PetLevel AND p.IsEnabled = 1";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PetLevel", petLevel);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new PetSkinColorPointSettingViewModel
                    {
                        Id = reader.GetInt32("Id"),
                        PetLevel = reader.GetInt32("PetLevel"),
                        RequiredPoints = reader.GetInt32("RequiredPoints"),
                        IsEnabled = reader.GetBoolean("IsEnabled"),
                        CreatedAt = reader.GetDateTime("CreatedAt"),
                        UpdatedAt = reader.GetDateTime("UpdatedAt"),
                        CreatedByName = reader.IsDBNull("CreatedByName") ? "" : reader.GetString("CreatedByName"),
                        UpdatedByName = reader.IsDBNull("UpdatedByName") ? "" : reader.GetString("UpdatedByName")
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據寵物等級取得寵物換色所需點數設定時發生錯誤，等級: {PetLevel}", petLevel);
                throw;
            }

            return null;
        }

        /// <summary>
        /// 新增設定
        /// </summary>
        public async Task<bool> CreateAsync(PetSkinColorPointSettingViewModel model, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO PetSkinColorPointSettings 
                    (PetLevel, RequiredPoints, IsEnabled, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
                    VALUES (@PetLevel, @RequiredPoints, @IsEnabled, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy)";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PetLevel", model.PetLevel);
                command.Parameters.AddWithValue("@RequiredPoints", model.RequiredPoints);
                command.Parameters.AddWithValue("@IsEnabled", model.IsEnabled);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                command.Parameters.AddWithValue("@CreatedBy", userId);
                command.Parameters.AddWithValue("@UpdatedBy", userId);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增寵物換色所需點數設定時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 更新設定
        /// </summary>
        public async Task<bool> UpdateAsync(PetSkinColorPointSettingViewModel model, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    UPDATE PetSkinColorPointSettings 
                    SET PetLevel = @PetLevel, RequiredPoints = @RequiredPoints, 
                        IsEnabled = @IsEnabled, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy
                    WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", model.Id);
                command.Parameters.AddWithValue("@PetLevel", model.PetLevel);
                command.Parameters.AddWithValue("@RequiredPoints", model.RequiredPoints);
                command.Parameters.AddWithValue("@IsEnabled", model.IsEnabled);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                command.Parameters.AddWithValue("@UpdatedBy", userId);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物換色所需點數設定時發生錯誤，ID: {Id}", model.Id);
                throw;
            }
        }

        /// <summary>
        /// 刪除設定
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "UPDATE PetSkinColorPointSettings SET IsEnabled = 0 WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除寵物換色所需點數設定時發生錯誤，ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        public async Task<bool> ToggleEnabledAsync(int id, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    UPDATE PetSkinColorPointSettings 
                    SET IsEnabled = ~IsEnabled, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy
                    WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                command.Parameters.AddWithValue("@UpdatedBy", userId);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換寵物換色所需點數設定啟用狀態時發生錯誤，ID: {Id}", id);
                throw;
            }
        }
    }
}
