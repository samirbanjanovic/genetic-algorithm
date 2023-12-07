using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.DataFile
{
    [Serializable]
    public abstract class DelimitedDataFileBase
    {
        protected DelimitedDataFileSettings _readerSettings;

        public DelimitedDataFileBase(string file, DelimitedDataFileSettings settings)
        {
            if (settings.Delimiter == default(char))
                throw new ArgumentException("Record Delimiter has not be specified");

            if (settings.RecordsGroupedByHeader)
            {
                if (settings.HeaderIdentifier == null && settings.DetailIdentifier == null)
                    throw new ArgumentException("To perform proper debatch either a detail (array of details), or header, or both identifiers have to be specified");
            }
            
            this._readerSettings = settings;
            this.OriginalFile = file;
        }

        public string OriginalFile { get; private set; }

        protected virtual void ProcessFile()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.OriginalFile;
        }
    }
}
