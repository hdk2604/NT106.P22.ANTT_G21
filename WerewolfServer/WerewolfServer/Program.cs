using WerewolfServer;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new GameServer();
        Console.WriteLine("=== WEREWOLF GAME SERVER ===");
        Console.WriteLine("Server đang khởi động với các tính năng:");
        Console.WriteLine("- Async/Await pattern");
        Console.WriteLine("- Connection pooling");
        Console.WriteLine("- Connection limits (1000 max)");
        Console.WriteLine("- Thread-safe broadcast");
        Console.WriteLine("================================");
        
        await server.StartAsync();
    }
}