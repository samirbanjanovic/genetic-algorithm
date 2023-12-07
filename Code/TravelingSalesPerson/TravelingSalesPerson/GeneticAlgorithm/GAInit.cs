using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesPerson.GeneticAlgorithm
{
    public class GAInit
    {
        public event EventHandler Ellapsed;

        public GAInit(string filePath)
        {
            this.DataLines = ProcessTSPDocument.GetDataLines(filePath);
            this.Cities = ProcessTSPDocument.GetNodes(this.DataLines);
            this.CitiesDictionary = ProcessTSPDocument.GetNodesDictionary(this.DataLines);
            this.Generations = new List<GAInstance>();
        }

        public IList<string> DataLines { get; private set; }
        public IList<Node> Cities { get; private set; }
        public IDictionary<int, Node> CitiesDictionary { get; private set; }
        public IList<GAInstance> Generations { get; private set; }

        protected void OnEllapsed()
        {
            EventHandler hander = Ellapsed;
            if (hander != null)
                hander(this, null);
        }
    }
}
