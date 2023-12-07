using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelingSalesPerson;

namespace K_GraphColoring
{
    public class K_Graph_Greedy_Coloring
    {
        public event EventHandler<IList<Node>> GreedyColoringComplete;

        protected virtual void OnGreedyColoringComplete(object sender, IList<Node> e)
        {
            var handler = this.GreedyColoringComplete;
            if (handler != null)
                handler(this, e);
        }

        private readonly K_Graph_GA_Init _init;
        private readonly int _availableColors;
        private readonly bool _randomizeInitial;
        private List<Color> _colorWheel;
        

        public K_Graph_Greedy_Coloring(K_Graph_GA_Init init, bool randomizeInitial = false)
        {
            _init = init;
            ResetNodeData();
            _availableColors = _init.NumberOfColorsToUse;
            ColorsUsed = 2;
            _colorWheel = new List<Color>();
            for (int i = 0; i < ColorsUsed; i++)
                this._colorWheel.Add((Color)i);
            _randomizeInitial = randomizeInitial;
        }

        public int ColorsUsed { get; private set; }
        public IList<Node> Results => _init.NodeData.Values.ToList();
        public bool IsComplete => _init.NodeData.Values.All(x => x.Color != null && x.IsExplored);
        public void BeginColoring()
        {
            Node currentNode = null;
            if(_randomizeInitial)
            {
                currentNode = SelectRanomNode();
            }
            else
            {
                currentNode = FindNextNode();
            }
            currentNode.IsInitialNode = true;
            
            while (currentNode != null)
            {
                ColorGraph(currentNode);
                currentNode = FindNextNode();
            }
            this.OnGreedyColoringComplete(this, Results);
        }

        private void ResetNodeData()
        {
            foreach (var node in Results)
            {
                node.Color = null;
                node.IsExplored = false;
                node.IsInitialNode = false;
            }
        }

        private Node SelectRanomNode()
        {
            var random = new Random();
            var searchSpace = _init.NodeData.Values.Where(x => !x.IsExplored && x.Color == null).ToList();
            var randomIndex = random.Next(0, searchSpace.Count);
            var initial = searchSpace[randomIndex];            
            return initial;
        }

        private void ColorGraph(Node currentNode)
        {
            while (currentNode != null)
            {
                // evalute if current node has any colored connections and what color they are
                var takenColors = currentNode.Connections.Where(x => x.Color != null).Select(x => x.Color).Distinct().ToList();

                // check if any colors are available in the color wheel
                var availableColors = _colorWheel.Where(x => !takenColors.Contains(x)).ToList();
                var hasAvailableColors = availableColors.Count > 0;
                if (!hasAvailableColors)
                {// no colors available, add a new color to the wheel
                    ColorsUsed++;
                    // add new color to color wheel
                    _colorWheel.Add((Color)(ColorsUsed - 1));
                }

                var assignableColor = hasAvailableColors ? availableColors[0] : _colorWheel.Last();

                // assign the current color and move to next color
                currentNode.Color = assignableColor;
                currentNode.IsExplored = true;

                currentNode = FindNextNode(currentNode);
            }
        }

        private Node FindNextNode()
            => _init.NodeData.Values
                    .Where(x => !x.IsExplored && x.Color == null)
                    .OrderByDescending(node => node.Connections.Count)
                    .OrderBy(x => x.Id)
                    .FirstOrDefault();

        private Node FindNextNode(Node currentNode)
            => currentNode
                .Connections
                .Where(x => !x.IsExplored && x.Color == null)
                .OrderByDescending(node => node.Connections.Count)
                .OrderBy(x => x.Id)
                .FirstOrDefault();
    }
}
