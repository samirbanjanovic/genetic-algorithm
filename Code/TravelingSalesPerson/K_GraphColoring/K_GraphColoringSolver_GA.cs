using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K_GraphColoring
{
    public class K_GraphColoringSolver_GA
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
        
        private K_GraphColoringSolverInit _solverInit;
        private K_GraphColoringSolver_GASettings _GA_settings;
        public K_GraphColoringSolver_GA(K_GraphColoringSolverInit solverInit, K_GraphColoringSolver_GASettings GA_settings)
        {
            this._solverInit = solverInit;
            this._GA_settings = GA_settings;            
        }

        public int GenerationIndex { get; private set; }


    }
}
