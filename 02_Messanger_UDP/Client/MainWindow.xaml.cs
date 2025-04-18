using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string serverAddress = "127.0.0.1";
        const int port = 4040;
        IPEndPoint server;
        UdpClient client;
        ObservableCollection<MessageInfo> messages;
        bool isJoined = false;
        InputBox inputBox;
        public MainWindow()
        {
            InitializeComponent();
            messages = new ObservableCollection<MessageInfo>();
            server = new IPEndPoint(IPAddress.Parse(serverAddress), port);
            client = new UdpClient();
            inputBox = new InputBox();
            this.DataContext = messages;
        }

        private void SendBtn(object sender, RoutedEventArgs e)
        {
            if (!isJoined)
            {
                MessageBox.Show("First you must be logged!", "Not logged", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string message = msgTextBox.Text;
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            msgTextBox.Text = "";
            SendMessage(message);
        }

        private void msgTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendBtn(sender, e);
            }
        }

        private void JoinBtn(object sender, RoutedEventArgs e)
        {
            if (isJoined)
            {
                return;
            }
            inputBox.ShowDialog();
            if (string.IsNullOrWhiteSpace(inputBox.UserName))
            {
                return;
            }
            SendMessage($"$<join>;{inputBox.UserName}");
            isJoined = true;
            Listener();
        }

        private async void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            await client.SendAsync(data, data.Length, server);
        }
        private async void Listener()
        {
            while (isJoined)
            {
                var data = await client.ReceiveAsync();
                string message = Encoding.Unicode.GetString(data.Buffer);
                var res = message.Split(";;");
                messages.Add(new MessageInfo(res[1], res[0]));
            }
        }

        private void LeaveBtn(object sender, RoutedEventArgs e)
        {
            SendMessage($"$<leave>;{inputBox.UserName}");
            isJoined = false;
        }

        private void ClearBtn(object sender, RoutedEventArgs e)
        {
            messages.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            inputBox.Close(); // бо програма залишається активною в фоні
        }
    }


    public class MessageInfo
    {
        public string Username { get; set; }
        public string Message { get; set; }
        private DateTime time;
        public string Time => time.ToLongTimeString();
        public MessageInfo(string username, string message)
        {
            Username = username;
            Message = message;
            time = DateTime.Now;
        }
        public override string ToString()
        {
            return $"{Username, -20} {Message, -20} {Time}";
        }
    }
}
