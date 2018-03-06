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
using System.Windows.Threading;

namespace TGH_Log_Viewer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CellValueWindow : Window
    {
        public CellValueWindow(String cellValue)
        {
            InitializeComponent();
            cellValueBox.Text = cellValue;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cellValueBox.Text != null) Clipboard.SetText(cellValueBox.Text);
            
            //Fade out copied text
            lbl.Visibility = Visibility.Visible;
            DispatcherTimer t = new DispatcherTimer();
            t.Interval = new TimeSpan(0, 0, 5);
            t.Tick += (EventHandler)delegate (object snd, EventArgs ea)
            {
                lbl.Visibility = Visibility.Collapsed;
                ((DispatcherTimer)snd).Stop();
            };
            t.Start();
        }
    }
}
