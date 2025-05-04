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

namespace Client
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public string UserName { get; set; }
        public InputBox()
        {
            InitializeComponent();
        }

        private void SubmitBtn(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbUserName.Text))
            {
                MessageBox.Show("Enter a username!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            UserName = tbUserName.Text;
            tbUserName.Text = "";
            this.Hide();
        }

        private void tbUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubmitBtn(sender, e);
            }
        }
    }
}
