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

        //General - Set the settings for this project
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
        //General - Filter on a given column name
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
        //General - Requery the last query with the new offset
        private void updatePageDataGrid()
        {
            if (queryBuilder != null)
            {
                Console.WriteLine("Updating grid!");
                setupDataGrid(queryBuilder.lastQueryNewPage(currentPage * appSettings.defaultRecords, appSettings.defaultRecords));
            }
        }

        //Topbar - Update the page counter in the top right
        private void updatePageCount()
        {
            String strSeperator = "/";
            int totalPages = (int)(queryBuilder.getLastResponseHits() / appSettings.defaultRecords) + 1;
            if ((currentPage > 98) && totalPages > 999) strSeperator = " /\n";
            pageLabel.Content = (currentPage + 1) + strSeperator + totalPages;
        }

        //Datagrid - Updates the dataGrid with data
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
        
        //Sidebar - Add the onContextCheck handler to all menuItems in the filter contextmenu
        private void assignCheckListeners()
        {
            MenuItem test = columnFilterContextMenu.Items[0] as MenuItem;
            foreach(Object menuItem in columnFilterContextMenu.Items)
            {
                ((MenuItem)menuItem).Checked += new RoutedEventHandler(onContextCheck);
            }
        }
        //Sidebar - Set the text in the filterbutton to the given name
        private void setFilterButtonText(String name)
        {
            columnButton.Content = name.ToUpper() + "        \u25BD  ";
        }
        //Sidebar - Set the filter options
        private void setFilter(String columnName, String searchTerm)
        {
            if (columnName != "Timestamp")
            {
                setFilterButtonText(columnName);
                filterTextBox.Text = rightClickContent;
                onlyCheckColumn(columnName);
            }
        }
        //Sidebar - Only checkmark this specific column in the contextmenu
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




        // ----------------- EVENTLISTENERS -----------------
        //Datagrid - Makes the scrollview scrollable with mousewheel
        private void mainScrollWindows_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
        //Sidebar - When entering while the filter textbox is in focus
        private void filterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) filterOnColumnName(dropDownFilterName, filterTextBox.Text);
        }
        //Sidebar - Eventhandler when a menuItem is checked in the filter contextMenu
        private void onContextCheck(object sender, RoutedEventArgs e)
        {
            dropDownFilterName = (String)((MenuItem)sender).Header;
            setFilterButtonText(dropDownFilterName);
            onlyCheckColumn(dropDownFilterName);
        }


        //---- LEFT CLICK LISTENERS ----
        //Topbar - Main - Get data button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (database.isValid()) setupDataGrid(queryBuilder.getAllData(currentPage, appSettings.defaultRecords));
        }
        //Topbar - Pages - Left button page selection
        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage != 0)
            {
                currentPage -= 1;
                updatePageCount();
                updatePageDataGrid();
            }
        }
        //Topbar - Pages - Right button page selection
        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < ((int)(queryBuilder.getLastResponseHits() / appSettings.defaultRecords)))
            {
                currentPage += 1;
                updatePageCount();
                updatePageDataGrid();
            }
        }
        //Topbar - Settings - Open settings window
        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(appSettings);
            settingsWindow.ShowDialog();
            setSettings(settingsWindow.getAppSettings());

        }
        //Topbar - Filter - Clicked the filter button
        private void filterButton_Click(object sender, RoutedEventArgs e)
        {
            if ((fromTimeDate.Value != null) && (toTimeDate.Value != null))
            {
                DateTime leftTimeBound = (DateTime)fromTimeDate.Value;
                DateTime rightTimeBound = (DateTime)toTimeDate.Value;
                if (rightTimeBound.Subtract(leftTimeBound).TotalSeconds > 0)
                {
                    if (queryBuilder != null) queryBuilder.setTimeBounds(leftTimeBound, rightTimeBound);
                    currentPage = 0;
                    updatePageDataGrid();
                }
                else MessageBox.Show("End date is earlier then start date!");

            }
            else MessageBox.Show("Dates not valid!");
        }
        //Topbar - Extra - Clicking the INFO button in the extra contextmenu
        private void infoMenu_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow infoWindow = new InfoWindow();
            infoWindow.ShowDialog();
        }
        //Topbar - Extra - Clicking the GRAPH button in the extra contextmenu
        private void graphMenu_Click(object sender, RoutedEventArgs e)
        {
            graphWindow = new GraphWindow();
            graphWindow.Show();
            graphWindow.refresh();
        }
        //Topbar - Extra - Clicking the ANALYZE button in the extra contextmenu
        private void analyzeMenu_Click(object sender, RoutedEventArgs e)
        {
            AnalyzeWindow analyzeWindow = new AnalyzeWindow();
            analyzeWindow.Show();
        }

        //Datagrid - Contextmenu 'filter on'
        private void filterOnMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Filtering on: " + rightClickColumnName + " : " + rightClickContent);
            currentPage = 0;
            filterOnColumnName(rightClickColumnName, rightClickContent);
            setFilter(rightClickColumnName, rightClickContent);
        }
        //Datagrid - Contextmenu 'copy cell'
        private void copyCell_Click(object sender, RoutedEventArgs e)
        {
            if (rightClickContent != null) Clipboard.SetText(rightClickContent);
        }

        //Sidebar - Show/Hide columns depending on selection in sidebar
        private void columnListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
        //Sidebar - When clicking on the columns button, open the context menu
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }
        //Sidebar - Enable filter
        private void applyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            filterOnColumnName(dropDownFilterName, filterTextBox.Text);
        }
        //Sidebar - Hide the 'hide columns' selector when clicking the header label
        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (columnListBox.Visibility == Visibility.Collapsed)
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


        //---- RIGHT CLICK LISTENERS ----
        //Topbar - Right clicked the page counter in top right
        private void pageLabel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            PageNumberWindow pageWindow = new PageNumberWindow();
            pageWindow.ShowDialog();
            int enteredPage = pageWindow.getPageNumber();
            if (queryBuilder != null)
            {
                if (enteredPage < ((int)(queryBuilder.getLastResponseHits() / appSettings.defaultRecords)))
                {
                    currentPage = enteredPage - 1;
                    updatePageCount();
                    updatePageDataGrid();
                }
                else MessageBox.Show("Page number not valid!");
            }
        }
        //Topbar - Right clicked the filter button (set time to default)
        private void filterButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Right clicked on filter!");
            if (queryBuilder != null) queryBuilder.setTimeBoundsDefault();
            fromTimeDate.Text = "";
            toTimeDate.Text = "";
            updatePageDataGrid();
        }

        //Datagrid - Cell right clicked
        private void mainDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;
            if (targetCell != null)
            {
                rightClickContent = ((TextBlock)targetCell.Content).Text;
                rightClickColumnName = targetCell.Column.Header.ToString();
            }
        }


        //---- DOUBLE CLICK LISTENER ----
        //Topbar - Copy over the date when doubleclicking the to label
        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (toTimeDate.Value != null) fromTimeDate.Value = toTimeDate.Value;
        }
        //Topbar - Copy over the date when doubleclicking the from label
        private void Label_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            if (fromTimeDate.Value != null) toTimeDate.Value = fromTimeDate.Value;
        }

        //Datagrid - Double click a cell to open up the value in a seperate window
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

        //Sidebar - Remove all in the filter textbox when you doubleclick it
        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            filterTextBox.Text = "";
        }

        
    }
}
