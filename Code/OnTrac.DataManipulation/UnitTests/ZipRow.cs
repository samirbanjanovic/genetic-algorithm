using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.Messaging.UnitTests
{
    public class ZipRow
    {
        public string Zip { get; private set; }
        public IList<string> ZoneMatrixList { get; private set; }

        public ZipRow(string row)
        {
        //    this.Zip = zip;
        //    this.ZoneMatrixList = new List<string>();
        }
    }
}
