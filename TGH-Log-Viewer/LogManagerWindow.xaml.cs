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
    public partial class LogManagerWindow : Window
    {
        
        int numberOfFiles;

        String logfilesPath = "";
        String logStasherPath = "";
        String remoteToolPath = "";

        String elasticIp, mainIndex;
        string[] txtFiles;

        public LogManagerWindow()
        {
            InitializeComponent();
            setPaths(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        }


        // ------- GENERAL -------
        //Error thrower when a step is incorrect
        private bool disablePart(Image image, String error)
        {
            image.Visibility = Visibility.Hidden;
            MessageBox.Show(error);
            return false;
        }
        //Parse the textbox to an integer
        private int parseFieldNumber(TextBox field)
        {
            int j;
            if (Int32.TryParse(field.Text, out j))
            {
                return j;
            }
            else return 9200;
        }
        //Delete all the files in the directory
        private void deleteFiles(System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles())
                file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories())
                directory.Delete(true);
        }
        //Set the default path depending on the executing directory
        private void setPaths(String executingPath)
        {
            executingPath = executingPath.Remove(executingPath.LastIndexOf('\\'));
            logStasherPath = executingPath + "\\LocalLogStash";
            remoteToolPath = executingPath + "\\RemoteLogStash";
        }


        // ------- SPECIAL -------
        //Prepare for execution of the main local script
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
        //Execute the local script
        private void executeScript(String path, String ipPort, String index)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            String argument = "Set-ExecutionPolicy Unrestricted;cd \'" + path + "\';.\\MainScriptSingle.ps1 -server "+ ipPort + " -index " + index;
            Console.WriteLine("Starting with: " + argument);
            start.Arguments = argument;
            start.FileName = "powershell";
            start.Verb = "runas";

            try
            { 
                using(Process proc = Process.Start(start))
                {
                    proc.WaitForExit();
                    Console.WriteLine("Script exited with code: " + proc.ExitCode);
                }
            } catch (System.ComponentModel.Win32Exception e)
            {
                MessageBox.Show("ERROR! " + e.Message);
            }

}
        //Prepare for execution of the remote script
        private void prepareRScript()
        {
            runRScriptButton.IsEnabled = false;
            infoRLabel.Content = "Executing script...";
            executeRScript(remoteToolPath, logfilesPath, mainIndex);
            infoRLabel.Content = "Done!";
            runRScriptButton.IsEnabled = true;
        }
        //Execute the remote script
        private void executeRScript(String path, String logFilePath, String index)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            String argument = "Set-ExecutionPolicy Unrestricted;cd \'" + path + "\';.\\MainScript.ps1 -bypassui";
            argument += " -elasticIndex \'" + index + "\'";
            argument += " -sourceLogfilePath \'" + logFilePath + "\\*\'";
            if (targetLogfilePathCheck.IsChecked == true) argument += " -targetLogfilePath \'" + targetLogfilePath.Text + "\'";
            if (targetScriptPathCheck.IsChecked == true) argument += " -targetScriptPath \'" + targetScriptPath.Text + "\'";
            if (targetServerCheck.IsChecked == true) argument += " -targetServer\'" + targetServer.Text + "\'";
            if (targetServerIpCheck.IsChecked == true) argument += " -targetServerIp \'" + targetServerIp.Text + "\'";
            if (targetServerUserCheck.IsChecked == true) argument += " -targetServerUser \'" + targetServerUser.Text + "\'";
            if (remoteElasticCheck.IsChecked == true) {
                argument += " -elasticServer \'" + remoteElasticIp.Text + "\'";
                argument += " -elasticPort \'" + remoteElasticPort.Text + "\'";
            }
            //argument += ";Read-Host";
            Console.WriteLine("Starting with: " + argument);
            start.Arguments = argument;
            start.FileName = "powershell";
            start.Verb = "runas";
            try
            {
                using (Process proc = Process.Start(start))
                {
                    proc.WaitForExit();
                    Console.WriteLine("Script exited with code: " + proc.ExitCode);
                }
            } catch (System.ComponentModel.Win32Exception e)
            {
                MessageBox.Show("ERROR! " + e.Message);
            }
        }

        // ------- DATACHECKS -------
        //Check if there are any text files in the given path
        private void checkForTextFiles(String path, Label numberLabel)
        {
            if (Directory.Exists(path))
            {
                txtFiles = System.IO.Directory.GetFiles(path, "*.txt");
                foreach (String txtFile in txtFiles) Console.WriteLine("File found: " + txtFile);
                numberLabel.Content = txtFiles.Count();
                numberOfFiles = txtFiles.Count();
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
        //Check if the script is present
        private bool checkIfScriptPresent(String path, String filename)
        {
            if (Directory.Exists(path))
            {
                string[] psFiles = System.IO.Directory.GetFiles(path, "*.ps1");
                foreach (String psFile in psFiles)
                {
                    if (psFile.Substring(psFile.LastIndexOf('\\') + 1) == filename) return true;
                }
            } 
            return false;
        }


        // ------- EVENTLISTENERS -------
        //Leftclick - SELECT LOG FOLDER - Select a folder where the logfiles are
        private void selectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select the folder where the logfiles are stored.";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    logfilesPath = dialog.SelectedPath;
                    Console.WriteLine("Path: " + logfilesPath);
                    if((Button)sender == selectFolderButton)
                    {
                        pathTextBox.Text = logfilesPath;
                        checkForTextFiles(logfilesPath, numberFilesFoundLabel);
                    } else
                    {
                        pathRTextBox.Text = logfilesPath;
                        checkForTextFiles(logfilesPath, numberRFilesFoundLabel);
                    }
                }
            }
        }
        //Leftclick - SELECT SCRIPT FOLDER - Open directory explorer to select de script folder
        private void selectScriptButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select the folder where the script is stored.";
                dialog.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    if ((Button)sender == selectScriptButton) logStasherPath = dialog.SelectedPath;
                    else remoteToolPath = dialog.SelectedPath;
                    Console.WriteLine("Script Path: " + dialog.SelectedPath);
                }
            }
        }
        //Leftclick - CHECK - Check if the entered path is ok
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if((Button)sender == checkButton)
            {
                logfilesPath = pathTextBox.Text;
                checkForTextFiles(logfilesPath, numberFilesFoundLabel);
            } else
            {
                logfilesPath = pathRTextBox.Text;
                checkForTextFiles(logfilesPath, numberRFilesFoundLabel);
            }
            
        }
        //Leftclick - RUN LOCAL SCRIPT - When clicked on the run script button
        private void runScriptButton_Click(object sender, RoutedEventArgs e)
        {
            bool aok = true;
            String ip = elasticIpBox.Text;
            int port = parseFieldNumber(elasticPortBox);
            String index = indexTextBox.Text;

            checkForTextFiles(logfilesPath, numberFilesFoundLabel);
            if (numberOfFiles == 0) aok = disablePart(folderCheckMark, "No text files found! Choose another folder.");
            else folderCheckMark.Visibility = Visibility.Visible;

            if (!checkIfScriptPresent(logStasherPath, "MainScriptSingle.ps1")) aok = disablePart(scriptCheckMark, "Script not found!");
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
        //Leftclick - RUN REMOTE SCRIPT - When clicked on the run script button
        private void runRScriptButton_Click(object sender, RoutedEventArgs e)
        {
            bool aok = true;
            String index = indexRTextBox.Text;

            checkForTextFiles(logfilesPath, numberRFilesFoundLabel);
            if (numberOfFiles == 0) aok = disablePart(folderRCheckMark, "No text files found! Choose another folder.");
            else folderRCheckMark.Visibility = Visibility.Visible;

            if (!checkIfScriptPresent(remoteToolPath, "MainScript.ps1")) aok = disablePart(scriptRCheckMark, "Script not found!");
            else scriptRCheckMark.Visibility = Visibility.Visible;

            if (!checkIndex(index)) aok = disablePart(indexRCheckMark, "Index name not valid");
            else indexRCheckMark.Visibility = Visibility.Visible;

            if (aok)
            {
                mainIndex = index;
                Console.WriteLine("Executing remote script!");
                prepareRScript();
            }
        }

        //Doubleclick - When doubleclicking on the indexlabel, copy the folder name
        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (logfilesPath != null) {
                String folderName = (logfilesPath.Substring(logfilesPath.LastIndexOf('\\') + 1)).ToLower();
                indexTextBox.Text = folderName;
                indexRTextBox.Text = folderName;
            }
        }
        //Doubleclick - Empty the textbox when doubleclicking in it.
        private void mouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBox).Text = "";
        }
    }
}
