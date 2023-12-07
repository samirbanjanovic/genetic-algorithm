using OnTrac.Core.WPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TravelingSalesPerson.ClosestVertex;

namespace TravelingSalesPerson.GeneticAlgorithm
{
    public class GAInstance
        : NotifyClassBase
    {
        #region Events

        public event EventHandler LifeComplete;
        public event EventHandler NewFittestMember;
        public event EventHandler<int> NewGenerationCreated;

        protected virtual void OnLifeComplete(object sender, EventArgs e)
        {
            EventHandler handler = LifeComplete;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnNewFittestMember(object sender, EventArgs e)
        {
            EventHandler handler = NewFittestMember;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnNewGenerationCreated(object sender, int gen)
        {
            EventHandler<int> handler = NewGenerationCreated;
            if (handler != null)
                handler(this, this.GenerationIndex);
        }

        #endregion Events

        public static readonly int FITTEST_MEMBER_DOMINANCE_CAP = 100;
        public static readonly Random RandomGenerator = new Random();

        private IList<string> _historicMembers;
        private List<PopulationMember> _population;

        public GAInstance(IList<Node> originalStrand, double mutationProbability = .0025D,
                                                double geneticMutationPercentageRange = 0.02D,    //how many genes are mutated            
                                                double geneticInheritancePercentage = 0.25D,
                                                int stablePopulationSize = 150,     
                                                bool useGenerationCap = true,
                                                int generationCap = 200,
                                                int fittestMemberDominanceCap = 50,                                
                                                int crossoverType = 0)                                                                                                                                                                                                
        {
            this.MutationProbability = mutationProbability;
            this.GeneticMutationPercentageRange = geneticMutationPercentageRange;
            this.StablePopulationSize = stablePopulationSize;

            this.UseGenerationCap = useGenerationCap;
            this.GenerationCap = generationCap;
            this.FittestMemberDominanceCap = fittestMemberDominanceCap;
            this.CrossoverMethodType = crossoverType;
            this.OriginalStrand = originalStrand;
            this.GeneticInheritanceAllowance = geneticInheritancePercentage;
            this.InitializeReproductionVariables();
        }

        #region Init Methods
        private void InitializeReproductionVariables()
        {
            this.GenerationIndex = 0;

            this.FittestMemberGenerationalRange = 0;


            this._historicMembers = new List<string>();
            this._population = new List<PopulationMember>();

            this.GenerationalData = new Dictionary<int, Tuple<int, PopulationMember, double>>();
        }
        #endregion Init Methods

        #region Properties
        public Dictionary<int, Tuple<int, PopulationMember, double>> GenerationalData { get; private set; }

        public int StablePopulationSize { get; private set; }
        
        public IList<Node> OriginalStrand { get; private set; }

        public double MutationProbability { get; private set; }

        public bool UseGenerationCap { get; set; }

        public int GenerationCap { get; set; }

        public double GeneticMutationPercentageRange { get; private set; }
        
        public double GeneticInheritanceAllowance { get; private set; }

        public int FittestMemberDominanceCap { get; private set; }

        public int FittestMemberGenerationalRange { get; private set; }

        public int CrossoverMethodType { get; set; }

        public bool CancelSearch { get; set; }

        public List<PopulationMember> Population
        {
            get { return this._population; }
        }
        
        private int _generationIndex;
        public int GenerationIndex 
        {
            get { return this._generationIndex; } 
            private set
            {
                this._generationIndex = value;
            }
        }
        
        private PopulationMember _fittestMember;
        public PopulationMember FittestMember 
        { 
            get
            {
                return this._fittestMember;
            }
            private set
            {
                this._fittestMember = value;
                this.OnNewFittestMember(this, null);
                this.OnPropertyChanged("FittestMember");
            }
        }

        #endregion Properties
        
        #region Execution Methods

        public void BeginLife()
        {
            if (this.CrossoverMethodType == 0)
                this.CreateFitSeedPopulation(this.OriginalStrand);
            else
                this.CreateUnfitRandomSeedPopulation(this.OriginalStrand);

            this.StartGrowing();
        }
        
        #endregion

        #region Private Instance Methods

        private bool AddToPopulation(PopulationMember member)
        {
            this._population.Add(member);
            this._historicMembers.Add(member.ToString());
            this.CompareToFittest(member);

            return true;
        }


        private bool CompareToFittest(PopulationMember member)
        {
            if (this.FittestMember == null)
            {
                this.FittestMember = member;
                this.FittestMember.IsFittest = true;
            }
            else if (member.Distance < FittestMember.Distance)
            {


                var tmp = FittestMember;
                tmp.IsFittest = false;

                this.FittestMemberGenerationalRange = 0;

                this.FittestMember = member;
                this.FittestMember.IsFittest = true;
            }
            else
            {
                return false;
            }

            return true;
        }

        private void CreateFitSeedPopulation(IList<Node> origin)
        {
            List<PopulationMember> g0 = new List<PopulationMember>();

            for (int n = 0; n < this.StablePopulationSize; n++)
            {
                var subset = new List<Node>(origin);
                
                var initial = GAInstance.RandomGenerator.Next(origin.Count);
                var nodes = subset.Where(s => s.Id != subset[initial].Id).ToList();

                List<int> insertionOrder = new List<int>();
                var cluster = TravelByClosestVertex.BuildCluster(subset[initial], nodes, insertionOrder);
                var initalComplete = TravelByClosestVertex.ExpandCluster(cluster, nodes, insertionOrder);

                List<Node> gene = new List<Node>();

                for(int x = 0; x < insertionOrder.Count; x++)
                {
                    var item = subset.Where(p => p.Id == insertionOrder[x]).First();
                    gene.Add(item);
                }

                PopulationMember oneOfTheOriginals = new PopulationMember(this.GenerationIndex, gene, null, null, this.GeneticInheritanceAllowance);
                this.AddToPopulation(oneOfTheOriginals);
                g0.Add(oneOfTheOriginals);
            }

            var averageFittness = this.Population.Average(x => x.Distance);
            this.GenerationalData.Add(GenerationIndex, new Tuple<int, PopulationMember, double>(this.Population.Count, this.FittestMember, averageFittness));

            var str = string.Format("Gen: {0} -- Fittness: {1} -- Fittest: {2}",
                                            this.GenerationIndex.ToString(),
                                            this.GenerationalData[this.GenerationIndex].Item3.ToString(),
                                            this.FittestMember.Distance.ToString());
            Console.WriteLine(str);

            this.OnNewGenerationCreated(this, this.GenerationIndex);
        }

        private void CreateUnfitRandomSeedPopulation(IList<Node> origin)
        {
            List<PopulationMember> g0 = new List<PopulationMember>();
            
            for (int n = 0; n < this.StablePopulationSize; n++)
            {
                var gene = new List<Node>(origin);
                MutationMethods.RandomMutate(gene, 0.9);

                PopulationMember oneOfTheOriginals = new PopulationMember(this.GenerationIndex, gene, null, null, this.GeneticInheritanceAllowance);

                bool accepted;
                do
                {
                    accepted = !this._historicMembers.Contains(oneOfTheOriginals.ToString());

                    if (accepted)
                    {
                        this.AddToPopulation(oneOfTheOriginals);
                        g0.Add(oneOfTheOriginals);
                    }                        
                    else
                        MutationMethods.RandomMutate(oneOfTheOriginals.Cities, 0.9);

                } while (!accepted);
            }

            var averageFittness = this.Population.Average(x => x.Distance);
            this.GenerationalData.Add(GenerationIndex, new Tuple<int, PopulationMember,double>(this.Population.Count, this.FittestMember,averageFittness));

            var str = string.Format("Gen: {0} -- Fittness: {1} -- Fittest: {2}", 
                                            this.GenerationIndex.ToString(), 
                                            this.GenerationalData[this.GenerationIndex].Item3.ToString(), 
                                            this.FittestMember.Distance.ToString());
            Console.WriteLine(str);

            this.OnNewGenerationCreated(this, this.GenerationIndex);
        }

        public bool Complete { get; private set; }
        private void StartGrowing()
        {            
            while (!this.Complete)
            {
                this.GenerationIndex++;
                this.NextGeneration();

                var str = string.Format("Gen: {0} -- Avg. Fitness: {1} -- Fittest: {2}", this.GenerationIndex.ToString(),
                                                                                         this.GenerationalData[this.GenerationIndex].Item3.ToString(),
                                                                                         this.FittestMember.Distance.ToString());
                Console.WriteLine(str);


                if (this.UseGenerationCap)
                {
                    if (this.GenerationIndex == this.GenerationCap)
                        this.Complete = true;
                }
                else if(this.FittestMemberGenerationalRange == this.FittestMemberDominanceCap)
                {
                    this.Complete = true;                
                }
                      
                this.FittestMemberGenerationalRange++;
                
                this.OnNewGenerationCreated(this, this.GenerationIndex);
                
                this.OnPropertyChanged("GenerationIndex");                
            }

            this.OnLifeComplete(this, null);

            if (this.CrossoverMethodType != 0)
            {
                var co_old = this.CrossoverMethodType;
                this.CrossoverMethodType = 0;
                this.MutationProbability = .0075D;
                this.GeneticMutationPercentageRange = .7D;
                int g = 0;

                while (g < 300)
                {
                    this.GenerationIndex++;
                    this.NextGeneration();

                    this.FittestMemberGenerationalRange++;

                    this.OnNewGenerationCreated(this, this.GenerationIndex);
                    g++;
                    this.OnPropertyChanged("GenerationIndex");
                }
            }
        }

        private void NextGeneration()
        {

            List<PopulationMember> generation = new List<PopulationMember>();
            this.Population.Sort();

            for (int i = 0; i < this.StablePopulationSize; i++)
            {
                // pick a random first parent
                int pId = GAInstance.RandomGenerator.Next(0, this.Population.Count);
                var p1 = this.Population[pId];

                // pick a random second parent
                pId = GAInstance.RandomGenerator.Next(0, this.Population.Count);
                var p2 = this.Population[pId];

                var offspring = this.CrossoverParents(p1, p2);

                generation.Add(offspring.Item1);
                generation.Add(offspring.Item2);
            }

            //var tmpFittest = this.FittestMember;
            foreach (var m in generation)
                this.AddToPopulation(m);

            var averageFittness = generation.Average(x => x.Distance);
            this.GenerationalData.Add(this.GenerationIndex, new Tuple<int, PopulationMember, double>(generation.Count, this.FittestMember, averageFittness));

            this.PopulationControl();
        }

        private Tuple<PopulationMember, PopulationMember> CrossoverParents(PopulationMember p1, PopulationMember p2)
        {
            PopulationMember childA = null;
            PopulationMember childB = null;

            if (this.CrossoverMethodType == 0)
            {
                var x = CrossoverMethods.OptimizeRouteUsingCrossoverSwaps(p1, p2, this);
                childA = x.Item1;
                childB = x.Item2;
            }
            else if (this.CrossoverMethodType == 1)
            {
                childA = CrossoverMethods.CreateChildUsingEdgeTechnique(p1, p2, this);
                childB = CrossoverMethods.CreateChildUsingEdgeTechnique(p2, p1, this);

            }
            else if (this.CrossoverMethodType == 2)
            {
                childA = CrossoverMethods.CreateChildUsingBasicInheritance(p1, p2, this);
                childB = CrossoverMethods.CreateChildUsingBasicInheritance(p2, p1, this);
            }
            else if(this.CrossoverMethodType == 3)
            {
                childA = CrossoverMethods.FillTheBlanks(p1, p2, this);
                childB = CrossoverMethods.FillTheBlanks(p2, p1, this);
            }

            return new Tuple<PopulationMember, PopulationMember>(childA, childB);
        }
        #endregion Private Instance Methods



        #region Population Control Methods


        private void PopulationControl()
        {
            this.Population.Sort();

            if (this.GenerationIndex % 5 == 0)
            {
                this.Population.RemoveRange(this.StablePopulationSize, this.Population.Count - this.StablePopulationSize);
            }
            else
            {
                //keep the best 80% of population
                var acceptableRange = (int)(this.Population.Count * 0.8D);

                this.Population.RemoveRange(acceptableRange, this.Population.Count - acceptableRange);
            }			
        }

        #endregion Population Control Methods
        
    }
}