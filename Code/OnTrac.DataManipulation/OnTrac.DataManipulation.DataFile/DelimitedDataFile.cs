using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.DataFile
{
    public class DelimitedDataFile
        : DelimitedDataFileBase
    {
        private bool _multiThreadProcessing;

        public DelimitedDataFile(string file, DelimitedDataFileSettings settings, bool multiThreadProcessing = false)
            : base(file, settings)
        {
            this._multiThreadProcessing = multiThreadProcessing;
            this.ProcessFile();
        }

        public IEnumerable<CharDelimitedRecord> Records { get; private set; }

        public int RecordCount
        {
            get
            {
                return this.Records.Count();
            }
        }

        protected override void ProcessFile()
        {
            if (!this._readerSettings.RecordsGroupedByHeader)
                this.Records = CharDelimitedFileReader.GranulizeCharDelimitedFile(this.OriginalFile, this._readerSettings.Delimiter, this._readerSettings.IgnoreFirstLine, this._readerSettings.RemoveQuotes, this._multiThreadProcessing);
        }
    }
}
