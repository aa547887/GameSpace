using GameSpace.Areas.MiniGame.Models;
using Microsoft.Data.SqlClient;

namespace GameSpace.Areas.MiniGame.Services
{
    public class SignInStatsService : ISignInStatsService
    {
        private readonly string _connectionString;

        public SignInStatsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IEnumerable<UserSignInStats>> GetAllSignInStatsAsync()
        {
            var stats = new List<UserSignInStats>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT * FROM UserSignInStats ORDER BY SignTime DESC", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                stats.Add(new UserSignInStats
                {
                    StatsID = reader.GetInt32("StatsID"),
                    UserID = reader.GetInt32("UserID"),
                    SignTime = reader.GetDateTime("SignTime"),
                    PointsEarned = reader.GetInt32("PointsEarned"),
                    PetExpEarned = reader.GetInt32("PetExpEarned"),
                    CouponEarned = reader.IsDBNull("CouponEarned") ? null : reader.GetInt32("CouponEarned"),
                    ConsecutiveDays = reader.GetInt32("ConsecutiveDays")
                });
            }
            return stats;
        }

        public async Task<IEnumerable<UserSignInStats>> GetSignInStatsByUserIdAsync(int userId)
        {
            var stats = new List<UserSignInStats>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT * FROM UserSignInStats 
                WHERE UserID = @UserId 
                ORDER BY SignTime DESC", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                stats.Add(new UserSignInStats
                {
                    StatsID = reader.GetInt32("StatsID"),
                    UserID = reader.GetInt32("UserID"),
                    SignTime = reader.GetDateTime("SignTime"),
                    PointsEarned = reader.GetInt32("PointsEarned"),
                    PetExpEarned = reader.GetInt32("PetExpEarned"),
                    CouponEarned = reader.IsDBNull("CouponEarned") ? null : reader.GetInt32("CouponEarned"),
                    ConsecutiveDays = reader.GetInt32("ConsecutiveDays")
                });
            }
            return stats;
        }

        public async Task<UserSignInStats?> GetSignInStatsByIdAsync(int statsId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT * FROM UserSignInStats WHERE StatsID = @StatsId", connection);
            command.Parameters.AddWithValue("@StatsId", statsId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserSignInStats
                {
                    StatsID = reader.GetInt32("StatsID"),
                    UserID = reader.GetInt32("UserID"),
                    SignTime = reader.GetDateTime("SignTime"),
                    PointsEarned = reader.GetInt32("PointsEarned"),
                    PetExpEarned = reader.GetInt32("PetExpEarned"),
                    CouponEarned = reader.IsDBNull("CouponEarned") ? null : reader.GetInt32("CouponEarned"),
                    ConsecutiveDays = reader.GetInt32("ConsecutiveDays")
                };
            }
            return null;
        }

        public async Task<bool> CreateSignInStatsAsync(UserSignInStats stats)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                INSERT INTO UserSignInStats (UserID, SignTime, PointsEarned, PetExpEarned, CouponEarned, ConsecutiveDays)
                VALUES (@UserID, @SignTime, @PointsEarned, @PetExpEarned, @CouponEarned, @ConsecutiveDays)", connection);

            command.Parameters.AddWithValue("@UserID", stats.UserID);
            command.Parameters.AddWithValue("@SignTime", stats.SignTime);
            command.Parameters.AddWithValue("@PointsEarned", stats.PointsEarned);
            command.Parameters.AddWithValue("@PetExpEarned", stats.PetExpEarned);
            command.Parameters.AddWithValue("@CouponEarned", (object?)stats.CouponEarned ?? DBNull.Value);
            command.Parameters.AddWithValue("@ConsecutiveDays", stats.ConsecutiveDays);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> UpdateSignInStatsAsync(UserSignInStats stats)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                UPDATE UserSignInStats SET 
                    UserID = @UserID,
                    SignTime = @SignTime,
                    PointsEarned = @PointsEarned,
                    PetExpEarned = @PetExpEarned,
                    CouponEarned = @CouponEarned,
                    ConsecutiveDays = @ConsecutiveDays
                WHERE StatsID = @StatsID", connection);

            command.Parameters.AddWithValue("@StatsID", stats.StatsID);
            command.Parameters.AddWithValue("@UserID", stats.UserID);
            command.Parameters.AddWithValue("@SignTime", stats.SignTime);
            command.Parameters.AddWithValue("@PointsEarned", stats.PointsEarned);
            command.Parameters.AddWithValue("@PetExpEarned", stats.PetExpEarned);
            command.Parameters.AddWithValue("@CouponEarned", (object?)stats.CouponEarned ?? DBNull.Value);
            command.Parameters.AddWithValue("@ConsecutiveDays", stats.ConsecutiveDays);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> DeleteSignInStatsAsync(int statsId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("DELETE FROM UserSignInStats WHERE StatsID = @StatsId", connection);
            command.Parameters.AddWithValue("@StatsId", statsId);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<UserSignInStats?> GetTodaySignInAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT TOP 1 * FROM UserSignInStats 
                WHERE UserID = @UserId 
                AND CAST(SignTime AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY SignTime DESC", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserSignInStats
                {
                    StatsID = reader.GetInt32("StatsID"),
                    UserID = reader.GetInt32("UserID"),
                    SignTime = reader.GetDateTime("SignTime"),
                    PointsEarned = reader.GetInt32("PointsEarned"),
                    PetExpEarned = reader.GetInt32("PetExpEarned"),
                    CouponEarned = reader.IsDBNull("CouponEarned") ? null : reader.GetInt32("CouponEarned"),
                    ConsecutiveDays = reader.GetInt32("ConsecutiveDays")
                };
            }
            return null;
        }

        public async Task<int> GetConsecutiveDaysAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT TOP 1 ConsecutiveDays FROM UserSignInStats 
                WHERE UserID = @UserId 
                ORDER BY SignTime DESC", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            var result = await command.ExecuteScalarAsync();
            return result != null ? (int)result : 0;
        }

        public async Task<bool> ProcessDailySignInAsync(int userId)
        {
            // Check if user already signed in today
            var todaySignIn = await GetTodaySignInAsync(userId);
            if (todaySignIn != null)
            {
                return false; // Already signed in today
            }

            // Get consecutive days
            var consecutiveDays = await GetConsecutiveDaysAsync(userId);
            
            // Check if user signed in yesterday
            var wasYesterday = await CheckYesterdaySignInAsync(userId);
            if (!wasYesterday)
            {
                consecutiveDays = 1; // Reset consecutive days
            }
            else
            {
                consecutiveDays++; // Increment consecutive days
            }

            // Calculate rewards based on consecutive days
            var pointsEarned = CalculatePointsReward(consecutiveDays);
            var petExpEarned = CalculatePetExpReward(consecutiveDays);
            var couponEarned = ShouldGrantCoupon(consecutiveDays) ? 1 : (int?)null;

            // Create sign-in record
            var signInStats = new UserSignInStats
            {
                UserID = userId,
                SignTime = DateTime.Now,
                PointsEarned = pointsEarned,
                PetExpEarned = petExpEarned,
                CouponEarned = couponEarned,
                ConsecutiveDays = consecutiveDays
            };

            return await CreateSignInStatsAsync(signInStats);
        }

        private async Task<bool> CheckYesterdaySignInAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT COUNT(*) FROM UserSignInStats 
                WHERE UserID = @UserId 
                AND CAST(SignTime AS DATE) = CAST(DATEADD(day, -1, GETDATE()) AS DATE)", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            var result = await command.ExecuteScalarAsync();
            return result != null && (int)result > 0;
        }

        private int CalculatePointsReward(int consecutiveDays)
        {
            // Base reward: 5 points, +2 points per consecutive day, max 30 points
            return Math.Min(5 + (consecutiveDays - 1) * 2, 30);
        }

        private int CalculatePetExpReward(int consecutiveDays)
        {
            // Base reward: 0 exp, +3 exp every 3 consecutive days, max 20 exp
            return Math.Min((consecutiveDays / 3) * 3, 20);
        }

        private bool ShouldGrantCoupon(int consecutiveDays)
        {
            // Grant coupon every 7 consecutive days
            return consecutiveDays % 7 == 0;
        }
    }
}
