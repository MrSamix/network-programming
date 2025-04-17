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

namespace Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public int ZipCode { get; set; }
    Connection conn;
    public MainWindow()
    {
        InitializeComponent();
        conn = new Connection();
    }

    private async void SearchButton(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbzipcode.Text))
        {
            MessageBox.Show("Enter a zip code!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        try
        {
            conn.Cities.Clear();
            dg.ItemsSource = null;
            ZipCode = int.Parse(tbzipcode.Text);
            tbzipcode.Text = "";
            conn.SentZipCode(ZipCode);
            await conn.GetCitiesAsync();
            if (conn.Cities.Count() == 0)
            {
                MessageBox.Show("Cities not found!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            dg.ItemsSource = conn.Cities;
        }
        catch (Exception)
        {
            MessageBox.Show("Enter a zip code!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }

    private async void tbzipcode_KeyUp(object sender, KeyEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbzipcode.Text))
        {
            return;
        }
        try
        {
            conn.Cities.Clear();
            dg.ItemsSource = null;
            ZipCode = int.Parse(tbzipcode.Text);
            conn.SentZipCode(ZipCode);
            await conn.GetCitiesAsync();
            if (conn.Cities.Count() == 0)
            {
                return;
            }
            dg.ItemsSource = conn.Cities;
        }
        catch (Exception)
        {
            return;
        }
    }
}