using System;
using System.Collections.Generic;
namespace OnTrac.DataManipulation.DelimitedDataFile.FileObject
{
    public interface IDataSegmentObject
    {
        object Parent { get; set; }

        object this[int fieldIndex] { get; set; }
        object this[string fieldName] { get; set; }

        IList<string> FieldNames { get; }
        IList<Field> Fields { get; }
        Field GetField(int index);
        Field GetField(string fieldName);

        object GetValue(int index);
        object GetValue(string fieldName);        
        
        bool SetValue(int index, object value);
        bool SetValue(string fieldName, object value);

        IList<IDataSegmentObject> Offspring { get; }
        void AddOffspring(IDataSegmentObject offspring);        
        void RemoveOffspring(IDataSegmentObject offspring);

        string LineTerminator { get; set; }
        string FieldDelimiter { get; set; }

        string ToString();
    }
}
