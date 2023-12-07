using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.DataFile
{
    [Serializable]
    public class DelimitedDataFileWithHeader
    {
        private string _batchText;
        
        public DelimitedDataFileWithHeader(CharDelimitedRecord header, IList<CharDelimitedRecord> records)
        {
            this.Header = header;
            this.Records = records;

            this._batchText = this.BuildBatchText();
        }

        public CharDelimitedRecord Header { get; private set; }
        public IList<CharDelimitedRecord> Records { get; private set; }

        protected string BuildBatchText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(this.Header.ToString());

            foreach (var r in this.Records)
                sb.AppendLine(r.ToString());

            return sb.ToString();
        }

        public override string ToString()
        {
            return this._batchText;    
        }
    }


}
