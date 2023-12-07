using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelingSalesPerson;
using K_GraphColoring;

namespace K_GraphColoring
{
    public class K_Graph_GA_Member
        : IComparable
    {
        public K_Graph_GA_Member(int generation, IDictionary<int, Color> nodeColors, IEnumerable<Node> nodeDetails, K_Graph_GA_Member p1, K_Graph_GA_Member p2, int solverIteration, int memberId)
        {
            this.Generation = generation;
            this.NodeColors = nodeColors;
            this.SolverIteration = solverIteration;
            this.MemberId = memberId;

            // start out assuming it's a complete graph
            this.IsComplete = true;

            this.P1 = p1;
            this.P2 = p2;
           
            this.CalculateColoringFitness(nodeDetails);
        }

        public int MemberId { get; set; }

        public int Generation { get; set; }

        public K_Graph_GA_Member P1 { get; set; }

        public K_Graph_GA_Member P2 { get; set; }

        public IDictionary<int, Color> NodeColors { get; private set; }

        public bool IsComplete { get; private set; }       

        public int NumberOfColorsUsed { get; private set; }

        public int SolverIteration { get; set; }

        private void CalculateColoringFitness(IEnumerable<Node> nodeDetails)
        {
            var colors = this.NodeColors.Values.Distinct();
            
            // are there any uncolored nodes
            var isIncomplete = colors.Contains(Color.White);

            if(isIncomplete)
            {// has uncolored points
                this.IsComplete = false;
            }
            else
            {// all points are colored
             // check if two connected points are the same color

                foreach(var nd in nodeDetails)
                {
                    if(!this.IsComplete)
                        break;
                    
                    var currentNodeColor = this.NodeColors[nd.Id];
                    foreach(var conn in nd.Connections)
                    {
                        var connectedNodeColor = this.NodeColors[conn.Id];
                        if(currentNodeColor == connectedNodeColor)
                        {
                            this.IsComplete = false;
                            break;
                        }
                    }                                           
                }
            }

            this.NumberOfColorsUsed = colors.Count(x => x != Color.White);            
        }

        public int CompareTo(object obj)
        {
            var comparee = obj as K_Graph_GA_Member;

            if (obj == null)
                return 1;

            if (comparee == null)
                throw new InvalidCastException("Received type if not member of K-Graph population");

            int colorComapre = this.NumberOfColorsUsed.CompareTo(comparee.NumberOfColorsUsed);

            if (colorComapre == -1 && this.IsComplete)
            {
                return -1;
            }
            else if (this.IsComplete && !comparee.IsComplete)
            {
                return -1;
            }
            else if(colorComapre == 0 && (this.IsComplete && comparee.IsComplete))
            {
                return 0;
            }
            else
            {
                return 1;
            }            
        }
    }
}
