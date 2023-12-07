using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelingSalesPerson;

namespace K_GraphColoring
{
    public class K_Graph_WisdomOfCrowds
    {
        private IList<Color> _additionalColors;
        
        
        public K_Graph_WisdomOfCrowds(int numberOfPoints, IDictionary<int, Node> nodeData)
        {
            this.NumberOfColors = K_Graph_Colorist.TotalNumberOfColorsAvailable;
            this.MostColorsUsedByAnExpert = 0;
            this.NumberOfPoints = numberOfPoints;
            this._additionalColors = new List<Color>();
            this.NodeData = nodeData;

            this.ExpertList = new List<K_Graph_GA_Member>();
            this.ColoringAgreementMatrix = new int[this.NumberOfPoints, this.NumberOfColors];
        }

        public IList<K_Graph_GA_Member> ExpertList { get; private set; }

        public int[,] ColoringAgreementMatrix { get; private set; }

        public int NumberOfPoints { get; private set; }
        public int NumberOfColors { get; private set; }
        public int MostColorsUsedByAnExpert { get; private set; }
        public IDictionary<int, Node> NodeData { get; private set; }
        public void AddExpert(K_Graph_GA_Member expert)
        {
            this.ExpertList.Add(expert);
            var xc = expert.NodeColors.Values.Distinct().Count(x => x != Color.White);

            if (xc > this.MostColorsUsedByAnExpert)
                this.MostColorsUsedByAnExpert = xc; 

            foreach (var nc in expert.NodeColors)
            {                
                var xcKey = nc.Key - 1;
                var xcColor = (int)nc.Value;

                if (xcColor != -1)
                    this.ColoringAgreementMatrix[xcKey, xcColor]++;
            }
        }

        
        public  K_Graph_GA_Member GetWoCSolution(bool userResolver = false)
        {
            var wocColors = new Dictionary<int, Color>();
            var colorWheel = new List<Color>();

            for (int n = 0; n < K_Graph_Colorist.TotalNumberOfColorsAvailable; n++)
                colorWheel.Add((Color)n);

            for (int p = 0; p < this.NumberOfPoints; p++)
            {
                Color color = Color.White;
                int agreement = 0;

                for (int c = 0; c < this.NumberOfColors; c++)
                {
                    if (this.ColoringAgreementMatrix[p, c] > agreement)
                    {
                        color = (Color)c;
                        agreement = this.ColoringAgreementMatrix[p, c];
                    }
                }


                if (userResolver)
                {
                    // write code to resolve nodes of the same color
                    // if the current color is taken by a connected node
                    // add an aditional color

                    // get available colors for given node
                    // all points connected to current node
                    var usedNeighborColors = new List<Color>();
                    var conn = this.NodeData[p + 1].Connections;

                    foreach (var xc in conn)
                    {
                        if (wocColors.ContainsKey(xc.Id))
                            usedNeighborColors.Add(wocColors[xc.Id]);
                    }

                    var uc = usedNeighborColors.Distinct();

                    if (uc.Contains((Color)color))
                    {// if any connection is of same color pick another color
                        if (this._additionalColors.Count > 0)
                        {
                            var availableColors = this._additionalColors.Where(x => !uc.Contains(x) && x != color).ToList();

                            if (availableColors.Count() > 0)
                                color = availableColors[0];
                            else
                            {
                                var additionalColor = colorWheel.Where(x => !uc.Contains(x) && x != color).ToList()[0];
                                this._additionalColors.Add(additionalColor);
                                color = additionalColor;
                            }
                        }
                        else
                        {
                            var additionalColor = colorWheel.Where(x => !uc.Contains(x) && x != color).ToList()[0];
                            this._additionalColors.Add(additionalColor);
                            color = additionalColor;
                        }
                    }
                }

                wocColors.Add(p + 1, color);
            }

            var wocMember = new K_Graph_GA_Member(-1, wocColors, this.NodeData.Values, null, null, -1, -1);

            return wocMember;
        }
    }
}
