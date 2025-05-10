using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Win32;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

namespace Self_Task_SMTP_IMAP
{
    /// <summary>
    /// Interaction logic for Reply.xaml
    /// </summary>
    public partial class Reply : Window
    {
        MimeMessage originalMessage;
        MailboxAddress to;
        string subject;
        const string SMTP_SERVER = "smtp.gmail.com";
        const int SMTP_PORT = 587;
        private string login;
        private string password;
        List<string> files = new List<string>();
        string text;
        public Reply(MimeMessage originalMessage, string login, string password)
        {
            InitializeComponent();
            to = originalMessage.To.Mailboxes.First();
            tbTo.Text = to.ToString();
            tbSubject.Text = subject = "Re: " + originalMessage.Subject;
            this.login = login;
            this.password = password;
            this.originalMessage = originalMessage;
        }
        public Task ReplyToMessageAsync()
        {
            return Task.Run(() =>
            {
                var replyMessage = new MimeMessage();


                replyMessage.Subject = subject;

                replyMessage.From.Add(to);
                replyMessage.To.Add(originalMessage.From.Mailboxes.First());

                var body = new TextPart("plain")
                {
                    Text = text
                };

                replyMessage.InReplyTo = originalMessage.MessageId;
                replyMessage.References.Add(originalMessage.MessageId);

                replyMessage.Body = body;

                if (files.Count > 0)
                {
                    var multipart = new Multipart("mixed");
                    foreach (var file in files)
                    {
                        var attachment = new MimePart()
                        {
                            Content = new MimeContent(File.OpenRead(file)),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = Path.GetFileName(file)
                        };
                        multipart.Add(attachment);
                    }
                    multipart.Add(body);
                    replyMessage.Body = multipart;
                }



                try
                {
                    using (var client = new SmtpClient())
                    {
                        client.Connect(SMTP_SERVER, SMTP_PORT, SecureSocketOptions.StartTls);
                        client.Authenticate(login, password);
                        client.Send(replyMessage);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("Reply sent successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        });
                        
                        //client.Disconnect(true);
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Error sending reply: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Close();
                    });
                    
                }
            });
            
        }

        private async void ReplyBtn(object sender, RoutedEventArgs e)
        {
            subject = tbSubject.Text;
            text = tbMessage.Text;
            await ReplyToMessageAsync();
        }

        private void AttachBtn(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
            {
                files = dialog.FileNames.ToList();
                lbAttachments.ItemsSource = files;
                btnClear.IsEnabled = true;
            }
        }

        private void ClearBtn(object sender, RoutedEventArgs e)
        {
            btnClear.IsEnabled = false;
            files.Clear();
            lbAttachments.ItemsSource = null;
        }
    }
}
