using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesPerson.ClosestVertex
{
    public class EdgePointDetails
    {
        public EdgePointDetails(ConnectionDetails edge, Node closestNode, double distance)
        {
            this.Edge = edge;
            this.ClosestNode = closestNode;
            this.Distance = distance;
        }

        private ConnectionDetails _edge;
        public ConnectionDetails Edge 
        {
            get { return this._edge; }
            set { this._edge = value; }
        }

        public Node ClosestNode { get; set; }

        public double Distance { get; set; }
    }
}
