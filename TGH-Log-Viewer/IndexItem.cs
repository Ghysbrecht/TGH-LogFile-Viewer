using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGH_Log_Viewer
{
    public class IndexItem : INotifyPropertyChanged
    {
        public String index { get; set; }
        public int count { get; set; }
        public String size { get; set; }


        public IndexItem(String index, String count, String size)
        {
            this.index = index;
            this.count = toInteger(count);
            this.size = size;
        }

        public IndexItem() : this("", "0", "0b"){ }

        private int toInteger(String stringVar)
        {
            int j;
            if (Int32.TryParse(stringVar, out j)) return j;
            else
            {
                return -1;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
