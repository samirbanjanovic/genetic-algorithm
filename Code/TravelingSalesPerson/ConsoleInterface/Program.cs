using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelingSalesPerson;
using System.ComponentModel;
using System.Diagnostics;
using TravelingSalesPerson.SearchAlg;
using TravelingSalesPerson.BruteForce;
using TravelingSalesPerson.GeneticAlgorithm;
using System.Timers;
using System.Threading;
using TravelingSalesPerson.WisdomOfCrowds;

namespace ConsoleInterface
{
    class Program
    {
        static Stopwatch sw = new Stopwatch();
        static IList<PopulationMember> _expertSolutions;
        static void Main(string[] args)
        {
            //string path = @"\\psf\Dropbox\School\CECS545\Projects\Project 5\Random11.tsp";          //VM Windows
            //string path = @"C:\Users\sbanjanovic\Dropbox\School\CECS545\Projects\Project 5\Random11.tsp";	//Windows (Office)
            //string path = @"C:\Users\Samir\Documents\My Dropbox\School\CECS545\Projects\Project 5\Random11.tsp";     //Windows (Home)

            IList<string> results = new List<string>();
            var op = new List<PopulationMember>();
            var agg = new List<Tuple<double, IList<Agreement>>>();
            string path = @"C:\Users\Samir\Documents\My Dropbox\School\CECS545\Projects\Project 5\Random77.tsp";     //Windows (Home)
            GAInit init = new GAInit(path);
            for (int x = 0; x < 1; x++ )
            {
                _expertSolutions = new List<PopulationMember>();
                for (int n = 0; n < 15; n++)
                {

                    var mutationProbability = 10D / 100;
                    var mutationSize = 2D / 100; //2D
                    var geneticInheritanceSize = 25D / 100;
                    var generationCap = 100;
                    var stablePopulationSize = 150;
                    var dominanceCap = 75;

                    int type = 0;

                    GAInstance ga = new GAInstance(init.Cities, mutationProbability, mutationSize, geneticInheritanceSize, stablePopulationSize, true, generationCap, dominanceCap, type);

                    ga.LifeComplete += geneticAlg_LifeComplete;
                    ga.BeginLife();
                }

                WisdomOfCrowds woc = new WisdomOfCrowds(init.Cities);
                foreach (var es in _expertSolutions)
                    woc.AddExpertSolution(es);

                //woc.ProcessExpertSolutionsByChosenDestination();

                var ap = string.Join(", ", woc.CombinedExpertsSolution.Cities.Select(c => c.Id).ToList());
                results.Add(string.Format("Optimal: {0} - Aggregate: {1}", woc.BestExpert.Distance, woc.CombinedExpertsSolution.Distance));
                results.Add(string.Format("Aggregate Path: {0}", ap));
                op.Add(woc.CombinedExpertsSolution);
                agg.Add(new Tuple<double, IList<Agreement>>(woc.CombinedExpertsSolution.Distance ,woc.AggregatePath));

                //Console.WriteLine("\r\r\r\r\r\r\r");
                //Console.WriteLine("Optimal: {0} - Aggregate: {1}", woc.BestExpert.Distance, woc.CombinedExpertsSolution.Distance);                
                //Console.WriteLine("Aggregate Path: {0}", ap);
            }

            Console.WriteLine("\r\r");
            foreach (var r in results)
                Console.WriteLine(r);

            Console.WriteLine("STOP!");
        }

        
        static void geneticAlg_LifeComplete(object sender, EventArgs e)
        {
            var s = (GAInstance)sender;
            var es = s.FittestMember;
            
            _expertSolutions.Add(es);
        }

        static void ga_NewFittestMember(object sender, EventArgs e)
        {
            Console.WriteLine("NEW!");
        }

        static void Results(object sender, EventArgs e)
        {
            sw.Stop();
            var x = sw.Elapsed.TotalMinutes;
            var ga = sender as GAInstance;

            var f = ga.FittestMember;
        }


