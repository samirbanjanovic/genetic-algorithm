using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.DataManipulation.FileBuilder
{
    public sealed class Field
    {
        public Field(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }
        
        public string Name { get; private set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
