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
            database = new Database("dummy","dummy");
            client = database.getClient();
            queryBuilder = new QueryBuilder(client);

            setupDataGrid(queryBuilder.getAllData(currentPage, defaultRequestSize));
            //setupDataGrid(queryBuilder.getSpecificData(currentPage, defaultRequestSize));
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
            else Console.WriteLine("Seting up datagrid failed -> Data is null");
        }
        //Update the page counter in the top right
        private void updatePageCount()
        {
            String strSeperator = "/";
            int totalPages = (int)(queryBuilder.getLastResponseHits() / defaultRequestSize) + 1;
            if ((currentPage > 98) && totalPages > 999) strSeperator = " /\n";
            pageLabel.Content = (currentPage + 1) + strSeperator + totalPages;
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
            if (queryBuilder != null)
            {
                Console.WriteLine("Updating grid!");
                setupDataGrid(queryBuilder.lastQueryNewPage(currentPage * defaultRequestSize, defaultRequestSize));
            }
        }

        //Copy a cell when copycell is clicked in the contextmenu
        private void copyCell_Click(object sender, RoutedEventArgs e)
        {   
            if(rightClickContent != null)Clipboard.SetText(rightClickContent);
        }

        //Double click a cell to open up the value in a seperate window
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
        //Right clicked the page counter in top right
        private void pageLabel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            PageNumberWindow pageWindow = new PageNumberWindow();
            pageWindow.ShowDialog();
            int enteredPage = pageWindow.getPageNumber();
            if(queryBuilder != null)
            {
                if (enteredPage < ((int)(queryBuilder.getLastResponseHits() / defaultRequestSize)))
                {
                    currentPage = enteredPage - 1;
                    updatePageCount();
                    updatePageDataGrid();
                }
                else MessageBox.Show("Page number not valid!");
            }
            
        }
        //Clicked the filter button
        private void filterButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Left clicked on filter!");
            if((fromTimeDate.Value != null) && (toTimeDate.Value != null))
            {
                DateTime leftTimeBound = (DateTime)fromTimeDate.Value;
                DateTime rightTimeBound = (DateTime)toTimeDate.Value;

                if(queryBuilder != null) queryBuilder.setTimeBounds(leftTimeBound, rightTimeBound);
                updatePageDataGrid();
            }
            else MessageBox.Show("Dates not valid!");  
        }
        //Right clicked the filter button (set time to default)
        private void filterButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Right clicked on filter!");
            if (queryBuilder != null) queryBuilder.setTimeBoundsDefault();
            fromTimeDate.Text = "";
            toTimeDate.Text = "";
            updatePageDataGrid();
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(toTimeDate.Value != null) fromTimeDate.Value = toTimeDate.Value;
        }

        private void Label_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            if(fromTimeDate.Value != null) toTimeDate.Value = fromTimeDate.Value;
        }
    }
}
