using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesPerson
{
    public class ConnectionDetails
    {
        public ConnectionDetails(Node initialNode, Node destinationNode)
        {
            this.InitialNode = initialNode;
            this.DestinationNode = destinationNode;

            this.Distance = CalculateDistance(this.InitialNode, this.DestinationNode);

        }

        /// <summary>
        /// What is our current node
        /// </summary>
        private Node _initialNode;
        public Node InitialNode 
        {
            get { return this._initialNode; }
            internal set
            {
                this._initialNode = value;
                
                if (this.DestinationNode != null)
                    this.Distance = CalculateDistance(this.InitialNode, this.DestinationNode);
            }
        }
        
        /// <summary>
        /// What node are we going ot
        /// </summary>
        private Node _destinationNode;
        public Node DestinationNode
        {
            get { return this._destinationNode; }
            set
            {
                this._destinationNode = value;

                if (this.InitialNode != null)
                    this.Distance = CalculateDistance(this.InitialNode, this.DestinationNode);
            }
        }

        /// <summary>
        /// What is the distance between the two nodes
        /// </summary>
        public double Distance{get; private set;}

        public override string ToString()
        {
            return string.Format("{0} -> {1}", this.InitialNode, this.DestinationNode);
        }

        /// <summary>
        /// Calculates the distance using standard distance formual for 2D plane
        /// </summary>
        /// <returns>The distance.</returns>
        /// <param name="n1">Initial node</param>
        /// <param name="n2">Destination node</param>
        public static double CalculateDistance(Node n1, Node n2)
        {
            // X distance
            var xpd = Math.Pow((n2.X - n1.X), 2);
            // Y distance
            var ypd = Math.Pow((n2.Y - n1.Y), 2);

            var distance = Math.Sqrt(xpd + ypd);
           
            // round up to 13 decimals since beyond 13 there's a discrepency in comparisions due to difference in the least significant bit
            // Details from Microsoft:
            /*
             * A mathematical or comparison operation that uses a floating-point number might not yield the same result if a decimal number 
             * is used, because the binary floating-point number might not equal the decimal number. 
             * When accuracy in numeric operations with fractional values is important, 
             * you can use the Decimal rather than the Double type
             */
            return Math.Round(distance, 13);
        }
    }
}
