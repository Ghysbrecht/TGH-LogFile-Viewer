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

    public partial class GlobalSearchWindow : Window
    {
        String searchTerm;

        public GlobalSearchWindow()
        {
            InitializeComponent();
            searchTerm = "";
            searchTermBox.Focus();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                setSearch();
            }
        }

        private void setSearch()
        {
            searchTerm = searchTermBox.Text;
            this.Close();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            setSearch();
        }

        public String getSearch()
        {
            return searchTerm;
        }
    }
}
