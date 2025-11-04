using System.Text.RegularExpressions;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 提供全面模糊搜尋功能，支援 OR 邏輯和 5 級優先順序排序
    /// 用於所有 MiniGame Area 查詢功能
    /// </summary>
    public interface IFuzzySearchService
    {
        /// <summary>
        /// 執行模糊搜尋並返回優先順序分數
        /// </summary>
        /// <param name="searchTerm">搜尋關鍵字</param>
        /// <param name="targetFields">目標欄位值列表</param>
        /// <returns>優先順序分數 (1-5，1 最高)，如果不匹配返回 0</returns>
        int CalculateMatchPriority(string? searchTerm, params string?[] targetFields);

        /// <summary>
        /// 檢查是否有任何欄位匹配搜尋條件 (OR 邏輯)
        /// </summary>
        bool IsMatch(string? searchTerm, params string?[] targetFields);

        /// <summary>
        /// 將搜尋關鍵字分詞為多個子詞 (支援空格、逗號分隔)
        /// </summary>
        List<string> TokenizeSearchTerm(string? searchTerm);

        /// <summary>
        /// 計算字串相似度 (Levenshtein Distance)
        /// </summary>
        int CalculateSimilarity(string source, string target);
    }

    public class FuzzySearchService : IFuzzySearchService
    {
        /// <summary>
        /// 執行模糊搜尋並返回優先順序分數
        ///
        /// 優先順序規則 (1-5):
        /// 1. 完全匹配 (Exact Match) - 最高優先級
        /// 2. 開頭匹配 (Starts With) - 高優先級
        /// 3. 包含匹配 (Contains) - 中等優先級
        /// 4. 模糊匹配 (Fuzzy Match, Levenshtein Distance <= 2) - 低優先級
        /// 5. 部分詞匹配 (Partial Token Match) - 最低優先級
        /// 0. 不匹配
        /// </summary>
        public int CalculateMatchPriority(string? searchTerm, params string?[] targetFields)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return 0;

            searchTerm = searchTerm.Trim().ToLowerInvariant();
            var tokens = TokenizeSearchTerm(searchTerm);

            int bestPriority = 0;

            foreach (var field in targetFields)
            {
                if (string.IsNullOrWhiteSpace(field))
                    continue;

                var fieldLower = field.Trim().ToLowerInvariant();

                // Priority 1: 完全匹配
                if (fieldLower == searchTerm)
                    return 1;

                // Priority 2: 開頭匹配
                if (fieldLower.StartsWith(searchTerm))
                {
                    bestPriority = Math.Max(bestPriority, 2);
                    continue;
                }

                // Priority 3: 包含匹配
                if (fieldLower.Contains(searchTerm))
                {
                    bestPriority = Math.Max(bestPriority, 3);
                    continue;
                }

                // Priority 4: 模糊匹配 (Levenshtein Distance <= 2)
                if (CalculateSimilarity(fieldLower, searchTerm) <= 2)
                {
                    bestPriority = Math.Max(bestPriority, 4);
                    continue;
                }

                // Priority 5: 部分詞匹配 (任一 token 匹配)
                foreach (var token in tokens)
                {
                    if (fieldLower.Contains(token))
                    {
                        bestPriority = Math.Max(bestPriority, 5);
                        break;
                    }
                }
            }

            return bestPriority;
        }

        /// <summary>
        /// 檢查是否有任何欄位匹配搜尋條件 (OR 邏輯)
        /// </summary>
        public bool IsMatch(string? searchTerm, params string?[] targetFields)
        {
            return CalculateMatchPriority(searchTerm, targetFields) > 0;
        }

        /// <summary>
        /// 將搜尋關鍵字分詞為多個子詞 (支援空格、逗號、分號分隔)
        /// </summary>
        public List<string> TokenizeSearchTerm(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<string>();

            // 使用正則表達式分割，支援空格、逗號、分號
            var tokens = Regex.Split(searchTerm.Trim(), @"[\s,;]+")
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.ToLowerInvariant())
                .Distinct()
                .ToList();

            return tokens;
        }

        /// <summary>
        /// 計算字串相似度 (Levenshtein Distance)
        /// 返回需要的最少編輯次數 (插入、刪除、替換)
        /// </summary>
        public int CalculateSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return target?.Length ?? 0;

            if (string.IsNullOrEmpty(target))
                return source.Length;

            var sourceLength = source.Length;
            var targetLength = target.Length;

            // 優化：如果長度差異過大，直接返回較大值
            if (Math.Abs(sourceLength - targetLength) > 3)
                return Math.Max(sourceLength, targetLength);

            var matrix = new int[sourceLength + 1, targetLength + 1];

            // 初始化第一行和第一列
            for (int i = 0; i <= sourceLength; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= targetLength; j++)
                matrix[0, j] = j;

            // 填充矩陣
            for (int i = 1; i <= sourceLength; i++)
            {
                for (int j = 1; j <= targetLength; j++)
                {
                    var cost = (source[i - 1] == target[j - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(
                            matrix[i - 1, j] + 1,      // 刪除
                            matrix[i, j - 1] + 1),     // 插入
                        matrix[i - 1, j - 1] + cost);  // 替換
                }
            }

            return matrix[sourceLength, targetLength];
        }
    }

    /// <summary>
    /// 模糊搜尋結果包裝類（包含優先順序）
    /// </summary>
    public class FuzzySearchResult<T>
    {
        public T Item { get; set; } = default!;
        public int Priority { get; set; }  // 1-5, 1 最高
    }

    /// <summary>
    /// IQueryable 擴展方法（用於 LINQ 查詢）
    /// </summary>
    public static class FuzzySearchExtensions
    {
        /// <summary>
        /// 對 IEnumerable 進行模糊搜尋並按優先順序排序
        /// </summary>
        public static IEnumerable<T> ApplyFuzzySearch<T>(
            this IEnumerable<T> source,
            string? searchTerm,
            IFuzzySearchService fuzzyService,
            Func<T, string?[]> fieldSelector,
            string? sortBy = null,
            bool descending = false)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // 無搜尋條件時，直接應用排序
                return ApplySort(source, sortBy, descending);
            }

            // 計算每個項目的優先順序
            var resultsWithPriority = source
                .Select(item => new FuzzySearchResult<T>
                {
                    Item = item,
                    Priority = fuzzyService.CalculateMatchPriority(searchTerm, fieldSelector(item))
                })
                .Where(r => r.Priority > 0)  // 過濾不匹配的項目
                .OrderBy(r => r.Priority)    // 按優先順序排序 (1 最高)
                .ThenBy(r => sortBy)         // 次要排序
                .Select(r => r.Item);

            // 應用額外排序
            return ApplySort(resultsWithPriority, sortBy, descending);
        }

        private static IEnumerable<T> ApplySort<T>(IEnumerable<T> source, string? sortBy, bool descending)
        {
            // 這裡可以根據 sortBy 參數動態應用排序
            // 目前返回原樣，由調用方處理具體排序邏輯
            return source;
        }
    }
}
