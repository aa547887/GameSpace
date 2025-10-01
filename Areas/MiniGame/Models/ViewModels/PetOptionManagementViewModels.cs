using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物選項統一管理視圖模型
    /// </summary>
    public class PetOptionManagementViewModel
    {
        /// <summary>
        /// 顏色選項列表
        /// </summary>
        public PetColorOptionListViewModel ColorOptions { get; set; } = new();

        /// <summary>
        /// 背景選項列表
        /// </summary>
        public PetBackgroundOptionListViewModel BackgroundOptions { get; set; } = new();

        /// <summary>
        /// 統計資訊
        /// </summary>
        public PetOptionStatisticsViewModel Statistics { get; set; } = new();
    }

    /// <summary>
    /// 寵物選項統計資訊視圖模型
    /// </summary>
    public class PetOptionStatisticsViewModel
    {
        /// <summary>
        /// 總顏色選項數量
        /// </summary>
        public int TotalColorOptions { get; set; }

        /// <summary>
        /// 啟用的顏色選項數量
        /// </summary>
        public int ActiveColorOptions { get; set; }

        /// <summary>
        /// 總背景選項數量
        /// </summary>
        public int TotalBackgroundOptions { get; set; }

        /// <summary>
        /// 啟用的背景選項數量
        /// </summary>
        public int ActiveBackgroundOptions { get; set; }

        /// <summary>
        /// 最近新增的顏色選項
        /// </summary>
        public DateTime? LastColorOptionAdded { get; set; }

        /// <summary>
        /// 最近新增的背景選項
        /// </summary>
        public DateTime? LastBackgroundOptionAdded { get; set; }
    }
}
