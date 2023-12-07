using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.Concurrent;

namespace OnTrac.DataFile
{
    public static class DelimitedDataFileExtensions
    {        
        internal static XElement BuildXNode(CharDelimitedRecord record, string rootName, string[] itemLayout)
        {
            XElement element = new XElement(rootName);

            if (itemLayout == null)
            {
                for (int n = 0; n < record.Items.Count; n++)
                    element.Add(new XElement("default_name_" + n, record.Items[n]));
            }
            else
            {
                for (int n = 0; n < itemLayout.Count(); n++)
                    element.Add(new XElement(itemLayout[n].Trim(), record.Items[n]));
            }
            
            return element;
        }

        public static IEnumerable<XElement> ToXmlEnumerable(this DelimitedDataFileBatch dataFile, string[] headerRecordLayout = null, string[] detailRecordLayout = null, string datasetRootName = "DataSetRoot", string datasetHeaderName = "DataSetHeader", string datasetDetailName = "DataSetDetail", bool runAsync = false)
        {
            IEnumerable<XElement> batchSet = null;

            if (runAsync)
            {
                batchSet = new ConcurrentBag<XElement>();
                Parallel.ForEach(dataFile.Batches, current =>
                    {
                        XElement eventSet = new XElement(datasetRootName);
                        eventSet.Add(DelimitedDataFileExtensions.BuildXNode(current.Header, datasetHeaderName, headerRecordLayout));
                        foreach (var record in current.Records)
                            eventSet.Add(DelimitedDataFileExtensions.BuildXNode(record, datasetDetailName, detailRecordLayout));

                        ((ConcurrentBag<XElement>)batchSet).Add(eventSet);
                    });
            }
            else
            {
                batchSet = new List<XElement>();
                foreach (var file in dataFile.Batches)
                {
                    XElement eventSet = new XElement(datasetRootName);
                    eventSet.Add(DelimitedDataFileExtensions.BuildXNode(file.Header, datasetHeaderName, headerRecordLayout));
                    foreach (var record in file.Records)
                        eventSet.Add(DelimitedDataFileExtensions.BuildXNode(record, datasetDetailName, detailRecordLayout));

                    ((IList<XElement>)batchSet).Add(eventSet);
                }
            }
            
            return batchSet;
        }

        // use generic to be definition of the file; enum defines the field names and order
        public static string ToXml(this DelimitedDataFileBatch dataFile, string[] headerRecordLayout = null, string[] detailRecordLayout = null, string documentRootName = "BatchSetRoot",string datasetRootName = "DataSetRoot", string datasetHeaderName = "DataSetHeader", string datasetDetailName = "DataSetDetail", bool runAsync = false)
        {
            XElement batchSetXml = new XElement(documentRootName);
            IEnumerable<XElement> batchSet = dataFile.ToXmlEnumerable(headerRecordLayout, detailRecordLayout, datasetRootName, datasetHeaderName, datasetDetailName, runAsync);
            foreach (var set in batchSet)
                batchSetXml.Add(set);

            return batchSetXml.ToString();
        }
     
        public static IEnumerable<XElement> ToXmlEnumerable(this DelimitedDataFile datafile, string[] recordLayout =  null, string datasetRootName = "DatasetRoot", bool runAsync = false)
        {
            IEnumerable<XElement> xmlRecords;

            if (runAsync)
            {
                xmlRecords = new ConcurrentBag<XElement>();
                Parallel.ForEach(datafile.Records, record =>
                    {
                        ((ConcurrentBag<XElement>)xmlRecords).Add(DelimitedDataFileExtensions.BuildXNode(record, datasetRootName, recordLayout));
                    });
            }
            else
            {
                xmlRecords = new List<XElement>();
                foreach (var record in datafile.Records)
                    ((IList<XElement>)xmlRecords).Add(DelimitedDataFileExtensions.BuildXNode(record, datasetRootName, recordLayout));
            }
            
            return xmlRecords;
        }

        public static string ToXml(this DelimitedDataFile datafile, string[] recordLayout = null, string documentRootName = "DocumentRoot",string datasetRootName = "DatasetRoot", bool runAsync = false)
        {
            XElement setXml = new XElement(documentRootName);
            IEnumerable<XElement> dataSet = datafile.ToXmlEnumerable(recordLayout, datasetRootName, runAsync);
            foreach (var set in dataSet)
                setXml.Add(set);

            return setXml.ToString();
        }
    }
}
