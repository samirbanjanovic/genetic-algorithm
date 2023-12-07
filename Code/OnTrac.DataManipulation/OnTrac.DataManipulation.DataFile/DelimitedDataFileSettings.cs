using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.DataFile
{
    [Serializable]
    public class DelimitedDataFileSettings
    {
        public char Delimiter { get; set; }        
        public bool RemoveQuotes { get; set; }
        public bool IgnoreFirstLine { get; set; }
        public bool RecordsGroupedByHeader { get; set; }        
        public string HeaderIdentifier { get; set; }
        public string[] DetailIdentifier { get; set; }
    }
}
