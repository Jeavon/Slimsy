using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slimsy
{
    public class SlimsyOptions
    {
        public SlimsyOptions()
        {
            DomainPrefix = "https://www.umbraco.com";
            DefaultQuality = 90;
        }
        public string DomainPrefix { get; set; }

        public int DefaultQuality { get; set; }
    }
}
