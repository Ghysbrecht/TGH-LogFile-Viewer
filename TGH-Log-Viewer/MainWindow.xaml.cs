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
using Elasticsearch.Net;
using System.Timers;

namespace TGH_Log_Viewer
{

    public partial class MainWindow : Window
    {
        LLDatabase database;
        ElasticLowLevelClient client;
        LLQueryBuilder queryBuilder;

        AppSettings appSettings;
        GraphWindow graphWindow;
        ScrollViewer scrollViewer;

        int currentPage = 0;
        int currentScrollOffset = 0;

        String rightClickColumnName = "";
        String rightClickContent = "";
        String doubleClickContent = "";
        String doubleClickColumnName = "";
        String dropDownFilterName = "";

        Timer timer1;

        Logger logger = new Logger();

        public MainWindow()
        {
            InitializeComponent();

            //Disable buttons
            leftButton.IsEnabled = false;
            rightButton.IsEnabled = false;

            assignCheckListeners();
            dropDownFilterName = "";
            appSettings = new AppSettings();
            appSettings = appSettings.restoreFromFile();

            logger.debug("=== Starting TGH Log Viewer ===");

            mainDataGrid.Loaded += attachScrollViewerListener;
            bottomStatusText.Text = "Ready";
        }
        //General - Append the next block of data to the current set.
        private void appendScroll()
        {
            fillGrid(queryBuilder.lastQueryNewPage((currentPage + currentScrollOffset ) * appSettings.defaultRecords, appSettings.defaultRecords),true);
        }
        //General - Set the settings for this project
        private void setSettings(AppSettings settings)
        {
            appSettings = settings;
            database = new LLDatabase(appSettings.elasticip, appSettings.defaultIndex);
            if (database.isValid())
            {
                bottomDBStatusText.Text = "Connected";
                client = database.getClient();
                if (queryBuilder == null) queryBuilder = new LLQueryBuilder(client);
                else queryBuilder.setClient(client);
                queryBuilder.setMainIndex(appSettings.defaultIndex);
            }
            else
            {
                bottomDBStatusText.Text = "No connection";
                MessageBox.Show("Connection failed! Check your settings...");
                logger.debug("setSettings failed! -> elastic ip: " + appSettings.elasticip + " elastic index" + appSettings.defaultIndex);
            }
        }
        //General - Filter on a given column name
        private void filterOnColumnName(String columnName, String filterContent)
        {
            if (database != null && database.isValid())
            {
                switch (columnName)
                {
                    case "Filename":
                        fillGrid(queryBuilder.filterOnFilename(currentPage * appSettings.defaultRecords, appSettings.defaultRecords, filterContent));
                        break;
                    case "Function":
                        fillGrid(queryBuilder.filterOnFunction(currentPage * appSettings.defaultRecords, appSettings.defaultRecords, filterContent));
                        break;
                    case "Process":
                        fillGrid(queryBuilder.filterOnProcess(currentPage * appSettings.defaultRecords, appSettings.defaultRecords, filterContent));
                        break;
                    case "PID":
                        fillGrid(queryBuilder.filterOnPID(currentPage * appSettings.defaultRecords, appSettings.defaultRecords, filterContent));
                        break;
                    case "TID":
                        fillGrid(queryBuilder.filterOnTID(currentPage * appSettings.defaultRecords, appSettings.defaultRecords, filterContent));
                        break;
                    case "Loglevel":
                        fillGrid(queryBuilder.filterOnLoglevel(currentPage * appSettings.defaultRecords, appSettings.defaultRecords, filterContent));
                        break;
                    case "Logtype":
                        fillGrid(queryBuilder.filterOnLogtype(currentPage * appSettings.defaultRecords, appSettings.defaultRecords, filterContent));
                        break;
                    case "Message":
                        fillGrid(queryBuilder.filterOnMessage(currentPage * appSettings.defaultRecords, appSettings.defaultRecords, filterContent));
                        break;
                    case "Timestamp":
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (fromTimeDate.Value == null && toTimeDate.Value == null) fromTimeDate.Text = filterContent;
                            else toTimeDate.Text = filterContent; 
                            bottomStatusText.Text = "Ready";
                        }));
                        break;
                    case "global":
                        fillGrid(queryBuilder.globalSearch(filterContent, currentPage * appSettings.defaultRecords, appSettings.defaultRecords));
                        break;
                    default:
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("Not yet supported for this column!");
                            logger.debug("Filtered on column that is not supported! -> " + columnName);
                            bottomStatusText.Text = "Ready";
                        }));
                        break;
                }
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show("Filtering disabled! No database connection.");
                }));
            }
        }
        //General - Requery the last query with the new offset
        private void updatePageDataGrid()
        {
            if (queryBuilder != null) fillGrid(queryBuilder.lastQueryNewPage(currentPage * appSettings.defaultRecords, appSettings.defaultRecords));
        }
        //General - Return to the previous query
        private void returnToPreviousQuery()
        {
            if (queryBuilder != null)
            {
                fillGrid(queryBuilder.previousQuery(currentPage * appSettings.defaultRecords, appSettings.defaultRecords));
                SearchRequest lastRequest = queryBuilder.getLastQueryData();
                setFilter(lastRequest.searchColumn, lastRequest.searchTerm);
            }
        }
        //General - Get all data
        private void getAllData()
        {
            fillGrid(queryBuilder.getAllData(currentPage, appSettings.defaultRecords));
        }
        //General - Fill the grid during an async call by invoking the setupDataGrid method
        private void fillGrid(List<LogLine> loglines, bool append = false)
        {
            Dispatcher.BeginInvoke(new Action(() => {
                setupDataGrid(loglines, append);
                if (queryBuilder.getLastError() != "") MessageBox.Show("WARNING: " + queryBuilder.getLastError());
            }));
            
        }
        //General - Get visual child
        private static T getVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = getVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
        //General - Attach ScrollViewer Listener
        private void attachScrollViewerListener(Object sender, RoutedEventArgs e)
        {
            scrollViewer = getVisualChild<ScrollViewer>(mainDataGrid);
            if (scrollViewer != null) scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            else logger.debug("No scrollviewer found!");
        }
        //General - Translate a string to int if possible.
        private int toInteger(String stringVar)
        {
            int j;
            if (Int32.TryParse(stringVar, out j)) return j;
            else
            {
                logger.error("String could not be converted! Value -> " + stringVar);
                return -1;
            }
        }

        //Topbar - Update the page counter in the top right
        private void updatePageCount()
        {
            String strSeperator = "/ ";
            int totalPages = (int)(queryBuilder.getLastResponseHits() / appSettings.defaultRecords) + 1;
            if (currentPage > totalPages) currentPage = totalPages - 1;
            pageTotalLabel.Content = strSeperator + totalPages;
            pageNumberLabel.Text = "" + (currentPage + 1);
            bottomBarTotalHitsText.Text = "" + (int)queryBuilder.getLastResponseHits();
        }
        //Topbar - Set the pagenumber to a different value after checking
        private void setPageNumber(int enteredPage)
        {
            if (queryBuilder != null)
            {
                if (enteredPage < ((int)(queryBuilder.getLastResponseHits() / appSettings.defaultRecords)) && enteredPage > 0)
                {
                    if (currentPage != enteredPage - 1)
                    {
                        currentPage = enteredPage - 1;
                        updatePageCount();
                        new Task(updatePageDataGrid).Start();
                    } 
                }
                else
                {
                    updatePageCount();
                    MessageBox.Show("Page number not valid!");
                }
            }
        }

        //Datagrid - Updates the dataGrid with data
        private void setupDataGrid(IReadOnlyCollection<LogLine> loglines, bool append = false)
        {
            if (loglines != null)
            {
                leftButton.IsEnabled = true;
                rightButton.IsEnabled = true;
                if (!append)
                {
                    currentScrollOffset = 0;
                    if (scrollViewer != null) scrollViewer.ScrollToTop();
                    mainDataGrid.Items.Clear();
                }
                foreach (LogLine logline in loglines) { mainDataGrid.Items.Add(logline); }
                rowLabel.Content = mainDataGrid.Items.Count;
                if (appSettings.autoTime && loglines.Count > 0)
                {
                    fromTimeDate.Value = ((LogLine)mainDataGrid.Items.GetItemAt(0)).timestamp;
                    toTimeDate.Value = ((LogLine)mainDataGrid.Items.GetItemAt(mainDataGrid.Items.Count - 1)).timestamp;
                }
                updatePageCount();
                if (graphWindow != null) graphWindow.createFromData(loglines);
            }
            else logger.debug("Setting up datagrid failed -> Data is null");
            bottomStatusText.Text = "Ready";
        }

        //Sidebar - Add the onContextCheck handler to all menuItems in the filter contextmenu
        private void assignCheckListeners()
        {
            MenuItem test = columnFilterContextMenu.Items[0] as MenuItem;
            foreach (Object menuItem in columnFilterContextMenu.Items)
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
                filterTextBox.Text = searchTerm;
                onlyCheckColumn(columnName);
            }
        }
        //Sidebar - Only checkmark this specific column in the contextmenu
        private void onlyCheckColumn(String columnName)
        {
            columnName = columnName.ToLower();
            foreach (MenuItem menuItem in columnFilterContextMenu.Items)
            {
                if (menuItem.IsChecked && (((String)menuItem.Header).ToLower() != columnName))
                {
                    menuItem.IsChecked = false;
                    menuItem.IsCheckable = true;
                }
                else if (((String)menuItem.Header).ToLower() == columnName)
                {
                    menuItem.IsChecked = true;
                    menuItem.IsCheckable = false;
                }
            }
        }
        //Sidebar - Fill the context menu for the textbox
        private void filContextMenu(List<String> suggestions)
        {
            filterTextBoxContextMenu.Items.Clear();
            foreach (String suggestion in suggestions)
            {
                MenuItem menuItem = new MenuItem();
                menuItem.Header = suggestion;
                menuItem.Click += suggestionSet;
                menuItem.KeyDown += suggestionKey;
                filterTextBoxContextMenu.Items.Add(menuItem);
            }
        }
        //Sidebar - Open the context menu
        private void openContextMenu()
        {
            if(queryBuilder != null)
            {
                List<String> suggestions = queryBuilder.getSuggestionsFor(dropDownFilterName.ToLower(), filterTextBox.Text);
                filContextMenu(suggestions);

                if (filterTextBoxContextMenu.Items.Count != 0)
                {
                    filterTextBox.ContextMenu.IsEnabled = true;
                    filterTextBox.ContextMenu.PlacementTarget = filterTextBox;
                    filterTextBox.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    filterTextBox.ContextMenu.IsOpen = true;
                    (filterTextBoxContextMenu.Items[0] as MenuItem).Focus();
                }
                else filterTextBox.Focus();
            }  
        }



        // ----------------- EVENTLISTENERS -----------------
        //Topbar - Change pagenumber to entered value when clicking out the textbox
        private void pageNumberLabel_LostFocus(object sender, RoutedEventArgs e)
        {
            setPageNumber(toInteger((sender as TextBox).Text));
        }
        //Topbar - Change the pagenumber to the changed value when pressing enter
        private void pageNumberLabel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                setPageNumber(toInteger((sender as TextBox).Text));
            }
        }
        //Datagrid - Makes the scrollview scrollable with mousewheel
        private void mainScrollWindows_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
        //Sidebar - When entering while the filter textbox is in focus OR Arrow down for sugestions
        private void filterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                currentPage = 0;
                filterOnColumnName(dropDownFilterName, filterTextBox.Text);
            }
            else if (e.Key == Key.Down) openContextMenu();
            else
            {
                if (timer1 == null)
                {
                    timer1 = new Timer(1000);
                    timer1.Elapsed += new ElapsedEventHandler(HandleTimerElapsed);
                    timer1.Enabled = true;
                }
                timer1.Stop();
                timer1.Start();
            }
        }
        //Sidebar - Eventhandler when a menuItem is checked in the filter contextMenu
        private void onContextCheck(object sender, RoutedEventArgs e)
        {
            dropDownFilterName = (String)((MenuItem)sender).Header;
            setFilterButtonText(dropDownFilterName);
            onlyCheckColumn(dropDownFilterName);
        }
        //Sidebar - Suggestion clicked
        private void suggestionSet(object sender, RoutedEventArgs e)
        {
            filterTextBox.Text = (String)(sender as MenuItem).Header;
        }
        //Sidebar - Key pressed while contextmenu is in focus
        private void suggestionKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) filterTextBox.Text = (String)(sender as MenuItem).Header;
            else
            {
                filterTextBox.Focus();
                timer1.Start();
            }
        }
        //Sidebar - Open the contextmenu after user stops typing
        private void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            timer1.Stop();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                openContextMenu();
            }));
        }
        //Datagrid - Event fires when scrolled in the datagrid
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            long totalHits = 0;
            if (queryBuilder != null) totalHits = queryBuilder.getLastResponseHits();
            int displayedHits = mainDataGrid.Items.Count;

            if (displayedHits > 0 && currentPage * appSettings.defaultRecords + displayedHits < totalHits  && scrollViewer.ContentVerticalOffset + scrollViewer.ViewportHeight > mainDataGrid.Items.Count - 1)
            {
                bottomStatusText.Text = "Getting Data";
                Console.WriteLine("Reached end of page, retrieving new data.");
                currentScrollOffset++;
                new Task(appendScroll).Start();
            }
        }

        //---- LEFT CLICK LISTENERS ----
        //Topbar - Main - Get data button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (appSettings == null || database == null) setSettings(new AppSettings());
            if (database.isValid())
            {
                bottomStatusText.Text = "Getting Data";
                new Task(getAllData).Start();
            }
            else MessageBox.Show("No connection to a database!");
        }
        //Topbar - Pages - Left button page selection
        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage != 0)
            {
                currentPage -= 1;
                updatePageCount();
                bottomStatusText.Text = "Getting Data";
                new Task(updatePageDataGrid).Start();
            }
        }
        //Topbar - Pages - Right button page selection
        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < ((int)(queryBuilder.getLastResponseHits() / appSettings.defaultRecords)))
            {
                currentPage += 1;
                updatePageCount();
                bottomStatusText.Text = "Getting Data";
                new Task(updatePageDataGrid).Start();
            }
        }
        //Topbar - Settings - Open settings window
        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            bottomStatusText.Text = "Settings Open";
            SettingsWindow settingsWindow = new SettingsWindow(appSettings);
            settingsWindow.ShowDialog();
            setSettings(settingsWindow.getAppSettings());
            bottomStatusText.Text = "Ready";
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
                    bottomStatusText.Text = "Getting Data";
                    new Task(updatePageDataGrid).Start();
                }
                else MessageBox.Show("End date is earlier then start date!");

            }
            else MessageBox.Show("Dates not valid!");
        }
        //Topbar - Clear Filter - Clicked the clear filter button
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Right clicked on filter!");
            if (queryBuilder != null) queryBuilder.setTimeBoundsDefault();
            fromTimeDate.Text = "";
            toTimeDate.Text = "";
            new Task(updatePageDataGrid).Start();
        }
        //Topbar - Extra - Clicking the INFO button in the extra contextmenu
        private void infoMenu_Click(object sender, RoutedEventArgs e)
        {
            bottomStatusText.Text = "Info Open";
            InfoWindow infoWindow = new InfoWindow();
            infoWindow.ShowDialog();
            bottomStatusText.Text = "Ready";
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
            bottomStatusText.Text = "Analyze Open";
            AnalyzeWindow analyzeWindow = new AnalyzeWindow();
            analyzeWindow.ShowDialog();
            bottomStatusText.Text = "Ready";
        }
        //Topbar - Extra - Clicking the GLOBAL SEARCH button in the extra contextmenu
        private void globalSearchMenu_Click(object sender, RoutedEventArgs e)
        {
            GlobalSearchWindow globalSearchWindow = new GlobalSearchWindow();
            bottomStatusText.Text = "Global Search Open";
            globalSearchWindow.ShowDialog();
            if (globalSearchWindow.getSearch() != "") {
                currentPage = 0;
                filterOnColumnName("global", globalSearchWindow.getSearch());
            }
            bottomStatusText.Text = "Ready";
        }
        //Topbar - Extra - Clicking the LOGSTASH button in the extra contextmenu
        private void logStashMenu_Click(object sender, RoutedEventArgs e)
        {
            LogManagerWindow logManagerWindow = new LogManagerWindow();
            bottomStatusText.Text = "Log Manager Open";
            logManagerWindow.ShowDialog();
            if (logManagerWindow.succesParse())
            {
                System.Windows.Forms.DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Logparser finished, do you want to copy the server settings to the current log viewer?", "Parser finished!", System.Windows.Forms.MessageBoxButtons.YesNo);
                if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    appSettings.defaultIndex = logManagerWindow.mainIndex;
                    appSettings.elasticip = logManagerWindow.elasticIp;
                }
            }
            bottomStatusText.Text = "Ready";
        }

        //Datagrid - Contextmenu 'filter on'
        private void filterOnMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Filtering on: " + rightClickColumnName + " : " + rightClickContent);
            currentPage = 0;
            if(rightClickColumnName == "" || rightClickContent == "")
            {
                Console.WriteLine("Filtering on empty string! Ignoring...");
            } else
            {
                bottomStatusText.Text = "Getting Data";
                new Task(() => { filterOnColumnName(rightClickColumnName, rightClickContent); }).Start();
                setFilter(rightClickColumnName, rightClickContent);
            } 
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
            currentPage = 0;
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
        //Sidebar - Previous filter
        private void previousFilterButton_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 0;
            returnToPreviousQuery();
        }

        

        //---- RIGHT CLICK LISTENERS ----
        //Topbar - Right clicked the page counter in top right
        private void pageLabel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            PageNumberWindow pageWindow = new PageNumberWindow();
            pageWindow.ShowDialog();
            setPageNumber(pageWindow.getPageNumber());
        }

        

        //Topbar - Right clicked the filter button (set time to default)
        private void filterButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Right clicked on filter!");
            if (queryBuilder != null) queryBuilder.setTimeBoundsDefault();
            fromTimeDate.Text = "";
            toTimeDate.Text = "";
            new Task(updatePageDataGrid).Start();
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
            if (sender as TextBox != null) (sender as TextBox).Text = "";
            else (sender as Xceed.Wpf.Toolkit.DateTimePicker).Text = "";
        }

    }
}
