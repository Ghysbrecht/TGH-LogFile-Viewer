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
        public int numberOfBars { get; set;}
        int[] barsArray;
        DateTime startDate, endDate;
        public String graphType { get; set; }
        IReadOnlyCollection<LogLine> loglines;
        bool restartInDockBool = false;
        bool restartInWindowBool = false;
        bool isDocked = false;
        Func<bool> mainMethod;
        Func<String,bool> filterMethod;
        Point downLocation, upLocation;
        Rectangle selectionRect;

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
        //Create the array that determense the heights of each bar/line
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
        //Increment the integers in the main array to count the bar length
        private void incrementAtPercentage(double percentage)
        {
            int index = (int)(numberOfBars * percentage);
            if (index == numberOfBars) index = numberOfBars - 1;
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

                if (selectionRect != null) {
                    selectionRect.Height = barGrid.ActualHeight;
                    barGrid.Children.Add(selectionRect);
                }
            }
        }
        //Update the bottom bar with the date info
        private void updateBottomBar()
        {
            startLabel.Content = "\u25B7 " + startDate.ToUniversalTime();
            endLabel.Content = endDate.ToUniversalTime() + " \u25C1";

            TimeSpan totalTime = endDate.Subtract(startDate);
            totalLabel.Content = buildTimeSpanString(totalTime);

            TimeSpan barDateTime = new TimeSpan(endDate.Subtract(startDate).Ticks / numberOfBars );
            singleBarLabel.Content = buildTimeSpanString(barDateTime);
        }
        //Get the settings from the textfield & radiobuttons
        private void retrieveSettings()
        {
            numberOfBars = (int)datapointNumberBox.Value;
            if (barRadioButton.IsChecked == true) graphType = "bar";
            else graphType = "line";
        }

        // ------------ CALCULATORS/BUILDERS ----------------
        //Return a string with the two filter time bounds after selection
        private String calculateFilterDates()
        {
            TimeSpan timeDiff = endDate.Subtract(startDate);

            double startperc, stopperc;

            if (downLocation.X > upLocation.X)
            {
                startperc = upLocation.X / barGrid.ActualWidth;
                stopperc = downLocation.X / barGrid.ActualWidth;
            }
            else
            {
                stopperc = upLocation.X / barGrid.ActualWidth;
                startperc = downLocation.X / barGrid.ActualWidth;
            }

            DateTime startTimeSel = startDate.AddMilliseconds(timeDiff.TotalMilliseconds * startperc);
            DateTime endTimeSel = startDate.AddMilliseconds(timeDiff.TotalMilliseconds * stopperc);

            String response = "Start time = " + startTimeSel.ToString("yyy-MM-dd HH:mm:ss.fff") + "\nStop time = " + endTimeSel.ToString("yyy-MM-dd HH:mm:ss.fff");

            return response;
        }
        //Build a string that represents a timeSpan
        private String buildTimeSpanString(TimeSpan timeSpan)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (timeSpan.TotalDays > 365) stringBuilder.Append(Math.Round(timeSpan.TotalDays / 365, 0) + "y ");
            if (timeSpan.TotalDays > 0) stringBuilder.Append(Math.Round(timeSpan.TotalDays % 365, 0) + "d ");
            stringBuilder.Append(Math.Round(timeSpan.TotalHours % 24, 0) + "h " + Math.Round(timeSpan.TotalMinutes % 60, 0) + "m " + Math.Round(timeSpan.TotalSeconds % 60, 3) + "s ");
            return stringBuilder.ToString();
        }
        //Calculate the percentage in refence to the start and endtime
        private double calculateTimePercentage(DateTime startTime, DateTime endTime, DateTime calcTime)
        {
            return (calcTime.Subtract(startTime).TotalMilliseconds) / (endTime.Subtract(startTime).TotalMilliseconds);
        }
        //Returns the maximum value in the array
        private int maxIntArray()
        {
            int max = 0;
            foreach (int value in barsArray)
            {
                if (value > max) max = value;
            }
            return max;
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

            //rectangle.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0D2C49"));
            rectangle.Fill = (SolidColorBrush)Application.Current.FindResource("DarkAccentColor");
            //rectangle.Stroke = new SolidColorBrush(Colors.White);
            rectangle.Stroke = (SolidColorBrush)Application.Current.FindResource("MainOutlineColor");

            long timeDiff = (endDate.Subtract(startDate).Ticks) / totalBars;
            DateTime startTimeBar = startDate.Add(new TimeSpan(timeDiff * barNumber));
            DateTime endTimeBar = startDate.Add(new TimeSpan(timeDiff * (barNumber + 1)));

            ToolTip tooltip = new ToolTip();

            tooltip.Content = "Start time = " + startTimeBar.ToString("yyy-MM-dd HH:mm:ss.fff") + "\nStop time = " + endTimeBar.ToString("yyy-MM-dd HH:mm:ss.fff");
            rectangle.ToolTip = tooltip;
            rectangle.MouseDown += barClicked;

            return rectangle;
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

            for (int i = 0; i < numberOfBars; i++)
            {
                xCoArray[i] = xRef + (int)(i * (gridWidth / numberOfBars) + ((gridWidth / numberOfBars) / 2.0));
                yCoArray[i] = yRef - (int)((barValues[i] / (double)maxBarNumber) * gridHeight);

                if (i == 0) lineArray[i] = createLine(xRef, xCoArray[i], yRef, yCoArray[i]);
                else lineArray[i] = createLine(xCoArray[i - 1], xCoArray[i], yCoArray[i - 1], yCoArray[i]);
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

            //line.Stroke = new SolidColorBrush(Colors.White);
            line.Stroke = (SolidColorBrush)Application.Current.FindResource("MainOutlineColor");
            line.StrokeThickness = 2;

            return line;
        }



        // ----------------- GETTER/SETTER ------------------
        //Retrieve if the graph needs to be restarted in a dock
        public Boolean restartInDock()
        {
            if (restartInDockBool)
            {
                restartInDockBool = false;
                return true;
            }
            else return false;
        }
        //Retrieve if the graph needs to be restarted in a window
        public Boolean restartInWindow()
        {
            if (restartInWindowBool)
            {
                restartInWindowBool = false;
                return true;
            }
            else return false;
        }
        //Set the docked state
        public void setDocked(bool status)
        {
            isDocked = status;
        }
        //Return the docked state
        public bool getDocked()
        {
            return isDocked;
        }
        //Attach the method that will be called when (un)docking
        public void attachMainMethod(Func<bool> method)
        {
            mainMethod = method;
        }
        //Attach the method that will be called when filtering on time
        public void attachFilterMethod(Func<String, bool> method)
        {
            filterMethod = method;
        }
        //Change the (un)dock button text depending on docked state
        public void setDockButton(bool undock)
        {
            if (undock) dockButton.Content = "UNDOCK";
            else dockButton.Content = "DOCK";
        }


        // ----------------- EVENTLISTENERS -----------------
        //Mouse button up - Check if end of time selection
        private void barGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            upLocation = e.GetPosition(barGrid);
            Console.WriteLine("Pointer UP at: X->" + upLocation.X + " Y->" + upLocation.Y);
            double xAxisMoved = Math.Abs(upLocation.X - downLocation.X);
            if (xAxisMoved != 0.0)
            {
                Console.WriteLine("Point dragged for " + xAxisMoved + " pixels");
                String dates = calculateFilterDates();
                filterMethod(dates);
                Console.WriteLine("Dates: " + dates);
            }
            if (selectionRect != null) barGrid.Children.Remove(selectionRect);

            selectionRect = null;
        }
        //Mouse button down - Save location
        private void barGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            downLocation = e.GetPosition(barGrid);
            Console.WriteLine("Pointer DOWN at: X->" + downLocation.X + " Y->" + downLocation.Y);
        }
        //Create a selection rectangle when dragging is detected
        private void barGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (selectionRect == null)
                {
                    Console.WriteLine("Creating a selectionRect");
                    selectionRect = new Rectangle();
                    Color selectionColor = Colors.AliceBlue;
                    selectionColor.A = 100;
                    selectionRect.Fill = new SolidColorBrush(selectionColor);
                    selectionRect.VerticalAlignment = VerticalAlignment.Top;
                    selectionRect.HorizontalAlignment = HorizontalAlignment.Left;

                    selectionRect.Name = "selectionRect";
                    barGrid.Children.Add(selectionRect);
                }
                Thickness margin = selectionRect.Margin;

                double width = e.GetPosition(barGrid).X - downLocation.X;
                selectionRect.Width = Math.Abs(width);
                if (width > 0) margin.Left = downLocation.X;
                else margin.Left = e.GetPosition(barGrid).X;

                selectionRect.Margin = margin;
                selectionRect.Height = barGrid.ActualHeight;
            }
        }
        //(Un)dock when the button is clicked
        private void dockButton_Click(object sender, RoutedEventArgs e)
        {
            if (isDocked)
            {
                restartInWindowBool = true;
                setDockButton(false);
                mainMethod();
            }
            else
            {
                this.Close();
                setDockButton(true);
                restartInDockBool = true;
                mainMethod();
            }
        }
        //Filter on bar time bounds when clicked
        private void barClicked(object sender, RoutedEventArgs e)
        {
            String tooltip = ((Rectangle)sender).ToolTip.ToString();
            filterMethod(tooltip);
        }
        //Refresh the graph when the refresh button is clicked
        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            refresh();
        }
        //Redraw when the window is resized
        private void barGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            refresh();
        }
    }
}
