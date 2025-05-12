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

namespace Self_Task_HTTP_Download_Files
{
    /// <summary>
    /// Interaction logic for Rename.xaml
    /// </summary>
    public partial class Rename : Window
    {
        public string NewFileName { get; set; }
        public bool IsCanceled { get; set; } = true;
        public Rename()
        {
            InitializeComponent();
        }

        private void SubmitBtn(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNewName.Text))
            {
                MessageBox.Show("Please enter a valid file name.");
            }
            else
            {
                NewFileName = tbNewName.Text;
                IsCanceled = false;
                this.Close();
            }
        }

        private void CancelBtn(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
            this.Close();
        }
    }
}
