using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.DataFile
{
    public static class CharDelimitedFileReader
    {
        internal static readonly string[] NewLineDelimiter = { "\r\n", "\n", "\r" };

        public static CharDelimitedRecord GranulizeCharDelimitedRecord(string record, char delimiter, bool ignoreQuotes = false)
        {
            return new CharDelimitedRecord(record, delimiter, 0,ignoreQuotes);
        }

        internal static IEnumerable<CharDelimitedRecord> GranulizeCharDelimitedFile(string document, char delimiter, bool ignoreFirstRecord = false, bool ignoreQuotes = false, bool unorganizedParallel = false)
        {
            IEnumerable<CharDelimitedRecord> fieldCollection;
            IList<string> records = document.Split(CharDelimitedFileReader.NewLineDelimiter, StringSplitOptions.None);

            if (records.Count() < 1)
            {
                return null;
            }
            else
            {
                if (unorganizedParallel)
                {
                    fieldCollection = new ConcurrentBag<CharDelimitedRecord>();

                    int startAt = ignoreFirstRecord ? 1 : 0;
                    Parallel.For(startAt, records.Count, i =>
                        {
                            ((ConcurrentBag<CharDelimitedRecord>)fieldCollection).Add(new CharDelimitedRecord(records[i], delimiter, i + 1, ignoreQuotes));
                        });
                }
                else
                {
                    fieldCollection = new List<CharDelimitedRecord>();
                    for (int i = ignoreFirstRecord ? 1 : 0; i < records.Count; i++)
                    {
                        ((IList<CharDelimitedRecord>)fieldCollection).Add(new CharDelimitedRecord(records[i], delimiter, i + 1,ignoreQuotes));
                    }
                }
            }

            return fieldCollection;
        }

        internal static IEnumerable<DelimitedDataFileWithHeader> DebatchGranulizeCharDelimitedFileWithHeaders(string document, string headerIdentifier, string[] detailIdentifier, char delimiter, bool ignoreQuotes = false, bool ignoreFirstRecord = false)
        {
            int batchIndex = -1;
            int lineIndex = 0;
            string line;

            bool hasHeaderDef = headerIdentifier != null;
            bool hasDetailDef = detailIdentifier != null;

            DelimitedDataFileWithHeader headerRecords;

            IList<DelimitedDataFileWithHeader> debatchedSets = new List<DelimitedDataFileWithHeader>();
            StringReader debatcher = new StringReader(document);

            if (hasHeaderDef)
            {// we know a definition for                 
                while ((line = debatcher.ReadLine()) != null)
                {
                    if (lineIndex == 0 && ignoreFirstRecord)
                    {
                        lineIndex++;
                        continue;
                    }

                    if (headerIdentifier == line.Substring(0, headerIdentifier.Length))
                    {
                        headerRecords = new DelimitedDataFileWithHeader(new CharDelimitedRecord(line, delimiter, lineIndex + 1, ignoreQuotes, headerIdentifier == null ? null : new string[] { headerIdentifier }), new List<CharDelimitedRecord>());
                        debatchedSets.Add(headerRecords);
                        batchIndex++;
                    }
                    else if (!string.IsNullOrWhiteSpace(line) && line != string.Empty)
                    {
                        debatchedSets[batchIndex].Records.Add(new CharDelimitedRecord(line, delimiter, lineIndex + 1, ignoreQuotes, detailIdentifier));
                    }

                    lineIndex++;
                }
            }
            else if (hasDetailDef)
            {
                while ((line = debatcher.ReadLine()) != null)
                {
                    if (lineIndex == 0 && ignoreFirstRecord)
                    {
                        lineIndex++;
                        continue;
                    }

                    if (detailIdentifier.Where(item => item == (line.Substring(0, detailIdentifier[0].Length))).FirstOrDefault() != null)
                    {
                        debatchedSets[batchIndex].Records.Add(new CharDelimitedRecord(line, delimiter, lineIndex + 1, ignoreQuotes, detailIdentifier));
                    }
                    else
                    {
                        headerRecords = new DelimitedDataFileWithHeader(new CharDelimitedRecord(line, delimiter, lineIndex + 1, ignoreQuotes, headerIdentifier == null ? null : new string[] { headerIdentifier }), new List<CharDelimitedRecord>());
                        debatchedSets.Add(headerRecords);
                        batchIndex++;
                    }

                    lineIndex++;
                }
            }
            else
                throw new Exception("Cannot debatch file. Unable to distinguish between records");

            debatcher.Dispose();

            return debatchedSets;
        }
    }
}
