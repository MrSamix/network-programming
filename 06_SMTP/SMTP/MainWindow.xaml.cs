using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

namespace SMTP_sendMessage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string server = "smtp.gmail.com";
        const short port = 587;

        List<string> files = new List<string>();
        string username;
        string password;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void AttachFileBtn(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
            {
                files = dialog.FileNames.ToList();
                listFiles.ItemsSource = files;
                btnClear.IsEnabled = true;
            }
        }
        private void SendMessage(object sender, RoutedEventArgs e)
        {
            MailMessage message = new MailMessage(username,toBox.Text,themeBox.Text,GetRichText(messageBox));


            if (IsHighPriority.IsChecked == true)
            {
                message.Priority = MailPriority.High;
            }
            else
            {
                message.Priority = MailPriority.Normal;
            }

            if (files.Count() > 0)
            {
                foreach (var file in files)
                {
                    message.Attachments.Add(new Attachment(file));
                }
            }

            SmtpClient client = new SmtpClient(server, port);
            client.Credentials = new NetworkCredential(username, password);
            client.EnableSsl = true;

            client.SendAsync(message, message);
            client.SendCompleted += (s,e) => MessageBox.Show("Sended!", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        string GetRichText(RichTextBox richTextBox)
        {
            TextRange textRange = new TextRange(
                richTextBox.Document.ContentStart,
                richTextBox.Document.ContentEnd
            );
            return textRange.Text;
        }

        private void SignInBtn(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbLogin.Text) || string.IsNullOrWhiteSpace(tbPassword.Text))
            {
                ChangeAvaibableElements(false);
                MessageBox.Show("Please enter your email and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!tbLogin.Text.Contains('@'))
            {
                ChangeAvaibableElements(false);
                MessageBox.Show("Please enter a valid email address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            username = tbLogin.Text;
            password = tbPassword.Text;


            ChangeAvaibableElements(true);
        }

        private void ChangeAvaibableElements(bool status = true)
        {
            themeBox.IsEnabled = status;
            toBox.IsEnabled = status;
            messageBox.IsEnabled = status;
            btnAttach.IsEnabled = status;
            btnSend.IsEnabled = status;
            listFiles.IsEnabled = status;
        }

        private void ClearBtn(object sender, RoutedEventArgs e)
        {
            files.Clear();
            listFiles.ItemsSource = null;
            btnClear.IsEnabled = false;
        }
    }
}
