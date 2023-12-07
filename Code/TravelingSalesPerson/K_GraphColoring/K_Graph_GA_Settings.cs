using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K_GraphColoring
{
    public class K_Graph_GA_Settings
    {
        public int PopulationSize { get; set; }

        public double MutationRate { get; set; }

        public int GenerationCap { get; set; }

        public bool SelectFirstPointAtRandom { get; set; }

        public bool UseColorist { get; set; }
    }
}
