using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelingSalesPerson;


namespace K_GraphColoring
{
    public class K_Graph_GA_Init
    {
        private int _defaultColorAvailability;
        public K_Graph_GA_Init(string filePath)
        {            
            // start with k = 2               
            this._defaultColorAvailability = 2;
            this.NumberOfColorsToUse = this._defaultColorAvailability;
            this.NodeData = K_Graph_GA_Init.ReadDataFile(filePath);
            
            this.AssignInitialNode();
        }

        public int NumberOfColorsToUse { get; set; }
        public IDictionary<int, Node> NodeData { get; set; }

        public void RestColorAvailabilityCount()
        {
            this.NumberOfColorsToUse = this._defaultColorAvailability;
        }

        #region Init Methods
        private void CreateRandomConnections()
        {
            // random starting point
            var random = new Random();
            var start = random.Next(1, this.NodeData.Values.Count + 1);

            var current = this.NodeData[start];

            // create random connections for start
            for (int n = 0; n < this.NumberOfColorsToUse - 1; )
            {
                var nr = random.Next(1, this.NodeData.Values.Count + 1);
                var next = this.NodeData[nr];

                if (nr != start && !current.Connections.Contains(next))
                {
                    current.Connections.Add(next);

                    next.Connections.Add(current);

                    n++;
                }
            }

            // connect everyone else
            foreach (var node in this.NodeData.Values)
            {// connect everyone else
                // random number of connections to create
                var cc = random.Next(1, this.NumberOfColorsToUse - 1);

                for(int i = node.Connections.Count; i < cc; )
                //for (int i = node.Connections.Count; i < this.ColorLimit - 2; )
                {
                    var nr = random.Next(1, this.NodeData.Values.Count + 1);
                    var next = this.NodeData[nr];

                    if(next != node && 
                        next.Connections.Count < this.NumberOfColorsToUse - 1 && 
                        !node.Connections.Contains(next) && 
                        node.Connections.Count < (this.NumberOfColorsToUse - 1) - node.Connections.Count)
                    {
                        node.Connections.Add(next);

                        if (next.Connections == null)
                            next.Connections = new List<Node>();

                        next.Connections.Add(node);

                        i++;
                    }
                }
            
            }


        }

        private void AssignInitialNode()
        {
            var maxConnections = this.NodeData.Values.Max(x => x.Connections.Count);
            var maxList = this.NodeData.Values.Where(x => x.Connections.Count == maxConnections).OrderBy(x => x.Id).ToList();
            maxList[0].IsInitialNode = true;            
        }
        #endregion Init Methods

        public static IDictionary<int, Node> ReadDataFile(string filePath)
        {
            var dataLines = System.IO.File.ReadAllLines(filePath);
            
            bool hasCM = dataLines.Contains("Connection Matrix");

            var nl = new Dictionary<int, Node>();

            int lineIndex = 0;
            do
            {
                var dl = dataLines[lineIndex].Split(' ');

                var id = int.Parse(dl[0]);
                var x = double.Parse(dl[1]);
                var y = double.Parse(dl[2]);

                var node = new Node(id, x, y);

                nl.Add(id, node);

            } while (++lineIndex < dataLines.Length && dataLines[lineIndex].Length > 0 && dataLines[lineIndex] != null);

            if (hasCM)
            {
                // process connection matrix
                for (int j = lineIndex + 2, cnId = 1; j < dataLines.Length; j++, cnId++)
                {
                    // current node
                    var cn = nl[cnId];

                    var cml = dataLines[j].Normalize().Split('\t');
                    for (int c = 0; c < cml.Length; c++)
                    {
                        if (cml[c] == "1")
                        {
                            var nodeToConnect = nl[c + 1];

                            if (!cn.Connections.Contains(nodeToConnect))
                            {
                                cn.Connections.Add(nodeToConnect);
                                nodeToConnect.Connections.Add(cn);
                            }
                        }
                    }
                }
            }


            return nl;
        }
    }
}
