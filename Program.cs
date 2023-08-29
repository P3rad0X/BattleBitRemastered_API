using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using System.Threading.Channels;
using System.Xml;
using System;
using System.IO;//Will be using sytem and system.IO for saving chats to text file.
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using DiscordWebhook;



internal class Program
{

    public const string DiscordWebhookUrl = "https://discord.com/api/webhooks/1146078070378348544/yYc9L0mjDbzuWpPJUzTKekouVTdsBd7x7gKNk6Zxtu8HWo13dbHy45ZDIZZvj1RRLFkJ";
    private static DiscordWebhookClient webhookClient;

    
    private static void Main(string[] args)
    {
        webhookClient = new DiscordWebhookClient(DiscordWebhookUrl);

        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.LogLevel = LogLevel.Sockets | LogLevel.All;
        listener.OnLog += OnLog;
        listener.Start(29294);//Start Listener
        Console.WriteLine("Started Listener on port: 29294");
        Thread.Sleep(-1);
      
    }
    private static void OnLog(LogLevel level, string msg, object? obj)
    {
        Console.WriteLine("Log (" + level + "): " + msg);    
    }

    public static void SendWebhookMessage(string message)
    {
        webhookClient.SendMessageAsync(message).Wait();
    }
}

internal class MyPlayer : Player<MyPlayer>
{
    


}

internal class MyGameServer : GameServer<MyPlayer>
{
   
    public override async Task OnPlayerConnected(MyPlayer player)
    {
      
        ForceStartGame();
       
    }

    public override async Task OnRoundStarted()
    {
        string message = "A round has started on " + Map + " with " + $"{CurrentPlayerCount} players";
        Program.SendWebhookMessage(message);
       

    }

    public override Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
        ForceEndGame();
        return base.OnPlayerTypedMessage(player, channel, msg);
    }




}