using Server;
using System.Collections.Frozen;
using System.Configuration;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        MainServer server = new MainServer();
        server.Start();
    }
}