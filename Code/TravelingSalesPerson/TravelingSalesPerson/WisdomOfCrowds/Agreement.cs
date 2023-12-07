using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesPerson.WisdomOfCrowds
{
    public class Agreement
    {
        public Agreement(int i, int j)
        {
            this.AgreementIndex = 0;            
            this.I = i;
            this.J = j;
        }

        public int AgreementIndex { get; set; }

        public int I { get; set; }

        public int J { get; set; }

        public ConnectionDetails Edge { get; set; }

        public bool IsMostAgreed { get; set; }

        public override string ToString()
        {
            return this.AgreementIndex.ToString();
        }

    }
}
