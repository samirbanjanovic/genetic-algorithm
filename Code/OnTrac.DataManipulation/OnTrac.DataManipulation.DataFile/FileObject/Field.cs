using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.DataManipulation.DelimitedDataFile.FileObject
{
    public class Field
    {
        public Field(string name,int index, object value, int size)
        {
            this.Name = name;
            this.Value = value;
            this.Index = index;
            this.Size = size;
        }
        public Field(string name, int index, object value)
            :this(name,index,value, value == null ? 1 :  value.ToString().Length)
        { }
        
        public string Name { get; private set; }
        public object Value { get; set; }
        public int Index { get; private set; }
        public int Size { get; private set; }

        public override string ToString()
        {
            return this.ToString(false);
        }

        public string ToString(bool isFixedWidth)
        {
            if(isFixedWidth)
            {
                return this.Value == null ? string.Empty.PadRight(this.Size) : Value.ToString().PadRight(this.Size);
            }
            else
            {
                return this.Value == null ? string.Empty : this.Value.ToString();
            }
        }
    }
}
