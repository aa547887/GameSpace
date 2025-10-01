using Areas.MiniGame.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換背景所需點數設定服務實作
    /// </summary>
    public class PetBackgroundPointSettingService : IPetBackgroundPointSettingService
    {
        private readonly string _connectionString;

        public PetBackgroundPointSettingService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// 取得所有設定
        /// </summary>
        public async Task<PetBackgroundPointSettingListViewModel> GetSettingsAsync(int page = 1, int pageSize = 10, string? searchKeyword = null, bool? isEnabledFilter = null)
        {
            var result = new PetBackgroundPointSettingListViewModel
            {
                CurrentPage = page,
                PageSize = pageSize,
                SearchKeyword = searchKeyword,
                IsEnabledFilter = isEnabledFilter
            };

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // 建立查詢條件
            var whereConditions = new List<string>();
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                whereConditions.Add("(PetLevel LIKE @searchKeyword OR Remarks LIKE @searchKeyword)");
                parameters.Add(new SqlParameter("@searchKeyword", $"%{searchKeyword}%"));
            }

            if (isEnabledFilter.HasValue)
            {
                whereConditions.Add("IsEnabled = @isEnabled");
                parameters.Add(new SqlParameter("@isEnabled", isEnabledFilter.Value));
            }

            var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

            // 取得總筆數
            var countQuery = $@"SELECT COUNT(*) FROM PetBackgroundPointSettings {whereClause}";
            using var countCommand = new SqlCommand(countQuery, connection);
            countCommand.Parameters.AddRange(parameters.ToArray());
            result.TotalCount = (int)await countCommand.ExecuteScalarAsync();

            // 取得分頁資料
            var offset = (page - 1) * pageSize;
            var dataQuery = $@"
                SELECT Id, PetLevel, RequiredPoints, IsEnabled, Remarks, CreatedAt, UpdatedAt
                FROM PetBackgroundPointSettings 
                {whereClause}
                ORDER BY PetLevel
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            using var dataCommand = new SqlCommand(dataQuery, connection);
            dataCommand.Parameters.AddRange(parameters.ToArray());
            dataCommand.Parameters.Add(new SqlParameter("@offset", offset));
            dataCommand.Parameters.Add(new SqlParameter("@pageSize", pageSize));

            using var reader = await dataCommand.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Settings.Add(new PetBackgroundPointSettingViewModel
                {
                    Id = reader.GetInt32("Id"),
                    PetLevel = reader.GetInt32("PetLevel"),
                    RequiredPoints = reader.GetInt32("RequiredPoints"),
                    IsEnabled = reader.GetBoolean("IsEnabled"),
                    Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks")
                });
            }

            return result;
        }

        /// <summary>
        /// 根據ID取得設定
        /// </summary>
        public async Task<PetBackgroundPointSettingViewModel?> GetSettingByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT Id, PetLevel, RequiredPoints, IsEnabled, Remarks
                FROM PetBackgroundPointSettings 
                WHERE Id = @id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@id", id));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new PetBackgroundPointSettingViewModel
                {
                    Id = reader.GetInt32("Id"),
                    PetLevel = reader.GetInt32("PetLevel"),
                    RequiredPoints = reader.GetInt32("RequiredPoints"),
                    IsEnabled = reader.GetBoolean("IsEnabled"),
                    Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks")
                };
            }

            return null;
        }

        /// <summary>
        /// 根據寵物等級取得設定
        /// </summary>
        public async Task<PetBackgroundPointSettingViewModel?> GetSettingByPetLevelAsync(int petLevel)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT Id, PetLevel, RequiredPoints, IsEnabled, Remarks
                FROM PetBackgroundPointSettings 
                WHERE PetLevel = @petLevel AND IsEnabled = 1";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@petLevel", petLevel));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new PetBackgroundPointSettingViewModel
                {
                    Id = reader.GetInt32("Id"),
                    PetLevel = reader.GetInt32("PetLevel"),
                    RequiredPoints = reader.GetInt32("RequiredPoints"),
                    IsEnabled = reader.GetBoolean("IsEnabled"),
                    Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks")
                };
            }

            return null;
        }

        /// <summary>
        /// 新增設定
        /// </summary>
        public async Task<bool> CreateSettingAsync(PetBackgroundPointSettingViewModel model, int createdBy)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                INSERT INTO PetBackgroundPointSettings 
                (PetLevel, RequiredPoints, IsEnabled, CreatedBy, CreatedAt, Remarks)
                VALUES 
                (@petLevel, @requiredPoints, @isEnabled, @createdBy, @createdAt, @remarks)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@petLevel", model.PetLevel));
            command.Parameters.Add(new SqlParameter("@requiredPoints", model.RequiredPoints));
            command.Parameters.Add(new SqlParameter("@isEnabled", model.IsEnabled));
            command.Parameters.Add(new SqlParameter("@createdBy", createdBy));
            command.Parameters.Add(new SqlParameter("@createdAt", DateTime.UtcNow));
            command.Parameters.Add(new SqlParameter("@remarks", (object?)model.Remarks ?? DBNull.Value));

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        /// <summary>
        /// 更新設定
        /// </summary>
        public async Task<bool> UpdateSettingAsync(PetBackgroundPointSettingViewModel model, int updatedBy)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                UPDATE PetBackgroundPointSettings 
                SET PetLevel = @petLevel, 
                    RequiredPoints = @requiredPoints, 
                    IsEnabled = @isEnabled, 
                    UpdatedBy = @updatedBy, 
                    UpdatedAt = @updatedAt,
                    Remarks = @remarks
                WHERE Id = @id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@id", model.Id));
            command.Parameters.Add(new SqlParameter("@petLevel", model.PetLevel));
            command.Parameters.Add(new SqlParameter("@requiredPoints", model.RequiredPoints));
            command.Parameters.Add(new SqlParameter("@isEnabled", model.IsEnabled));
            command.Parameters.Add(new SqlParameter("@updatedBy", updatedBy));
            command.Parameters.Add(new SqlParameter("@updatedAt", DateTime.UtcNow));
            command.Parameters.Add(new SqlParameter("@remarks", (object?)model.Remarks ?? DBNull.Value));

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        /// <summary>
        /// 刪除設定
        /// </summary>
        public async Task<bool> DeleteSettingAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM PetBackgroundPointSettings WHERE Id = @id";
            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@id", id));

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        public async Task<bool> ToggleEnabledAsync(int id, int updatedBy)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                UPDATE PetBackgroundPointSettings 
                SET IsEnabled = ~IsEnabled, 
                    UpdatedBy = @updatedBy, 
                    UpdatedAt = @updatedAt
                WHERE Id = @id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("@id", id));
            command.Parameters.Add(new SqlParameter("@updatedBy", updatedBy));
            command.Parameters.Add(new SqlParameter("@updatedAt", DateTime.UtcNow));

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        /// <summary>
        /// 檢查寵物等級是否已存在設定
        /// </summary>
        public async Task<bool> IsPetLevelExistsAsync(int petLevel, int? excludeId = null)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT COUNT(*) FROM PetBackgroundPointSettings WHERE PetLevel = @petLevel";
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@petLevel", petLevel)
            };

            if (excludeId.HasValue)
            {
                query += " AND Id != @excludeId";
                parameters.Add(new SqlParameter("@excludeId", excludeId.Value));
            }

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddRange(parameters.ToArray());

            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }
    }
}
