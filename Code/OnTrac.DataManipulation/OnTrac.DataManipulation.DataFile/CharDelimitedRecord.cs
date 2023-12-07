using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.DataFile
{
    [Serializable]
    public class CharDelimitedRecord
    {
        public CharDelimitedRecord(string text, char delimiter, int recordIndex = 0, bool ignoreQuotes = false, string[] potentialIdentifyingValue = null)
        {
            this.RecordIndex = recordIndex;
            this.PotentialIdentifyingValue = potentialIdentifyingValue;
            this.Text = text;

            if(ignoreQuotes)
            {
                this.Text = this.Text.Replace("\"", "");
            }
                
            this.Items = this.Text.Split(new string[] { delimiter.ToString() }, StringSplitOptions.None);
        }

        public string Text { get; private set; }
        public IList<string> Items { get; private set; }
        public int RecordIndex { get; set; }
        public string[] PotentialIdentifyingValue { get; private set; }

        public override string ToString()
        {
            return this.Text;
        }
    }
}
