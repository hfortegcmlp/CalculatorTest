using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Contract
{
    public class Adjustment
    {
        public AdjustmentType AdjustmentType { get; set; }

        public List<NullableFact> Facts { get; set; }
    }
}
