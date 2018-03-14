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
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace TGH_Log_Viewer
{
    /// <summary>
    /// Interaction logic for LogManagerWindow.xaml
    /// </summary>
    public partial class LogManagerWindow : Window
    {
        String path;
        int numberOfFiles;

        public LogManagerWindow()
        {
            InitializeComponent();
        }

        private void selectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select the folder where the logfiles are stored.";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    path = dialog.SelectedPath;
                    Console.WriteLine("Path: " + path);
                    checkForTextFiles(path);
                }
            }
        }

        private void checkForTextFiles(String path)
        {
            string[] txtFiles = System.IO.Directory.GetFiles(path, "*.txt");
            foreach (String txtFile in txtFiles) Console.WriteLine("File found: " + txtFile);
            filesFoundLabel.Visibility = Visibility.Visible;
            numberFilesFoundLabel.Content = txtFiles.Count();
            numberOfFiles = txtFiles.Count(); 
            numberFilesFoundLabel.Visibility = Visibility.Visible;
        }

        private bool checkIp(string host, int port, TimeSpan timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    if (!success)
                    {
                        return false;
                    }
                    client.EndConnect(result);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private bool checkIndex(String index)
        {
            Regex pattern = new Regex("^[a-z\\d][+\\-_a-z\\d]{1,255}$");
            if (pattern.IsMatch(index)) return true;
            return false;
        }

        private void runScriptButton_Click(object sender, RoutedEventArgs e)
        {
            bool aok = true;
            String ip = elasticIpBox.Text;
            int port = parseFieldNumber(elasticPortBox);
            String index = indexTextBox.Text;

            if (numberOfFiles == 0)
            {
                aok = false;
                folderCheckMark.Visibility = Visibility.Hidden;
                MessageBox.Show("No text files found! Choose another folder.");
            }
            else folderCheckMark.Visibility = Visibility.Visible;

            if (!checkIp(ip, port, new TimeSpan(0, 0, 1)))
            {
                aok = false;
                ipCheckMark.Visibility = Visibility.Hidden;
                MessageBox.Show("No connection to the server!");
            }
            else ipCheckMark.Visibility = Visibility.Visible;

            if (!checkIndex(index))
            {
                aok = false;
                indexCheckMark.Visibility = Visibility.Hidden;
                MessageBox.Show("Index name is not valid!");
            }
            else indexCheckMark.Visibility = Visibility.Visible;

            if (aok)
            {
                Console.WriteLine("Executing script!");
            }
        }

        private void mouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBox).Text = "";
        }

        private int parseFieldNumber(TextBox field)
        {
            int j;
            if (Int32.TryParse(field.Text, out j))
            {
                return j;
            }
            else return 9200;
        }
    }
}
