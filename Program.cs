using System;
using System.Text;
using System.Text.Json;
using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;

namespace DiscordWebhook;

internal static class Program
{
    public const string DiscordWebhookUrlServerLog = "https://discord.com/api/webhooks/1146078070378348544/yYc9L0mjDbzuWpPJUzTKekouVTdsBd7x7gKNk6Zxtu8HWo13dbHy45ZDIZZvj1RRLFkJ";
    public const string DiscordWebhookUrlChat = "https://discord.com/api/webhooks/1146730782241599548/rAYAwHSmIfZfa-Gl705XztlHybWgp0zgzi-iovnw2lKLFfQZstXBN-LNup4al17CC515";
    public const string DiscordWebhookUrlReport = "https://discord.com/api/webhooks/1146730858301116509/Ht5SkOe31kPppGIyOOlQOhvdbaufMUO_HaUkCw60qHIfzLY3irE19XnrFedK4NudTlhh";
    public const string DiscordWebhookUrlRound= "https://discord.com/api/webhooks/1146731792523591720/wB9hLkXta27n8WzNUpM44LNlA5NwaX7tdTkCjdFHztvkbvK6GkT2_8480tXzDDj-TsLS";
    public const string DiscordWebhookUrlKillFeed = "https://discord.com/api/webhooks/1146733122378010714/_kNJ3OC8bv0rCdxPLBkuCKK1PsAjEcIAIeRHY5ZxbLumJYr0Bq1JmlPPH_S4OxctPb60";
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

    public override async Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
    {
        args.Stats.Progress.Rank = 200;
        args.Stats.Progress.Prestige = 10;
        ServerSettings.UnlockAllAttachments = true;
       
        
    }

    public override async Task OnConnected()
    {
       
        await SendDiscordMessageServerLog($"Gameserver {ServerName} connected");
    }

    public override async Task OnDisconnected()
    {
        await SendDiscordMessageServerLog($"Gameserver {ServerName} disconnected");
    }

    public override async Task OnPlayerReported(
        MyPlayer reporter,
        MyPlayer reportedPlayer,
        ReportReason reason,
        string note)
    {
        await Reportlog($"{reportedPlayer.SteamID} - {reportedPlayer.Name} \n" +
            $" was reported by {reporter.SteamID} - {reporter.Name} \n" +
            $" for {reason.ToString()}. Note: {note}");
    }

    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<MyPlayer> args)
    {
        await killLog($"{args.Killer.Name} killed {args.Victim.Name}" + "with " + args.KillerTool);
    }

    public override async Task OnRoundStarted()
    {
        await RoundLog("a Round has started on " + Map + "with " + $" {CurrentPlayerCount} " + "players" );
    }

    public override async Task OnPlayerDisconnected(MyPlayer player)
    {
        await SendDiscordMessageServerLog($"{player.Name} disconnected from the server");
    }

    public override async Task OnPlayerConnected(MyPlayer player)
    {
        ForceStartGame();
        await SendDiscordMessageServerLog($"{player.Name} has connected to the server");
    }

    public override async Task<bool> OnPlayerTypedMessage(MyPlayer player, ChatChannel channel, string msg)
    {
       
        Console.WriteLine("Entering OnPlayerTypedMessage method.");

        List<string> profaneWords = new List<string>(File.ReadAllLines("profane_words.txt", Encoding.UTF8));

        Console.WriteLine($"Number of profane words: {profaneWords.Count}");

        await Chatlog($"{player.GameServer.ServerName} - {player.Name}: {msg}");
        foreach (string profaneWord in profaneWords)
        {
            if (msg.Contains(profaneWord, StringComparison.OrdinalIgnoreCase))
            {
                player.Message("Please do not use profanity in chat.", 10f);
                return false;
            }


        }

        return true;

    }









    // Sends a Discord message by sending a POST request to our webhook.
    // Use embeds if you want to make it look nicer.
    private static async Task SendDiscordMessageServerLog(string message)
    {
        using StringContent jsonContent = new(
        JsonSerializer.Serialize(new
        {
            content = message
        }),
        Encoding.UTF8,
        "application/json");

        await Program.Client.PostAsync(Program.DiscordWebhookUrlServerLog, jsonContent);
    }

    private static async Task RoundLog(string message)
    {
        using StringContent jsonContent = new(
        JsonSerializer.Serialize(new
        {
            content = message
        }),
        Encoding.UTF8,
        "application/json");

        await Program.Client.PostAsync(Program.DiscordWebhookUrlRound, jsonContent);
    }

    private static async Task Chatlog(string message)
    {
        using StringContent jsonContent = new(
        JsonSerializer.Serialize(new
        {
            content = message
        }),
        Encoding.UTF8,
        "application/json");

        await Program.Client.PostAsync(Program.DiscordWebhookUrlChat, jsonContent);
    }



    private static async Task Reportlog(string message)
    {
        using StringContent jsonContent = new(
        JsonSerializer.Serialize(new
        {
            content = message
        }),
        Encoding.UTF8,
        "application/json");

        await Program.Client.PostAsync(Program.DiscordWebhookUrlReport, jsonContent);
    }

    private static async Task killLog(string message)
    {
        using StringContent jsonContent = new(
        JsonSerializer.Serialize(new
        {
            content = message
        }),
        Encoding.UTF8,
        "application/json");

        await Program.Client.PostAsync(Program.DiscordWebhookUrlKillFeed, jsonContent);
    }
}