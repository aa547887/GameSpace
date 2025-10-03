using MiniGameEntity = GameSpace.Models.MiniGame;
using Microsoft.Data.SqlClient;

namespace GameSpace.Areas.MiniGame.Services
{
    public class MiniGameService : IMiniGameService
    {
        private readonly string _connectionString;

        public MiniGameService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IEnumerable<MiniGameEntity>> GetAllMiniGamesAsync()
        {
            var games = new List<MiniGameEntity>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT * FROM MiniGame ORDER BY StartTime DESC", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                games.Add(new MiniGameEntity
                {
                    GameID = reader.GetInt32(reader.GetOrdinal("GameID")),
                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                    PetID = reader.GetInt32(reader.GetOrdinal("PetID")),
                    GameType = reader.GetString(reader.GetOrdinal("GameType")),
                    StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                    GameResult = reader.IsDBNull(reader.GetOrdinal("GameResult")) ? null : reader.GetString(reader.GetOrdinal("GameResult")),
                    PointsEarned = reader.GetInt32(reader.GetOrdinal("PointsEarned")),
                    PetExpEarned = reader.GetInt32(reader.GetOrdinal("PetExpEarned")),
                    CouponEarned = reader.IsDBNull(reader.GetOrdinal("CouponEarned")) ? null : reader.GetInt32(reader.GetOrdinal("CouponEarned")),
                    SessionID = reader.GetString(reader.GetOrdinal("SessionID"))
                });
            }
            return games;
        }

        public async Task<IEnumerable<MiniGameEntity>> GetMiniGamesByUserIdAsync(int userId)
        {
            var games = new List<MiniGameEntity>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT * FROM MiniGame WHERE UserID = @UserId ORDER BY StartTime DESC", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                games.Add(new MiniGameEntity
                {
                    GameID = reader.GetInt32(reader.GetOrdinal("GameID")),
                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                    PetID = reader.GetInt32(reader.GetOrdinal("PetID")),
                    GameType = reader.GetString(reader.GetOrdinal("GameType")),
                    StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                    GameResult = reader.IsDBNull(reader.GetOrdinal("GameResult")) ? null : reader.GetString(reader.GetOrdinal("GameResult")),
                    PointsEarned = reader.GetInt32(reader.GetOrdinal("PointsEarned")),
                    PetExpEarned = reader.GetInt32(reader.GetOrdinal("PetExpEarned")),
                    CouponEarned = reader.IsDBNull(reader.GetOrdinal("CouponEarned")) ? null : reader.GetInt32(reader.GetOrdinal("CouponEarned")),
                    SessionID = reader.GetString(reader.GetOrdinal("SessionID"))
                });
            }
            return games;
        }

        public async Task<IEnumerable<MiniGameEntity>> GetMiniGamesByPetIdAsync(int petId)
        {
            var games = new List<MiniGameEntity>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT * FROM MiniGame WHERE PetID = @PetId ORDER BY StartTime DESC", connection);
            command.Parameters.AddWithValue("@PetId", petId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                games.Add(new MiniGameEntity
                {
                    GameID = reader.GetInt32(reader.GetOrdinal("GameID")),
                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                    PetID = reader.GetInt32(reader.GetOrdinal("PetID")),
                    GameType = reader.GetString(reader.GetOrdinal("GameType")),
                    StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                    GameResult = reader.IsDBNull(reader.GetOrdinal("GameResult")) ? null : reader.GetString(reader.GetOrdinal("GameResult")),
                    PointsEarned = reader.GetInt32(reader.GetOrdinal("PointsEarned")),
                    PetExpEarned = reader.GetInt32(reader.GetOrdinal("PetExpEarned")),
                    CouponEarned = reader.IsDBNull(reader.GetOrdinal("CouponEarned")) ? null : reader.GetInt32(reader.GetOrdinal("CouponEarned")),
                    SessionID = reader.GetString(reader.GetOrdinal("SessionID"))
                });
            }
            return games;
        }

        public async Task<MiniGameEntity?> GetMiniGameByIdAsync(int gameId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT * FROM MiniGame WHERE GameID = @GameId", connection);
            command.Parameters.AddWithValue("@GameId", gameId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new MiniGameEntity
                {
                    GameID = reader.GetInt32(reader.GetOrdinal("GameID")),
                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                    PetID = reader.GetInt32(reader.GetOrdinal("PetID")),
                    GameType = reader.GetString(reader.GetOrdinal("GameType")),
                    StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                    GameResult = reader.IsDBNull(reader.GetOrdinal("GameResult")) ? null : reader.GetString(reader.GetOrdinal("GameResult")),
                    PointsEarned = reader.GetInt32(reader.GetOrdinal("PointsEarned")),
                    PetExpEarned = reader.GetInt32(reader.GetOrdinal("PetExpEarned")),
                    CouponEarned = reader.IsDBNull(reader.GetOrdinal("CouponEarned")) ? null : reader.GetInt32(reader.GetOrdinal("CouponEarned")),
                    SessionID = reader.GetString(reader.GetOrdinal("SessionID"))
                };
            }
            return null;
        }

        public async Task<bool> CreateMiniGameAsync(MiniGameEntity miniGame)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                INSERT INTO MiniGame (UserID, PetID, GameType, StartTime, EndTime, GameResult, PointsEarned, PetExpEarned, CouponEarned, SessionID)
                VALUES (@UserID, @PetID, @GameType, @StartTime, @EndTime, @GameResult, @PointsEarned, @PetExpEarned, @CouponEarned, @SessionID)", connection);

            command.Parameters.AddWithValue("@UserID", miniGame.UserID);
            command.Parameters.AddWithValue("@PetID", miniGame.PetID);
            command.Parameters.AddWithValue("@GameType", miniGame.GameType);
            command.Parameters.AddWithValue("@StartTime", miniGame.StartTime);
            command.Parameters.AddWithValue("@EndTime", (object?)miniGame.EndTime ?? DBNull.Value);
            command.Parameters.AddWithValue("@GameResult", (object?)miniGame.GameResult ?? DBNull.Value);
            command.Parameters.AddWithValue("@PointsEarned", miniGame.PointsEarned);
            command.Parameters.AddWithValue("@PetExpEarned", miniGame.PetExpEarned);
            command.Parameters.AddWithValue("@CouponEarned", (object?)miniGame.CouponEarned ?? DBNull.Value);
            command.Parameters.AddWithValue("@SessionID", miniGame.SessionID);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> UpdateMiniGameAsync(MiniGameEntity miniGame)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                UPDATE MiniGame SET 
                    UserID = @UserID,
                    PetID = @PetID,
                    GameType = @GameType,
                    StartTime = @StartTime,
                    EndTime = @EndTime,
                    GameResult = @GameResult,
                    PointsEarned = @PointsEarned,
                    PetExpEarned = @PetExpEarned,
                    CouponEarned = @CouponEarned,
                    SessionID = @SessionID
                WHERE GameID = @GameID", connection);

            command.Parameters.AddWithValue("@GameID", miniGame.GameID);
            command.Parameters.AddWithValue("@UserID", miniGame.UserID);
            command.Parameters.AddWithValue("@PetID", miniGame.PetID);
            command.Parameters.AddWithValue("@GameType", miniGame.GameType);
            command.Parameters.AddWithValue("@StartTime", miniGame.StartTime);
            command.Parameters.AddWithValue("@EndTime", (object?)miniGame.EndTime ?? DBNull.Value);
            command.Parameters.AddWithValue("@GameResult", (object?)miniGame.GameResult ?? DBNull.Value);
            command.Parameters.AddWithValue("@PointsEarned", miniGame.PointsEarned);
            command.Parameters.AddWithValue("@PetExpEarned", miniGame.PetExpEarned);
            command.Parameters.AddWithValue("@CouponEarned", (object?)miniGame.CouponEarned ?? DBNull.Value);
            command.Parameters.AddWithValue("@SessionID", miniGame.SessionID);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> DeleteMiniGameAsync(int gameId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("DELETE FROM MiniGame WHERE GameID = @GameId", connection);
            command.Parameters.AddWithValue("@GameId", gameId);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> StartGameAsync(int userId, int petId, string gameType)
        {
            var sessionId = Guid.NewGuid().ToString();
            var miniGame = new MiniGameEntity
            {
                UserID = userId,
                PetID = petId,
                GameType = gameType,
                StartTime = DateTime.Now,
                SessionID = sessionId
            };

            return await CreateMiniGameAsync(miniGame);
        }

        public async Task<bool> EndGameAsync(int gameId, string gameResult, int pointsEarned, int petExpEarned, int? couponEarned = null)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                UPDATE MiniGame SET 
                    EndTime = GETDATE(),
                    GameResult = @GameResult,
                    PointsEarned = @PointsEarned,
                    PetExpEarned = @PetExpEarned,
                    CouponEarned = @CouponEarned
                WHERE GameID = @GameID", connection);

            command.Parameters.AddWithValue("@GameID", gameId);
            command.Parameters.AddWithValue("@GameResult", gameResult);
            command.Parameters.AddWithValue("@PointsEarned", pointsEarned);
            command.Parameters.AddWithValue("@PetExpEarned", petExpEarned);
            command.Parameters.AddWithValue("@CouponEarned", (object?)couponEarned ?? DBNull.Value);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> AbortGameAsync(int gameId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                UPDATE MiniGame SET 
                    EndTime = GETDATE(),
                    GameResult = 'Abort'
                WHERE GameID = @GameID", connection);

            command.Parameters.AddWithValue("@GameID", gameId);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<IEnumerable<MiniGameEntity>> GetGamesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var games = new List<MiniGameEntity>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT * FROM MiniGame 
                WHERE StartTime >= @StartDate AND StartTime <= @EndDate
                ORDER BY StartTime DESC", connection);
            command.Parameters.AddWithValue("@StartDate", startDate);
            command.Parameters.AddWithValue("@EndDate", endDate);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                games.Add(new MiniGameEntity
                {
                    GameID = reader.GetInt32(reader.GetOrdinal("GameID")),
                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                    PetID = reader.GetInt32(reader.GetOrdinal("PetID")),
                    GameType = reader.GetString(reader.GetOrdinal("GameType")),
                    StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                    GameResult = reader.IsDBNull(reader.GetOrdinal("GameResult")) ? null : reader.GetString(reader.GetOrdinal("GameResult")),
                    PointsEarned = reader.GetInt32(reader.GetOrdinal("PointsEarned")),
                    PetExpEarned = reader.GetInt32(reader.GetOrdinal("PetExpEarned")),
                    CouponEarned = reader.IsDBNull(reader.GetOrdinal("CouponEarned")) ? null : reader.GetInt32(reader.GetOrdinal("CouponEarned")),
                    SessionID = reader.GetString(reader.GetOrdinal("SessionID"))
                });
            }
            return games;
        }

        public async Task<Dictionary<string, int>> GetGameStatisticsAsync()
        {
            var statistics = new Dictionary<string, int>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT 
                    COUNT(*) as TotalGames,
                    SUM(CASE WHEN GameResult = 'Win' THEN 1 ELSE 0 END) as WinCount,
                    SUM(CASE WHEN GameResult = 'Lose' THEN 1 ELSE 0 END) as LoseCount,
                    SUM(CASE WHEN GameResult = 'Abort' THEN 1 ELSE 0 END) as AbortCount,
                    SUM(PointsEarned) as TotalPointsEarned,
                    SUM(PetExpEarned) as TotalExpEarned
                FROM MiniGame", connection);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                statistics["TotalGames"] = reader.GetInt32(reader.GetOrdinal("TotalGames"));
                statistics["WinCount"] = reader.GetInt32(reader.GetOrdinal("WinCount"));
                statistics["LoseCount"] = reader.GetInt32(reader.GetOrdinal("LoseCount"));
                statistics["AbortCount"] = reader.GetInt32(reader.GetOrdinal("AbortCount"));
                statistics["TotalPointsEarned"] = reader.GetInt32(reader.GetOrdinal("TotalPointsEarned"));
                statistics["TotalExpEarned"] = reader.GetInt32(reader.GetOrdinal("TotalExpEarned"));
            }
            return statistics;
        }

        public async Task<int> GetTodayGameCountByUserIdAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT COUNT(*) FROM MiniGame 
                WHERE UserID = @UserId 
                AND CAST(StartTime AS DATE) = CAST(GETDATE() AS DATE)", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            var result = await command.ExecuteScalarAsync();
            return result != null ? (int)result : 0;
        }

        public async Task<bool> CanUserPlayTodayAsync(int userId)
        {
            var todayGameCount = await GetTodayGameCountByUserIdAsync(userId);
            // Assuming daily limit is 3 games per day (this could be configurable)
            return todayGameCount < 3;
        }
    }
}

