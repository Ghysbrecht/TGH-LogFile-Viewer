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
using System.IO;
using System.Diagnostics;
using System.Timers;

namespace TGH_Log_Viewer
{
    /// <summary>
    /// Interaction logic for LogManagerWindow.xaml
    /// </summary>
    public partial class LogManagerWindow : Window
    {
        
        int numberOfFiles;

        String path = "C:\\Users\\ThomasGH\\Documents\\Project\\Logfiles\\CompleteList";
        String logStasherPath = "C:\\Users\\ThomasGH\\Documents\\Project\\Playground\\MyTool Single Conf";
        String elasticIp, mainIndex;
        string[] txtFiles;

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
                    pathTextBox.Text = path;
                    checkForTextFiles(path);
                }
            }
        }
        //Check if there are any text files in the given path
        private void checkForTextFiles(String path)
        {
            if (Directory.Exists(path))
            {
                txtFiles = System.IO.Directory.GetFiles(path, "*.txt");
                foreach (String txtFile in txtFiles) Console.WriteLine("File found: " + txtFile);
                filesFoundLabel.Visibility = Visibility.Visible;
                numberFilesFoundLabel.Content = txtFiles.Count();
                numberOfFiles = txtFiles.Count();
                numberFilesFoundLabel.Visibility = Visibility.Visible;
            }
            else MessageBox.Show("The entered path does not exist!");
        }
        //Check if the ip addres îs valid and the port is open
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
        //Check if the index is following the rules
        private bool checkIndex(String index)
        {
            Regex pattern = new Regex("^[a-z\\d][+\\-_a-z\\d]{1,255}$");
            if (pattern.IsMatch(index)) return true;
            return false;
        }

        //When clicked on the run script button
        private void runScriptButton_Click(object sender, RoutedEventArgs e)
        {
            bool aok = true;
            String ip = elasticIpBox.Text;
            int port = parseFieldNumber(elasticPortBox);
            String index = indexTextBox.Text;

            checkForTextFiles(path);
            if (numberOfFiles == 0) aok = disablePart(folderCheckMark, "No text files found! Choose another folder.");
            else folderCheckMark.Visibility = Visibility.Visible;

            if (!checkIfScriptPresent(logStasherPath)) aok = disablePart(scriptCheckMark, "Script not found!");
            else scriptCheckMark.Visibility = Visibility.Visible;

            if (!checkIp(ip, port, new TimeSpan(0, 0, 1))) aok = disablePart(ipCheckMark, "No connection to the server!");
            else ipCheckMark.Visibility = Visibility.Visible;

            if (!checkIndex(index)) aok = disablePart(indexCheckMark, "Index name not valid");
            else indexCheckMark.Visibility = Visibility.Visible;

            if (aok)
            {
                elasticIp = ip + ":" + port;
                mainIndex = index;
                Console.WriteLine("Executing script!");
                prepareScript();
            }
        }

        private bool disablePart(Image image, String error)
        {
            image.Visibility = Visibility.Hidden;
            MessageBox.Show(error);
            return false;
        }
        //Empty the textbox when doubleclicking in it.
        private void mouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBox).Text = "";
        }
        //Parse the textbox to an intiger
        private int parseFieldNumber(TextBox field)
        {
            int j;
            if (Int32.TryParse(field.Text, out j))
            {
                return j;
            }
            else return 9200;
        }
        //Prepare for execution of the main script
        private void prepareScript()
        {
            runScriptButton.IsEnabled = false;
            //Copy the files to the destination
            if (Directory.Exists(logStasherPath + "\\LogFiles"))
            {
                Console.WriteLine("LogStash folder exists!");
                //If not empty, empty it...
                if (Directory.EnumerateFileSystemEntries(logStasherPath + "\\LogFiles").Any())
                {
                    Console.WriteLine("...but not empty. Emptying...");
                    infoLabel.Content = "Emptying LogFiles folder...";
                    deleteFiles(new DirectoryInfo(logStasherPath + "\\LogFiles"));
                }
            }
            else
            {
                Console.WriteLine("LogFile folder does not exist! Ceating one...");
                Directory.CreateDirectory(logStasherPath + "\\LogFiles");
            }

            infoLabel.Content = "Copying textfiles...";
            foreach (String file in txtFiles)
            {
                Console.WriteLine("Copying: " + file);
                File.Copy(file, logStasherPath + "\\LogFiles\\" + file.Substring(file.LastIndexOf('\\') + 1), false);
            }
            infoLabel.Content = "Executing script...";
            executeScript(logStasherPath, elasticIp, mainIndex);
            infoLabel.Content = "Done!";
            runScriptButton.IsEnabled = true;
        }

        //Open directory explorer to select de script folder
        private void selectScriptButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select the folder where the script is stored.";
                dialog.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    logStasherPath = dialog.SelectedPath;
                    Console.WriteLine("Script Path: " + logStasherPath);
                }
            }
        }

        //Check if the script is present
        private bool checkIfScriptPresent(String path)
        {
            string[] psFiles = System.IO.Directory.GetFiles(path, "*.ps1");
            foreach (String psFile in psFiles)
            {
                if (psFile.Substring(psFile.LastIndexOf('\\') + 1) == "MainScriptSingle.ps1") return true;
            }
            return false;
        }
        //Delete all the files in the directory
        private void deleteFiles(System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles())
                file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories())
                directory.Delete(true);
        }
        //Check if the entered path is ok
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            path = pathTextBox.Text;
            checkForTextFiles(path);
        }
        //When doubleclicking on the indexlabel, copy the folder name
        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (path != null) indexTextBox.Text = (path.Substring(path.LastIndexOf('\\') + 1)).ToLower();
        }

        //Execute the script
        private void executeScript(String path, String ipPort, String index)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            String argument = "Set-ExecutionPolicy Unrestricted;cd \'" + path + "\';.\\MainScriptSingle.ps1 -server "+ ipPort + " -index " + index;
            Console.WriteLine("Starting with: " + argument);
            start.Arguments = argument;
            start.FileName = "powershell";
            start.Verb = "runas";

            using(Process proc = Process.Start(start))
            {
                proc.WaitForExit();
                Console.WriteLine("Script exited with code: " + proc.ExitCode);
            }

        }
    }
}
