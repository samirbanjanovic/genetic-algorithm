using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TravelingSalesPerson.SearchAlg;

namespace TravelingSalesPerson.SearchAlg
{
    public class TSP_SearchAlg
    {                                        
        public IDictionary<int, IList<int>> _nodeConnections;

        public TSP_SearchAlg(string filePath, int finalNodeId)
        {
            this._nodeConnections = ValidNodeConnections();
            this.ConnectionDetails = new List<ConnectionDetails>();

            this.DataLines = ProcessTSPDocument.GetDataLines(filePath);
            this.Nodes = ProcessTSPDocument.GetNodes(this.DataLines); ;

            this.FinalNode = this.Nodes.Where(f => f.Id == finalNodeId).FirstOrDefault();

            TSP_SearchAlg.BuildNodeLinksWithObjects(this.Nodes, this._nodeConnections);
        }

        private Node _finalNode;
        public Node FinalNode 
        {
            get 
            {
                return this._finalNode;
            }
            set
            {
                if(value != this._finalNode)
                {
                    this._finalNode = value;

                    // mark all nodes are non terminal
                    this.Nodes.Select(s => s.IsFinalNode = false).ToList();

                    // mark new final node as terminal
                    this._finalNode.IsFinalNode = true;
                }
            }
        }

        public IList<ConnectionDetails> ConnectionDetails { get; private set; }

        public IList<Node> OptimalPath { get; private set; }
        public IList<Node> DFS_First { get; private set; }
        public TimeSpan DFS_First_ExecutionTime { get; private set; }
        public double DFS_First_Distance { get; private set; }
        public int DFS_Potential_PathCount { get; private set; }
        public int NumberOfHops { get { return this.OptimalPath.Count; } }
        public double Distance { get; private set; }
        public TimeSpan ExecutionTime { get; private set; }
        public IList<string> DataLines { get; private set; }      
        public IList<Node> Nodes { get; private set; }

        public void BFS_Search()
        {
            
            ResetNodeExpansionAndExploration(this.Nodes);

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            this.OptimalPath = Search.BFS(this.Nodes);
            sw.Stop();
            this.ConnectionDetails = CalculateCostForPath(this.OptimalPath);
            this.Distance = this.ConnectionDetails.Sum(cn => cn.Distance);
            this.ExecutionTime = sw.Elapsed;
        }

        public void DFS_Search()
        {
            ResetNodeExpansionAndExploration(this.Nodes);
            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            this.DFS_First = Search.DFS(this.Nodes).Item2;
            sw.Stop();
            this.DFS_First_ExecutionTime = sw.Elapsed;
            this.DFS_First_Distance = CalculateCostForPath(this.DFS_First).Sum(cn => cn.Distance);

            ResetNodeExpansionAndExploration(this.Nodes);
            
            sw.Reset();
            sw.Start();
            var op = Search.DFS(this.Nodes, true);
            sw.Stop();
            
            this.OptimalPath = op.Item2;
            this.DFS_Potential_PathCount = op.Item1;
 
            this.ConnectionDetails = CalculateCostForPath(this.OptimalPath);
            this.Distance = this.ConnectionDetails.Sum(cn => cn.Distance);
            this.ExecutionTime = sw.Elapsed;


        }
        
        #region Private Helpers
        
        private static IList<ConnectionDetails> CalculateCostForPath(IList<Node> nodes)
        {
            var cn = new List<ConnectionDetails>();
            for (int i = 0; i < nodes.Count - 2; i++)
            {
                var conn = new ConnectionDetails(nodes[i], nodes[i + 1]);
                cn.Add(conn);
            }   
        
            return cn;
        }

        private static void BuildNodeLinksWithObjects(IList<Node> nodes, IDictionary<int, IList<int>> nodeConnections)
        {
            for(int n = 0; n < 10; n++)            
                nodes[n].Connections = nodes.Where(i => nodeConnections[nodes[n].Id].Contains(i.Id)).ToList();

            nodes[0].IsInitialNode = true;            
        }

        private static IDictionary<int, IList<int>> ValidNodeConnections()
        {
            var nl = new Dictionary<int, IList<int>>();

            nl.Add(1, new int[] { 2 ,3, 4 });
            nl.Add(2, new int[] { 3 });
            nl.Add(3, new int[] { 4, 5 });
            nl.Add(4, new int[] { 5, 6, 7 });
            nl.Add(5, new int[] { 7, 8 });
            nl.Add(6, new int[] { 8 });
            nl.Add(7, new int[] { 9, 10 });
            nl.Add(8, new int[] { 9, 10, 11 });
            nl.Add(9, new int[] { 11 });
            nl.Add(10, new int[] { 11 });

            return nl;
        }

        private static void ResetNodeExpansionAndExploration(IList<Node> nodes)
        {
            foreach(var n in nodes)
            {
                n.IsExpanded = false;
                n.IsExplored = false;
                n.Backtrack = null;
            }         
        }

        #endregion Private Helpers
    }

}
