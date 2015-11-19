using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Contract
{
    public class Request
    {
        public List<NullableFact> Margins { get; set; }

        public List<NullableFact> Spend { get; set; }

        public List<Adjustment> Adjustments { get; set; }

        public List<ConcreteFact> Sales { get; set; }

        public List<Relation> ProductHierarchy { get; set; }

        public List<Relation> GeographyHierarchy { get; set; }

        public List<Relation> SalesComponentHierarchy { get; set; }

        public List<Relation> PeriodHierarchy { get; set; }
    }
}
