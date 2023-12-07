using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelingSalesPerson.ClosestVertex;
using TravelingSalesPerson.GeneticAlgorithm;

namespace TravelingSalesPerson.WisdomOfCrowds
{
    public class WisdomOfCrowds
    {               
        private  Agreement[,] _agreementMatrix;        
        private IList<PopulationMember> _expertSolutions;
        

        public WisdomOfCrowds(IList<Node> nodeList)
        {
            this.Nodes = nodeList;
            this.NodeCount = nodeList.Count;
            this._agreementMatrix = new Agreement[this.NodeCount, this.NodeCount];
            this._expertSolutions = new List<PopulationMember>();

            this.InitialCityStatistics = new Dictionary<int, int>();

            this.InitializeMatrix();
        }

        public int NodeCount { get; private set; }

        public IList<Node> Nodes { get; private set; }

        public Dictionary<int, int> InitialCityStatistics { get; private set; }

        public PopulationMember BestExpert { get; private set; }

        public PopulationMember CombinedExpertsSolution { get; private set; }

        public IList<Agreement> AggregatePath { get; private set; }

        public IList<PopulationMember> ExpertSolutions
        {
            get
            {
                return this._expertSolutions.ToList();
            }
        }

        public void AddExpertSolution(PopulationMember es)
        {
            this._expertSolutions.Add(es);

            if (this.BestExpert == null || this.BestExpert.Distance > es.Distance)
                this.BestExpert = es;

            // add starting city statistics 
            if (this.InitialCityStatistics.ContainsKey(es.Cities[0].Id))
                this.InitialCityStatistics[es.Cities[0].Id]++;
            else
                this.InitialCityStatistics.Add(es.Cities[0].Id, 1);

            foreach (var p in es.Connections)
            {
                var s = p.InitialNode;
                var e = p.DestinationNode;

                var index = this._agreementMatrix[s.Id - 1, e.Id - 1];

                index.AgreementIndex++;

                if (index.Edge == null)
                    index.Edge = p;
            }
        }

        private void InitializeMatrix()
        {
            for (int i = 0; i < this.NodeCount; i++)
                for (int j = 0; j < this.NodeCount; j++)
                    this._agreementMatrix[i, j] = new Agreement(i, j);
        }

