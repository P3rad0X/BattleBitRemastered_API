using System.Text;
using System.Text.Json;
using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;

namespace DiscordWebhook;

internal static class Program
{
    public const string DiscordWebhookUrl = "https://discord.com/api/webhooks/1146078070378348544/yYc9L0mjDbzuWpPJUzTKekouVTdsBd7x7gKNk6Zxtu8HWo13dbHy45ZDIZZvj1RRLFkJ";

    public static HttpClient Client;

    private static void Main(string[] args)
    {
        // Client is used to send POST request to our Discord webhook.
        // You could also initialize a Discord bot here, but we're keeping it simple as this is just an example.
        // Using a custom Discord bot you can make a nicer bridge with the game to be able to manage and monitor
        // the gameserver from within Discord. To get started on that, see https://discordnet.dev/index.html
        Client = new HttpClient();
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        listener.LogLevel = LogLevel.Sockets | LogLevel.All;
        listener.OnLog += OnLog;
        listener.Start(29294);
        Console.WriteLine("Started Listener on port: 29294");

        // The listener is running, now we just want this thread to sleep so that the program does not exit.
        // Alternatively, you could make the listener a hosted service, which allows for more flexibility including
        // dependency injection. See https://github.com/Julgers/Database-connection-example/blob/main/Program.cs for an
        // example on how to do that.
        Thread.Sleep(-1);
    }

    private static void OnLog(LogLevel level, string msg, object? obj)
    {
        Console.WriteLine("Log (" + level + "): " + msg);
    }


}

internal class MyPlayer : Player<MyPlayer>
{

}

internal class MyGameServer : GameServer<MyPlayer>
{
    public override async Task OnConnected()
    {
        await SendDiscordMessage($"Gameserver {ServerName} connected");
    }

    public override async Task OnDisconnected()
    {
        await SendDiscordMessage($"Gameserver {ServerName} disconnected");
    }

    public override async Task OnPlayerReported(
        MyPlayer reporter,
        MyPlayer reportedPlayer,
        ReportReason reason,
        string note)
    {
        await SendDiscordMessage($"{reportedPlayer.SteamID} - {reportedPlayer.Name} \n" +
            $" was reported by {reporter.SteamID} - {reporter.Name} \n" +
            $" for {reason.ToString()}. Note: {note}");
    }

    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<MyPlayer> args)
    {
        await SendDiscordMessage($"{args.Killer.Name} killed {args.Victim.Name}");
    }

    public override async Task OnPlayerDisconnected(MyPlayer player)
    {
        await SendDiscordMessage($"{player.Name} disconnected from {player.GameServer.ServerName}");
    }

    public override async Task OnPlayerConnected(MyPlayer player)
    {
        await SendDiscordMessage($"{player.Name} connected to {player.GameServer.ServerName}");
    }

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
        // Keep in mind that this application could listen to multiple servers.
        await SendDiscordMessage($"{player.GameServer.ServerName} - {player.Name}: {msg}");

        // You can return false to block certain message from being posted in the chat.
        // But we still post it to our webhook to monitor it from Discord.
        if (msg.Contains("nice cars"))
            return false;

        return true;
    }

    // Sends a Discord message by sending a POST request to our webhook.
    // Use embeds if you want to make it look nicer.
    private static async Task SendDiscordMessage(string message)
    {
        using StringContent jsonContent = new(
        JsonSerializer.Serialize(new
        {
            content = message
        }),
        Encoding.UTF8,
        "application/json");

        await Program.Client.PostAsync(Program.DiscordWebhookUrl, jsonContent);
    }
}