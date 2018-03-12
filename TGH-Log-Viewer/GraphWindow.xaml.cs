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
        String graphType = "bar";
        IReadOnlyCollection<LogLine> loglines;

        public GraphWindow()
        {
            InitializeComponent();
        }
        //Create a graph from logdata
        public void createFromData(IReadOnlyCollection<LogLine> loglines)
        {
            this.loglines = loglines;
            refresh();
        }
        private void createBarsArray()
        {
            if(loglines != null)
            {
                barsArray = new int[numberOfBars];
                startDate = loglines.First().timestamp;
                endDate = loglines.Last().timestamp;

                foreach (var logline in loglines)
                {
                    incrementAtPercentage(calculateTimePercentage(startDate, endDate, logline.timestamp));
                }
            }
        }
        //Increment the intigers in the main array to count the bar length
        private void incrementAtPercentage(double percentage)
        {
            barsArray[(int)((numberOfBars - 1) * percentage)] += 1;
        }
        //Refresh the bargraph with the current window size and data
        public void refresh()
        {
            retrieveSettings();
            createBarsArray();
            if (barsArray != null)
            {
                int maxValue = maxIntArray();
                maxBarNumber.Content = "" + maxValue;
                Line[] lineArray = createLineArray(barsArray, numberOfBars);
                barGrid.Children.Clear();
                for (int i = 0; i < numberOfBars; i++)
                {
                    if(graphType == "bar")barGrid.Children.Add(createBar((double)barsArray[i] / (double)maxValue, i, numberOfBars));
                    if(graphType == "line")barGrid.Children.Add(lineArray[i]);
                }
                updateBottomBar();
                
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

        //Create the array with lines
        private Line[] createLineArray(int[] barValues, int numberOfBars)
        {
            Line[] lineArray = new Line[numberOfBars];

            int[] xCoArray = new int[numberOfBars];
            int[] yCoArray = new int[numberOfBars];

            int yRef = (int)barGrid.ActualHeight;
            int xRef = 0;

            double gridHeight = barGrid.ActualHeight;
            double gridWidth = barGrid.ActualWidth;

            int maxBarNumber = maxIntArray();

            for(int i=0; i<numberOfBars; i++)
            {
                xCoArray[i] = xRef + (int)(i * (gridWidth / numberOfBars) + ((gridWidth / numberOfBars) / 2.0));
                yCoArray[i] = yRef - (int)((barValues[i] / (double)maxBarNumber) * gridHeight);

                if (i == 0) lineArray[i] = createLine(xRef, xCoArray[i], yRef, yCoArray[i]);
                else lineArray[i] = createLine(xCoArray[i-1], xCoArray[i], yCoArray[i-1], yCoArray[i]);
            }

            return lineArray;
        }
        //Create a line object with the given start and endpoint
        private Line createLine(int x1, int x2, int y1, int y2)
        {
            Line line = new Line();

            line.X1 = x1;
            line.X2 = x2;
            line.Y1 = y1;
            line.Y2 = y2;

            line.Stroke = new SolidColorBrush(Colors.White);
            line.StrokeThickness = 2;

            return line;
        }

        //Create a bar (percentage determines the height, barNumber is the count of what bar it is in relation to totalBars)
        private Rectangle createBar(double percentage, int barNumber, int totalBars)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Width = barGrid.ActualWidth / totalBars;
            rectangle.Height = (double)barGrid.ActualHeight * percentage;

            //Set the margin to barnumber * barwidth
            Thickness margin = rectangle.Margin;
            margin.Left = barNumber * (barGrid.ActualWidth / totalBars);
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
        private void printArray(int[] array)
        {
            Console.WriteLine("Printing array...");
            for (int i = 0; i < numberOfBars; i++)
            {
                Console.WriteLine("Value " + i + " : " + array[i]);
            }
        }
        private void updateBottomBar()
        {
            startLabel.Content = "\u25B7 " + startDate.ToUniversalTime();
            endLabel.Content = endDate.ToUniversalTime() + " \u25C1";

            TimeSpan totalTime = endDate.Subtract(startDate);
            totalLabel.Content = buildTimeSpanString(totalTime);

            double barSeconds = endDate.Subtract(startDate).TotalSeconds / (double)numberOfBars;
            TimeSpan barDateTime = new TimeSpan(0, 0, (int)barSeconds);
            singleBarLabel.Content = buildTimeSpanString(barDateTime);
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            refresh();
        }
        private void retrieveSettings()
        {
            numberOfBars = (int)datapointNumberBox.Value;
            if (barRadioButton.IsChecked == true) graphType = "bar";
            else graphType = "line";
        }

        private String buildTimeSpanString(TimeSpan timeSpan)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if(timeSpan.TotalDays > 365) stringBuilder.Append(Math.Round(timeSpan.TotalDays / 365,0) + "y ");
            if (timeSpan.TotalDays > 0) stringBuilder.Append(Math.Round(timeSpan.TotalDays % 365,0) + "d ");
            stringBuilder.Append(Math.Round(timeSpan.TotalHours % 24,0) + "h " + Math.Round(timeSpan.TotalMinutes % 60,0) + "m " + Math.Round(timeSpan.TotalSeconds % 60,0) + "s ");
            Console.WriteLine(stringBuilder.ToString());
            return stringBuilder.ToString();
        }
    }
}
