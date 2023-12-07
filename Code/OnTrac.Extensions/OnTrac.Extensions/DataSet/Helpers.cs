using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using OnTrac.Extensions.DataTable;

namespace OnTrac.Extensions.DataSet
{
    public static class Helpers
    {
        public static string ToXml(this System.Data.DataSet set)
        {
            XmlWriterSettings xws = new XmlWriterSettings()
            {
                Indent = false,
                OmitXmlDeclaration = true
            };

            return set.ToXml(xws);
        }

        public static string ToXml(this System.Data.DataSet set, XmlWriterSettings xws)
        {
            StringBuilder sb = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(sb, xws))
            {
                set.WriteXml(writer);
            }

            return sb.ToString();
        }

        public static string ToFlatFile(this System.Data.DataSet set, string fieldDelimiter, string recordDelimiter, bool isFixedWidth)
        {
            IList<string> records = new List<string>();

            foreach(System.Data.DataTable dataTable in set.Tables)
            {
                records.Add(dataTable.ToFlatFile(fieldDelimiter, recordDelimiter, isFixedWidth));
            }
            

            var doc = string.Join(recordDelimiter, records);
            return doc;
        }

    }
}
