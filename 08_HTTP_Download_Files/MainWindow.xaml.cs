using Microsoft.WindowsAPICodePack.Dialogs;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;

namespace Self_Task_HTTP_Download_Files;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    ObservableCollection<CopyProcessInfo> files = new ObservableCollection<CopyProcessInfo>();
    public MainWindow()
    {
        InitializeComponent();
        lbFiles.ItemsSource = files;
    }

    private void SourceBtn(object sender, RoutedEventArgs e)
    {
        CommonOpenFileDialog dialog = new CommonOpenFileDialog();
        dialog.IsFolderPicker = true; // only folder
        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            tbPath.Text = dialog.FileName;
        }
    }

    private async void DownloadBtn(object sender, RoutedEventArgs e)
    {
        if (tbPath == null || tbURL == null)
        {
            MessageBox.Show("Enter a filepath and url", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        var file = new CopyProcessInfo(Path.GetFileName(tbURL.Text), tbURL.Text, tbPath.Text, cancellationTokenSource);
        files.Add(file);
        await DownloadFile(file);
    }

    public async Task DownloadFile(CopyProcessInfo file)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file.Path));

            using (var client = new HttpClient())
            {

                // Open the file stream for writing
                using (var response = await client.GetAsync(file.URL, HttpCompletionOption.ResponseHeadersRead, file.Token))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 0L;
                    var buffer = new byte[8192];
                    var bytesRead = 0L;

                    using (var fileStream = new FileStream(Path.Combine(file.Path, file.FileName), FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, true))
                    using (var contentStream = await response.Content.ReadAsStreamAsync(file.Token))
                    {
                        int read;
                        while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length, file.Token)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, read, file.Token);
                            bytesRead += read;

                            // Progress
                            file.Percentage = (int)((bytesRead * 100) / totalBytes);
                            file.Progress = file.Percentage + "%";
                        }
                    }
                }

                file.Progress = "Completed";
                file.IsCompleted = true;
            }
        }
        catch (OperationCanceledException)
        {
            file.Progress = "Canceled";
            file.IsCanceled = true;
        }
        catch (Exception ex)
        {
            file.Progress = "Error: " + ex.Message;
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class CopyProcessInfo
    {
        public string FileName { get; set; }
        public string URL { get; set; }
        public string Path { get; set; }
        public int Percentage { get; set; }
        public string Progress { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public CancellationToken Token { get; set; }
        public bool IsCanceled { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
        public CopyProcessInfo(string filename, string url, string path, CancellationTokenSource cancellationTokenSource)
        {
            FileName = filename;
            URL = url;
            Path = path;
            Progress = "0%";
            CancellationTokenSource = cancellationTokenSource;
            Token = CancellationTokenSource.Token;
        }
        public void Cancel()
        {
            try
            {
                if (IsCompleted == false)
                {
                    CancellationTokenSource.Cancel();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }

    private void lbFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (files[lbFiles.SelectedIndex].Percentage < 100)
        {
            MessageBox.Show("File don't downloaded fully", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        if (files[lbFiles.SelectedIndex].IsCanceled == true)
        {
            MessageBox.Show("Download was canceled!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer",
            Arguments = Path.Combine(files[lbFiles.SelectedIndex].Path, files[lbFiles.SelectedIndex].FileName),
            UseShellExecute = true
        });
    }

    private void CancelBtn(object sender, RoutedEventArgs e)
    {
        var selected_elem = ((ListBoxItem)lbFiles.ContainerFromElement((Button)sender)).Content;
        Button btn = (Button)sender;
        btn.IsEnabled = false;
        CopyProcessInfo file = (CopyProcessInfo)selected_elem;
        if (file.IsCompleted == false)
        {
            file.Cancel();
        }
        else
        {
            MessageBox.Show("File already downloaded", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}