using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using Server;

internal class Program
{
    private static void Main(string[] args)
    {
        ChatServer server = new ChatServer();
        server.Start();
    }
}