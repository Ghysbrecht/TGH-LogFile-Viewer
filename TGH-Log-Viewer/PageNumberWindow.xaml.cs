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
    public partial class PageNumberWindow : Window
    {
        int pageNumber = 1;

        public PageNumberWindow()
        {
            InitializeComponent();
            numberBox.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            getNumber();
        }

        private void getNumber()
        {
            int j;
            if (Int32.TryParse(numberBox.Text, out j))
            {
                if (j == 0) j = 1;
                pageNumber = j;
                this.Close();
            }
            else
            {
                MessageBox.Show("Number is invalid!");
            }
        }

        private void numberBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) getNumber();
        }

        public int getPageNumber()
        {
            return pageNumber;
        }
    }
}
