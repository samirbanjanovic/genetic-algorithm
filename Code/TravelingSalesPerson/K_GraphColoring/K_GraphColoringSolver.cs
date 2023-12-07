using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelingSalesPerson;

namespace K_GraphColoring
{
    public class K_GraphColoringSolver
    {
        public K_GraphColoringSolver(int colorLimit, IDictionary<int, Node> nodeData, bool hasConnectionMatrix)
        {
            this.ColorLimit = colorLimit;
            this.NodeData = nodeData;
            this.HasConnectionMatrix = hasConnectionMatrix;

            if (!hasConnectionMatrix)
                this.CreateRandomConnections();
        }

        public int ColorLimit { get; set; }
        public IDictionary<int, Node> NodeData { get; set; }
        public bool HasConnectionMatrix { get; set; }




        #region Init Methods
        private void CreateRandomConnections()
        {
            // random starting point
            var random = new Random();
            var start = random.Next(1, this.NodeData.Values.Count + 1);

            var current = this.NodeData[start];

            // create random connections for start
            for (int n = 0; n < this.ColorLimit - 1; )
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
                var cc = random.Next(1, 3);

                for(int i = node.Connections.Count; i < cc; )//i++)
                {
                    var nr = random.Next(1, this.NodeData.Values.Count + 1);
                    var next = this.NodeData[nr];


                    if(next != node && 
                        next.Connections.Count < this.ColorLimit - 1 && 
                        !node.Connections.Contains(next) && 
                        node.Connections.Count < (this.ColorLimit - 1) - node.Connections.Count)
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

        #endregion Init Methods

    }
}
