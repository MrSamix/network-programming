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
    /// Interaction logic for CreateFolder.xaml
    /// </summary>
    public partial class CreateFolder : Window
    {
        private IList<IMailFolder> folders;

        public string FolderName { get; set; }

        public CreateFolder(IList<MailKit.IMailFolder> folders)
        {
            InitializeComponent();
            this.folders = folders;
        }

        private void CreateBtn(object sender, RoutedEventArgs e)
        {
            FolderName = tbFolderName.Text;
            if (string.IsNullOrWhiteSpace(FolderName))
            {
                MessageBox.Show("Folder name cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            foreach (var folder in folders)
            {
                if (folder.Name == FolderName)
                {
                    MessageBox.Show("Folder with this name already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            this.Close();
        }
    }
}
