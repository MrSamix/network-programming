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

namespace Self_Task_SMTP_IMAP
{
    /// <summary>
    /// Interaction logic for SendMessage.xaml
    /// </summary>
    public partial class SendMessage : Window
    {
        string subject;
        string text;
        string to;
        const string SMTP_SERVER = "smtp.gmail.com";
        const int SMTP_PORT = 587;
        private string login;
        private string password;
        List<string> files = new List<string>();
        public SendMessage(string login, string password)
        {
            InitializeComponent();
            this.login = login;
            this.password = password;
        }
        public Task SendMessageAsync()
        {
            return Task.Run(() =>
            {
                var message = new MimeMessage();

                message.Subject = subject;

                MailboxAddress mailboxAddress = new MailboxAddress(login.Split('@')[0], login);
                message.From.Add(mailboxAddress);


                MailboxAddress mailboxAddressTo = new MailboxAddress(to.Split('@')[0], to);
                message.To.Add(mailboxAddressTo);


                var body = new TextPart("plain")
                {
                    Text = text
                };

                

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
                    message.Body = multipart;
                }
                else
                {
                    message.Body = body;
                }


                try
                {
                    using (var client = new SmtpClient())
                    {
                        client.Connect(SMTP_SERVER, SMTP_PORT, SecureSocketOptions.StartTls);
                        client.Authenticate(login, password);
                        client.Send(message);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("Message sent successfully!", "Success", MessageBoxButton.OK,MessageBoxImage.Information);
                            this.Close();
                        });
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Error sending message: " + ex.Message, "Error", MessageBoxButton.OK,MessageBoxImage.Error);
                        btnSend.IsEnabled = true;
                    });

                }
            });

        }

        private async void SendBtn(object sender, RoutedEventArgs e)
        {
            subject = tbSubject.Text;
            text = tbMessage.Text;
            to = tbTo.Text;
            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(text) || string.IsNullOrEmpty(to))
            {
                MessageBox.Show("Fill all fields!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            btnSend.IsEnabled = false;
            await SendMessageAsync();
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
