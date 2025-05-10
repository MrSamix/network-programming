using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using Org.BouncyCastle.Asn1.X509;
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

namespace Self_Task_SMTP_IMAP;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    const string IMAP_SERVER = "imap.gmail.com";
    const int IMAP_PORT = 993;
    IMailFolder CurrentFolder;
    ImapClient client;
    string login;
    string password;
    CancellationTokenSource cancelTokenSource;
    CancellationToken token;
    List<MimeMessage> messages;
    private IList<IMailFolder> folders;
    bool sortedByOldest = true;
    private List<MimeMessage> filteredMessages;

    public bool IsSearched { get; set; } = false;

    bool? isImportant;
    DateTime? from;
    DateTime? to;
    string? author;

    public MainWindow()
    {
        InitializeComponent();
        cbProvider.Items.Add("Gmail");
        client = new ImapClient();
        messages = new List<MimeMessage>();
        cancelTokenSource = new CancellationTokenSource();
        token = cancelTokenSource.Token;
    }

    private async void LoadBtn(object sender, RoutedEventArgs e)
    {
        if (cbProvider.Text == "")
        {
            MessageBox.Show("Choose email provider!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            btnsendMessage.IsEnabled = false;
            return;
        }
        if (tbLogin.Text == "" || tbPassword.Text == "")
        {
            MessageBox.Show("Enter login and password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            btnsendMessage.IsEnabled = false;
            return;
        }
        login = tbLogin.Text;
        password = tbPassword.Text;
        btnsendMessage.IsEnabled = true;
        lbFolders.Items.Clear();
        if (client.IsConnected)
        {
            client.Disconnect(true);
        }
        await LoadFoldersAsync();
    }
    private async void lbFolders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        sortedByOldest = true;
        // Cancel current task if it is running
        cancelTokenSource.Cancel();
        cancelTokenSource = new CancellationTokenSource();
        token = cancelTokenSource.Token;

        int selectedItem = lbFolders.SelectedIndex;

        try
        {
            Thread.Sleep(500);
            // Load message with new token
            await LoadMessagesAsync(selectedItem, token);
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private Task LoadMessagesAsync(int selectedItem, CancellationToken token)
    {
        return Task.Run(() =>
        {
            token.ThrowIfCancellationRequested();

            Application.Current.Dispatcher.Invoke(() => lbMessages.Items.Clear());
            messages.Clear();

            try
            {
                client.GetFolders(client.PersonalNamespaces[0])[selectedItem].Open(FolderAccess.ReadOnly);
                CurrentFolder = client.GetFolders(client.PersonalNamespaces[0])[selectedItem];
                foreach (var messageid in CurrentFolder.Search(SearchQuery.All))
                {
                    token.ThrowIfCancellationRequested();

                    var message = CurrentFolder.GetMessageAsync(messageid);
                    messages.Add(message.Result);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        lbMessages.Items.Add(string.IsNullOrWhiteSpace(message.Result.Subject) ? "(empty subject)" : message.Result.Subject);
                    });
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Error with opening folder: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }, token);
    }

    private void lbMessages_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        int selectedMessage = lbMessages.SelectedIndex;
        var message = messages[selectedMessage];
        Message dialog = new Message(message, login, password);
        Task.Run(() => {
            Application.Current.Dispatcher.Invoke(() =>
            {
                dialog.Show();
            });
        });
        
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        cancelTokenSource.Cancel();
        if (client.IsConnected)
        {
            client.Disconnect(true);
        }
    }

    public Task LoadFoldersAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                client.Connect(IMAP_SERVER, IMAP_PORT, SecureSocketOptions.SslOnConnect);
                client.Authenticate(login, password);

                Application.Current.Dispatcher.Invoke(() => lbFolders.Items.Clear);
                folders = client.GetFolders(client.PersonalNamespaces[0]);
                foreach (var item in folders)
                {
                    Application.Current.Dispatcher.Invoke(() => lbFolders.Items.Add(item.Name));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
        
    }

    private void SendMessageBtn(object sender, RoutedEventArgs e)
    {
        SendMessage sendMessage = new SendMessage(login, password);
        Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() => sendMessage.Show());
        });
    }

    private void CreateFolder_Click(object sender, RoutedEventArgs e)
    {
        if (client == null || client.IsConnected == false)
        {
            MessageBox.Show("Connect to your email first!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        CreateFolder createFolder = new CreateFolder(folders);
        Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                createFolder.ShowDialog();
                var foldername = createFolder.FolderName;
                if (foldername == null)
                {
                    return;
                }
                var topLevel = folders[0].ParentFolder;
                try
                {
                    var folder = topLevel.Create(foldername, true);
                    if (folder != null)
                    {
                        MessageBox.Show("Folder created successfully!", "Success", MessageBoxButton.OK,MessageBoxImage.Information);
                        folders = client.GetFolders(client.PersonalNamespaces[0]);
                        lbFolders.Items.Add(folder.Name);
                    }
                    else
                    {
                        MessageBox.Show("Failed to create folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

        });
        

    }

    private void RenameFolder_Click(object sender, RoutedEventArgs e)
    {
        if (client == null || client.IsConnected == false)
        {
            MessageBox.Show("Connect to your email first!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        RenameFolder renameFolder = new RenameFolder(folders);
        Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() => 
            { 
                renameFolder.ShowDialog();
                var foldername = renameFolder.FolderName;
                if (foldername == null)
                {
                    return;
                }
                try
                {
                    var selectedFolder = folders[lbFolders.SelectedIndex];
                    selectedFolder.Rename(selectedFolder.ParentFolder,foldername);
                    MessageBox.Show("Folder renamed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    lbFolders.Items[lbFolders.SelectedIndex] = foldername;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        });
    }

    private void DeleteMessage_Click(object sender, RoutedEventArgs e)
    {
        int selectedMessage = lbMessages.SelectedIndex;
        if (selectedMessage == -1)
        {
            return;
        }
        var res = MessageBox.Show("Are you sure you want to delete this message?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (res == MessageBoxResult.No)
        {
            return;
        }
        var message = messages[selectedMessage];
        try
        {
            var id = CurrentFolder.Search(MailKit.Search.SearchQuery.All)[selectedMessage];
            CurrentFolder.Open(FolderAccess.ReadWrite);
            CurrentFolder.AddFlags(id, MessageFlags.Deleted, true);
            CurrentFolder.Expunge();
            lbMessages.Items.RemoveAt(selectedMessage);
            MessageBox.Show("Message deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MoveMessage_Click(object sender, RoutedEventArgs e)
    {
        MoveMessage dialog = new MoveMessage(folders, CurrentFolder);
        dialog.ShowDialog();
        if (dialog.IsCanceled)
        {
            return;
        }
        var selectedFolder = dialog.SelectedFolder;
        var selectedMessage = lbMessages.SelectedIndex;
        var message = messages[selectedMessage];
        try
        {
            var id = CurrentFolder.Search(MailKit.Search.SearchQuery.All)[selectedMessage];
            CurrentFolder.Open(FolderAccess.ReadWrite);
            CurrentFolder.MoveTo(id, selectedFolder);
            lbMessages.Items.RemoveAt(selectedMessage);
            MessageBox.Show("Message moved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SortBtn(object sender, RoutedEventArgs e)
    {
        Sort sort = new Sort(sortedByOldest);
        sort.ShowDialog();
        if (sort.IsCanceled)
        {
            return;
        }
        sortedByOldest = sort.IsOld;
        if (!sortedByOldest)
        {
            messages = messages.OrderByDescending(m => m.Date).ToList();
            lbMessages.Items.Clear();
            foreach (var message in messages)
            {
                lbMessages.Items.Add(string.IsNullOrWhiteSpace(message.Subject) ? "(empty subject)" : message.Subject);
            }
        }
        else
        {
            messages = messages.OrderBy(m => m.Date).ToList();
            lbMessages.Items.Clear();
            foreach (var message in messages)
            {
                lbMessages.Items.Add(string.IsNullOrWhiteSpace(message.Subject) ? "(empty subject)" : message.Subject);
            }
        }
    }

    private void FilterBtn(object sender, RoutedEventArgs e)
    {
        Filter filter = new Filter(isImportant, from, to, author);
        filter.ShowDialog();
        if (filter.IsCanceled)
        {
            return;
        }
        isImportant = filter.IsImportant;
        from = filter.from;
        to = filter.to;
        author = filter.Author;
        filteredMessages = messages.Where(m =>
            (isImportant == null || m.Importance == (isImportant == true ? MessageImportance.High : MessageImportance.Normal)) &&
            (from == null || m.Date >= from) &&
            (to == null || m.Date <= to) &&
            (author == null || m.From.Mailboxes.Any(a => a.Address.Contains(author)))
        ).ToList();
        lbMessages.Items.Clear();
        foreach (var message in filteredMessages)
        {
            lbMessages.Items.Add(string.IsNullOrWhiteSpace(message.Subject) ? "(empty subject)" : message.Subject);
        }
    }

    private void SearchBtn(object sender, RoutedEventArgs e)
    {
        if (IsSearched == true && string.IsNullOrWhiteSpace(tbSearch.Text))
        {
            if (filteredMessages == null)
            {
                lbMessages.Items.Clear();
                foreach (var message in messages)
                {
                    lbMessages.Items.Add(string.IsNullOrWhiteSpace(message.Subject) ? "(empty subject)" : message.Subject);
                }
                IsSearched = false;
            }
            else
            {
                lbMessages.Items.Clear();
                foreach (var message in filteredMessages)
                {
                    lbMessages.Items.Add(string.IsNullOrWhiteSpace(message.Subject) ? "(empty subject)" : message.Subject);
                }
                IsSearched = false;
            }
        }
        else
        {
            var searchText = tbSearch.Text.ToLower();
            lbMessages.Items.Clear();
            if (filteredMessages == null)
            {
                foreach (var message in messages)
                {
                    if (message.Subject.ToLower().Contains(searchText))
                    {
                        lbMessages.Items.Add(string.IsNullOrWhiteSpace(message.Subject) ? "(empty subject)" : message.Subject);
                    }
                }
            }
            else
            {
                foreach (var message in filteredMessages)
                {
                    if (message.Subject.ToLower().Contains(searchText))
                    {
                        lbMessages.Items.Add(string.IsNullOrWhiteSpace(message.Subject) ? "(empty subject)" : message.Subject);
                    }
                }
            }

        }
    }
}