using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelingSalesPerson.ClosestVertex;

namespace TravelingSalesPerson.GeneticAlgorithm
{
    public static class CrossoverMethods
    {
        public static PopulationMember FillTheBlanks(PopulationMember p1, PopulationMember p2, GAInstance ga)
        {
            PopulationMember offspring = null;
            Node[] gene = new Node[p1.Cities.Count];
            //insert section starting point
            var insertionPoint = GAInstance.RandomGenerator.Next(p1.Cities.Count);

            var p1Gene = p1.FittestNondiscriminatedRange;
            PopulationMember.SetDiscriminatedRange(p2, p1.FittestNondiscriminatedRange);

            if (insertionPoint + p1Gene.Count < gene.Length)
            {
                int iIndex = 0;

                for (int n = insertionPoint; n < insertionPoint + p1Gene.Count; n++)
                    gene[n] = p1Gene[iIndex++];
            }
            else
            {

                int iIndex = 0;

                //int L = gene.Length - insertionPoint;

                for (int n = insertionPoint; n < gene.Length; n++)
                    gene[n] = p1Gene[iIndex++];

                int R = p1Gene.Count - iIndex;
                for (int n = 0; n < R; n++)
                    gene[n] = p1Gene[iIndex++];
            }

            //filter out p1 items from p2 set
            var p2Gene = p2.FittestDiscriminatedleRange;

            int p2GI = 0;
            for (int n = 0; n < gene.Length; n++)
            {
                if (gene[n] == null)
                {
                    gene[n] = p2Gene[p2GI++];

                    if (p2GI == p2Gene.Count)
                        break;
                }
            }

            var p2Allowed = p2.Cities.Where(c => !p1Gene.Contains(c) && !p2Gene.Contains(c)).ToList();
            int p2AIndex = 0;

            var x = gene.Where(g => g == null).Count();

            for (int n = 0; n < gene.Length; n++)
            {
                if (gene[n] == null)
                {
                    gene[n] = p2Allowed[p2AIndex++];
                }
            }

            //var luckOfTheDraw = Math.Round(environment._randomGenerator.NextDouble(), 5);
            var luckOfTheDraw = GAInstance.RandomGenerator.NextDouble();
            if (luckOfTheDraw <= ga.MutationProbability)
            {// mutate entire g
                MutationMethods.RandomMutate(gene, ga.GeneticMutationPercentageRange);
            }

            offspring = new PopulationMember(ga.GenerationIndex, gene.ToList(), p1, p2, ga.GeneticInheritanceAllowance);
            return offspring;
        }





        public static Tuple<PopulationMember, PopulationMember> OptimizeRouteUsingCrossoverSwaps(PopulationMember p1, PopulationMember p2, GAInstance ga)
        {// code based off slides and book
            PopulationMember c1 = null;
            PopulationMember c2 = null;
            List<Node> geneC1 = new List<Node>();
            List<Node> geneC2 = new List<Node>();

            int crossoverPoint = GAInstance.RandomGenerator.Next(p1.Cities.Count);

            if (crossoverPoint == 0 || crossoverPoint == p1.Cities.Count - 1)
            {
                if (crossoverPoint == p1.Cities.Count - 1)
                {
                    geneC1 = new List<Node>(p1.Cities);
                    geneC2 = new List<Node>(p2.Cities);
                }
                else if (crossoverPoint == 0)
                {
                    geneC1 = new List<Node>(p2.Cities);
                    geneC2 = new List<Node>(p1.Cities);
                }
            }
            else
            {
                //first child
                geneC1.AddRange(p1.Cities.GetRange(0, crossoverPoint));
                var p2Remainder = p2.Cities.Where(c => !geneC1.Contains(c));
                geneC1.AddRange(p2Remainder);

                //second child
                geneC2.AddRange(p2.Cities.GetRange(0, crossoverPoint));
                var p1Remainder = p1.Cities.Where(c => !geneC2.Contains(c));
                geneC2.AddRange(p1Remainder);
            }

            var luckOfTheDraw = GAInstance.RandomGenerator.NextDouble();
            if (luckOfTheDraw <= ga.MutationProbability)
            {// mutate entire g
                MutationMethods.RandomMutate(geneC1, ga.GeneticMutationPercentageRange);
            }

            luckOfTheDraw = GAInstance.RandomGenerator.NextDouble();
            if (luckOfTheDraw <= ga.MutationProbability)
            {
                MutationMethods.RandomMutate(geneC2, ga.GeneticMutationPercentageRange);
            }

            c1 = new PopulationMember(ga.GenerationIndex, geneC1, p1, p2, ga.GeneticInheritanceAllowance);
            c2 = new PopulationMember(ga.GenerationIndex, geneC2, p1, p2, ga.GeneticInheritanceAllowance);

            return new Tuple<PopulationMember, PopulationMember>(c1, c2);
        }