        /*
         * 
         * This gives a decent solution.  It doesn't beat the optimal, but it 
         * gives something I can live with.
         * 
         */
        public void ProcessExpertSolutionsByChosenDestinationV2()
        {
            var mtrx = PrintOutMatrix();
            var edges = new List<ConnectionDetails>();
            var visitationOrder = new List<Node>();
            var omit = new List<int>();
            var tAgreement = this._agreementMatrix.Cast<Agreement>().Where(a => a.AgreementIndex > 0);

            var ma = tAgreement.Max(x => x.AgreementIndex);
            var mostAgreed = tAgreement.Where(x => x.AgreementIndex == ma);

            // most agreed connection yields our first path
            Agreement nxtc = null;
            foreach (var max in mostAgreed)
            {
                if ((nxtc == null) || (max.Edge.Distance < nxtc.Edge.Distance))
                    nxtc = max;
            }

            // register columns as unavailaboe in the omit list
            omit.Add(nxtc.I);
            omit.Add(nxtc.J);
            visitationOrder.Add(nxtc.Edge.InitialNode);
            visitationOrder.Add(nxtc.Edge.DestinationNode);

            edges.Add(nxtc.Edge);

            var current = nxtc.Edge.DestinationNode;

            while (visitationOrder.Count < this.NodeCount)
            {// keep looping until our visitionat order count matches our node count

                // find the matrix location of current node
                int cRowIndex = current.Id - 1;

                Agreement agreedEdge = null;
                for (int j = 0; j < this.NodeCount; j++)
                {
                    if (j != cRowIndex && !omit.Contains(j))
                    {// current column represents a node that has not be connected to
                        Agreement curr = this._agreementMatrix[cRowIndex, j];

                        if (curr.AgreementIndex > 0)
                        {
                            if (agreedEdge == null || (agreedEdge != null && agreedEdge.AgreementIndex < curr.AgreementIndex))
                                agreedEdge = curr;
                            else if (agreedEdge.AgreementIndex == curr.AgreementIndex && curr.Edge.Distance < agreedEdge.Edge.Distance)
                                agreedEdge = curr;
                        }
                    }
                }

                if (agreedEdge != null && agreedEdge.Edge != null)
                {// we found an agreed next point
                    current = agreedEdge.Edge.DestinationNode;                    
                    omit.Add(agreedEdge.J);
                    visitationOrder.Add(agreedEdge.Edge.DestinationNode);
                    
                    edges.Add(agreedEdge.Edge);
                    
                }
                else if(agreedEdge == null && omit.Count == this.NodeCount )
                {// couldn't find agreement but we are on our last node

                    var currToProcess = this.Nodes.Where(x => x.Id == cRowIndex + 1).First(); // find last remaining node                    
                    // check if there's a node that's not been connected
                    var notVisitted = this.Nodes.Where(x => !visitationOrder.Contains(x)).ToList();

                    if(notVisitted.Count() == 1 )
                    {
                        // find edge it's closest to
                        var closestEdge = TravelByClosestVertex.FindClosestLineSegmentToNode(currToProcess, edges);

                        //var tmp = closestEdge.InitialNode;

                        
                        ConnectionDetails cn = new ConnectionDetails(currToProcess, notVisitted[0]);

                        current = cn.DestinationNode;
                        omit.Add(current.Id - 1);
                        visitationOrder.Add(current);                                                
                    }
                    else
                    {
                        visitationOrder.Add(currToProcess);
                    }                    
                }
                else if(agreedEdge == null)
                {
                    // find all nodes that haven't been visitted
                    var notVisitted = this.Nodes.Where(x => !visitationOrder.Contains(x));

                    // find the closest neighbor to our unagreed point
                    ConnectionDetails shortest = null;

                    foreach(var nv in notVisitted)
                    {
                        var dn = new ConnectionDetails(current, nv);

                        if (shortest == null || shortest.Distance > dn.Distance)
                            shortest = dn;
                    }

                    current = shortest.DestinationNode;
                    omit.Add(current.Id - 1);
                    visitationOrder.Add(current);

                    edges.Add(shortest);                                                            
                }
            }


            this.CombinedExpertsSolution = new PopulationMember(-1, visitationOrder, null, null, 20D, false);

            this.CombinedExpertsSolution.IsAggregateResult = true;
        }


