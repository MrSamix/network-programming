using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Message;

namespace Server
{
    public class ChatServer
    {
        const string ADDRESS = "127.0.0.1";
        const int PORT = 4040;
        TcpListener listener;
        List<TcpClient> clients;
        public ChatServer()
        {
            listener = new TcpListener(IPAddress.Parse(ADDRESS), PORT);
            clients = new List<TcpClient>();
        }
        public void Start()
        {
            try
            {
                listener.Start();
                Console.WriteLine($"Server started on {ADDRESS}:{PORT}");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Client connected");
                    Task.Run(() => AddClient(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                listener.Stop();
                Console.WriteLine("Server stopped");
            }
        }

        private void AddClient(TcpClient client)
        {
            clients.Add(client);
            Console.WriteLine($"Client added. Total clients: {clients.Count}");
            Listener(client);
        }

        private async void Listener(TcpClient client)
        {
            while (true)
            {
                var ns = client.GetStream();
                StreamWriter sw = new StreamWriter(ns);
                StreamReader sr = new StreamReader(ns);

                string json = await sr.ReadLineAsync();
                if (json != null)
                {
                    if (json == "$<leave>")
                    {
                        ns.Close();
                        sw.Close();
                        sr.Close();
                        clients.Remove(client);
                        Console.WriteLine($"Client disconected. Total clients: {clients.Count}");
                        return;
                    }
                    foreach (var item in clients)
                    {
                        if (item == client)
                            continue;
                        else
                        {
                            ns = item.GetStream();
                            sw = new StreamWriter(ns);
                            sw.WriteLine(json);
                            sw.Flush();
                        }
                    }
                }

            }
        }
    }
}