        private static void ExecuteSearchAlg()
        {

            /* Project 2
                
                string path = @"/Users/samir/Dropbox/School/CECS545/Projects/Project 2/11PointDFSBFS.tsp"; 		    //OS X 
                string path = @"\\psf\Dropbox\School\CECS545\Projects\Project 2\11PointDFSBFS.tsp";          //VM Windows
                string path = @"C:\Users\sbanjanovic\Dropbox\School\CECS545\Projects\Project 2\11PointDFSBFS.tsp";	//Windows (Office)
                string path = @"C:\Users\Samir\Documents\My Dropbox\School\CECS545\Projects\Project 2\11PointDFSBFS.tsp";	//Windows (Home)
             */
            //Console.WriteLine("Enter TSP document path:> ");
            //var path = Console.ReadLine();

            //if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path.Trim()))
            //{
            //    Console.WriteLine("Must specify a file path");
            //    return;
            //}
            string path = @"C:\Users\sbanjanovic\Dropbox\School\CECS545\Projects\Project 3\random30.tsp";	//Windows (Office)

            Console.Write("Enter final node's Id:> ");
            int fn = int.Parse(Console.ReadLine());

            Console.Write("Enter number test iterations:> ");
            int iterations = int.Parse(Console.ReadLine());
            double[] executionTime = null;
            var os = new TSP_SearchAlg(path, fn);

            // run first instance of BFS search, this causes the .NET platform to perform it's 
            // optimization            
            os.BFS_Search();

            Console.WriteLine("\r\n\r\n\r\n(Priming run) Elapsed Time BFS (ms): {0}", os.ExecutionTime.TotalMilliseconds);
            Console.WriteLine("BFS travel path: {0}", string.Join(", ", os.OptimalPath));
            Console.WriteLine("BFS calculated Distance: {0}", os.Distance);
            Console.WriteLine("BFS minimum hops taken: {0}", os.NumberOfHops);

            executionTime = new double[iterations];
            for (int i = 0; i < iterations; i++)
            {
                os.BFS_Search();
                executionTime[i] = os.ExecutionTime.TotalMilliseconds;
            }

            Console.WriteLine("BFS execution details for {0} iterations, all times are in milliseconds", iterations);
            Console.WriteLine("Average: {0}", Math.Round(executionTime.Average(), 8));
            Console.WriteLine("Min: {0}", executionTime.Min());
            Console.WriteLine("Max: {0}", executionTime.Max());

            // run first instance of DFS search, this causes the .NET platform to perform it's 
            // optimization 
            os.DFS_Search();

            var firstExecTime = new double[iterations];
            Console.WriteLine("\r\n\r\n\r\n(Priming run without optimal path search) Elapsed Time DFS (ms): {0}", os.DFS_First_ExecutionTime.TotalMilliseconds);
            Console.WriteLine("(Priming run with optimal path search) Elapsed Time DFS (ms): {0}", os.ExecutionTime.TotalMilliseconds);
            Console.WriteLine("Potential number of DFS paths: {0}", os.DFS_Potential_PathCount);
            Console.WriteLine("DFS optimal travel path: {0}", string.Join(", ", os.OptimalPath));
            Console.WriteLine("DFS optimal calculated Distance: {0}", os.Distance);

            Console.WriteLine("DFS first travel path: {0}", string.Join(", ", os.DFS_First));
            Console.WriteLine("DFS first calculated Distance: {0}", os.DFS_First_Distance);

            Console.WriteLine("DFS minimum hops taken: {0}", os.NumberOfHops);

            // perform search N number of times
            executionTime = new double[iterations];
            for (int i = 0; i < iterations; i++)
            {
                os.DFS_Search();
                executionTime[i] = os.ExecutionTime.TotalMilliseconds;
                firstExecTime[i] = os.DFS_First_ExecutionTime.TotalMilliseconds;
            }

            // round to 8 decimal places 
            Console.WriteLine("DFS execution details for {0} iterations, all times are in milliseconds", iterations);
            Console.WriteLine("Average (First): {0}", Math.Round(firstExecTime.Average(), 8));
            Console.WriteLine("Min (First): {0}", firstExecTime.Min());
            Console.WriteLine("Max (First): {0}", firstExecTime.Max());

            Console.WriteLine("Average (Optimal): {0}", Math.Round(executionTime.Average(), 8));
            Console.WriteLine("Min (Optimal): {0}", executionTime.Min());
            Console.WriteLine("Max (Optimal): {0}", executionTime.Max());

        }

        private static double CalculateVariance(double[] executionTimeSet)
        {
            var sum = executionTimeSet.Sum();
            var sumOfSquared = executionTimeSet.Sum(s => s * s);

            var variance = (sumOfSquared - ((sum * sum) / executionTimeSet.Count())) / (executionTimeSet.Count() - 1);
            var stdDev = Math.Sqrt(variance);

            return variance;
        }

        private static void ExecuteBruteForce()
        {
            /*  Project 1
                string path = @"/Users/samir/Dropbox/School/CECS545/Projects/Project 1/Project1/Random7.tsp"; 		    //OS X 
                string path = @"\\psf\Dropbox\School\CECS545\Projects\Project 1\Project1\Random4.tsp";                  //VM Windows   
                string path = @"C:\Users\sbanjanovic\Dropbox\School\CECS545\Projects\Project 1\Project1Banjanovic\InputData\Random4.tsp";	//Windows (Office)
                string path = @"C:\Users\Samir\Documents\My Dropbox\School\CECS545\Projects\Project 1\Project1\Random4.tsp";	//Windows (Home)
            */
            Console.WriteLine("Enter TSP document path: > ");
            var path = Console.ReadLine();

            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path.Trim()))
            {
                Console.WriteLine("Must specify a file path");
                return;
            }


            Stopwatch sw = new Stopwatch();
            sw.Start();
            var cp = new TSP_BruteForce(path);
            cp.FindOptimalPaths();
            sw.Stop();
            Console.WriteLine("Analysis complete");
            Console.WriteLine("Nodes in set: {0}", string.Join(", ", cp.Nodes));
            Console.WriteLine("Elapsed time (minutes): {0}\r\n\r\n", sw.Elapsed.TotalMinutes);
            Console.WriteLine("Optimal Paths");
            foreach (var p in cp.OptimalPaths)
            {
                Console.WriteLine("Path {0}; distance {1}", string.Join(", ", p.Nodes), p.RoundTripDistance);
            }

        }
    }


}