        #region Matrix Traverse Functions current in development
        /*
         * WORK IN PROGRESS
         *  These don't quite work well...if at all
         */
        public void ProcessExpertSolutionsByDescendingChoiceOption()
        {
            var mtrx = PrintOutMatrix();

            Node start = null;
            var omitInitial = new List<int>();
            var omitDestination = new List<int>();

            List<Agreement> agreedConnections = new List<Agreement>();

            var tAgreement = this._agreementMatrix.Cast<Agreement>().Where(a => a.AgreementIndex > 0);

            var ma = tAgreement.Max(x => x.AgreementIndex);
            var mostAgreed = tAgreement.Where(x => x.AgreementIndex == ma);

            // most agreed connection yields our first 
            // path
            Agreement nxtc = null;
            foreach (var max in mostAgreed)
            {
                if ((nxtc == null) || (max.Edge.Distance < nxtc.Edge.Distance))
                    nxtc = max;
            }

            start = nxtc.Edge.InitialNode;

            omitInitial.Add(nxtc.I);
            omitDestination.Add(nxtc.I);
            omitDestination.Add(nxtc.J);

            agreedConnections.Add(nxtc);

            // find connection with fewest options
            while (omitInitial.Count != this.NodeCount)
            {
                var connectionStats = new Dictionary<int, int>();

                for (int i = 0; i < this.NodeCount; i++)
                {
                    if (!omitInitial.Contains(i))
                    {// connection statistics for current row
                        connectionStats.Add(i, 0);
                        for (int j = 0; j < this.NodeCount; j++)
                        {
                            if (i != j && !omitDestination.Contains(j))
                            {
                                var curr = this._agreementMatrix[i, j];
                                int ag = 0;
                                if (curr.AgreementIndex > 0)
                                {
                                    ag++;
                                    connectionStats[i] = ag;
                                }
                            }
                        }
                    }
                }

                bool hasNonZeroAgreement = true;


                int mcs = -1;
                var mcsList = connectionStats.Where(x => x.Value != 0).ToList();

                if (mcsList.Count == 0)
                    hasNonZeroAgreement = false;
                else
                    mcs = mcsList.Min(x => x.Value);


                Agreement nxtD = null;

                if (hasNonZeroAgreement)
                {// there are connections with agreement 
                    // available for evaluation
                    var nxMin = connectionStats.Where(x => x.Value == mcs).ToList();
                    int rIndex = 0;

                    if (nxMin.Count > 1)
                    {
                        // if there are multiple; use smallest number as winner
                        var lowId = nxMin.Min(x => x.Key);
                        rIndex = lowId;
                    }
                    else if (nxMin.Count == 1)
                    {
                        rIndex = nxMin.First().Key;
                    }


                    // find closest un connected neighbor

                    for (int j = 0; j < this.NodeCount; j++)
                    {
                        if (rIndex != j && !omitDestination.Contains(j))
                        {
                            var curr = this._agreementMatrix[rIndex, j];
                            if (nxtD == null)
                                nxtD = curr;
                            else if (nxtD.AgreementIndex < curr.AgreementIndex)
                                nxtD = curr;
                        }
                    }

                }
                else
                {// there are no agreed upon connections

                    // last remaining node; attach to end
                    if (connectionStats.Keys.Count == 1)
                    {
                        var xk = connectionStats.First().Key;
                        Node last = this.Nodes.Where(x => x.Id == xk + 1).First();

                        // specific case for a closed loop
                        if (last.Id != agreedConnections[0].Edge.DestinationNode.Id)
                        {
                            nxtD = this._agreementMatrix[xk, omitInitial[0]];
                            nxtD.Edge = new ConnectionDetails(last, start);
                        }
                        else
                        {// if we would be stuck of closed local, perform point insertion
                            // finde the closest edge


                            var edges = agreedConnections.Select(x => x.Edge).Where(x => x.InitialNode != start && x.DestinationNode != last);

                            var closestEdge = TravelByClosestVertex.FindClosestLineSegmentToNode(last, edges);

                            // check which point of the edge the last point is closest to
                            var dX1 = ConnectionDetails.CalculateDistance(last, closestEdge.InitialNode);
                            var dX2 = ConnectionDetails.CalculateDistance(last, closestEdge.DestinationNode);

                            ConnectionDetails el = null;
                            ConnectionDetails ei = null;
                            ConnectionDetails cl = null;
                            if (dX1 < dX2)
                            {
                                el = new ConnectionDetails(closestEdge.InitialNode, last);
                                ei = new ConnectionDetails(start, closestEdge.DestinationNode);
                                cl = new ConnectionDetails(last, start);
                            }
                            else
                            {
                                el = new ConnectionDetails(closestEdge.InitialNode, start);
                                ei = new ConnectionDetails(last, closestEdge.DestinationNode);
                                cl = new ConnectionDetails(start, last);
                            }


                            // agreement to remove
                            var actr = agreedConnections.Where(x => x.Edge == closestEdge).First();
                            var bctr = agreedConnections.Where(x => x.Edge.InitialNode == start && x.Edge.DestinationNode == last).First();

                            agreedConnections.Remove(actr);
                            agreedConnections.Remove(bctr);

                            // fix agreement pointers                            
                            var p1 = this._agreementMatrix[el.InitialNode.Id - 1, el.DestinationNode.Id - 1];
                            p1.Edge = el;
                            agreedConnections.Add(p1);

                            var p2 = this._agreementMatrix[ei.InitialNode.Id - 1, ei.DestinationNode.Id - 1];
                            p2.Edge = ei;
                            agreedConnections.Add(p2);

                            nxtD = this._agreementMatrix[cl.InitialNode.Id - 1, cl.DestinationNode.Id - 1];
                            nxtD.Edge = cl;
                        }
                    }
                    else
                    {
                        // evalute nodes based on smallest id value
                        var minRow = connectionStats.Keys.Min();

                        // find points that have no connection to them
                        var iNode = this.Nodes.Where(x => x.Id == minRow + 1).First();
                        var possibleDest = this.Nodes.Where(x => !omitDestination.Contains(x.Id - 1));

                        ConnectionDetails bfcd = null;
                        foreach (var pd in possibleDest)
                        {
                            var cd = new ConnectionDetails(iNode, pd);

                            if (bfcd == null || bfcd.Distance > cd.Distance)
                                bfcd = cd;
                        }

                        nxtD = this._agreementMatrix[minRow, bfcd.DestinationNode.Id - 1];
                        nxtD.Edge = bfcd;
                    }

                }

                omitInitial.Add(nxtD.I);
                omitDestination.Add(nxtD.J);

                agreedConnections.Add(nxtD);
            }


            // evaluate the aggregate path            
            this.AggregatePath = agreedConnections;
            List<Node> vo = new List<Node>();
            vo.Add(agreedConnections[0].Edge.InitialNode);
            vo.Add(agreedConnections[0].Edge.DestinationNode);

            Node ncurr = agreedConnections[0].Edge.DestinationNode;

            // find where current is initial
            for (int n = 1; n < this.NodeCount - 1; n++)
            {
                var nxt = agreedConnections.Where(x => x.Edge.InitialNode == ncurr).First();

                vo.Add(nxt.Edge.DestinationNode);

                ncurr = nxt.Edge.DestinationNode;
            }

            this.CombinedExpertsSolution = new PopulationMember(-1, vo, null, null, 20D, false);
            this.CombinedExpertsSolution.IsAggregateResult = true;
            var totalDistnace = this.CombinedExpertsSolution.Distance;

            string s = string.Join(Environment.NewLine, agreedConnections.Select(x => x.Edge).ToList());
            string sx = string.Join(Environment.NewLine, vo);
        }

