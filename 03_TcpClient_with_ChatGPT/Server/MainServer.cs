using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class MainServer
    {
        TcpListener server;
        const string IP = "127.0.0.1";
        const int PORT = 4040;
        public MainServer()
        {
            server = new TcpListener(IPAddress.Parse(IP),PORT);
        }
        public void Start()
        {
            server.Start();
            Console.WriteLine("Server started! Waiting connections");
            while (true)
            {
                var client = server.AcceptTcpClient();
                Console.WriteLine("Client Connected");
                NetworkStream ns = client.GetStream();
                StreamWriter sw = new StreamWriter(ns);
                StreamReader sr = new StreamReader(ns);
                try
                {
                    string? message = sr.ReadLine();
                    if (message != null)
                    {
                        var category = MLModel1.Predict(new MLModel1.ModelInput()
                        {
                            Слово = message
                        });
                        string[] words = File.ReadAllLines("../../../ukrainian_words.csv", Encoding.UTF8);
                        var wordsSortedByCategories = words.Select(l => l.Split(',')).GroupBy(c => c[1]).Skip(1).ToFrozenDictionary(c => c.Key, c => c.Select(c => c[0]).Distinct().ToList());
                        if (!wordsSortedByCategories.ContainsKey(category.PredictedLabel))
                        {
                            string answer = "Вибач, я не можу дати відповідь :(";
                            sw.WriteLine(answer);
                            sw.Flush();
                        }
                        else
                        {
                            Random rnd = new Random();
                            var result = wordsSortedByCategories[category.PredictedLabel][rnd.Next(wordsSortedByCategories[category.PredictedLabel].Count)];

                            sw.WriteLine(result);
                            sw.Flush();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
