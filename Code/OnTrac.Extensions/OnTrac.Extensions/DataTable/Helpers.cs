using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace OnTrac.Extensions.DataTable
{
    public static class Helpers
    {
        public static string ToFlatFile(this System.Data.DataTable dataTable, string fieldDelimiter, string recordDelimiter, bool isFixedWidth)
        {
            IList<string> records = new List<string>();

            foreach (DataRow row in dataTable.Rows)
            {
                string record;
                if(isFixedWidth)
                {
                    IList<string> paddedValues = new List<string>();
                    foreach(DataColumn dc in dataTable.Columns)
                    {
                        string value = row[dc].ToString().PadLeft(dc.MaxLength);
                    }

                    record = string.Join(fieldDelimiter, paddedValues);
                }
                else
                {
                    record = string.Join(fieldDelimiter, row.ItemArray);
                }
                
                records.Add(record);
            }

            var doc = string.Join(recordDelimiter, records);
            return doc;
        }
    }
}
