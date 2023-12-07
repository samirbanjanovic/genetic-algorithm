using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TravelingSalesPerson
{
    public class TSPData
    {
        private string _filePath;
        private List<IList<Node>> _evaluationPaths;

        public TSPData(string path)
        {
            this._filePath = path;
            this.DataLines = System.IO.File.ReadAllLines(this._filePath);
            this.Nodes = GetNodeList(this);
            this.NumberOfPossiblePaths = 0;
            this.OptimalPaths = new List<PathDetails>();
            this._evaluationPaths = new List<IList<Node>>();

            //this.BeginPermutationAsyncProcessing();
        }

        public IList<string> DataLines { get; private set; }
        public IList<Node> Nodes { get; private set; }
        public IList<PathDetails> OptimalPaths { get; private set; }
        public int NumberOfPossiblePaths { get; private set; }

        public void FindOptimalPaths()
        {
            foreach(var qp in QuickPerm(this.Nodes))
            {
                var perm = new List<Node>(qp);

                PathDetails pd = new PathDetails(perm);
                if (this.OptimalPaths.Count == 0)
                {
                    this.OptimalPaths.Add(pd);
                }
                else if (pd.RoundTripDistance == this.OptimalPaths.First().RoundTripDistance)
                {
                    this.OptimalPaths.Add(pd);
                }
                else if (pd.RoundTripDistance < this.OptimalPaths.First().RoundTripDistance)
                {
                    this.OptimalPaths = new List<PathDetails>();
                    this.OptimalPaths.Add(pd);
                }
                else
                {
                    pd = null;
                }
            }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, this.DataLines);
        }

        // write up details on quick perm using
        // method was written with assitance of 
        // stackoveroverflow article: http://stackoverflow.com/questions/10887530/algorithm-to-retrieve-every-possible-combination-of-sublists-of-a-two-lists
        // and details found at http://www.quickperm.org/ for a more detailed understanding of what is done
        // method is based on Base-N-Odometer (http://www.quickperm.org/odometers.php)
        public static IEnumerable<IList<Node>> QuickPerm(IEnumerable<Node> set)
        {
            int nodeCount = set.Count(); // number of objects to permute
            int[] arb = new int[nodeCount]; // arbitrary objects for permutational assistance
            int[] permi = new int[nodeCount]; // controls iterations 

            var list = new List<Node>(set); // local copy of list to permute

            int i; // Upper Index i;             
            int j; // Lower Index j
            int tmp; // temporary variable for permutation index swaps
            Node tmpNode; // stores temporary node during swap
            //PathDetails currentPath; // stores details on current permutation, distance (cost), order, etc.

            // prime permutation node indexer and iterator
            for (i = 0; i < nodeCount; i++)
            {
                arb[i] = i + 1;
                permi[i] = 0;
            }

            // generate new path details, send in new copy of 
            // permutation list, otherwise we'd lose the order
            // on our next iteration through the permutation
            //currentPath = new PathDetails(new List<Node>(list));

            // return initial permutation path (orignal list)
            //yield return currentPath;
            yield return list;

            // on next call to this method we continue from here
            i = 1; // first swap points are 1 and 0
            while (i < nodeCount)
            {// perform permutations
                if (permi[i] < i)
                {
                    j = i % 2 * permi[i]; // if i is odd then j = p[i], else j = 0

                    // swap nodes
                    tmpNode = list[arb[j] - 1];
                    list[arb[j] - 1] = list[arb[i] - 1];
                    list[arb[i] - 1] = tmpNode;

                    // swap permutation indexers used to perform node swaps
                    // "arb" contains permutatoins based on index locations
                    // which are applied to our list
                    // performs swap based on N-1 data and then preps swap for N + 1.  
                    // The "arb" swaps performed below are
                    // applied in the next iteration
                    // swap(arb[j], arb[i])
                    tmp = arb[j];
                    arb[j] = arb[i];
                    arb[i] = tmp;

                    //currentPath = new PathDetails(new List<Node>(list));

                    // return current permutatoin details and MoveNext in iterator
                    //yield return currentPath;
                    yield return list;

                    permi[i]++; // increment p[i] by 1
                    i = 1; // reset i to 1
                }// end if
                else
                {// p[i] == i
                    permi[i] = 0; // reset p[i] to zero
                    i++; // increment i by 1
                } // end else (p[i] == i
            } // end while(i < nodeCount)
        }


        /// <summary>
        /// Reads in the TSP file
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="filePath">File path.</param>
        private static IList<Node> GetNodeList(TSPData file)
        {
            var nl = new List<Node>();
            // parses data segment of file
            for (int n = 7; n < file.DataLines.Count; n++)
            {
                var data = file.DataLines[n].Split(' ');

                // perform conversions
                var id = Int32.Parse(data[0]);
                var x = double.Parse(data[1]);
                var y = double.Parse(data[2]);

                nl.Add(new Node(id, x, y));
            }

            return nl;
        }
    }
}
