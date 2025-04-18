using Microsoft.VisualBasic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;


public class ChatServer
{
    const int port = 4040;
    const string JOIN_CMD = "$<join>";
    const string LEAVE_CMD = "$<leave>";
    UdpClient server;
    IPEndPoint client = null;
    Dictionary<IPEndPoint, string> members;
    public ChatServer()
    {
        server = new UdpClient(port);
        members = new Dictionary<IPEndPoint, string>();
    }
    private void AddMember(IPEndPoint member, string username)
    {
        if (!members.Keys.Contains(member))
        {
            members.Add(member, username);
            //members.Add(member);
        }
        Console.WriteLine($"Members {username} was added ---- members: {members.Count}");
    }
    private void RemoveMember(IPEndPoint member)
    {
        if (members.Keys.Contains(member))
        {
            members.Remove(member);
            Console.WriteLine($"Member {member} was removed");
        }
    }

    private async void SendAllMembers(string message, string username)
    {
        string res = $"{message};;{username}";
        byte[] data = Encoding.Unicode.GetBytes(res);
        foreach (var item in members.Keys)
        {
            await server.SendAsync(data, data.Length, item);
        }
    }
    public void Start()
    {
        while (true)
        {
            byte[] data = server.Receive(ref client);
            string message = Encoding.Unicode.GetString(data);
            var cmd_username = message.Split(';');
            switch (cmd_username[0])
            {
                case JOIN_CMD:
                    AddMember(client, cmd_username[1]);
                    break;
                case LEAVE_CMD:
                    RemoveMember(client);
                    break;
                default:
                    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} -- {message} -- from {client}");
                    SendAllMembers(cmd_username[0], members[client]);
                    break;

            }
        }
    }

}


internal class Program 
{
    private static void Main(string[] args)
    {
        ChatServer chat = new ChatServer();
        chat.Start();
    }
}