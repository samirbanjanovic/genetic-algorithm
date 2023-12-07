using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace OnTrac.DataManipulation.DelimitedDataFile.FileObject
{
    public class DataObject
    {
        public DataObject()
            : this(new List<string>())
        { }
        
        public DataObject(IList<string> columnNames)
        {

        }

        public IList<string> ColumnNames { get; private set; }
    }
}
