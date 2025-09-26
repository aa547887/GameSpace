using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 數據庫模型集合
    /// </summary>
    public class DatabaseModels
    {
        /// <summary>
        /// 數據庫連接配置
        /// </summary>
        public class DatabaseConnectionConfig
        {
            /// <summary>
            /// 連接字串
            /// </summary>
            public string ConnectionString { get; set; } = string.Empty;
            
            /// <summary>
            /// 數據庫名稱
            /// </summary>
            public string DatabaseName { get; set; } = string.Empty;
            
            /// <summary>
            /// 伺服器名稱
            /// </summary>
            public string ServerName { get; set; } = string.Empty;
            
            /// <summary>
            /// 是否使用信任連接
            /// </summary>
            public bool UseTrustedConnection { get; set; } = true;
            
            /// <summary>
            /// 連接超時時間（秒）
            /// </summary>
            public int ConnectionTimeout { get; set; } = 30;
            
            /// <summary>
            /// 命令超時時間（秒）
            /// </summary>
            public int CommandTimeout { get; set; } = 30;
        }

        /// <summary>
        /// 數據庫表配置
        /// </summary>
        public class DatabaseTableConfig
        {
            /// <summary>
            /// 表名稱
            /// </summary>
            public string TableName { get; set; } = string.Empty;
            
            /// <summary>
            /// 主鍵欄位
            /// </summary>
            public string PrimaryKey { get; set; } = string.Empty;
            
            /// <summary>
            /// 索引欄位
            /// </summary>
            public List<string> IndexFields { get; set; } = new();
            
            /// <summary>
            /// 外鍵關聯
            /// </summary>
            public List<ForeignKeyRelation> ForeignKeys { get; set; } = new();
            
            /// <summary>
            /// 檢查約束
            /// </summary>
            public List<CheckConstraint> CheckConstraints { get; set; } = new();
        }

        /// <summary>
        /// 外鍵關聯
        /// </summary>
        public class ForeignKeyRelation
        {
            /// <summary>
            /// 外鍵欄位
            /// </summary>
            public string ForeignKeyField { get; set; } = string.Empty;
            
            /// <summary>
            /// 參考表名稱
            /// </summary>
            public string ReferenceTable { get; set; } = string.Empty;
            
            /// <summary>
            /// 參考欄位
            /// </summary>
            public string ReferenceField { get; set; } = string.Empty;
            
            /// <summary>
            /// 級聯刪除
            /// </summary>
            public bool CascadeDelete { get; set; } = false;
            
            /// <summary>
            /// 級聯更新
            /// </summary>
            public bool CascadeUpdate { get; set; } = false;
        }

        /// <summary>
        /// 檢查約束
        /// </summary>
        public class CheckConstraint
        {
            /// <summary>
            /// 約束名稱
            /// </summary>
            public string ConstraintName { get; set; } = string.Empty;
            
            /// <summary>
            /// 約束條件
            /// </summary>
            public string ConstraintCondition { get; set; } = string.Empty;
            
            /// <summary>
            /// 約束描述
            /// </summary>
            public string Description { get; set; } = string.Empty;
        }

        /// <summary>
        /// 數據庫查詢配置
        /// </summary>
        public class DatabaseQueryConfig
        {
            /// <summary>
            /// 查詢名稱
            /// </summary>
            public string QueryName { get; set; } = string.Empty;
            
            /// <summary>
            /// SQL查詢語句
            /// </summary>
            public string SqlQuery { get; set; } = string.Empty;
            
            /// <summary>
            /// 參數列表
            /// </summary>
            public List<QueryParameter> Parameters { get; set; } = new();
            
            /// <summary>
            /// 查詢類型
            /// </summary>
            public string QueryType { get; set; } = "Select";
            
            /// <summary>
            /// 是否使用分頁
            /// </summary>
            public bool UsePaging { get; set; } = false;
            
            /// <summary>
            /// 預設頁面大小
            /// </summary>
            public int DefaultPageSize { get; set; } = 10;
        }

        /// <summary>
        /// 查詢參數
        /// </summary>
        public class QueryParameter
        {
            /// <summary>
            /// 參數名稱
            /// </summary>
            public string ParameterName { get; set; } = string.Empty;
            
            /// <summary>
            /// 參數類型
            /// </summary>
            public string ParameterType { get; set; } = string.Empty;
            
            /// <summary>
            /// 是否為輸出參數
            /// </summary>
            public bool IsOutput { get; set; } = false;
            
            /// <summary>
            /// 預設值
            /// </summary>
            public object? DefaultValue { get; set; }
            
            /// <summary>
            /// 是否必填
            /// </summary>
            public bool IsRequired { get; set; } = false;
        }

        /// <summary>
        /// 數據庫事務配置
        /// </summary>
        public class DatabaseTransactionConfig
        {
            /// <summary>
            /// 事務隔離級別
            /// </summary>
            public string IsolationLevel { get; set; } = "ReadCommitted";
            
            /// <summary>
            /// 事務超時時間（秒）
            /// </summary>
            public int TransactionTimeout { get; set; } = 60;
            
            /// <summary>
            /// 是否自動提交
            /// </summary>
            public bool AutoCommit { get; set; } = true;
            
            /// <summary>
            /// 是否使用分散式事務
            /// </summary>
            public bool UseDistributedTransaction { get; set; } = false;
        }

        /// <summary>
        /// 數據庫備份配置
        /// </summary>
        public class DatabaseBackupConfig
        {
            /// <summary>
            /// 備份路徑
            /// </summary>
            public string BackupPath { get; set; } = string.Empty;
            
            /// <summary>
            /// 備份檔案名稱
            /// </summary>
            public string BackupFileName { get; set; } = string.Empty;
            
            /// <summary>
            /// 備份類型
            /// </summary>
            public string BackupType { get; set; } = "Full";
            
            /// <summary>
            /// 是否壓縮
            /// </summary>
            public bool Compress { get; set; } = true;
            
            /// <summary>
            /// 備份保留天數
            /// </summary>
            public int RetentionDays { get; set; } = 30;
            
            /// <summary>
            /// 是否加密
            /// </summary>
            public bool Encrypt { get; set; } = false;
        }

        /// <summary>
        /// 數據庫效能監控配置
        /// </summary>
        public class DatabasePerformanceConfig
        {
            /// <summary>
            /// 是否啟用效能監控
            /// </summary>
            public bool EnableMonitoring { get; set; } = true;
            
            /// <summary>
            /// 慢查詢閾值（毫秒）
            /// </summary>
            public int SlowQueryThreshold { get; set; } = 1000;
            
            /// <summary>
            /// 連接池大小
            /// </summary>
            public int ConnectionPoolSize { get; set; } = 100;
            
            /// <summary>
            /// 最大連接數
            /// </summary>
            public int MaxConnections { get; set; } = 200;
            
            /// <summary>
            /// 連接生命週期（分鐘）
            /// </summary>
            public int ConnectionLifetime { get; set; } = 30;
        }

        /// <summary>
        /// 數據庫安全配置
        /// </summary>
        public class DatabaseSecurityConfig
        {
            /// <summary>
            /// 是否啟用SQL注入防護
            /// </summary>
            public bool EnableSqlInjectionProtection { get; set; } = true;
            
            /// <summary>
            /// 是否啟用參數化查詢
            /// </summary>
            public bool EnableParameterizedQueries { get; set; } = true;
            
            /// <summary>
            /// 是否啟用查詢日誌
            /// </summary>
            public bool EnableQueryLogging { get; set; } = true;
            
            /// <summary>
            /// 是否啟用敏感資料遮罩
            /// </summary>
            public bool EnableDataMasking { get; set; } = false;
            
            /// <summary>
            /// 敏感欄位列表
            /// </summary>
            public List<string> SensitiveFields { get; set; } = new();
        }
    }
}
