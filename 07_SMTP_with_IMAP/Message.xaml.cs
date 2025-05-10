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
using System.Windows.Shapes;

namespace Self_Task_SMTP_IMAP
{
    /// <summary>
    /// Interaction logic for Message.xaml
    /// </summary>
    public partial class Message : Window
    {
        public MimeMessage OriginalMessage { get; set; }
        private string login;
        private string password;
        public Message(MimeMessage message, string login, string password)
        {
            InitializeComponent();
            this.login = login;
            this.password = password;
            OriginalMessage = message;
            tbFrom.Text = message.From.ToString();
            tbTo.Text = message.To.ToString();
            tbSubject.Text = message.Subject;
            tbDate.Text = message.Date.ToString();
            tbMessage.Text = message.TextBody;
            if (message.Attachments.Count() > 0)
            {
                foreach (var file in message.Attachments)
                {
                    var filename = file.ContentDisposition.FileName;
                    lbAttachments.Items.Add(filename);
                }
            }
        }
        private void ReplyBtn(object sender, RoutedEventArgs e)
        {
            Reply reply = new Reply(OriginalMessage, login, password);
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() => reply.Show());
            });
        }
    }
}
