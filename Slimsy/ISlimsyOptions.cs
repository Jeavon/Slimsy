using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slimsy
{
    public interface ISlimsyOptions
    {
        string Format { get; set; }
        string BackgroundColor { get; set; }
        int DefaultQuality { get; set; }
        int MaxWidth { get; set; }
        int WidthStep { get; set; }
        string DomainPrefix { get; set; }
    } 
}
