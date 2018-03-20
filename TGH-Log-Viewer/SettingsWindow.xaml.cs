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

namespace TGH_Log_Viewer
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        AppSettings settings;

        public SettingsWindow(AppSettings appSettings)
        {
            if (appSettings == null) appSettings = new AppSettings();
            InitializeComponent();
            this.settings = appSettings;
            serverTextBox.Text = settings.elasticip;
            defaultIndexTextBox.Text = settings.defaultIndex;
            defaultRecordsTextBox.Text = settings.defaultRecords.ToString();
            onAutoTimeButton.IsChecked = settings.autoTime;
        }
        private void Button_Confirm(object sender, RoutedEventArgs e)
        {
            saveAll();
            this.Close();
        }
        private int parseFieldNumber(TextBox field)
        {
            int j;
            if (Int32.TryParse(field.Text, out j))
            {
                if (j == 0) j = 1;
                return j;
            }
            else return 100;
        }

        public AppSettings getAppSettings()
        {
            return settings;
        }

        private void Button_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                saveAll();
                this.Close();
            }
        }

        private void saveAll()
        {
            settings.elasticip = serverTextBox.Text;
            settings.defaultIndex = defaultIndexTextBox.Text;
            settings.defaultRecords = parseFieldNumber(defaultRecordsTextBox);
            settings.autoTime = (bool)onAutoTimeButton.IsChecked;
        }
    }
}
