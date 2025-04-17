using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class CityServer
    {
        const string address = "127.0.0.1";
        const int port = 4140;
        IPEndPoint endPoint;
        EndPoint remoteEndPoint;
        Socket listenSocket;
        List<City> cities;
        Db_Initiziler db;
        public int ZipCode { get; set; }

        public CityServer()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(address), port);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // ip client
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            cities = new List<City>();
            db = new Db_Initiziler();
        }

        public void Start()
        {
            listenSocket.Bind(endPoint);
            Console.WriteLine("Server started!");
            try
            {
                while (true)
                {
                    byte[] zipCode = new byte[1024];
                    listenSocket.ReceiveFrom(zipCode, ref remoteEndPoint);

                    ZipCode = BitConverter.ToInt32(zipCode);
                    Console.WriteLine($"{DateTime.Now.ToShortTimeString()} :: {ZipCode} from {remoteEndPoint}");

                    Console.WriteLine("Search cities: ");
                    SearchCities();

                    Console.WriteLine($"Searched {cities.Count()} cities. Sends to client: ");

                    if (cities.Count == 0)
                    {
                        const string NOT_FOUND = "!<not found>";
                        byte[] message = Encoding.Unicode.GetBytes(NOT_FOUND);
                        listenSocket.SendTo(message, remoteEndPoint);
                    }
                    else
                    {
                        foreach (var city in cities)
                        {
                            listenSocket.SendTo(city.ToBytesArray(), remoteEndPoint);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                listenSocket.Close();
            }
        }

        public void SearchCities()
        {
            if (cities.Count() != 0)
            {
                cities.Clear();
            }
            cities = db.Cities.Where(c => c.ZipCode == ZipCode).ToList();
        }

    }
}
