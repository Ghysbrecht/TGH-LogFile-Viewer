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
    public partial class ThemeWindow : Window
    {

        ThemeManager manager;
        int theme;
        AppSettings settings;

        public ThemeWindow(ThemeManager manager, AppSettings appSettings)
        {
            InitializeComponent();
            this.manager = manager;
            settings = appSettings;
            theme = settings.themeNumber;
            checkRadioButton(theme);
        }

        private void Radio_Checked(object sender, RoutedEventArgs e)
        {
            String name = (sender as RadioButton).Name;
            String number = name.Substring(name.IndexOf('o') + 1);
            theme = toInteger(number);
            manager.setTheme(theme);
            settings.themeNumber = theme;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            settings.saveToFile();
            this.Close();
        }

        private int toInteger(String value)
        {
            int j;
            if (Int32.TryParse(value, out j)) return j;
            else return -1;
        }

        private void checkRadioButton(int number)
        {
            RadioButton button = (RadioButton)this.FindName("Radio"+number);
            button.IsChecked = true;
        }

        private void Button_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P) manager.start(); 
        }
    }
}
