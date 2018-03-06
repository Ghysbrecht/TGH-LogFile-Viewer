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
        QueryBuilder queryBuilder;
        Database database;
        ElasticClient client;

        //Database&Table navigation variables
        int defaultRequestSize = 100;   
        int currentPage = 0;            

        String rightClickContent;
        String rightClickColumnName;
        String doubleClickContent;
        String doubleClickColumnName;


        public MainWindow()
        {
            InitializeComponent();

            //Disable buttons
            leftButton.IsEnabled = false;
            rightButton.IsEnabled = false;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            database = new Database("localost:9200","mainindex");
            client = database.getClient();
            queryBuilder = new QueryBuilder(client);

            //setupDataGrid(queryBuilder.getAllData(currentPage, defaultRequestSize));
            setupDataGrid(queryBuilder.getAllData(currentPage, defaultRequestSize));
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
            if (loglines != null)
            {
                leftButton.IsEnabled = true;
                rightButton.IsEnabled = true;
                mainDataGrid.ItemsSource = loglines;
                mainScrollWindow.Visibility = Visibility.Visible;
                rowLabel.Content = loglines.Count;
                updatePageCount();
            }
        }
        //Update the page counter in the top right
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

        //Left button page selection clicked
        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            if(currentPage != 0)
            {
                currentPage -= 1;
                updatePageCount();
                updatePageDataGrid();
            }
        }
        //Right button page selection clicked
        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < ((int)(queryBuilder.getLastResponseHits() / defaultRequestSize)))
            {
                currentPage += 1;
                updatePageCount();
                updatePageDataGrid();
            }
        }

        //Datagrid cell right clicked
        private void mainDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;
            if(targetCell != null)
            {
                rightClickContent = ((TextBlock)targetCell.Content).Text;
                rightClickColumnName = targetCell.Column.Header.ToString();
            }
        }
        //Rightclick context menu filter on clicked
        private void filterOnMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Filtering on: " + rightClickColumnName + " : " + rightClickContent);
            currentPage = 0;
            
            switch (rightClickColumnName)
            {
                case "Filename":
                    setupDataGrid(queryBuilder.filterOnFilename(currentPage, defaultRequestSize, rightClickContent));
                    break;
                case "Function":
                    setupDataGrid(queryBuilder.filterOnFunction(currentPage, defaultRequestSize, rightClickContent));
                    break;
                case "Process":
                    setupDataGrid(queryBuilder.filterOnProcess(currentPage, defaultRequestSize, rightClickContent));
                    break;
                case "PID":
                    setupDataGrid(queryBuilder.filterOnPID(currentPage, defaultRequestSize, rightClickContent));
                    break;
                case "TID":
                    setupDataGrid(queryBuilder.filterOnTID(currentPage, defaultRequestSize, rightClickContent));
                    break;
                case "Loglevel":
                    setupDataGrid(queryBuilder.filterOnLoglevel(currentPage, defaultRequestSize, rightClickContent));
                    break;
                case "Logtype":
                    setupDataGrid(queryBuilder.filterOnLogtype(currentPage, defaultRequestSize, rightClickContent));
                    break;
                case "Message":
                    setupDataGrid(queryBuilder.filterOnMessage(currentPage, defaultRequestSize, rightClickContent));
                    break;
                default:
                    MessageBox.Show("Not yet supported for this column!");
                    break;
            }
            
        }

        //Update the datagrid by requering the last query with the new offset
        private void updatePageDataGrid()
        {
            setupDataGrid(queryBuilder.lastQueryNewPage(currentPage * defaultRequestSize, defaultRequestSize));
        }

        //Copy a cell when copycell is clicked in the contextmenu
        private void copyCell_Click(object sender, RoutedEventArgs e)
        {   
            if(rightClickContent != null)Clipboard.SetText(rightClickContent);
        }

        private void mainDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;
            if (targetCell != null)
            {
                doubleClickContent = ((TextBlock)targetCell.Content).Text;
                doubleClickColumnName = targetCell.Column.Header.ToString();
                
                Dispatcher.BeginInvoke(new Action(() => {
                    var window = new CellValueWindow(doubleClickContent);
                    window.Show();
                    window.Focus();
                }));
                
            }
        }
    }
}
