using OopFactory.X12.Parsing;
using OopFactory.X12.Parsing.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.Messaging.Parsers
{
    public class EDIParser
    {
        public IEnumerable<string> ToMultipleXml(string input)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            Stream data = new MemoryStream(buffer, 0, buffer.Length);

            return this.ToMultipleXml(data);
        }

        public IEnumerable<string> ToMultipleXml(Stream input)
        {
            if (input == null)
                throw new ArgumentNullException("No data to parse");

            X12Parser parser = new X12Parser();
            IList<Interchange> interchange = parser.ParseMultiple(input);
            IList<Interchange> transactions = parser.UnbundleByTransaction(interchange[0]);
            ConcurrentBag<string> ediXmls = new ConcurrentBag<string>();
            Parallel.ForEach(transactions, current =>
                {
                    ediXmls.Add(current.Serialize(true));
                });

            return ediXmls;
        }

        public string ToBatchXml(string input)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            Stream data = new MemoryStream(buffer, 0, buffer.Length);

            return this.ToBatchXml(data);
        }

        public string ToBatchXml(Stream input)
        {
            if (input == null)
                throw new ArgumentNullException("No data to parse");            

            X12Parser parser = new X12Parser();
            string ediXml = parser.ParseMultiple(input)[0].Serialize(true);

            return ediXml;
        }


    }
}