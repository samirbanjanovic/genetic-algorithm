using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesPerson.BruteForce
{
    public class PathDetails
    {
        public PathDetails(IList<Node> nodes)
        {
            this.Nodes = nodes;
            this.NodeCount = this.Nodes.Count;
            //this.RoundTripDistance = CalculateRoundTripDistance(this.Nodes);
            this.ConnectionDetails = this.ProcessConnections(this.Nodes);
            
        }

        public int NodeCount { get; private set; }
        public IList<Node> Nodes { get; private set; }

        public IList<ConnectionDetails> ConnectionDetails { get; private set; }

        public double RoundTripDistance { get; private set; }

        private IList<ConnectionDetails> ProcessConnections(IList<Node> nodes)
        {
            var cdList = new List<ConnectionDetails>();

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                var cd = new ConnectionDetails(nodes[i], nodes[i + 1]);
                this.RoundTripDistance += cd.Distance;
                cdList.Add(cd);

                // return home
                if (i == nodes.Count - 2)
                {
                    var cdHome = new ConnectionDetails(nodes[i + 1], nodes[0]);
                    this.RoundTripDistance += cdHome.Distance;
                    cdList.Add(cdHome);
                }
            }

            return cdList;
        }

        public static double CalculateRoundTripDistance(IList<Node> nodes)
        {
            double rtd = 0;

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                var nd = CalculateDistance(nodes[i], nodes[i + 1]);
                rtd += nd;                

                // return home
                if (i == nodes.Count - 2)
                {
                    var ndh = CalculateDistance(nodes[i + 1], nodes[0]);
                    rtd += ndh;   
                }
            }

            return rtd;
        }

        /// <summary>
        /// Calculates the distance using standard distance formual for 2D plane
        /// </summary>
        /// <returns>The distance.</returns>
        /// <param name="n1">Initial node</param>
        /// <param name="n2">Destination node</param>
        private static double CalculateDistance(Node n1, Node n2)
        {
            // X distance
            var xpd = Math.Pow((n2.X - n1.X), 2);
            // Y distance
            var ypd = Math.Pow((n2.Y - n1.Y), 2);

            var distance = Math.Sqrt(xpd + ypd);

            // round up to 13 decimals since beyond 13 there's a discrepency in comparisions due to difference in the least significant bit
            // details from Microsoft:
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
