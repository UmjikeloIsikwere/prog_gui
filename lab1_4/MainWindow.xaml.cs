using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lab1_4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitYear();
            InitMonth();
        }

        private void InitYear()
        {
            for( int year = 1900; year <= DateTime.Now.Year; year++ )
            {
                yearBox.Items.Add( year );
            }
        }

        private void InitMonth()
        {
            for(int month = 1; month <= 12;  month++ )
            {
                monthBox.Items.Add( month );
            }
        }

        private void Select_Box( object sender, RoutedEventArgs e) {
            if( yearBox.SelectedItem != null && monthBox.SelectedItem != null) {
                int year = (int)yearBox.SelectedItem;
                int month = (int)monthBox.SelectedItem;
                int daysInMont = DateTime.DaysInMonth(year, month);

                daysBox.Items.Clear();
                for( int day = 1; day <= daysInMont; day++ )
                {
                    daysBox.Items.Add ( day );
                }

                daysBox.IsEnabled = true;
            }
        }

        private void Select_day( object sender, RoutedEventArgs e )
        {
            if(yearBox.SelectedIndex != -1 && monthBox.SelectedIndex != -1 )
            {
                int year = (int)yearBox.SelectedValue;
                int month = (int)monthBox.SelectedValue;
                int day = (int)daysBox.SelectedValue;

                DateTime selectedDate = new DateTime(year, month, day);
                TimeSpan diff = DateTime.Now - selectedDate;

                int years = (int)(diff.TotalDays / 365);
                int months = (int)(diff.TotalDays % 365) / 30;
                int days = (int)(diff.TotalDays % 365) % 30;

                MessageBox.Show($"Прошло {years} лет, {months} месяцев и {days} дней.");
            }
        }
    }
}