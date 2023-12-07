using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesPerson
{
    public static class ProcessTSPDocument
    {
        public static IList<string> GetDataLines(string filePath)
        {
            return System.IO.File.ReadAllLines(filePath);
        }
        
        
        /// <summary>
        /// Reads in the TSP file
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="filePath">File path.</param>
        public static IList<Node> GetNodes(IList<string> dataLines)
        {
            var nl = new List<Node>();
            // parses data segment of file
            for (int n = 7; n < dataLines.Count; n++)
            {
                var data = dataLines[n].Split(' ');

                // perform conversions
                var id = Int32.Parse(data[0]);
                var x = double.Parse(data[1]);
                var y = double.Parse(data[2]);

                nl.Add(new Node(id, x, y));
            }

            return nl;
        }


        /// <summary>
        /// Reads in the TSP file
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="filePath">File path.</param>
        public static IDictionary<int, Node> GetNodesDictionary(IList<string> dataLines)
        {
            var nl = new Dictionary<int, Node>();
            
            // parses data segment of file
            for (int n = 7; n < dataLines.Count; n++)
            {
                var data = dataLines[n].Split(' ');

                // perform conversions
                var id = Int32.Parse(data[0]);
                var x = double.Parse(data[1]);
                var y = double.Parse(data[2]);

                nl.Add(id,new Node(id, x, y));
            }

            return nl;
        }
    }
}