        #endregion Matrix Traverse Functions current in development


        private static int FindStartingCityId(IDictionary<int, int> startingCityStatistics)
        {
            IList<int> idStats = new List<int>();

            int mr = startingCityStatistics.Max(m => m.Value);

            foreach(var scs in startingCityStatistics)
            {
                if (scs.Value == mr)
                    idStats.Add(scs.Key);
            }

            int sc = idStats.Min(m => m);

            return sc;
        }


        public string PrintOutMatrix()
        {
            List<string> m = new List<string>();

            var nc = this.NodeCount;
            var ex = this._expertSolutions.Count;

            int padding = 4;
            StringBuilder sb = new StringBuilder();
            sb.Append(" ".PadLeft(padding + 1,' '));
            for (int c = 0; c < this.NodeCount; c++)
            {
                sb.Append((c + 1).ToString().PadLeft(padding, ' ') + "|");
            }

            m.Add(sb.ToString());

            List<List<Agreement>> parsedA = new List<List<Agreement>>();
            List<string> p = new List<string>();
            p.Add(sb.ToString());
            for (int i = 0; i < this.NodeCount; i++)
            {
                List<Agreement> pa = new List<Agreement>();
                for (int j = 0; j < this.NodeCount; j++)
                {
                    var curr = this._agreementMatrix[i, j];
                    pa.Add(curr);
                }

                Agreement ma = null;
                foreach(var px in pa)
                {
                    if (ma == null || 
                        (ma != null && 
                         (px.AgreementIndex > ma.AgreementIndex || 
                          (px.AgreementIndex == ma.AgreementIndex && px.Edge != null && ma.Edge != null && px.Edge.Distance < ma.Edge.Distance)
                        )))
                    {
                        ma = px;
                    }
                        
                }                
                ma.IsMostAgreed = true;

                StringBuilder sb2 = new StringBuilder();
                sb2.Append((i + 1).ToString().PadLeft(padding, ' ') + "|");
                foreach(var a in pa)
                {
                    string v = a.IsMostAgreed ? "*" + a.ToString() + "*" : a.ToString();

                    sb2.Append(v.PadLeft(padding, ' ') + "|");
                }
                p.Add(sb2.ToString());

                parsedA.Add(pa);


            }
            var x2 = string.Join(Environment.NewLine, p.ToArray());

            return x2;
        }
    }
}