        public static PopulationMember CreateChildUsingEdgeTechnique(PopulationMember p1, PopulationMember p2, GAInstance ga)
        {
            // build the initial cluster using the first node of the fittest p1 set
            var p1NDF = p1.FittestNondiscriminatedRange;
            var nodes = p1NDF.GetRange(1, p1NDF.Count - 1);
            List<int> insertionOrder = new List<int>();
            var initial = TravelByClosestVertex.BuildCluster(p1NDF[0], nodes, insertionOrder);
            var initalComplete = TravelByClosestVertex.ExpandCluster(initial, nodes, insertionOrder);

            var gene = new List<Node>();
            for (int i = 0; i < insertionOrder.Count; i++)
            {
                gene.Add(p1NDF.Where(x => x.Id == insertionOrder[i]).First());
            }

            PopulationMember.SetDiscriminatedRange(p2, p1NDF);
            var p2DF = p2.FittestDiscriminatedleRange;

            // insert middle of the gene using exclusion set
            var midSection = p1.Cities.Where(c => !p1NDF.Contains(c) && !p2DF.Contains(c)).ToList();
            gene.AddRange(midSection);

            nodes = p2DF.GetRange(1, p2DF.Count - 1);
            insertionOrder = new List<int>();
            initial = TravelByClosestVertex.BuildCluster(p2DF[0], nodes, insertionOrder);
            initalComplete = TravelByClosestVertex.ExpandCluster(initial, nodes, insertionOrder);

            for (int i = 0; i < insertionOrder.Count; i++)
            {
                gene.Add(p2DF.Where(x => x.Id == insertionOrder[i]).First());
            }

            var mr = GAInstance.RandomGenerator.NextDouble();
            if (mr <= ga.MutationProbability)
                MutationMethods.RandomMutate(gene, ga.GeneticMutationPercentageRange);


            var child = new PopulationMember(ga.GenerationIndex, gene, p1, p2, ga.GeneticInheritanceAllowance);

            return child;
        }


        public static PopulationMember CreateChildUsingBasicInheritance(PopulationMember primaryParent, PopulationMember secondaryParent, GAInstance ga)
        {
            PopulationMember offspring = null;

            PopulationMember.SetDiscriminatedRange(secondaryParent, primaryParent.FittestNondiscriminatedRange);

            var head = primaryParent.FittestNondiscriminatedRange;
            var tail = secondaryParent.FittestDiscriminatedleRange;

            var center = primaryParent.Cities.Where(c => !head.Contains(c) && !tail.Contains(c)).ToList();

            List<Node> gene = new List<Node>();
            gene.AddRange(new List<Node>(head));
            gene.AddRange(new List<Node>(center));
            gene.AddRange(new List<Node>(tail));


            var mr = GAInstance.RandomGenerator.NextDouble();
            if (mr <= ga.MutationProbability)
            {
                MutationMethods.RandomMutate(gene, ga.GeneticMutationPercentageRange);
            }

            offspring = new PopulationMember(ga.GenerationIndex, gene, primaryParent, secondaryParent, ga.GeneticInheritanceAllowance);

            return offspring;
        }
    }
}
