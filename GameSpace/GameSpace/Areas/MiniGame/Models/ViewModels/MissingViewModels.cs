using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// GameRecordViewModel - wrapper for individual game record views
    /// This extends GameSpace.Models.MiniGame with additional properties for display
    /// </summary>
    public class GameRecordViewModel
    {
        public int Id { get; set; }
        public int PlayId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public int Score { get; set; }
        public int PointsEarned { get; set; }
        public int Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Level { get; set; }
        public int ExpGained { get; set; }

        /// <summary>
        /// Create from MiniGame entity
        /// </summary>
        public static GameRecordViewModel FromMiniGame(GameSpace.Models.MiniGame game, User? user = null)
        {
            return new GameRecordViewModel
            {
                Id = game.PlayId,
                PlayId = game.PlayId,
                UserId = game.UserId,
                UserName = user?.UserName ?? "Unknown",
                GameName = game.GameType ?? "Mini Game",
                Result = game.Result,
                Score = game.Level * 100, // Calculate score based on level
                PointsEarned = game.PointsGained,
                Duration = game.EndTime.HasValue
                    ? (int)(game.EndTime.Value - game.StartTime).TotalSeconds
                    : 0,
                StartTime = game.StartTime,
                EndTime = game.EndTime,
                Level = game.Level,
                ExpGained = game.ExpGained
            };
        }
    }

    /// <summary>
    /// SignInRecordViewModel - for displaying individual sign-in records
    /// This extends UserSignInStat with additional properties for display
    /// </summary>
    public class SignInRecordViewModel
    {
        public int Id { get; set; }
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SignInDate { get; set; }
        public int ConsecutiveDays { get; set; }
        public int PointsEarned { get; set; }
        public int ExpGained { get; set; }
        public string? BonusType { get; set; }
        public string? IPAddress { get; set; }

        /// <summary>
        /// Create from UserSignInStat entity
        /// </summary>
        public static SignInRecordViewModel FromUserSignInStat(UserSignInStat stat, User? user = null, int consecutiveDays = 1)
        {
            return new SignInRecordViewModel
            {
                Id = stat.LogId,
                LogId = stat.LogId,
                UserId = stat.UserId,
                UserName = user?.UserName ?? "Unknown",
                SignInDate = stat.SignTime,
                ConsecutiveDays = consecutiveDays,
                PointsEarned = stat.PointsGained,
                ExpGained = stat.ExpGained,
                BonusType = "Daily",
                IPAddress = "Unknown"
            };
        }
    }
}
