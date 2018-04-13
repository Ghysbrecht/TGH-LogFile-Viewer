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
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            InitializeComponent();
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            String executingPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            executingPath = executingPath.Remove(executingPath.LastIndexOf('\\'));
            string[] pdfFiles = System.IO.Directory.GetFiles(executingPath , "*.pdf");
            if (pdfFiles.Length > 0)
            {
                Uri pdf = new Uri(pdfFiles.First(), UriKind.RelativeOrAbsolute);
                try
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = pdf.LocalPath;
                    process.Start();
                }
                catch (Exception error)
                {
                    MessageBox.Show("Could not open the PDF -> " + error + "\n\nTried to open: " + pdf.LocalPath);
                }
            }
            else Console.WriteLine("No PDF files found to open, tried to find @ " + executingPath);
        }
    }
}
