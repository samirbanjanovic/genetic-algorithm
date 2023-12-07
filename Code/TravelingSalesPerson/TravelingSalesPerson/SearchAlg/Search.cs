using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesPerson.SearchAlg
{
    public static class Search
    {
        public static IList<Node> BFS(IList<Node> nodes)
        {
            var paths = new List<IList<Node>>();
            
            // use FIFO queue to traverse
            var q = new Queue<Node>();

            // set node 0 (Id = 1) as root node
            q.Enqueue(nodes[0]);

            while (q.Count > 0)
            {// keep looping until our queue is empty
                var current = q.Dequeue();
                current.IsExplored = true;

                // if we've arrived at our terminal then stop exploring                   
                if (current.IsFinalNode)                
                    return GetTravelPath(current);                

                if (current.Connections != null)
                {// current node has unexplored expandable node
                    foreach (var cn in current.Connections)
                    {// evaluate connections

                        if (cn.Backtrack == null || (cn.Backtrack.Id > current.Id))
                            cn.Backtrack = current;

                        // queue for exploration if it hasn't already been explored or queued
                        if (!cn.IsExplored && !cn.IsExpanded)
                        {
                            cn.IsExpanded = true;
                            q.Enqueue(cn);                        
                        }
                                                        
                    } // end foreach
                } //end if
            } //end while

            return new List<Node>();
        }

        public static Tuple<int, IList<Node>> DFS(IList<Node> nodes, bool findOptimal = false)
        {
            int pp = 0;
            IList<Node> currentOptimal = null;

            // use LIFO stack to traverse
            var s = new Stack<Node>();
            s.Push(nodes[0]);

            while (s.Count > 0)
            {
                var current = s.Pop();
                current.IsExplored = true;

                if(current.IsFinalNode)
                {
                    pp++;
                    var p = GetTravelPath(current);
                                                          
                    if (!findOptimal)
                    {// if we're not looking for optimal path return the first path found
                        return new Tuple<int, IList<Node>>(pp, p);
                    }
                                            
                    if(currentOptimal == null)
                    {// we're looking for optimal path; hold on to the first path discovered
                        currentOptimal = p;
                    }
                    else if (currentOptimal.Count > p.Count)
                    {// does the current optimal path have more hops than our newly discovered path
                        currentOptimal = p;
                    }
                    else if (currentOptimal.Count == p.Count)
                    {// is the number of hops the same
                        
                        // perfrom numeric comparison
                        for (int i = 0; i < p.Count; i++)
                        {
                            if (currentOptimal[i].Id > p[i].Id)
                            {
                                currentOptimal = p;
                                break;
                            }
                            else if (currentOptimal[i].Id < p[i].Id)
                                break;
                        }
                    }

                    current.IsExplored = false;
                }
                    
                // check if the current node has connections, if it does are any of them unexplored
                if (current.Connections != null)
                {
                    // find unexplored connected nodes
                    var conn = current.Connections;

                    if(!findOptimal)
                        conn = conn.Where(w => !w.IsExplored).ToList();

                    foreach (var cn in conn)
                    {// push each node to the LIFE stack

                        if (cn.Backtrack == null || (cn.Backtrack.Id > current.Id))
                            cn.Backtrack = current;

                        s.Push(cn);
                    }// end foreach
                }// end if
            }// end while

            return new Tuple<int,IList<Node>>(pp, currentOptimal);
        }

        private static IList<Node> GetTravelPath(Node final)
        {
            var node = final;            
            var op = new Stack<Node>();
            
            while (node != null)
            {
                op.Push(node);            
                node = node.Backtrack;
            }

            return op.ToList();
        }
    }
}
