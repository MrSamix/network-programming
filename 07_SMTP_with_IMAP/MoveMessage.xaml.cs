using MailKit;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for MoveMessage.xaml
    /// </summary>
    public partial class MoveMessage : Window
    {
        public bool IsCanceled { get; set; } = true;
        public IMailFolder SelectedFolder { get; set; }
        public IList<IMailFolder> Folders { get; }

        public MoveMessage(IList<MailKit.IMailFolder> folders, MailKit.IMailFolder currentFolder)
        {
            InitializeComponent();
            this.Folders = folders;
            foreach (var folder in folders)
            {
                if (folder != currentFolder)
                {
                    cbFolders.Items.Add(folder.Name);
                }
            }
        }

        private void MoveBtn(object sender, RoutedEventArgs e)
        {
            SelectedFolder = Folders.Where(f => f.Name == cbFolders.Text).First();
            if (SelectedFolder == null)
            {
                MessageBox.Show("Please select a folder to move the message to.");
                return;
            }
            IsCanceled = false;
            this.Close();
        }

        private void CancelBtn(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
            this.Close();
        }
    }
}
