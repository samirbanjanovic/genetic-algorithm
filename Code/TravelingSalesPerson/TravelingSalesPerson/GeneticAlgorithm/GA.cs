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
    public class GA
        : NotifyClassBase
    {
        #region Events

        public event EventHandler EndOfLife;
        public event EventHandler NewFittest;
        public event EventHandler GenerationIncreased;

        #endregion

        public static readonly int FITTEST_MEMBER_DOMINANCE_CAP = 100;
        public static readonly Random RandomGenerator = new Random();

        private IList<string> _historicMembers;
        private List<PopulationMember> _population;
        private BackgroundWorker _manBehindCurtain;

        public GA(IList<Node> originalStrand,   double mutationProbability = .0025D,
                                                double geneticMutationPercentageRange = 0.7D,    //how many genes are mutated
                                                double geneticInheritanceAllowance = 0.25D,  // rename to geneticCrossoverInheritanceMaximum? ; how many genes are passed on
                                                int stablePopulationSize = 500, 
                                                int fittestMemberDominanceCap = 100)
        {
            this.MutationProbability = mutationProbability;
            this.GeneticMutationPercentageRange = geneticMutationPercentageRange;
            this.StablePopulationSize = stablePopulationSize;
            this.GeneticInheritanceAllowance = geneticInheritanceAllowance;
            this.FittestMemberDominanceCap = fittestMemberDominanceCap;

            this.OriginalStrand = originalStrand;

            this.InitializeReproductionVariables();
            this.InitializeBackgroundWorker();
        }

        #region Init Methods
        private void InitializeBackgroundWorker()
        {
            this._manBehindCurtain = new BackgroundWorker();
            this._manBehindCurtain.WorkerSupportsCancellation = true;
            this._manBehindCurtain.DoWork += StartGrowing;
            this._manBehindCurtain.RunWorkerCompleted += (s, e) => { EndOfLife(this, e); };
        }

        private void InitializeReproductionVariables()
        {
            this.GenerationIndex = 0;
            this.MutatedMembers = 0;
            this.ClonedMembers = 0;

            this.TotalMembersCreatedInLifeSpan = 0;
            this.FittestMemberGenerationalRange = 0;


            this._historicMembers = new List<string>();
            this._population = new List<PopulationMember>();

            this.GenerationalData = new Dictionary<int, Tuple<int, PopulationMember, double, List<PopulationMember>>>();
        }
        #endregion Init Methods

        #region Properties

        public int FittestMemberDominanceCap { get; private set; }
        public IList<Node> OriginalStrand { get; private set; }        
        public double MutationProbability { get; private set; }
        public double GeneticMutationPercentageRange { get; private set; }
        public double GeneticInheritanceAllowance { get; private set; }
        public int StablePopulationSize { get; private set; }
        public int TotalMembersCreatedInLifeSpan { get; private set; }

        public bool Complete { get; private set; }

        private int _generationIndex;
        public int GenerationIndex 
        {
            get { return this._generationIndex; } 
            private set
            {
                this._generationIndex = value;
                this.OnPropertyChanged("GenerationIndex");
            }
        }
        public int MutatedMembers { get; private set; }
        public int ClonedMembers { get; private set; }
        public int FittestMemberGenerationalRange { get; private set; }

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
                this.OnPropertyChanged("FittestMember");
            }
        }

        public double LifeSpanTime { get; private set; }

        public Dictionary<int, Tuple<int, PopulationMember, double, List<PopulationMember>>> GenerationalData { get; private set; }
        public List<PopulationMember> Population
        {
            get { return this._population; }
        }

        #endregion Properties

        public void BeginLife()
        {
            this.CreateSeedPopulation(this.OriginalStrand);
            this._manBehindCurtain.RunWorkerAsync();
        }

        #region Private Execution Methods

        private void StartGrowing(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (!worker.CancellationPending)
            {
                GA.NextGeneration(this);

                var str = string.Format("Gen: {0} -- Avg. Fitness: {1} -- Fittest: {2}", this.GenerationIndex.ToString(),
                                                                                         this.GenerationalData[this.GenerationIndex].Item3.ToString(),
                                                                                         this.FittestMember.Distance.ToString());
                Console.WriteLine(str);

                if (this.FittestMemberGenerationalRange == this.FittestMemberDominanceCap)
                    break;

                this.FittestMemberGenerationalRange++;

                this.GenerationIndex++;
                this.GenerationIncreased(this, null);
            }

            e.Cancel = true;
        }

        #endregion

        #region Private Instance Methods

        private bool AddToPopulation(PopulationMember member)
        {
            this._population.Add(member);
            this._historicMembers.Add(member.ToString());

            // check how well our new member compares to the 
            // current fittest member.  If our new member is 
            // fitter it'll be made the fittest in the population
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

                // mutate the fittest member that's been around for more
                // than 10 generations so that we promote diversity
                // and so that our populatoin doesn't stagnate into
                // just fittest gene members
                //if (this.FittestMemberGenerationalRange > 5)
                //    GA.Mutate(tmp.Cities, 0.5);//this._randomGenerator);

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

        private void CreateSeedPopulation(IList<Node> origin)
        {
            List<PopulationMember> g0 = new List<PopulationMember>();
            
            for (int n = 0; n < this.StablePopulationSize; n++)
            {
                var gene = new List<Node>(origin);
                GA.Mutate2(gene, 0.9);// this._randomGenerator);

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
                        GA.Mutate2(oneOfTheOriginals.Cities, 0.9);// this._randomGenerator);

                } while (!accepted);
            }

            var averageFittness = this.Population.Average(x => x.Distance);
            this.GenerationalData.Add(GenerationIndex, new Tuple<int, PopulationMember,double, List<PopulationMember>>(this.Population.Count, this.FittestMember,averageFittness, g0));

            var str = string.Format("Gen: {0} -- Fittness: {1} -- Fittest: {2}", 
                                            this.GenerationIndex.ToString(), 
                                            this.GenerationalData[this.GenerationIndex].Item3.ToString(), 
                                            this.FittestMember.Distance.ToString());
            Console.WriteLine(str);

            this.GenerationIndex++;
            this.OnPropertyChanged("GenerationalData");
        }


        #endregion Private Instance Methods

        #region Crossover Methods

        private static void NextGeneration(GA environment)
        {
            
            List<PopulationMember> generation = new List<PopulationMember>();
            environment.Population.Sort();
            
            for (int i = 0; i < environment.StablePopulationSize; i++)
            {
                // pick a random first parent
                int pId = GA.RandomGenerator.Next(0, environment.Population.Count);
                var p1 = environment.Population[pId];

                // pick a random second parent
                pId = GA.RandomGenerator.Next(0, environment.Population.Count);
                var p2 = environment.Population[pId];

                var offspring = GA.CrossoverParents(p1, p2, environment);

                generation.Add(offspring.Item1);
                generation.Add(offspring.Item2);
            }


            var tmpFittest = environment.FittestMember;
            //environment._population = new List<PopulationMember>();
            //environment._population.Add(tmpFittest);            
            foreach (var m in generation)
                environment.AddToPopulation(m);


            if (tmpFittest != environment.FittestMember)
                environment.NewFittest(environment, new EventArgs());

            var averageFittness = generation.Average(x => x.Distance);
            environment.GenerationalData.Add(environment.GenerationIndex, new Tuple<int, PopulationMember, double, List<PopulationMember>>(generation.Count, environment.FittestMember,averageFittness, generation));
            
            GA.PopulationControl(environment);

            environment.OnPropertyChanged("GenerationalData");
        }

        private static void NextGeneration2(GA environment)
        {
            
            List<PopulationMember> generation = new List<PopulationMember>();
            environment.Population.Sort();
            int n = 0;

            for (int i = 0; i < environment.Population.Count; i++)
            {
                // pick a random first parent
                //int pId = GA.RandomGenerator.Next(0, environment.Population.Count);
                //var p1 = environment.Population[memberIndecies[pId]];
                var p1 = environment.Population[i];

                // pick a random second parent
                int pId;
                PopulationMember p2;
                do
                {
                    pId = GA.RandomGenerator.Next(0, environment.Population.Count);
                    //var p2 = environment.Population[memberIndecies[pId]];
                    p2 = environment.Population[pId];
                } while (pId == i);
                
                //memberIndecies.RemoveAt(pId); //remove parents from potential procreation list
                var offspring = GA.CrossoverParents(p1, p2, environment);

                generation.Add(offspring.Item1);
                generation.Add(offspring.Item2);
            }


            var tmpFittest = environment.FittestMember;
            //environment._population = new List<PopulationMember>();
            //environment._population.Add(tmpFittest);            
            foreach (var m in generation)
                environment.AddToPopulation(m);


            if (tmpFittest != environment.FittestMember)
                environment.NewFittest(environment, new EventArgs());

            var averageFittness = generation.Average(x => x.Distance);
            environment.GenerationalData.Add(environment.GenerationIndex, new Tuple<int, PopulationMember, double, List<PopulationMember>>(generation.Count, environment.FittestMember, averageFittness, generation));

            GA.PopulationControl(environment);

            environment.OnPropertyChanged("GenerationalData");
        }

        private static Tuple<PopulationMember, PopulationMember> CrossoverParents(PopulationMember p1, PopulationMember p2, GA environment)
        {
            //PopulationMember offspring1 = GA.BirthChild(p1, p2, environment);
            //PopulationMember offspring2 = GA.BirthChild(p2, p1, environment);

            PopulationMember offspring1 = GA.BirthChildUsingEdges(p1, p2, environment);
            PopulationMember offspring2 = GA.BirthChildUsingEdges(p2, p1, environment);
			
			//var children = GA.BirthChild4(p1, p2, environment);
			
            environment.TotalMembersCreatedInLifeSpan += 2;

			//return children;
			
            return new Tuple<PopulationMember, PopulationMember>(offspring1, offspring2);
        }

        private static PopulationMember BirthChildUsingEdges(PopulationMember p1, PopulationMember p2, GA environment)
        {
            // build the initial cluster using the first node of the fittest p1 set
            var p1C = p1.FittestNondiscriminatedRange;
            var nodes = p1C.GetRange(1, p1C.Count - 1);
            List<int> insertionOrder = new List<int>();
            var initial = TravelByClosestVertex.BuildCluster(p1C[0], nodes, insertionOrder);
            var initalComplete = TravelByClosestVertex.ExpandCluster(initial, nodes, insertionOrder);

            var gene = new List<Node>();
            for(int i = 0; i < insertionOrder.Count; i++)
            {
                gene.Add(p1C.Where(x => x.Id == insertionOrder[i]).First());
            }

            PopulationMember.SetDiscriminatedRange(p2, p1C);
            var p2C = p2.FittestDiscriminatedleRange;

            // insert middle of the gene using exclusion set
            var midSection = p1.Cities.Where(c => !p1C.Contains(c) && !p2C.Contains(c)).ToList();
            gene.AddRange(midSection);
            
            nodes = p2C.GetRange(1, p2C.Count - 1);
            insertionOrder = new List<int>();
            initial = TravelByClosestVertex.BuildCluster(p2C[0], nodes, insertionOrder);
            initalComplete = TravelByClosestVertex.ExpandCluster(initial, nodes, insertionOrder);
            
            for (int i = 0; i < insertionOrder.Count; i++)
            {
                gene.Add(p2C.Where(x => x.Id == insertionOrder[i]).First());
            }

            var mr = GA.RandomGenerator.NextDouble();
            if (mr <= environment.MutationProbability)
            {// mutate entire g
                GA.Mutate2(gene, environment.GeneticMutationPercentageRange);//, environment._randomGenerator);
                environment.MutatedMembers++;
            }

            var child = new PopulationMember(environment.GenerationIndex, gene, p1, p2, environment.GeneticInheritanceAllowance);

            return child;
        }

            
        private static PopulationMember BirthChild(PopulationMember primaryParent, PopulationMember secondaryParent, GA environment)
        {
            PopulationMember offspring = null;

            PopulationMember.SetDiscriminatedRange(secondaryParent, primaryParent.FittestNondiscriminatedRange);


            // build sections of genetic make up
            // primary parent gives the front seciont, center section is built from primarys parents nodes that are not in the head or tail
            // tial is built from the secondary parent
            var head = primaryParent.FittestNondiscriminatedRange;
            var tail = secondaryParent.FittestDiscriminatedleRange;

            var center = primaryParent.Cities.Where(c => !head.Contains(c) && !tail.Contains(c)).ToList();

            // apply small mutation to center section? this would minimize possiblity of clones even if parents
            // procreate more than once
            //GA.MutateGeneSection(center, r);

            // add the c1 head using p1 nodes
            List<Node> gene = new List<Node>();
            gene.AddRange(new List<Node>(head));
            gene.AddRange(new List<Node>(center));
            gene.AddRange(new List<Node>(tail));

            
            var mr = GA.RandomGenerator.NextDouble();
            if (mr <= environment.MutationProbability)
            {// mutate entire g
                GA.Mutate2(gene, environment.GeneticMutationPercentageRange);//, environment._randomGenerator);
                environment.MutatedMembers++;
            }

            offspring = new PopulationMember(environment.GenerationIndex, gene, primaryParent, secondaryParent, environment.GeneticInheritanceAllowance);

            return offspring;
        }
        
        #endregion Crossover Methods

        #region Population Control Methods


        private static void PopulationControl(GA environment)
        {
            environment.Population.Sort();           
			
            if(environment.GenerationIndex % 5 == 0)
            {
                environment.Population.RemoveRange(environment.StablePopulationSize, environment.Population.Count - environment.StablePopulationSize);
            }
            else
            {
                //keep the best 80% of population
                var acceptableRange = (int)(environment.Population.Count * 0.8D);

                environment.Population.RemoveRange(acceptableRange, environment.Population.Count - acceptableRange);
            }			
        }

        #endregion Population Control Methods

        #region Mutation Members


        internal static void Mutate2(IList<Node> genesToMutate, double geneticMutationPercentageRange)
        {
            int i = 0;
            var indecies = genesToMutate.Select(m => i++).ToList();

            int mutationSize = (int)(geneticMutationPercentageRange * genesToMutate.Count);

            for (int n = 0; n < mutationSize; n += 2)
            {
                int ri = GA.RandomGenerator.Next(indecies.Count);
                var ich1 = indecies[ri];
                indecies.RemoveAt(ri);

                ri = GA.RandomGenerator.Next(indecies.Count);
                var ich2 = indecies[ri];
                indecies.RemoveAt(ri);

                var tmp = genesToMutate[ich1];
                genesToMutate[ich1] = genesToMutate[ich2];
                genesToMutate[ich2] = tmp;
            }
        }
		
        private static void Mutate(IList<Node> genesToMutate, double geneticMutationRange)
        {
            IList<int> locationIndecies = new List<int>();
            int cityCount = genesToMutate.Count;

            // percentage of nodes to swap; represents a consecutive range
            int range = (int)Math.Ceiling(cityCount * geneticMutationRange);

            // is odd?
            if (range % 2 == 1)
                range += 1;

            // pick random starting node
            // pick a random starting city
            //int N = r.Next(0, cityCount - 1);
            int N = GA.RandomGenerator.Next(0, cityCount - 1);

            if (N + range < cityCount)
            {// no looping to head requiered; initial node + range will keep us inbound of array
                int K = N + range;

                while (N < K)
                    locationIndecies.Add(N++);
            }
            else
            {// starting node would lead to out of range exception when looping with range
                int K = N - range;

                // insert first range
                while (N < cityCount)
                    locationIndecies.Add(N++);

                for (int nr = 0; nr < K; nr++)
                    locationIndecies.Add(nr);
            }

            RandomizedSwap(genesToMutate, locationIndecies);//, r);
        }

        private static void RandomizedSwap(IList<Node> locations, IList<int> locationIndecies)//, Random r)
        {
            int n1Index = 0;
            int n2Index = 0;
            int metaIndex = 0;

            while (locationIndecies.Count > 3)
            {
                //metaIndex = r.Next(0, locationIndecies.Count - 1); // pick first swap destination
                metaIndex = GA.RandomGenerator.Next(0, locationIndecies.Count); // pick first swap destination
                n1Index = locationIndecies[metaIndex];
                locationIndecies.RemoveAt(metaIndex);

                //metaIndex = r.Next(0, locationIndecies.Count); // pick second swap destination
                metaIndex = GA.RandomGenerator.Next(0, locationIndecies.Count); // pick second swap destination
                n2Index = locationIndecies[metaIndex];
                locationIndecies.RemoveAt(metaIndex);

                var tmp = locations[n1Index];
                locations[n1Index] = locations[n2Index];
                locations[n2Index] = tmp;
            }

            var tmpF = locations[locationIndecies[0]];
            locations[locationIndecies[0]] = locations[locationIndecies[1]];
            locations[locationIndecies[1]] = tmpF;
        }

        #endregion Mutation Members

    }
}