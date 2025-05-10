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
    /// Interaction logic for Sort.xaml
    /// </summary>
    public partial class Sort : Window
    {
        public bool IsCanceled { get; set; } = true;
        public bool IsOld { get; set; } = true;
        public Sort(bool sortedByOldest)
        {
            InitializeComponent();
            if (sortedByOldest)
            {
                rbold.IsChecked = true;
                rbnew.IsChecked = false;
            }
            else
            {
                rbold.IsChecked = false;
                rbnew.IsChecked = true;
            }
            IsOld = sortedByOldest;
        }

        private void ButtonOk(object sender, RoutedEventArgs e)
        {
            var optionOld = rbold.IsChecked;
            var optionNew = rbnew.IsChecked;

            if (optionOld == true)
            {
                IsCanceled = false;
                IsOld = true;
            }
            else
            {
                IsCanceled = false;
                IsOld = false;
            }
            this.Close();
        }

        private void ButtonCancel(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
            this.Close();
        }
    }
}
