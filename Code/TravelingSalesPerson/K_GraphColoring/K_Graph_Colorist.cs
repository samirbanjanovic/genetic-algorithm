using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelingSalesPerson;

namespace K_GraphColoring
{
    public enum Color
    {
        White = -1,
        Red = 0,
        Blue,
        Green,
        Orange,
        Violet,
        Indigo,
        Yellow,
        Coral,
        Cyan,
        Fuchsia,
        Gainsboro,
        HotPink,
        LavenderBlush,
        LightGreen,
        LightYellow,
        Lime,
        Magenta,
        Maroon,
        Navy,
        Olive,
        Orchid,
        PaleGreen,
        Pink,
        Purple,
        Salmon,
        Silver,
        SkyBlue,
        Tan,
        Teal,
        Thistle,
        Turquoise,
        Wheat,
        WhiteSmoke,
        YellowGreen
    }

    public static class K_Graph_Colorist        
    {
        private static int _totalNumberOfColorsAvailable = Enum.GetValues(typeof(Color)).Cast<int>().Max();
        public static int TotalNumberOfColorsAvailable
        {            
            get
            {
                return _totalNumberOfColorsAvailable;
            }            
        }

        // set based coloring
        public static IDictionary<int, Color> ColorGraph(IEnumerable<Node> nodes, int numberOfColorsToUse, bool randomFirst = false)
        {

            int nodeCount = nodes.Count();
            var nodeColors = new Dictionary<int, Color>();

            var colorWheel = new List<Color>();
            for (int c = 0; c < numberOfColorsToUse; c++)
                colorWheel.Add((Color)c);
            
            // color the chosen initial node using random color
            var randColor = (Color)K_Graph_GA.RandomGenerator.Next(0, numberOfColorsToUse);
            
            Node initial = null;
            if (randomFirst)
            {
                var randInitial = K_Graph_GA.RandomGenerator.Next(1, nodeCount + 1);
                initial = nodes.Where(x => x.Id == randInitial).First();
            }
            else
                initial = nodes.Where(x => x.IsInitialNode).First();

            nodeColors.Add(initial.Id, randColor);

            // find uncolored nodes that belong to N connection set
            // N is current available maximum of connections per nodes

            do
            {
                // set of nodes not colored
                var blank = nodes.Where(x => !nodeColors.Keys.Contains(x.Id));

                // uncolored nodes with most connections            
                // find max number of connections in uncolored nodes
                var maxConnAvailable = blank.Max(x => x.Connections.Count);

                // find all nodes that show number of connections
                var toColor = blank.Where(x => x.Connections.Count == maxConnAvailable);

                foreach (var tc in toColor)
                {
                    IList<Color> takenColors = new List<Color>();

                    foreach (var conn in tc.Connections)
                    {
                        Color tk;
                        if (nodeColors.TryGetValue(conn.Id, out tk))
                            if (tk != Color.White)
                                takenColors.Add(tk);
                    }

                    var usedColors = takenColors.Distinct().ToList();
                    var usedColorCount = usedColors.Count;
                   
                    if (usedColorCount == numberOfColorsToUse)
                    {// no possible color to choose
                        var failure = Color.White;
                        nodeColors.Add(tc.Id, failure);
                    }
                    else
                    {
                        int anyColorIndex = 0;
                        var availableColors = colorWheel.Where(x => !usedColors.Contains(x)).ToList();
                        
                        if (availableColors.Count > 1)
                            anyColorIndex = K_Graph_GA.RandomGenerator.Next(0, availableColors.Count);

                        var color = availableColors[anyColorIndex];
                        nodeColors.Add(tc.Id, color);
                    }
                }
            } while (nodeColors.Count < nodeCount);


            return nodeColors;
        }
    }
}
