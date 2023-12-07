using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K_GraphColoring
{
    public class K_Graph_GA
    {
        private static readonly Random _randomGenerator = new Random();
        public static Random RandomGenerator { get { return K_Graph_GA._randomGenerator; } }


        #region Events

        public event EventHandler LifeComplete;        
        public event EventHandler<int> NumberOfColorsIncreased;
        public event EventHandler<int> NewGenerationCreated;
        public event EventHandler<K_Graph_GA_Member> NewFittestMember;
        public event EventHandler<K_Graph_GA_Member> NewPopulationMember;

        
        protected virtual void OnLifeComplete(object sender, EventArgs e)
        {
            var handler = this.LifeComplete;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnNewFittestMember(object sender, K_Graph_GA_Member fittest)
        {
            var handler = this.NewFittestMember;
            if (handler != null)
                handler(this, fittest);
        }

        protected virtual void OnNumberOfColorsIncreased(object sender, int colorCount)
        {
            var handler = this.NumberOfColorsIncreased;
            if (handler != null)
                handler(this, colorCount);
        }

        protected virtual void OnNewGenerationCreated(object sender, int gen)
        {
           var handler = this.NewGenerationCreated;
            if (handler != null)
                handler(this, this.GenerationCount);
        }

        protected virtual void OnNewPopulationMember(object sender, K_Graph_GA_Member newMember)
        {
            var handler = this.NewPopulationMember;
            if (handler != null)
                handler(this, newMember);
        }

        #endregion Events
        
        private K_Graph_GA_Init _ga_Init;
        private K_Graph_GA_Settings _ga_settings;

        //private int _currentGeneration;

        public K_Graph_GA(K_Graph_GA_Init gaInit, K_Graph_GA_Settings gaSettings)
        {
            this._ga_Init = gaInit;
            this._ga_settings = gaSettings;

            this.ColoringAttemptIteration = 0;
            this.TotalHistoryMemeberCount = 0;
            this.GlobalGenerationCount = 0;
            this.DiscoveryQueue = new Queue<IDictionary<int, Color>>();            
        }

        public Queue<IDictionary<int, Color>> DiscoveryQueue { get; private set; }

        public int GlobalGenerationCount { get; private set; }

        public int GenerationCount { get; private set; }

        public int ColoringAttemptIteration { get; private set; }

        public bool IsComplete { get; private set; }

        public IList<Color> ColorWheel { get; private set; }

        public K_Graph_GA_Member FittestMember { get; private set; }

        public int FittestMemberGenerationalRange { get; private set; }

        public List<K_Graph_GA_Member> Population { get; private set; }

        public int TotalHistoryMemeberCount { get; set; }

        public void BeginEvolution()
        {
            if (this._ga_settings.UseColorist)
                this.CreateGeneration0WithColorist();
            else
                this.CreateGeneration0();

            while (GenerationCount < _ga_settings.GenerationCap)
            {
                this.GenerationCount++;
                this.GlobalGenerationCount++;
                this.CreateNextGeneration();

                if (this.GenerationCount == this._ga_settings.GenerationCap && this.FittestMember == null)
                {// no answer found with given color limit;
                    // increase number of colors and start over;
                    this._ga_Init.NumberOfColorsToUse++;

                    if (this._ga_Init.NumberOfColorsToUse > K_Graph_Colorist.TotalNumberOfColorsAvailable)
                        throw new Exception("Not enough colors for coloring");

                    this.OnNumberOfColorsIncreased(this, this._ga_Init.NumberOfColorsToUse);

                    if (this._ga_settings.UseColorist)
                        this.CreateGeneration0WithColorist();
                    else
                        this.CreateGeneration0();

                    this.GlobalGenerationCount++;
                }
                else if (this.GenerationCount == this._ga_settings.GenerationCap && this.FittestMember != null)
                {
                    this.IsComplete = true;
                }
                    
                this.OnNewGenerationCreated(this, this.GenerationCount);
            }

            if (this.IsComplete)
            {
                this.DiscoveryQueue.Enqueue(this.FittestMember.NodeColors);
                this.OnLifeComplete(this, null);
                this.OnNewFittestMember(this, this.FittestMember);
            }
        }

        private void CreateGeneration0WithColorist()
        {
            this.GenerationCount = 0;
            this.ColoringAttemptIteration++;
            this.Population = new List<K_Graph_GA_Member>();
            
            // set the color wheel
            this.ColorWheel = new List<Color>();
            for (int i = 0; i < this._ga_Init.NumberOfColorsToUse; i++)
                this.ColorWheel.Add((Color)i);
            
            for (int i = 0; i < this._ga_settings.PopulationSize; i++)
            {
                var colorSet = K_Graph_Colorist.ColorGraph(this._ga_Init.NodeData.Values, this._ga_Init.NumberOfColorsToUse, this._ga_settings.SelectFirstPointAtRandom);
                var member = new K_Graph_GA_Member(this.GlobalGenerationCount, colorSet, this._ga_Init.NodeData.Values, null, null, this.ColoringAttemptIteration, ++this.TotalHistoryMemeberCount);

                this.AddToPopulation(member);
                this.DiscoveryQueue.Enqueue(member.NodeColors);

                this.OnNewPopulationMember(this, member);
            }

            //this._currentGeneration++;
        }

        private void CreateGeneration0()
        {
            this.GenerationCount = 0;
            this.ColoringAttemptIteration++;
            this.Population = new List<K_Graph_GA_Member>();
            
            // set the color wheel
            this.ColorWheel = new List<Color>();
            for (int i = 0; i < this._ga_Init.NumberOfColorsToUse; i++)
                this.ColorWheel.Add((Color)i);

            for (int i = 0; i < this._ga_settings.PopulationSize; i++)
            {
                var colorSet = new Dictionary<int, Color>();

                foreach(var n in this._ga_Init.NodeData.Keys)
                {
                    var key = n;
                    var color = (Color)K_Graph_GA.RandomGenerator.Next(1, this._ga_Init.NumberOfColorsToUse + 1);

                    colorSet.Add(key, color);
                }

                var member = new K_Graph_GA_Member(this.GlobalGenerationCount, colorSet, this._ga_Init.NodeData.Values, null, null, this.ColoringAttemptIteration, ++this.TotalHistoryMemeberCount);
                
                //this.Population.Add(member);

                this.AddToPopulation(member);
                this.DiscoveryQueue.Enqueue(member.NodeColors);
                this.OnNewPopulationMember(this, member);
            }

            //this._currentGeneration++;
        }

        private void CreateNextGeneration()
        {
            var newGeneration = new List<K_Graph_GA_Member>();
            this.Population.Sort();

            for (int i = 0; i < this._ga_settings.PopulationSize; i++)
            {
                // pick random first parent 
                int randomParentIndex = K_Graph_GA.RandomGenerator.Next(0, this.Population.Count);
                var p1 = this.Population[randomParentIndex];

                randomParentIndex = K_Graph_GA.RandomGenerator.Next(0, this.Population.Count);
                var p2 = this.Population[randomParentIndex];

                var offspring = this.RandomPointCrossover(p1, p2);

                newGeneration.Add(offspring.Item1);
                newGeneration.Add(offspring.Item2);
            }

            foreach (var nm in newGeneration)
                this.AddToPopulation(nm);

            this.PopulationControl();
        }

        private void AddToPopulation(K_Graph_GA_Member member)
        {
            this.Population.Add(member);

            if((this.FittestMember == null && member.IsComplete)|| 
                member.CompareTo(this.FittestMember) == -1)
            {
                this.FittestMemberGenerationalRange = 0;
                this.FittestMember = member;

                this.OnNewFittestMember(this, this.FittestMember);
            }
            else if(this.FittestMember != null)
            {
                this.FittestMemberGenerationalRange++;
            }

            this.DiscoveryQueue.Enqueue(member.NodeColors);
            this.OnNewPopulationMember(this, member);
        }

        private void PopulationControl()
        {
            this.Population.Sort();

            if(this.GenerationCount % 5 == 0)
            {
                // kill bottom of the barrel
                this.Population.RemoveRange(this._ga_settings.PopulationSize, this.Population.Count - this._ga_settings.PopulationSize);
            }
            else
            {// after each generation keep 80%
                var numberToKeep = (int)(this.Population.Count * 0.80D);
                this.Population.RemoveRange(numberToKeep, this.Population.Count - numberToKeep);                
            }
        }

        #region Mutation and Crossover

        private Tuple<K_Graph_GA_Member, K_Graph_GA_Member> RandomPointCrossover(K_Graph_GA_Member p1, K_Graph_GA_Member p2)
        {
            K_Graph_GA_Member c1 = null;
            K_Graph_GA_Member c2 = null;

            var c1Colors = new Dictionary<int, Color>();
            var c2Colors = new Dictionary<int, Color>();

            int crossoverPoint = K_Graph_GA.RandomGenerator.Next(0, p1.NodeColors.Count);

            if(crossoverPoint == 0 || crossoverPoint == p1.NodeColors.Count - 1)
            {
                if(crossoverPoint == p1.NodeColors.Count -1)
                {
                    c1Colors = new Dictionary<int, Color>(p1.NodeColors);
                    c2Colors = new Dictionary<int, Color>(p2.NodeColors);
                }
                else
                {
                    c1Colors = new Dictionary<int, Color>(p2.NodeColors);
                    c2Colors = new Dictionary<int, Color>(p1.NodeColors);
                }                
            }
            else
            {
                // first child
                for (int i = 1; i <= crossoverPoint; i++)
                    c1Colors.Add(i, p1.NodeColors[i]);
                
                var complementC1_P2 = p2.NodeColors.Where(x => !c1Colors.Keys.Contains(x.Key));

                foreach (var compl in complementC1_P2)
                    c1Colors.Add(compl.Key, compl.Value);

                // second child
                for (int i = 1; i <= crossoverPoint; i++)
                    c2Colors.Add(i, p2.NodeColors[i]);

                var complementC2_P1 = p1.NodeColors.Where(x => !c2Colors.Keys.Contains(x.Key));

                foreach (var compl in complementC2_P1)
                    c2Colors.Add(compl.Key, compl.Value);
            }

            // apply mutation
            double luckOfTheDraw = 0D;
            
            // mutate child 1
            luckOfTheDraw = K_Graph_GA.RandomGenerator.NextDouble();                        
            if (luckOfTheDraw <= this._ga_settings.MutationRate)            
                this.RandomMutation(c1Colors);
            
            // mutate child 2
            luckOfTheDraw = K_Graph_GA.RandomGenerator.NextDouble();
            if (luckOfTheDraw <= this._ga_settings.MutationRate)            
                this.RandomMutation(c2Colors);

            c1 = new K_Graph_GA_Member(this.GlobalGenerationCount, c1Colors, this._ga_Init.NodeData.Values, p1, p2, this.ColoringAttemptIteration, ++this.TotalHistoryMemeberCount);
            c2 = new K_Graph_GA_Member(this.GlobalGenerationCount, c2Colors, this._ga_Init.NodeData.Values, p1, p2, this.ColoringAttemptIteration, ++this.TotalHistoryMemeberCount);

            return new Tuple<K_Graph_GA_Member,K_Graph_GA_Member>(c1, c2);
        }

        private void RandomMutation(IDictionary<int, Color> nodeColors, int numberOfPointsToRecolor = 2)
        {
            var points = nodeColors.Keys.ToList();
            
            // recolor two points at random...any color that isn't the old color
            for(int n =0; n < numberOfPointsToRecolor; n++)
            {
                var randomIndex = K_Graph_GA.RandomGenerator.Next(0, points.Count);


                int randomPointKey = points[randomIndex];                
                var nColor = nodeColors[randomPointKey];

                // if white you can select any color in the color wheel
                if(nColor == Color.White)
                {
                    var rci = K_Graph_GA.RandomGenerator.Next(0, this.ColorWheel.Count);
                    nodeColors[randomPointKey] = this.ColorWheel[rci];
                }
                else
                {// select a random color that doesn't include current color;
                    var availableColors = ColorWheel.Where(c => c != nColor).ToList();

                    if(availableColors.Count != 0)
                    {
                        var aci = K_Graph_GA.RandomGenerator.Next(0, availableColors.Count);
                        nodeColors[randomPointKey] = availableColors[aci];
                    }                    
                }

                points.Remove(randomPointKey);
            }
        }

        #endregion Mutation and Crossover
    }
}
