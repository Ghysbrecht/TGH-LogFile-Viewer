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
using TGH_Log_Viewer.Resources;
using static TGH_Log_Viewer.Resources.IEnums;

namespace TGH_Log_Viewer
{
    public partial class ExclusionWindow : Window
    {
        List<FileExclusion> list;
        LLQueryBuilder queryBuilder;
        AppSettings settings;

        public ExclusionWindow(LLQueryBuilder queryBuilder, AppSettings appSettings)
        {
            InitializeComponent();
            this.queryBuilder = queryBuilder;

            settings = appSettings;
            if (settings.exclusions == null) list = new List<FileExclusion>();
            else list = settings.exclusions;

            exclusionsGrid.ItemsSource = list;
            updateData();
        }
        //Get a list of all the logfiles that are still included
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            updateData();
        }

        private void updateData()
        {
            if (queryBuilder != null)
            {
                filesList.Items.Clear();
                List<LogFile> logFiles = queryBuilder.getLogFiles(list);
                foreach (LogFile logFile in logFiles)
                {
                    filesList.Items.Add(logFile.fileName.PadRight(30) + "(" + logFile.count + ")");
                }
            }
            else MessageBox.Show("No database connection!");
        }
        //Listener to right click remove a line
        private void removeLine_Click(object sender, RoutedEventArgs e)
        {
            if(exclusionsGrid.SelectedItem != null)
            {
                FileExclusion exclusion = exclusionsGrid.SelectedItem as FileExclusion;
                if(exclusion != null)
                {
                    Console.WriteLine("Removing item: " + exclusion.type + "   " + exclusion.rule);
                    list.Remove(exclusion);
                    refreshRuleList();
                }
            }
        }
        //Add a new rule when clicking hte add button
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            addNewRule();
        }
        //Bind the enum values to the combobox when the window is loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            typeBox.ItemsSource = Enum.GetValues(typeof(ExclusionType)).Cast<ExclusionType>();
            typeBox.SelectedIndex = 0;
        }
        //Refresh the rule list
        private void refreshRuleList()
        {
            exclusionsGrid.ItemsSource = null;
            exclusionsGrid.ItemsSource = list;
        }
        //When entering in the textbox add the rule
        private void ruleBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) addNewRule();
        }
        //Add the rule by retrieving the values from the boxes
        private void addNewRule()
        {
            Object type = typeBox.SelectedItem;
            if (type != null)
            {
                list.Add(new FileExclusion(ruleBox.Text, (ExclusionType)type));
                refreshRuleList();
            }
            else MessageBox.Show("No type selected!");
        }
        //Add the rule that excludes bak files
        private void excludeBakButton_Click(object sender, RoutedEventArgs e)
        {
            list.Add(new FileExclusion("*_BAK", IEnums.ExclusionType.Wildcard));
            refreshRuleList();
        }
        //Save the list and close the window
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            settings.exclusions = list;
            this.Close();
        }
        //Retrieve the settings
        public AppSettings getSettings()
        {
            return settings;
        }
        //Add  a file to the exclusion list when double clicking on it
        private void filesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (filesList.SelectedItem != null) {
                String selected = filesList.SelectedItem.ToString();
                selected = selected.Remove(selected.IndexOf("(") - 1).TrimEnd(' ');
                list.Add(new FileExclusion(selected, IEnums.ExclusionType.Term));
                refreshRuleList();
            }
        }
        //Set the current settings as default by saving them to a file
        private void defaultButton_Click(object sender, RoutedEventArgs e)
        {
            settings.exclusions = list;
            settings.saveToFile();
        }
    }
}
