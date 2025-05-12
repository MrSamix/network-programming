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
using System.Windows.Threading;
//using System.Windows.Shapes;

namespace Self_Task_HTTP_Download_Files;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    ObservableCollection<CopyProcessInfo> files = new ObservableCollection<CopyProcessInfo>();
    List<string> tags;
    int ProcessorCount = Environment.ProcessorCount;
    List<Task> tasks;
    string[] status = ["All", "Downloading", "Paused", "Completed", "Canceled", "Error"];
    public MainWindow()
    {
        InitializeComponent();
        lbFiles.ItemsSource = files;
        numberBox.Maximum = ProcessorCount;
        tasks = new List<Task>();
        tags = Enum.GetNames(typeof(Tag)).ToList();
        cbTags.ItemsSource = tags;
        cbFilter.ItemsSource = tags;
        statusFiles.ItemsSource = status;
        statusFiles.SelectedIndex = 0;

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
        if (string.IsNullOrWhiteSpace(tbPath.Text) || string.IsNullOrWhiteSpace(tbURL.Text))
        {
            MessageBox.Show("Enter a filepath and url", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        Tag tag;
        if (string.IsNullOrWhiteSpace(cbTags.Text))
        {
            tag = Self_Task_HTTP_Download_Files.Tag.None;
        }
        else
        {
            tag = (Tag)Enum.Parse(typeof(Tag), cbTags.Text);
        }

        if (numberBox.Value > ProcessorCount)
        {
            MessageBox.Show("Number of threads can't be more than " + ProcessorCount, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        if (tasks.Count == numberBox.Value)
        {
            MessageBox.Show("Number of threads is full", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var file = new CopyProcessInfo(Path.GetFileName(tbURL.Text), tbURL.Text, tbPath.Text, tag, cancellationTokenSource);
        if (files.Where(f => f.FileName==file.FileName && (f.IsPaused)).Count() > 0)
        {
            MessageBox.Show("This file already paused. First cancel or resume this file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        else if (files.Where(f => f.FileName == file.FileName && (!f.IsCompleted && !f.IsCanceled)).Count() > 0)
        {
            MessageBox.Show("This file already downloading.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        files.Add(file);
        var task = DownloadFile(file);
        tasks.Add(task);
        statusFiles_SelectionChanged(null, null);
        await task;
        tasks.Remove(task);
    }

    public async Task DownloadFile(CopyProcessInfo file)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file.Path));

            using (var client = new HttpClient())
            {
                // Set Range header if resuming
                if (file.PausedAt > 0)
                {
                    client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(file.PausedAt, null);
                }

                using (var response = await client.GetAsync(file.URL, HttpCompletionOption.ResponseHeadersRead, file.Token))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 0L;
                    if (file.PausedAt > 0 && response.Content.Headers.ContentRange != null)
                    {
                        totalBytes += file.PausedAt;
                    }

                    var buffer = new byte[8192];
                    var bytesRead = file.PausedAt;

                    // Open file in append mode if resuming
                    using (var fileStream = new FileStream(Path.Combine(file.Path, file.FileName), file.PausedAt > 0 ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, true))
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
                            file.PausedAt = bytesRead;
                        }
                    }
                }

                file.Progress = "Completed";
                file.IsCompleted = true;
                statusFiles_SelectionChanged(null, null);
            }
        }
        catch (OperationCanceledException)
        {
            if (file.IsCanceled)
            {
                file.Progress = "Canceled";
                statusFiles_SelectionChanged(null, null);
            }
            else
            {
                file.Progress = "Paused";
                statusFiles_SelectionChanged(null, null);
                file.TextBtnStr = "Resume";
            }            
        }
        catch (Exception ex)
        {
            file.Progress = "Error: " + ex.Message;
            file.IsCanceled = true;
            statusFiles_SelectionChanged(null, null);
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
        public bool IsPaused { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
        public bool IsActionEnabled => !IsCompleted && !IsCanceled;
        public Tag Tag { get; set; }
        public string TagStr => Tag.ToString();
        public string TextBtnStr { get; set; }
        public long PausedAt { get; set; } = 0;
        public CopyProcessInfo(string filename, string url, string path, Tag tag, CancellationTokenSource cancellationTokenSource)
        {
            FileName = filename;
            URL = url;
            Path = path;
            Progress = "0%";
            CancellationTokenSource = cancellationTokenSource;
            Token = CancellationTokenSource.Token;
            Tag = tag;
            TextBtnStr = "Pause";
        }
        public void Cancel()
        {
            try
            {
                CancellationTokenSource.Cancel();
                IsCanceled = true;
                if (IsPaused)
                {
                    IsPaused = false;
                    Progress = "Canceled";
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
        public void Pause()
        {
            try
            {
                CancellationTokenSource.Cancel();
                IsPaused = true;
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
        try
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
        catch (Exception)
        {
        }
        
    }

    private void CancelBtn(object sender, RoutedEventArgs e)
    {
        var selected_elem = ((ListBoxItem)lbFiles.ContainerFromElement((Button)sender)).Content;
        Button btn = (Button)sender;
        btn.IsEnabled = false;
        CopyProcessInfo file = (CopyProcessInfo)selected_elem;
        file.Cancel();
    }

    private void ActionBtn(object sender, RoutedEventArgs e)
    {
        var selected_elem = ((ListBoxItem)lbFiles.ContainerFromElement((Button)sender)).Content;
        Button btn = (Button)sender;
        CopyProcessInfo file = (CopyProcessInfo)selected_elem;
        if (btn.Content.ToString() == "Pause")
        {
            file.IsPaused = true;
            file.Pause(); // raise OperationCanceledException in DownloadFile
            file.TextBtnStr = "Resume";
        }
        else if (btn.Content.ToString() == "Resume")
        {
            // Create a new CancellationTokenSource for resuming
            file.CancellationTokenSource = new CancellationTokenSource();
            file.Token = file.CancellationTokenSource.Token;
            file.IsPaused = false;
            file.TextBtnStr = "Pause";
            // resume download
            var task = DownloadFile(file);
            tasks.Add(task);
            task.ContinueWith(_ => tasks.Remove(task));
        }
    }

    private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        statusFiles_SelectionChanged(null, null);
    }

    private void ClearBtn(object sender, RoutedEventArgs e)
    {
        btnClear.IsEnabled = false;
        cbFilter.SelectedItem = null;
        statusFiles_SelectionChanged(null, null);
    }

    private void statusFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        IEnumerable<CopyProcessInfo> filtred = Enumerable.Empty<CopyProcessInfo>();

        if ((string)statusFiles.SelectedItem == "All")
        {
            filtred = files;
        }
        else if ((string)statusFiles.SelectedItem == "Downloading")
        {
            lbFiles.ItemsSource = null;
            filtred = files.Where(f => !f.IsCanceled && !f.IsPaused && !f.IsCompleted);
        }
        else if ((string)statusFiles.SelectedItem == "Paused")
        {
            lbFiles.ItemsSource = null;
            filtred = files.Where(f => f.IsPaused);
        }
        else if ((string)statusFiles.SelectedItem == "Completed")
        {
            lbFiles.ItemsSource = null;
            filtred = files.Where(f => f.IsCompleted);
        }
        else if ((string)statusFiles.SelectedItem == "Canceled")
        {
            lbFiles.ItemsSource = null;
            filtred = files.Where(f => f.IsCanceled);
        }
        else
        {
            lbFiles.ItemsSource = null;
            filtred = files.Where(f => f.Progress.Contains((string)statusFiles.SelectedItem));
        }

        if (cbFilter.SelectedItem != null)
        {
            var selectedTag = (Tag)Enum.Parse(typeof(Tag), cbFilter.SelectedItem.ToString());
            lbFiles.ItemsSource = filtred.Where(f => f.Tag == selectedTag).ToList();
            btnClear.IsEnabled = true;
        }
        else
        {
            lbFiles.ItemsSource = filtred;
            btnClear.IsEnabled = false;
        }
    }

    private void MoveFileBtn(object sender, RoutedEventArgs e)
    {
        try
        {
            CopyProcessInfo file = lbFiles.SelectedItem as CopyProcessInfo;
            if (file != null)
            {
                if (file.IsCompleted == false)
                {
                    MessageBox.Show("File is not downloaded", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true; // only folder
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string moveTo = dialog.FileName;
                    string newFilePath = Path.Combine(moveTo, file.FileName);
                    if (File.Exists(newFilePath))
                    {
                        MessageBox.Show("File with this name already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    File.Move(Path.Combine(file.Path, file.FileName), newFilePath);
                    file.Path = moveTo;
                    MessageBox.Show("File moved successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error moving file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RenameFileBtn(object sender, RoutedEventArgs e)
    {
        try
        {
            CopyProcessInfo file = lbFiles.SelectedItem as CopyProcessInfo;
            if (file != null)
            {
                if (file.IsCompleted == false)
                {
                    MessageBox.Show("File is not downloaded", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Rename dialog = new Rename();
                dialog.ShowDialog();
                if (dialog.IsCanceled == false)
                {
                    if (File.Exists(Path.Combine(file.Path, dialog.NewFileName)))
                    {
                        MessageBox.Show("File with this name already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    File.Move(Path.Combine(file.Path, file.FileName), Path.Combine(file.Path, dialog.NewFileName + Path.GetExtension(file.FileName)));
                    file.FileName = dialog.NewFileName + Path.GetExtension(file.FileName);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
    }

    private void OpenFileBtn(object sender, RoutedEventArgs e)
    {
        lbFiles_MouseDoubleClick(null, null);
    }

    private void OpenFolderBtn(object sender, RoutedEventArgs e)
    {
        CopyProcessInfo file = lbFiles.SelectedItem as CopyProcessInfo;
        if (file != null)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = file.Path,
                UseShellExecute = true
            });
        }
    }
    private void RemoveBtn(object sender, RoutedEventArgs e)
    {
        CopyProcessInfo file = lbFiles.SelectedItem as CopyProcessInfo;
        if (file != null)
        {
            if (file.IsCompleted == false && file.IsCanceled == false)
            {
                MessageBox.Show("File is not downloaded. First, cancel downloading", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var dialog = MessageBox.Show("Are you sure you want to delete this download?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (dialog == MessageBoxResult.No)
            {
                return;
            }
            files.Remove(file);
        }
    }

    private void RemoveFileBtn(object sender, RoutedEventArgs e)
    {
        try
        {
            CopyProcessInfo file = lbFiles.SelectedItem as CopyProcessInfo;
            if (file != null)
            {
                if (file.IsCompleted == false && file.IsCanceled == false)
                {
                    MessageBox.Show("File is not downloaded. First, cancel downloading", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var dialog = MessageBox.Show("Are you sure you want to delete this file?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (dialog == MessageBoxResult.No)
                {
                    return;
                }
                files.Remove(file);
                File.Delete(Path.Combine(file.Path, file.FileName));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
    }


    private void lbFiles_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (lbFiles.SelectedIndex == -1)
        {
            e.Handled = true;
        }
    }
}