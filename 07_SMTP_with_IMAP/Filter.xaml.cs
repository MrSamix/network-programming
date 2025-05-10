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
    /// Interaction logic for Filter.xaml
    /// </summary>
    public partial class Filter : Window
    {
        public bool IsCanceled { get; set; } = true;
        public bool? IsImportant { get; set; }
        public DateTime? from { get; set; }
        public DateTime? to { get; set; }
        public string? Author { get; set; }
        public Filter(bool? isImportant, DateTime? from, DateTime? to, string? author)
        {
            InitializeComponent();
            IsImportant = isImportant;
            this.from = from;
            this.to = to;
            Author = author;
            if (IsImportant != null)
            {
                if (IsImportant == true)
                    cbIsImportant.IsChecked = true;
                else
                    cbIsImportant.IsChecked = false;
            }
            if (from != null)
            {
                tbFrom.Text = from.Value.ToShortDateString();
            }
            if (to != null)
            {
                tbTo.Text = to.Value.ToShortDateString();
            }
            if (author != null)
            {
                tbAuthor.Text = author;
            }
        }

        private void ButtonOk(object sender, RoutedEventArgs e)
        {
            IsImportant = cbIsImportant.IsChecked;
            IsCanceled = false;
            try
            {
                from = tbFrom.Text == "" ? null : DateTime.Parse(tbFrom.Text);
                to = tbTo.Text == "" ? null : DateTime.Parse(tbTo.Text);
                if (from != null && to != null && from > to)
                {
                    MessageBox.Show("From date must be less than To date");
                    return;
                }
                if (from != null && from > DateTime.Now)
                {
                    MessageBox.Show("From date must be less than current date");
                    return;
                }
                if (to != null && to > DateTime.Now)
                {
                    MessageBox.Show("To date must be less than current date");
                    return;
                }
                Author = tbAuthor.Text == "" ? null : tbAuthor.Text;
                if (Author != null && !Author.Contains('@'))
                {
                    MessageBox.Show("Author must be in the format 'name@domain'");
                    return;
                }
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid date format. Please use 'YYYY/MM/dd' format.", "Invalid date", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonCancel(object sender, RoutedEventArgs e)
        {
            this.IsCanceled = true;
            this.Close();
        }
    }
}
