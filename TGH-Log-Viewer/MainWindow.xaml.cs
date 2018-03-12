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
        AppSettings appSettings;
        GraphWindow graphWindow;

        //Database&Table navigation variables
        int currentPage = 0;
        
        String rightClickColumnName;
        String rightClickContent;
        String doubleClickContent;
        String doubleClickColumnName;
        String dropDownFilterName;

        public MainWindow()
        {
            InitializeComponent();

            //Disable buttons
            leftButton.IsEnabled = false;
            rightButton.IsEnabled = false;

            setSettings(new AppSettings());
            assignCheckListeners();

        }
        //Main button click
        private void Button_Click(object sender, RoutedEventArgs e)
        { 
            if(database.isValid()) setupDataGrid(queryBuilder.getAllData(currentPage, appSettings.defaultRecords));
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
                if (appSettings.autoTime && loglines.Count > 0)
                {
                    fromTimeDate.Value = loglines.First().timestamp;
                    toTimeDate.Value = loglines.Last().timestamp;
                }
                updatePageCount();
                if (graphWindow != null) graphWindow.createFromData(loglines);
            }
            else Console.WriteLine("Seting up datagrid failed -> Data is null");
        }
        //Update the page counter in the top right
        private void updatePageCount()
        {
            String strSeperator = "/";
            int totalPages = (int)(queryBuilder.getLastResponseHits() / appSettings.defaultRecords) + 1;
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
            if (currentPage < ((int)(queryBuilder.getLastResponseHits() /  appSettings.defaultRecords)))
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
            filterOnColumnName(rightClickColumnName, rightClickContent);
            setFilter(rightClickColumnName, rightClickContent);
        }
        private void filterOnColumnName(String columnName, String filterContent)
        {
            switch (columnName)
            {
                case "Filename":
                    setupDataGrid(queryBuilder.filterOnFilename(currentPage, appSettings.defaultRecords, filterContent));
                    break;
                case "Function":
                    setupDataGrid(queryBuilder.filterOnFunction(currentPage, appSettings.defaultRecords, filterContent));
                    break;
                case "Process":
                    setupDataGrid(queryBuilder.filterOnProcess(currentPage, appSettings.defaultRecords, filterContent));
                    break;
                case "PID":
                    setupDataGrid(queryBuilder.filterOnPID(currentPage, appSettings.defaultRecords, filterContent));
                    break;
                case "TID":
                    setupDataGrid(queryBuilder.filterOnTID(currentPage, appSettings.defaultRecords, filterContent));
                    break;
                case "Loglevel":
                    setupDataGrid(queryBuilder.filterOnLoglevel(currentPage, appSettings.defaultRecords, filterContent));
                    break;
                case "Logtype":
                    setupDataGrid(queryBuilder.filterOnLogtype(currentPage, appSettings.defaultRecords, filterContent));
                    break;
                case "Message":
                    setupDataGrid(queryBuilder.filterOnMessage(currentPage, appSettings.defaultRecords, filterContent));
                    break;
                case "Timestamp":
                    if (fromTimeDate.Value == null && toTimeDate.Value == null) fromTimeDate.Text = filterContent;
                    else toTimeDate.Text = filterContent;
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
                setupDataGrid(queryBuilder.lastQueryNewPage(currentPage *  appSettings.defaultRecords,  appSettings.defaultRecords));
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
                if (enteredPage < ((int)(queryBuilder.getLastResponseHits() /  appSettings.defaultRecords)))
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
                if( rightTimeBound.Subtract(leftTimeBound).TotalSeconds > 0 )
                {
                    if (queryBuilder != null) queryBuilder.setTimeBounds(leftTimeBound, rightTimeBound);
                    currentPage = 0;
                    updatePageDataGrid(); 
                }
                else MessageBox.Show("End date is earlier then start date!");

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
        //Copy over the date when doubleclicking the to label
        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(toTimeDate.Value != null) fromTimeDate.Value = toTimeDate.Value;
        }
        //Copy over the date when doubleclicking the from label
        private void Label_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            if(fromTimeDate.Value != null) toTimeDate.Value = fromTimeDate.Value;
        }
        //Open Settings window
        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(appSettings);
            settingsWindow.ShowDialog();
            setSettings(settingsWindow.getAppSettings());

        }
        //Set the settings for this project
        private void setSettings(AppSettings settings)
        {
            appSettings = settings;
            database = new Database(appSettings.elasticip, appSettings.defaultIndex);
            if (database.isValid())
            {
                client = database.getClient();
                if (queryBuilder == null) queryBuilder = new QueryBuilder(client);
                else queryBuilder.setClient(client);
            }
            else MessageBox.Show("Connection failed! Check your settings...");
        }
        //When clicking on the columns button, open the context menu
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
           (sender as Button).ContextMenu.IsEnabled = true;
           (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
           (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
           (sender as Button).ContextMenu.IsOpen = true;
        }
        //Add the onContextCheck handler to all menuItems in the filter contextmenu
        private void assignCheckListeners()
        {
            MenuItem test = columnFilterContextMenu.Items[0] as MenuItem;
            foreach(Object menuItem in columnFilterContextMenu.Items)
            {
                ((MenuItem)menuItem).Checked += new RoutedEventHandler(onContextCheck);
            }
        }
        //Eventhandler when a menuItem is checked in the filter contextMenu
        private void onContextCheck( object sender, RoutedEventArgs e)
        {
            dropDownFilterName = (String)((MenuItem)sender).Header;
            setFilterButtonText(dropDownFilterName);
            onlyCheckColumn(dropDownFilterName);
        }
        //Set the text in the filterbutton to the given name
        private void setFilterButtonText(String name)
        {
            columnButton.Content = name.ToUpper() + "        \u25BD  ";
        }
        //Set the filter options
        private void setFilter(String columnName, String searchTerm)
        {
            if (columnName != "Timestamp")
            {
                setFilterButtonText(columnName);
                filterTextBox.Text = rightClickContent;
                onlyCheckColumn(columnName);
            }
        }
        //Only chekmark this specific column in the contextmenu
        private void onlyCheckColumn(String columName)
        {
            foreach (MenuItem menuItem in columnFilterContextMenu.Items)
            {
                if (menuItem.IsChecked && ((String)menuItem.Header != columName))
                {
                    menuItem.IsChecked = false;
                    menuItem.IsCheckable = true;
                }
                else if ((String)menuItem.Header == columName)
                {
                    menuItem.IsChecked = true;
                    menuItem.IsCheckable = false;
                }
            }
        }
        //Enable filtering
        private void applyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            filterOnColumnName(dropDownFilterName, filterTextBox.Text);
        }
        //Remove all in the filter textbox when you doubleclick it
        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            filterTextBox.Text = "";
        }
        //Hide the 'hide columns' selector when clicking the header label
        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(columnListBox.Visibility == Visibility.Collapsed)
            {
                columnListBox.Visibility = Visibility.Visible;
                hideColumnsLabel.Content = "       HIDE COLUMNS";
            }
            else
            {
                columnListBox.Visibility = Visibility.Collapsed;
                hideColumnsLabel.Content = "       HIDE COLUMNS     \u25BD";
            }
        }
        //When entering while the filter textbox is in focus
        private void filterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter) filterOnColumnName(dropDownFilterName, filterTextBox.Text);
        }
        //Clicking the graph button in the extra contextmenu
        private void graphMenu_Click(object sender, RoutedEventArgs e)
        {
            graphWindow = new GraphWindow();
            graphWindow.Show();
            graphWindow.refresh();
        }
        //Clicking the info button in the extra contextmenu
        private void infoMenu_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow infoWindow = new InfoWindow();
            infoWindow.ShowDialog();
        }
    }
}
