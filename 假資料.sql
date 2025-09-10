/* ===========================================================
   GameSpace 假資料 Seeder  (對齊你的 schema)
   目標：metric_sources / metrics / games / game_source_map / game_metric_daily
   =========================================================== */

SET NOCOUNT ON;
BEGIN TRAN;

DECLARE @utcnow DATETIME2 = SYSUTCDATETIME();
DECLARE @Days INT = 30;                   -- 近幾天
DECLARE @Today DATE = CAST(@utcnow AS DATE);

------------------------------------------------------------
-- 1) 來源：metric_sources  (name, note, created_at)
------------------------------------------------------------
;WITH S(name, note) AS (
    SELECT N'Steam',       N'Steam source' UNION ALL
    SELECT N'巴哈姆特',    N'Bahamut source' UNION ALL
    SELECT N'YouTube',     N'YouTube source'
)
MERGE dbo.metric_sources AS T
USING (SELECT name, note FROM S) AS S
ON T.name = S.name
WHEN NOT MATCHED BY TARGET THEN
  INSERT (name, note, created_at) VALUES (S.name, S.note, @utcnow);

-- 暫存 source 對照
DECLARE @Source TABLE (name NVARCHAR(50) PRIMARY KEY, source_id INT);
INSERT INTO @Source(name, source_id)
SELECT name, source_id FROM dbo.metric_sources
WHERE name IN (N'Steam', N'巴哈姆特', N'YouTube');

------------------------------------------------------------
-- 2) 指標：metrics  (source_id, code, unit, description, is_active, created_at)
------------------------------------------------------------
;WITH M(src, code, unit, descr) AS (
    SELECT N'Steam',    N'ccu',         N'users',   N'同時在線' UNION ALL
    SELECT N'Steam',    N'peak_ccu',    N'users',   N'最高同時在線' UNION ALL
    SELECT N'巴哈姆特', N'topic_count', N'count',   N'討論串數量' UNION ALL
    SELECT N'巴哈姆特', N'reply_count', N'count',   N'回覆數量' UNION ALL
    SELECT N'YouTube',  N'views_24h',   N'views',   N'近24小時觀看數' UNION ALL
    SELECT N'YouTube',  N'like_ratio',  N'percent', N'喜好比率'
)
MERGE dbo.metrics AS T
USING (
    SELECT s.source_id, m.code, m.unit, m.descr
    FROM M m
    JOIN @Source s ON s.name = m.src
) AS S
ON  T.source_id = S.source_id AND T.code = S.code
WHEN NOT MATCHED BY TARGET THEN
  INSERT (source_id, code, unit, description, is_active, created_at)
  VALUES (S.source_id, S.code, S.unit, S.descr, 1, @utcnow);

-- 暫存 metric 對照
DECLARE @Metric TABLE (metric_id INT PRIMARY KEY, source_id INT, code NVARCHAR(50));
INSERT INTO @Metric(metric_id, source_id, code)
SELECT metric_id, source_id, code
FROM dbo.metrics
WHERE code IN (N'ccu', N'peak_ccu', N'topic_count', N'reply_count', N'views_24h', N'like_ratio');

------------------------------------------------------------
-- 3) 遊戲：games  (name, genre, created_at, name_zh)
------------------------------------------------------------
;WITH G(name, name_zh) AS (
    SELECT N'Dota 2',                               N'刀塔2' UNION ALL
    SELECT N'League of Legends',                    N'英雄聯盟' UNION ALL
    SELECT N'VALORANT',                             N'瓦羅蘭特' UNION ALL
    SELECT N'Arena of Valor',                       N'傳說對決' UNION ALL
    SELECT N'Honor of Kings',                       N'王者榮耀' UNION ALL
    SELECT N'Forza Horizon 5',                      N'極限競速：地平線5' UNION ALL
    SELECT N'Assetto Corsa Competizione',           N'神力科莎 競爭版' UNION ALL
    SELECT N'F1 24',                                N'一級方程式24' UNION ALL
    SELECT N'Mario Kart Tour',                      N'瑪利歐賽車 巡迴賽' UNION ALL
    SELECT N'Hollow Knight: Silksong',              N'空洞騎士：絲之歌' UNION ALL
    SELECT N'Alan Wake 2',                          N'心靈殺手2' UNION ALL
    SELECT N'Black Myth: Wukong',                   N'黑神話：悟空' UNION ALL
    SELECT N'Arknights',                            N'明日方舟' UNION ALL
    SELECT N'Sky: Children of the Light',           N'光遇' UNION ALL
    SELECT N'Sid Meier''s Civilization VI',         N'文明帝國6' UNION ALL
    SELECT N'Cities: Skylines II',                  N'都市：天際線2' UNION ALL
    SELECT N'Total War: PHARAOH',                   N'全面戰爭：法老' UNION ALL
    SELECT N'MapleStory',                           N'新楓之谷' UNION ALL
    SELECT N'Baldur''s Gate 3',                     N'博德之門3' UNION ALL
    SELECT N'Cyberpunk 2077: Phantom Liberty',      N'賽博龐克2077：自由幻夢' UNION ALL
    SELECT N'Genshin Impact',                       N'原神' UNION ALL
    SELECT N'Honkai: Star Rail',                    N'崩壞：星穹鐵道' UNION ALL
    SELECT N'Blue Archive',                         N'碧藍檔案' UNION ALL
    SELECT N'Elden Ring',                           N'艾爾登法環'
)
MERGE dbo.games AS T
USING (SELECT name, name_zh FROM G) AS S
ON T.name = S.name
WHEN NOT MATCHED BY TARGET THEN
  INSERT (name, genre, created_at, name_zh)
  VALUES (S.name, NULL, @utcnow, S.name_zh);

