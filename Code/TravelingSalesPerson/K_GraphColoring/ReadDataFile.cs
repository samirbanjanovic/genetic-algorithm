using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelingSalesPerson;
using OnTrac.Extensions.String;

namespace K_GraphColoring
{
    public static class ReadDataFile
    {
        public static Tuple<int, IList<Node>, bool> GetGraphDetailsList(string filePath)
        {
            var x = ReadDataFile.GetGraphDetails(filePath);

            return new Tuple<int, IList<Node>, bool>(x.Item1, x.Item2.Values.ToList(), x.Item3);
        }


        /// <summary>
        /// Reads in the TSP file
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="filePath">File path.</param>
        public static Tuple<int, IDictionary<int, Node>, bool> GetGraphDetails(string filePath)
        {
            var dataLines = System.IO.File.ReadAllLines(filePath);
            var colors = int.Parse(dataLines[0].Split(':')[1]);
            bool hasCM = dataLines.Contains("Connection Matrix");

            var nl = new Dictionary<int, Node>();

            int lineIndex = 1;
            do
            {
                var dl = dataLines[lineIndex].Split(' ');

                var id = int.Parse(dl[0]);
                var x = double.Parse(dl[1]);
                var y = double.Parse(dl[2]);

                var node = new Node(id, x, y);

                nl.Add(id, node);
                
            } while (++lineIndex < dataLines.Length && !dataLines[lineIndex].IsNullOrWhiteSpaceEmpty());

            if(hasCM)
            {
                // process connection matrix
                for (int j = lineIndex + 2, cnId = 1; j < dataLines.Length; j++, cnId++)
                {
                    // current node
                    var cn = nl[cnId];

                    if (cn.Connections == null)
                        cn.Connections = new List<Node>();

                    var cml = dataLines[j].Split(' ');
                    for (int c = 0; c < cml.Length; c++)
                    {
                        if (cml[c] == "1")
                        {
                            var nId = c + 1;
                            cn.Connections.Add(nl[nId]);

                            if (nl[nId].Connections == null)
                                nl[nId].Connections = new List<Node>();

                            if (!nl[nId].Connections.Contains(cn))
                                nl[nId].Connections.Add(cn);
                        }
                    }
                }
            }


            return new Tuple<int, IDictionary<int, Node>, bool> (colors, nl, hasCM);
        }
    }
}
