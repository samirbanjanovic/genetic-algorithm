using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.DataFile
{
    [Serializable]
    public class DelimitedDataFileBatch
        : DelimitedDataFileBase
    {
        public DelimitedDataFileBatch(string file, DelimitedDataFileSettings settings)
            : base(file, settings)
        {
            this.ProcessFile();        
        }

        public IEnumerable<DelimitedDataFileWithHeader> Batches { get; private set; }

        public int BatchCount
        {
            get
            {
                return this.Batches.Count();
            }
        }

        protected override void ProcessFile()
        {
            if (this._readerSettings.RecordsGroupedByHeader)
                this.Batches = CharDelimitedFileReader.DebatchGranulizeCharDelimitedFileWithHeaders(this.OriginalFile, this._readerSettings.HeaderIdentifier, this._readerSettings.DetailIdentifier, this._readerSettings.Delimiter, this._readerSettings.RemoveQuotes, _readerSettings.IgnoreFirstLine);
        }
    }
}
