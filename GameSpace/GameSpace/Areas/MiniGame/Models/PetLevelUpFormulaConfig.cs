using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物升級公式配置（從 SystemSettings 讀取的 JSON 結構）
    /// ✅ 已修正：此 Model 結構與資料庫中實際的 JSON 格式完全一致
    /// </summary>
    public class PetLevelUpFormulaConfig
    {
        /// <summary>
        /// 公式描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 公式階段配置列表
        /// </summary>
        public List<FormulaTier> Tiers { get; set; } = new List<FormulaTier>();
    }

    /// <summary>
    /// 公式階段配置
    /// </summary>
    public class FormulaTier
    {
        /// <summary>
        /// 最小等級（包含）
        /// </summary>
        public int MinLevel { get; set; }

        /// <summary>
        /// 最大等級（包含），-1 代表無上限
        /// </summary>
        public int MaxLevel { get; set; }

        /// <summary>
        /// 公式類型：linear（線性）、quadratic（二次）、exponential（指數）
        /// </summary>
        public string Type { get; set; } = "linear";

        /// <summary>
        /// 公式字串（人類可讀格式，例如：\"40 * level + 60\"）
        /// </summary>
        public string Formula { get; set; } = string.Empty;

        /// <summary>
        /// 階段描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 從公式字串解析參數
        /// 支援格式：
        /// - 線性：\"A * level + B\" (例如：\"40 * level + 60\")
        /// - 二次：\"A * level^2 + B\" (例如：\"0.8 * level^2 + 380\")
        /// - 指數：\"A * (Base^level)\" (例如：\"285.69 * (1.06^level)\")
        /// </summary>
        public (double A, double B, double Base) ParseFormulaParams()
        {
            if (string.IsNullOrWhiteSpace(Formula))
            {
                return (0, 0, 1.0);
            }

            try
            {
                // 移除空格
                var cleanFormula = Formula.Replace(" ", "");

                switch (Type.ToLower())
                {
                    case "linear":
                        // 格式: A*level+B 或 A*level-B
                        var linearMatch = Regex.Match(cleanFormula, @"(\d+\.?\d*)\*level([+\-])(\d+\.?\d*)");
                        if (linearMatch.Success)
                        {
                            double a = double.Parse(linearMatch.Groups[1].Value);
                            double b = double.Parse(linearMatch.Groups[3].Value);
                            if (linearMatch.Groups[2].Value == "-") b = -b;
                            return (a, b, 1.0);
                        }
                        break;

                    case "quadratic":
                        // 格式: A*level^2+B 或 A*level²+B
                        var quadMatch = Regex.Match(cleanFormula, @"(\d+\.?\d*)\*level[\^²2]2?([+\-])(\d+\.?\d*)");
                        if (quadMatch.Success)
                        {
                            double a = double.Parse(quadMatch.Groups[1].Value);
                            double b = double.Parse(quadMatch.Groups[3].Value);
                            if (quadMatch.Groups[2].Value == "-") b = -b;
                            return (a, b, 1.0);
                        }
                        break;

                    case "exponential":
                        // 格式: A*(Base^level)
                        var expMatch = Regex.Match(cleanFormula, @"(\d+\.?\d*)\*\((\d+\.?\d*)\^level\)");
                        if (expMatch.Success)
                        {
                            double a = double.Parse(expMatch.Groups[1].Value);
                            double baseVal = double.Parse(expMatch.Groups[2].Value);
                            return (a, 0, baseVal);
                        }
                        break;
                }

                return (0, 0, 1.0);
            }
            catch
            {
                return (0, 0, 1.0);
            }
        }
    }
}