-- 暫存 game 對照
DECLARE @Game TABLE (game_id INT PRIMARY KEY, name NVARCHAR(50));
INSERT INTO @Game(game_id, name)
SELECT game_id, name
FROM dbo.games
WHERE name IN (SELECT name FROM (VALUES
(N'Dota 2'),(N'League of Legends'),(N'VALORANT'),(N'Arena of Valor'),(N'Honor of Kings'),
(N'Forza Horizon 5'),(N'Assetto Corsa Competizione'),(N'F1 24'),(N'Mario Kart Tour'),(N'Hollow Knight: Silksong'),
(N'Alan Wake 2'),(N'Black Myth: Wukong'),(N'Arknights'),(N'Sky: Children of the Light'),
(N'Sid Meier''s Civilization VI'),(N'Cities: Skylines II'),(N'Total War: PHARAOH'),(N'MapleStory'),
(N'Baldur''s Gate 3'),(N'Cyberpunk 2077: Phantom Liberty'),(N'Genshin Impact'),(N'Honkai: Star Rail'),
(N'Blue Archive'),(N'Elden Ring')) AS X(name));

------------------------------------------------------------
-- 4) game_source_map  (game_id, source_id, external_key, created_at)
-- external_key = 簡易 slug：小寫、空白→連字號、去部份符號
------------------------------------------------------------
;WITH GS AS (
    SELECT g.game_id, g.name AS game_name, s.source_id, s.name AS source_name
    FROM @Game g CROSS JOIN @Source s
)
INSERT INTO dbo.game_source_map (game_id, source_id, external_key, CreatedAt)
SELECT GS.game_id, GS.source_id,
       LOWER(
            REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                    GS.game_name, N' ', N'-'), N':', N''), N'''', N''), N',', N''), N'.', N'')
       )
       + N'-' +
       CASE GS.source_name
            WHEN N'Steam' THEN N'steam'
            WHEN N'巴哈姆特' THEN N'bahamut'
            WHEN N'YouTube' THEN N'youtube'
            ELSE N'src'
       END AS external_key,
       @utcnow
FROM GS
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.game_source_map m
    WHERE m.game_id = GS.game_id AND m.source_id = GS.source_id
);

------------------------------------------------------------
-- 5) game_metric_daily  (game_id, metric_id, date, value, agg_method, quality, created_at, updated_at)
-- 指標：ccu / peak_ccu / topic_count / reply_count / views_24h / like_ratio
-- 最近 @Days 天，避免重複插入
------------------------------------------------------------
;WITH Tally AS (
    SELECT TOP (@Days) ROW_NUMBER() OVER (ORDER BY (SELECT 1)) - 1 AS d
    FROM sys.all_objects
),
Dates AS (
    SELECT DATEADD(DAY, -d, @Today) AS the_day FROM Tally
),
PickMetrics AS (
    SELECT m.metric_id, m.code
    FROM @Metric m
    WHERE m.code IN (N'ccu', N'peak_ccu', N'topic_count', N'reply_count', N'views_24h', N'like_ratio')
),
AllCombos AS (
    SELECT g.game_id, pm.metric_id, pm.code, d.the_day
    FROM @Game g
    CROSS JOIN PickMetrics pm
    CROSS JOIN Dates d
)
INSERT INTO dbo.game_metric_daily
    (game_id, metric_id, date, value, agg_method, quality, created_at, updated_at)
SELECT c.game_id, c.metric_id, c.the_day,
       CASE c.code
            WHEN N'ccu'         THEN CAST(ROUND(1000  + (ABS(CHECKSUM(NEWID())) % 199001), 0) AS DECIMAL(18,4))
            WHEN N'peak_ccu'    THEN CAST(ROUND(5000  + (ABS(CHECKSUM(NEWID())) % 395001), 0) AS DECIMAL(18,4))
            WHEN N'topic_count' THEN CAST(ROUND(10    + (ABS(CHECKSUM(NEWID())) % 4991),  0) AS DECIMAL(18,4))
            WHEN N'reply_count' THEN CAST(ROUND(20    + (ABS(CHECKSUM(NEWID())) % 15000), 0) AS DECIMAL(18,4))
            WHEN N'views_24h'   THEN CAST(ROUND(1000  + (ABS(CHECKSUM(NEWID())) % 999001),0) AS DECIMAL(18,4))
            WHEN N'like_ratio'  THEN CAST(ROUND(700   + (ABS(CHECKSUM(NEWID())) % 301),   1) / 10.0 AS DECIMAL(18,4)) -- 70.0~100.0
            ELSE 0
       END AS value,
       N'raw'   AS agg_method,
       N'seed'  AS quality,
       @utcnow  AS created_at,
       @utcnow  AS updated_at
FROM AllCombos c
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.game_metric_daily d
    WHERE d.game_id = c.game_id AND d.metric_id = c.metric_id AND d.date = c.the_day
);

COMMIT TRAN;
PRINT N'? Seed 完成（sources / metrics / games / maps / dailies 已填入，30 天）';
