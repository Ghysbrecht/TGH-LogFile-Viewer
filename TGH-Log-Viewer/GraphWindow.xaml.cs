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
    public partial class GraphWindow : Window
    {
        int numberOfBars = 30;
        int[] barsArray;
        DateTime startDate, endDate;

        public GraphWindow()
        {
            InitializeComponent();
        }
        //Create a graph from logdata
        public void createFromData(IReadOnlyCollection<LogLine> loglines)
        {
            barsArray = new int[numberOfBars];
            startDate = loglines.First().timestamp;
            endDate = loglines.Last().timestamp;

            foreach (var logline in loglines)
            {
                incrementAtPercentage(calculateTimePercentage(startDate, endDate, logline.timestamp));
            }
            printArray();
            refresh();
        }
        //Increment the intigers in the main array to count the bar length
        private void incrementAtPercentage(double percentage)
        {
            barsArray[(int)((numberOfBars - 1) * percentage)] += 1;
        }
        //Refresh the bargraph with the current window size and data
        public void refresh()
        {
            if(barsArray != null)
            {
                int maxValue = maxIntArray();
                barGrid.Children.Clear();
                for (int i = 0; i < numberOfBars; i++)
                {
                    barGrid.Children.Add(createBar((double)barsArray[i] / (double)maxValue, i, numberOfBars));
                }
            }
        }
        //Returns the maximum value in the array
        private int maxIntArray()
        {
            int max = 0;
            foreach(int value in barsArray)
            {
                if (value > max) max = value;
            }
            return max;
        }

        //Create a bar (percentage determense the height, barNumber is the count of what bar it is in relation to totalBars)
        private Rectangle createBar(double percentage, int barNumber, int totalBars)
        {
            Rectangle rectangle = new Rectangle();
            int barWidth = (int)barGrid.ActualWidth / totalBars;
            rectangle.Width = barWidth;
            rectangle.Height = (double)barGrid.ActualHeight * percentage;

            //Set the margin to barnumber * barwidth
            Thickness margin = rectangle.Margin;
            margin.Left = barNumber * barWidth;
            rectangle.Margin = margin;

            rectangle.VerticalAlignment = VerticalAlignment.Bottom;
            rectangle.HorizontalAlignment = HorizontalAlignment.Left;

            rectangle.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0D2C49"));
            rectangle.Stroke = new SolidColorBrush(Colors.White);


            return rectangle;
        }
        //Triggered when the window is resized
        private void barGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            refresh();
        }
        //Calculate the percentage in refence to the start and endtime
        private double calculateTimePercentage(DateTime startTime, DateTime endTime, DateTime calcTime)
        {
            return (calcTime.Subtract(startTime).TotalSeconds) / (endTime.Subtract(startTime).TotalSeconds);
        }
        //Print the array for debugging
        private void printArray()
        {
            for (int i = 0; i < numberOfBars; i++)
            {
                Console.WriteLine("Value " + i + " : " + barsArray[i]);
            }
        }
    }
}
