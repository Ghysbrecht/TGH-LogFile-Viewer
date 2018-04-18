﻿using System;
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
        public int numberOfBars { get; set;}
        int[] barsArray;
        DateTime startDate, endDate;
        public String graphType { get; set; }
        IReadOnlyCollection<LogLine> loglines;
        bool restartInDockBool = false;
        bool restartInWindowBool = false;
        bool isDocked = false;
        Func<bool> mainMethod;

        public GraphWindow(int barNumber = 30, String type = "bar")
        {
            InitializeComponent();
            numberOfBars = barNumber;
            graphType = type;

            datapointNumberBox.Value = (short)numberOfBars;
            if (graphType == "bar") barRadioButton.IsChecked = true;
            else lineRadioButton.IsChecked = true;
        }
        //Create a graph from logdata
        public void createFromData(IReadOnlyCollection<LogLine> loglines)
        {
            this.loglines = loglines;
            refresh();
        }
        private void createBarsArray()
        {
            if(loglines != null && loglines.Count() > 0)
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
            int index = (int)((numberOfBars - 1) * percentage);
            if (index >= 0 && index < numberOfBars) barsArray[index] += 1;
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
                if (graphType == "line") barGrid.Children.Add(lineArray[numberOfBars]);
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
            Line[] lineArray = new Line[numberOfBars + 1];

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

            lineArray[numberOfBars] = createLine(xCoArray[numberOfBars - 1], (int)gridWidth, yCoArray[numberOfBars - 1], yRef);

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

            long timeDiff = (endDate.Subtract(startDate).Ticks)/totalBars;
            DateTime startTimeBar = startDate.Add(new TimeSpan(timeDiff * barNumber));
            DateTime endTimeBar = startDate.Add(new TimeSpan(timeDiff * (barNumber+1)));

            ToolTip tooltip = new ToolTip();

            tooltip.Content = "Start time = "+ startTimeBar.ToString("yyy-MM-dd HH:mm:ss.fff") +"\nStop time = " + endTimeBar.ToString("yyy-MM-dd HH:mm:ss.fff");
            rectangle.ToolTip = tooltip;
            
            
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

            TimeSpan barDateTime = new TimeSpan(endDate.Subtract(startDate).Ticks / numberOfBars );
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
            stringBuilder.Append(Math.Round(timeSpan.TotalHours % 24,0) + "h " + Math.Round(timeSpan.TotalMinutes % 60,0) + "m " + Math.Round(timeSpan.TotalSeconds % 60,3) + "s ");
            return stringBuilder.ToString();
        }

        public Boolean restartInDock()
        {
            if (restartInDockBool)
            {
                restartInDockBool = false;
                return true;
            }
            else return false;
        }
        public Boolean restartInWindow()
        {
            if (restartInWindowBool)
            {
                restartInWindowBool = false;
                return true;
            }
            else return false;
        }
        public void setDocked(bool status)
        {
            isDocked = status;
        }

        public bool getDocked()
        {
            return isDocked;
        }

        private void dockButton_Click(object sender, RoutedEventArgs e)
        {
            if (isDocked) {
                restartInWindowBool = true;
                setDockButton(false);
                mainMethod();
            } 
            else
            {
                //this.Close();
                this.Close();
                setDockButton(true);
                restartInDockBool = true;
                mainMethod();
            }
        }

        public void attachMainMethod(Func<bool> method)
        {
            mainMethod = method;
        }

        public void setDockButton(bool undock)
        {
            if (undock) dockButton.Content = "UNDOCK";
            else dockButton.Content = "DOCK";
        }
    }
}
