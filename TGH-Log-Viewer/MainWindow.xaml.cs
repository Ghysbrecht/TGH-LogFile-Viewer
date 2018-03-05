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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nest;

namespace TGH_Log_Viewer
{
    
    public partial class MainWindow : Window
    {
        QueryBuilder queryBuilder = new QueryBuilder();
        Database database;

        //Database/Table navigation variables
        int defaultRequestSize = 100;   //Ammount of returned lines per page
        int currentPage = 0;            //Current page
        
        public MainWindow()
        {
            InitializeComponent();

            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            database = new Database("localost:9200","mainindex");
            ElasticClient client = database.getClient();
           
            setupDataGrid(queryBuilder.getAllData(client, currentPage, defaultRequestSize));
        }

        //Prints out logines via console
        private void printLogLines(IReadOnlyCollection<LogLine> loglines)
        {
            foreach (var logline in loglines)
            {
                Console.WriteLine(logline.timestamp.ToShortDateString() + " Process: " + logline.process + " Logtype: " + logline.logtype + " Messagedata: " + logline.messagedata);
            }
        }

        //Updates the dataGrid with data
        private void setupDataGrid(IReadOnlyCollection<LogLine> loglines)
        {
            mainDataGrid.ItemsSource = loglines;
            mainScrollWindow.Visibility = Visibility.Visible;
            rowLabel.Content = loglines.Count;
            updatePageCount();
        }

        private void updatePageCount()
        {
            pageLabel.Content = (currentPage + 1) + "/" + (int)(queryBuilder.getLastResponseHits() / defaultRequestSize);
        }

        //Makes the scrollview scrollable with mousewheel
        private void mainScrollWindows_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        //Show/Hide columns depending on selection in sidebar
        private void columnListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("Selection changed!");
            
            foreach (var item in mainDataGrid.Columns)
            {
                var test = (ListBoxItem)columnListBox.FindName((String)item.Header);
                if (test.IsSelected)
                {
                    item.Visibility = Visibility.Collapsed;
                }
                else
                {
                    item.Visibility = Visibility.Visible;
                }
            }
        }

        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            if(currentPage != 0)
            {
                currentPage -= 1;
                updatePageCount();
            }
        }

        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage != ((int)(queryBuilder.getLastResponseHits() / defaultRequestSize) - 1))
            {
                currentPage += 1;
                updatePageCount();
            }
        }
    }
}
