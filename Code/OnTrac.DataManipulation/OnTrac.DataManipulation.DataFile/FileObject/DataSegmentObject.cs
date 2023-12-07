using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  OnTrac.DataManipulation.DelimitedDataFile.FileObject
{
    public class DataSegmentObject<TEnum> 
        : IDataSegmentObject where TEnum : struct, IConvertible
    {
        public DataSegmentObject()
            : this(null)
        { }
        
        public DataSegmentObject(object parent)
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("T shall be an enum");
            }

            this.Parent = parent;

            this.FieldDelimiter = ",";
            this.LineTerminator = Environment.NewLine;
            this.FieldEncapsulator = string.Empty;

            this.Offspring = new List<IDataSegmentObject>();
            this.FieldNames = Enum.GetNames(typeof(TEnum)).ToList();
            this.BuildFieldElements();
        }

        #region Private Methods
        private void BuildFieldElements()
        {
            this.Fields = new List<Field>(); 
            for(int n = 0; n < this.FieldNames.Count; n++)
            {
                this.Fields.Add(new Field(this.FieldNames[n], n, string.Empty));
            }
        }
        #endregion

        #region Public Members

        public object Parent { get; set; }
        public string FieldDelimiter { get; set; }
        public string LineTerminator { get; set; }
        public string FieldEncapsulator { get; set; }

        public IList<IDataSegmentObject> Offspring { get; private set; }
        public IList<string> FieldNames { get; private set; }
        public IList<Field> Fields { get; private set; }

        public object this[string fieldName]
        {
            get
            {
                return this.GetValue(fieldName);
            }
            set
            {
                this.SetValue(fieldName, value);
            }
        }

        public object this[int fieldIndex]
        {
            get
            {
                return this.GetValue(fieldIndex);
            }
            set
            {
                this.SetValue(fieldIndex, value);
            }
        }

        public object GetValue(string fieldName)
        {
            return this.GetValue(this.FieldNames.IndexOf(fieldName));
        }

        public bool SetValue(string fieldName, object value)
        {
            return this.SetValue(this.FieldNames.IndexOf(fieldName), value);
        }
        
        public object GetValue(int index)
        {
            if (index >= 0 && index < this.Fields.Count)
                return this.Fields[index].Value;

            return null;
        }

        public bool SetValue(int index, object value)
        {
            if (index >= 0 && index < this.Fields.Count)
            {
                this.Fields[index].Value = value;
                return true;
            }

            return false;
        }

        public Field GetField(string fieldName)
        {
            return this.GetField(this.FieldNames.IndexOf(fieldName));
        }

        public Field GetField(int index)
        {
            if (index >= 0 && index < this.Fields.Count)
                return this.Fields[index];

            return null;
        }
        
        public void AddOffspring(IDataSegmentObject offspring)
        {
            this.Offspring.Add(offspring);
        }

        public void RemoveOffspring(IDataSegmentObject offspring)
        {
            this.Offspring.Remove(offspring);
        }

        public DataSet ToDataSet()
        {
            DataSet ds = new DataSet();
            ds.Tables.Add();

            foreach (var f in this.Fields)
            {
                ds.Tables[0].Columns.Add(f.Name);
                ds.Tables[0].Columns[f.Name].DataType = f.Value.GetType();
            }

            return ds;
        }

        public override string ToString()
        {
            // create "this" .ToString()
            string thisString = string.Join(this.FieldDelimiter, this.Fields) + this.LineTerminator;
            
            string offspringStrings = string.Empty;
            if (this.Offspring.Count > 0)            
                offspringStrings = string.Concat(this.Offspring);
                        
            string complete = string.Concat(thisString, offspringStrings);

            return complete;
        }
        #endregion
        
    }
}
