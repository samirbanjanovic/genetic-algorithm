using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesPerson
{
    /// <summary>
    /// Stores details on a node.  Coordinates, if it's initial node, and if we've visitted it
    /// </summary>
    public class Node
        : IComparable,
          IDisposable
    {
        public Node(int id, double x, double y)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;

            this.IsExplored = false;
            this.IsExpanded = false;

            this.IsInitialNode = false;
            this.IsFinalNode = false;
           
            this.Connections = new List<Node>();
        }


        public int Id { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }

        public object Color { get; set; }
        public Node Backtrack { get; set; }
        public IList<Node> Connections { get; set; }

        public bool IsExplored { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsFinalNode { get; set; }
        public bool IsInitialNode { get; set; }

        public override string ToString()
        {
            return this.Id.ToString();
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            var nd = obj as Node;
            if (nd != null)
                return this.Id.CompareTo(nd.Id);
            else
                throw new ArgumentException("Comparer value is not of type Node");
        }

        public void Dispose()
        {
            this.Backtrack = null;
            this.Connections = null;
        }
    }
}
