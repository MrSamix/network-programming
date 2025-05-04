using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Message;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

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
        TcpClient client;
        NetworkStream ns;
        StreamWriter sw;
        StreamReader sr;
        ObservableCollection<MessageInfo> messages;
        bool isJoined = false;
        InputBox inputBox;
        public MainWindow()
        {
            InitializeComponent();
            messages = new ObservableCollection<MessageInfo>();
            server = new IPEndPoint(IPAddress.Parse(serverAddress), port);
            inputBox = new InputBox();
            this.DataContext = messages;
            tbStatus.Text = "Disconnected";
            tbStatus.Foreground = Brushes.Red;
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
            try
            {
                client = new TcpClient();
                client.Connect(server);
                ns = client.GetStream();
                sw = new StreamWriter(ns);
                sr = new StreamReader(ns);
                isJoined = true;
                tbStatus.Text = "Connected";
                tbStatus.Foreground = Brushes.Green;
                Listener();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}", "Connection Error", MessageBoxButton.OK);
            }
            
        }

        private async void SendMessage(string message)
        {
            if (isJoined)
            {
                MessageInfo msg = new MessageInfo("", inputBox.UserName, message, "", DateTime.Now);
                string json = JsonConvert.SerializeObject(msg);
                sw.WriteLine(json);
                sw.Flush();
                messages.Add(msg);
            }
        }
        private async void Listener()
        {
            while (isJoined)
            {
                try
                {
                    string json = await sr.ReadLineAsync();
                    if (json != null)
                    {
                        if (json == "Max clients reached.")
                        {
                            tbStatus.Text = "Disconnected";
                            tbStatus.Foreground = Brushes.Red;
                            Application.Current.Dispatcher.Invoke(() => MessageBox.Show(json, "Max Clients", MessageBoxButton.OK, MessageBoxImage.Error));
                            isJoined = false;
                            break;
                        }
                        MessageInfo message = JsonConvert.DeserializeObject<MessageInfo>(json);
                        if (message != null)
                        {
                            message.Answer = message.Message;
                            message.Message = "";
                            message.Username = message.MyUsername;
                            message.MyUsername = "";
                            messages.Add(message);
                        }
                    }
                }
                catch (System.IO.IOException)
                {
                    tbStatus.Text = "Disconnected";
                    tbStatus.Foreground = Brushes.Red;
                    isJoined = false;
                    return;
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => MessageBox.Show($"Error: {ex.Message}"));
                    return;
                }
            }
        }

        private void LeaveBtn(object sender, RoutedEventArgs e)
        {
            if (isJoined)
            {
                sw.WriteLine("$<leave>");
                sw.Flush();
                isJoined = false;
                tbStatus.Text = "Disconnected";
                tbStatus.Foreground = Brushes.Red;
                ns.Close();
                client.Close();
                sw.Close();
                sr.Close();
            }
        }

        private void ClearBtn(object sender, RoutedEventArgs e)
        {
            messages.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            inputBox.Close(); // бо програма залишається активною в фоні
            if (isJoined)
            {
                LeaveBtn(sender, new RoutedEventArgs());
            }
        }
    }


    
}