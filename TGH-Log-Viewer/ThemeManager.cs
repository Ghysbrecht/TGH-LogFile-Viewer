using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TGH_Log_Viewer
{
    
    public class ThemeManager
    {
        bool stop = false;
        String[] colorValues = {
            "DarkAccentColor",
            "DarkDefaultColor",
            "LightDefaultColor",
            "MainTextColor",
            "SubTextColor",
            "MainOutlineColor",
            "SubOutlineColor",
            "MainDataGridColor",
            "SubDataGridColor",
            "BottomBarColor",
            "BottomBarTextColor"
        };

        public void setTheme(int number)
        {
            try
            {
                foreach (String colorValue in colorValues)
                {
                    Application.Current.Resources[colorValue] = Application.Current.FindResource(colorValue + number);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR! -> " + e.Message);
            }
        }

        public void random()
        {
            while (!stop)
            {
                Random r = new Random();
                foreach (String colorValue in colorValues) Application.Current.Resources[colorValue] = new SolidColorBrush(Color.FromRgb((Byte)r.Next(0, 255), (Byte)r.Next(0, 255), (Byte)r.Next(0, 255)));
                Thread.Sleep(500);
            }
        }

        public void start()
        {
            Thread thread = new Thread(random);
            thread.Start();
        }
    }
}
