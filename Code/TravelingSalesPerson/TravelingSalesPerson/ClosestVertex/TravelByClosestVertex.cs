using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesPerson.ClosestVertex
{
    public class TravelByClosestVertex
    {
        public TravelByClosestVertex(string filePath)
        {
            this.FilePath = filePath;
            this.DataLines = ProcessTSPDocument.GetDataLines(this.FilePath);
            this.Nodes = ProcessTSPDocument.GetNodesDictionary(this.DataLines);
            this.InsertionOrder = new List<int>();
        }

        public string FilePath { get; private set; }
        public IList<string> DataLines { get; private set; }
        public IDictionary<int, Node> Nodes { get; private set; }
        public IList<ConnectionDetails> OptimalPath { get; private set; }
        public TimeSpan EllapsedSearchTime { get; private set; }
        public IList<int> InsertionOrder { get; set; }

        public int InitialNodeId { get; set; }

        public void FindOptimalPath()
        {// begin evaluation
            Stopwatch sw = new Stopwatch();

            sw.Start();
            var nodes = this.Nodes.Values.Where(n => n.Id != this.InitialNodeId).ToList();
            
            var ic = BuildCluster(this.Nodes[InitialNodeId], nodes, this.InsertionOrder);
            OptimalPath = ExpandCluster(ic, nodes, this.InsertionOrder);
            sw.Stop();
            this.EllapsedSearchTime = sw.Elapsed;
        }

        public static IList<ConnectionDetails> BuildCluster(Node initialNode, IList<Node> allNodes, IList<int> insertionOrder)
        {
            IList<ConnectionDetails> connectionDetails = new List<ConnectionDetails>();
            
            var p1 = initialNode;
            insertionOrder.Add(p1.Id);

            Node p2 = null;
            double minDistance = 0D;
            foreach (var node in allNodes)
            {
                var nd = ConnectionDetails.CalculateDistance(p1, node);

                if (p2 == null || nd < minDistance)
                {
                    p2 = node;
                    minDistance = nd;
                }
            }

            allNodes.Remove(p2);

            // build our first cluster; p1 to p2
            var p1_p2 = new ConnectionDetails(p1, p2);
            // add node id to insertion order list
            insertionOrder.Add(p2.Id);
            // remove inserted node from available list of nodes
            allNodes.Remove(p2);


            // find closest point (p0) our root edge            
            var p0 = FindClosestNodeToLineSegment(p1_p2, allNodes);
            
            // save p0.Id to insertion order       
            insertionOrder.Add(p0.ClosestNode.Id);
            
            // remove p0.Node from available node list
            allNodes.Remove(p0.ClosestNode);

            // store p2 in tmp variable
            var tmpNode = p1_p2.DestinationNode;
            //  set destination to p0
            p1_p2.DestinationNode = p0.ClosestNode;

            var p0_p2 = new ConnectionDetails(p0.ClosestNode, tmpNode);
            var p2_p1 = new ConnectionDetails(tmpNode, p1);

            connectionDetails.Add(p1_p2);
            connectionDetails.Add(p0_p2);
            connectionDetails.Add(p2_p1);

            return connectionDetails;
        }

        public static IList<ConnectionDetails> ExpandCluster(IList<ConnectionDetails> connectionDetails, IList<Node> allNodes, IList<int> insertionOrder)
        {
            while (allNodes.Count > 0)
            {// loop until we've connected to every node
                EdgePointDetails p0 = null;
                foreach (var cd in connectionDetails)                
                {
                    var p = FindClosestNodeToLineSegment(cd, allNodes);
                    
                    if (p0 == null || p0.Distance > p.Distance)
                        p0 = p;
                }

                ////find which part of the line the point is closer to;
                //var dp0ToP1 = ConnectionDetails.CalculateDistance(p0.ClosestNode, p0.Edge.InitialNode);
                //var dp0ToP2 = ConnectionDetails.CalculateDistance(p0.ClosestNode, p0.Edge.DestinationNode);


                //if (dp0ToP1 < dp0ToP2)
                //{// new point is closer to initial
                //    var tmp = p0.Edge.DestinationNode;
                //    p0.Edge.DestinationNode = p0.ClosestNode;

                //    var newCn = new ConnectionDetails(p0.ClosestNode, tmp);

                //    insertionOrder.Add(p0.ClosestNode.Id);
                //    connectionDetails.Add(newCn);

                //}
                //else
                //{// new point is closer to destination
                //    var tmp = p0.Edge.InitialNode;
                //    p0.Edge.InitialNode = p0.ClosestNode;

                //    var p2_p0 = new ConnectionDetails(tmp, p0.ClosestNode);

                //    insertionOrder.Add(p0.ClosestNode.Id);
                //    connectionDetails.Add(p2_p0);
                //}



                // change connection from p1 to p2 => p1 to p0
                var p2 = p0.Edge.DestinationNode;
                p0.Edge.DestinationNode = p0.ClosestNode;
                // create new edge from p2 to p0
                var p2_p0 = new ConnectionDetails(p2, p0.ClosestNode);
                // save p0.Id to insertion order 
                insertionOrder.Add(p0.ClosestNode.Id);
                // add new edge to list
                connectionDetails.Add(p2_p0);
                // remove p0.Node from available node list
                allNodes.Remove(p0.ClosestNode);
            }

            return connectionDetails;
        }

        /// <summary>
        /// Code is based of topic found on StackOverflow.  
        /// Code link: http://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
        /// this approach was used as we are dealing with finite lines 
        /// http://problems1.seas.harvard.edu/wiki-hpcm/index.php/2D_Geometry
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="remainingSet"></param>
        /// <returns></returns>
        private static EdgePointDetails FindClosestNodeToLineSegment(ConnectionDetails edge, IEnumerable<Node> remainingSet)
        {
            // node 1 coordinates
            double x1 = edge.InitialNode.X;
            double y1 = edge.InitialNode.Y;

            // node 2 coordinates
            double x2 = edge.DestinationNode.X;
            double y2 = edge.DestinationNode.Y;

            double C = (x2 - x1);
            double D = (y2 - y1);

            EdgePointDetails nearestNode = null;

            // find closest vertex
            foreach (var node in remainingSet)
            {
                double x0 = node.X;
                double y0 = node.Y;
                
                // deltas for coordinates between p0 and p1
                double A = x0 - x1;
                double B = y0 - y1;

                var p = A * C + B * D;

                var len_sq = C * C + D * D;


                var param = p / len_sq;

                double xx;
                double yy;

                if (param < 0 || (x1 == x2 && y1 == y2))
                {
                    xx = x1;
                    yy = y1;
                }
                else if (param > 1)
                {
                    xx = x2;
                    yy = y2;
                }
                else
                {
                    xx = x1 + param * C;
                    yy = y1 + param * D;
                }

                var dx = x0 - xx;
                var dy = y0 - yy;

                var d = Math.Sqrt(dx * dx + dy * dy);

                if (nearestNode == null || nearestNode.Distance > d)
                    nearestNode = new EdgePointDetails(edge, node, d);                
            }


            return nearestNode;
        }


        public static ConnectionDetails FindClosestLineSegmentToNode(Node node, IEnumerable<ConnectionDetails> edges)
        {        
            ConnectionDetails closestEdge = null;
            double distanceToEdge = 0D;

            foreach(var edge in edges)
            {
                // node 1 coordinates
                double x1 = edge.InitialNode.X;
                double y1 = edge.InitialNode.Y;

                // node 2 coordinates
                double x2 = edge.DestinationNode.X;
                double y2 = edge.DestinationNode.Y;

                double C = (x2 - x1);
                double D = (y2 - y1);

                double x0 = node.X;
                double y0 = node.Y;

                // deltas for coordinates between p0 and p1
                double A = x0 - x1;
                double B = y0 - y1;

                var p = A * C + B * D;

                var len_sq = C * C + D * D;

                var param = p / len_sq;

                double xx;
                double yy;

                if (param < 0 || (x1 == x2 && y1 == y2))
                {
                    xx = x1;
                    yy = y1;
                }
                else if (param > 1)
                {
                    xx = x2;
                    yy = y2;
                }
                else
                {
                    xx = x1 + param * C;
                    yy = y1 + param * D;
                }

                var dx = x0 - xx;
                var dy = y0 - yy;

                var d = Math.Sqrt(dx * dx + dy * dy);

                if (closestEdge == null || distanceToEdge > d)
                {
                    closestEdge = edge;
                    distanceToEdge = d;
                }
                    
            }

            return closestEdge;
        }
    }
}
