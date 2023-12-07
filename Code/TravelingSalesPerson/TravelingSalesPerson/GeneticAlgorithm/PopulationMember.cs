using OnTrac.Core.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesPerson.GeneticAlgorithm
{
    public class PopulationMember
        :  IComparable
        
    {
        private bool _isP1GeneFromFittest;
        private bool _findInheritance;

        public PopulationMember(int generation, List<Node> cities, PopulationMember p1, PopulationMember p2, double geneticInheritanceAllowance, bool findBestInheritance = true)
        {
            this.Generation = generation;
            this.Parent1 = p1;
            this.Parent2 = p2;

            if (p1 != null)
                this._isP1GeneFromFittest = p1.IsFittest;

            this.Children = new List<PopulationMember>();
            this.Cities = cities;

            this._findInheritance = findBestInheritance;

            if (this._findInheritance)
                this.GeneticInheritanceAllowance = geneticInheritanceAllowance;

            this.EvaluateFitness();


        }

        #region Public Instance Members
        public int Generation { get; private set; }

        public bool IsAggregateResult { get; internal set; }

        public double GeneticInheritanceAllowance { get; private set; }

        public bool IsFittest { get; set; }

        public PopulationMember Parent1 { get; internal set; }
        public PopulationMember Parent2 { get; internal set; }

        public IList<PopulationMember> Children { get; internal set; }

        private List<Node> _fittestNondiscriminatedRange; 
        public List<Node> FittestNondiscriminatedRange 
        { 
            get
            {
                var mf = GAInstance.RandomGenerator.NextDouble();

                if (this._isP1GeneFromFittest && mf <= 0.005D)
                    MutationMethods.RandomMutate(this._fittestNondiscriminatedRange, 0.08D);

                return this._fittestNondiscriminatedRange;
            }
            private set
            {
                this._fittestNondiscriminatedRange = value;
            }
        }
        public List<Node> FittestDiscriminatedleRange { get; private set; }

        private List<Node> _cities;
        public List<Node> Cities
        {
            get { return this._cities; }
            private set
            {
                this._cities = value;
            }
        }

        public IEnumerable<ConnectionDetails> Connections { get; private set; }

        public double Distance { get; private set; }

        public void EvaluateFitness()
        {
            var dc = PopulationMember.GetConnectionsAndFitness(this.Cities, true);

            this.Distance = dc.Item1;
            this.Connections = dc.Item2;

            this.FittestNondiscriminatedRange = FittestGeneRange(this.Cities, this.GeneticInheritanceAllowance);
        }

        public override string ToString()
        {
            return string.Join(", ", this.Cities);
        }

        #endregion

        #region Private Static Members

        private static Tuple<double, IEnumerable<ConnectionDetails>> GetConnectionsAndFitness(IList<Node> cities, bool loopBack)
        {
            IList<ConnectionDetails> cdl = new List<ConnectionDetails>();
            double totalDistance = 0;
            int cc = cities.Count();
            for (int i = 0; i < cc - 1; i++)
            {
                var cd = new ConnectionDetails(cities[i], cities[i + 1]);
                totalDistance += cd.Distance;
                cdl.Add(cd);

                if (loopBack && i == cc - 2)
                {// close our travel
                    var closure = new ConnectionDetails(cities[i + 1], cities[0]);
                    totalDistance += closure.Distance;
                    cdl.Add(closure);
                }
            }

            return new Tuple<double, IEnumerable<ConnectionDetails>>(totalDistance, cdl);
        }

        internal static void SetDiscriminatedRange(PopulationMember member, List<Node> exclusionSet)
        {
            if(member._findInheritance)
            {
                var acceptable = member.Cities.Where(g => !exclusionSet.Contains(g)).ToList();
                member.FittestDiscriminatedleRange = FittestGeneRange(acceptable, member.GeneticInheritanceAllowance);
            }            
        }

        private static List<Node> FittestGeneRange(List<Node> cities, double maxAllowedInheritance)
        {
            List<Node> fittestRange = null;
            double distance = 0D;           
            //int range = maxAllowedInheritance;
            //int range = GA.RandomGenerator.Next(2, maxAllowedInheritance);
            
            int range = (int)(cities.Count * maxAllowedInheritance);

            for (int n = 0; n < cities.Count; n++)
            {
                List<Node> subRange = null;

                if ((n + range) < cities.Count)
                {
                    subRange = cities.GetRange(n, range);

                }
                else
                {
                    int piece1 = 0;
                    int piece2 = 0;

                    if (n < cities.Count)
                    {
                        piece1 = (cities.Count - n);
                    }
                    else
                    {
                        piece1 = 1;
                    }

                    piece2 = range - piece1;

                    subRange = cities.GetRange(n, piece1);
                    subRange.AddRange(cities.GetRange(0, piece2));
                }

                double d = 0D;
                for (int x = 0; x < subRange.Count - 1; x++)
                    d += ConnectionDetails.CalculateDistance(subRange[x], subRange[x + 1]);
                
                if (fittestRange == null || d < distance)
                {
                    distance = d;
                    fittestRange = subRange;
                }

            }

            return fittestRange;
        }

        #endregion

        #region IComparable

        public int CompareTo(object obj)
        {
            var comparer = obj as PopulationMember;

            if (comparer == null)
                throw new ArgumentException("obj is not of type PopulationMember");

            return this.Distance.CompareTo(comparer.Distance);
        }

        #endregion
    }
}