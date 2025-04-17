using Server;

internal class Program
{
    private static void Main(string[] args)
    {
        
        CityServer server = new CityServer();
        server.Start();
    }
}