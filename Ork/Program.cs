using Ork.Network;
using Ork.Tests;
using Ork.Users;
using System.Net;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Running Tests...");
        PacketFormatTest.Run();

        Thread.Sleep(200);

        Console.WriteLine("Setting up...");

        Console.WriteLine("Creating Server");
        Server server = new Server(IPAddress.Any, 4041);

        Console.WriteLine("Setting up UserManager");
        UserManager userManager = new UserManager();
        userManager.UseServer(server);

        bool active = true;

        server.Start();

        while (active)
        {
            server.Tick();
        }

        server.Stop();
    }
}