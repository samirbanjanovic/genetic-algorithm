using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.DataManipulation.FileBuilder
{
    public class DataObject<T> where T : struct, IConvertible
    {
        public static string DefaultDelimiter { get { return ","; } }
        public static string DefaultLineTerminator { get { return Environment.NewLine; } }
                
        public object Parent { get; set; }
        public object Offspring { get; set; }

        private Dictionary<string, Field> _items;

        public IList<Field> Fields;
        
        public DataObject()
            : this(null, null)
        { }
        
        public DataObject(object parent)
            : this(parent, null)
        { }

        public DataObject(object parent, object child)
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("Expecting T to be enum");
            }

            this.Parent = parent;
            this.Offspring = child;

            this._items = new Dictionary<string,Field>();
            Enum.GetNames(typeof(T))
                .Select
                    (n =>
                        {
                            this._items.Add(n, new Field(n, null));
                            return n;

                        }).ToList();

            this.Fields = this._items.Values.Select(v => v).ToList();
        }

        /// <summary>
        /// true = added
        /// false = not added
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddValue(string field, object value)
        {
            if (this._items.ContainsKey(field))
            {
                Field f = this._items[field];
                f.Value = value;

                return true;
            }

            return false;
        }

        public object GetValue(string field)
        {
            Field value = null;
            this._items.TryGetValue(field, out value);

            return value.Value;
        }

        public override string ToString()
        {
            return this.ToString(DataObject<T>.DefaultDelimiter, DataObject<T>.DefaultLineTerminator);
        }

        public string ToString(string fieldDelimiter)
        {
            return this.ToString(fieldDelimiter, DataObject<T>.DefaultLineTerminator);
        }
        
        public string ToString(string fieldDelimiter, string lineTerminator)
        {
            return string.Join(fieldDelimiter, this.Fields.Select(v => v.Value)) + lineTerminator;
        }


    }
}
