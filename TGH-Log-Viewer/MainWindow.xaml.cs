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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        QueryBuilder queryBuilder = new QueryBuilder();
        Database database;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            database = new Database("localost:9200","mainindex");
            ElasticClient client = database.getClient();
            Console.WriteLine("Get all data:");
            printLogLines(queryBuilder.getAllData(client, 0, 5));
            Console.WriteLine("Get specific data:");
            printLogLines(queryBuilder.getSpecificData(client, 0, 5));
        }

        private void printLogLines(IReadOnlyCollection<LogLine> loglines)
        {
            foreach (var logline in loglines)
            {
                Console.WriteLine(logline.timestamp.ToShortDateString() + " Process: " + logline.process + " Logtype: " + logline.logtype + " Messagedata: " + logline.messagedata);
            }
        }

        

    }
}
