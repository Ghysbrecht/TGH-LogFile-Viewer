using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGH_Log_Viewer.Resources;

namespace TGH_Log_Viewer
{

    public class FileExclusion : INotifyPropertyChanged
    {
        public String rule { get; set; }
 
        public IEnums.ExclusionType type { get; set; }

        public FileExclusion() : this("", IEnums.ExclusionType.Term)
        {
           
        }
        public FileExclusion(String _rule, IEnums.ExclusionType exclusionType)
        {
            rule = _rule;
            type = exclusionType;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
