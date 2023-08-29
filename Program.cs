using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using System.Threading.Channels;
using System.Xml;
using System;
using System.IO;//Will be using sytem and system.IO for saving chats to text file.


internal class Program

{


    private static void Main(string[] args)
    {
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
}

internal class MyPlayer : Player<MyPlayer>
{



}

internal class MyGameServer : GameServer<MyPlayer>
{










}