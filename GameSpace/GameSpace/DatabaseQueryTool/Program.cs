using Microsoft.Data.SqlClient;
using System;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("🔍 GameSpace 数据库查询工具");
        Console.WriteLine("=====================================");
        
        await TestConnection("DefaultConnection (LocalDB)", 
            "Server=(localdb)\\mssqllocaldb;Database=aspnet-GameSpace-38e0b594-8684-40b2-b330-7fb94b733c73;Trusted_Connection=True;MultipleActiveResultSets=true");
        
        await TestConnection("GameSpace (SQL Express)", 
            "Data Source=(local)\\SQLEXPRESS01;Initial Catalog=GameSpacedatabase;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True");
    }
    
    static async Task TestConnection(string name, string connectionString)
    {
        try
        {
            Console.WriteLine($"\n📊 测试连接: {name}");
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            Console.WriteLine("✅ 连接成功！");
            
            var cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME", conn);
            var reader = await cmd.ExecuteReaderAsync();
            
            Console.WriteLine("\n📋 数据库表列表:");
            int tableCount = 0;
            while (await reader.ReadAsync())
            {
                tableCount++;
                Console.WriteLine($"  {tableCount}. {reader["TABLE_NAME"]}");
            }
            reader.Close();
            
            if (tableCount == 0)
            {
                Console.WriteLine("  (没有找到表)");
            }
            else
            {
                Console.WriteLine($"\n📈 总共找到 {tableCount} 个表");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 连接失败: {ex.Message}");
        }
    }
}
