using System.IO;
using System.Net.Sockets;
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

namespace Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    TcpClient client;
    public string ServerIP { get; set; }
    public int Port { get; set; }
    NetworkStream ns;
    StreamWriter sw;
    StreamReader sr;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SendBtn(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbip.Text) || string.IsNullOrWhiteSpace(tbport.Text))
        {
            MessageBox.Show("Enter server ip and port", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        if (string.IsNullOrWhiteSpace(tbmessage.Text))
        {
            MessageBox.Show("Enter a message!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        try
        {
            client = new TcpClient();
            ServerIP = tbip.Text;
            Port = int.Parse(tbport.Text);
            client.Connect(ServerIP, Port);
            ns = client.GetStream();
            sw = new StreamWriter(ns);
            sr = new StreamReader(ns);

            sw.WriteLine(tbmessage.Text);
            sw.Flush();
            lbmessage.Items.Add(new MessageInfo(tbmessage.Text, "", DateTime.Now));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        Listener();
    }

    private async void Listener()
    {
        try
        {
            while (true)
            {
                string? answer = await sr.ReadLineAsync();

                if (answer != null)
                {
                    lbmessage.Items.Add(new MessageInfo("", answer, DateTime.Now));
                    ns.Close();
                    client.Close();
                }

            }
        }
        catch (Exception)
        {
            return;
        }
        
    }

}

public class MessageInfo
{
    public string? Message { get; set; }
    public string? Answer { get; set; }
    DateTime time;
    public string Time => time.ToShortTimeString();

    public MessageInfo(string? message, string? answer, DateTime time)
    {
        Message = message;
        Answer = answer;
        this.time = time;
    }
}