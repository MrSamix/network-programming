using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    public class Connection
    {
        static string address = "127.0.0.1";
        static int port = 4140;
        const string NOT_FOUND = "!<not found>";

        IPEndPoint endPoint;
        EndPoint remoteEndPoint;
        Socket socket;
        public List<City> Cities { get; set; }
        public Connection()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(address), port);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Cities = new List<City>();
        }

        public void SentZipCode(int zipCode)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            byte[] data = BitConverter.GetBytes(zipCode);
            socket.SendTo(data, endPoint);
        }


        public void GetCities()
        {            
            List<byte> bytes = new List<byte>();

            do
            {
                byte[] data = new byte[1024]; // 1 KB
                socket.ReceiveFrom(data, ref remoteEndPoint);
                string message = Encoding.Unicode.GetString(data);
                if (message.Contains(NOT_FOUND))
                {
                    return;
                }
                City city = new City(data);
                Cities.Add(city);
            } while (socket.Available > 0);

            socket.Close();
        }

        public Task GetCitiesAsync()
        {
            return Task.Run(() =>
            {
                List<byte> bytes = new List<byte>();

                do
                {
                    byte[] data = new byte[1024]; // 1 KB
                    socket.ReceiveFrom(data, ref remoteEndPoint);
                    string message = Encoding.Unicode.GetString(data);
                    if (message.Contains(NOT_FOUND))
                    {
                        return;
                    }
                    City city = new City(data);
                    Cities.Add(city);
                } while (socket.Available > 0);

                socket.Close();
            });
        }

    }
}
