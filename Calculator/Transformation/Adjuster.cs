using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator.Contract;

namespace Calculator.Transformation
{
    public class Adjuster
    {
        private IHierarchy products;
        private IHierarchy geographies;
        private IHierarchy causals;
        private IHierarchy periods;

        public Adjuster(IHierarchy products, IHierarchy geographies, IHierarchy causals, IHierarchy periods)
        {
            this.products = products;
            this.geographies = geographies;
            this.causals = causals;
            this.periods = periods;
        }

        public List<ConcreteFact> AdjustSales(Request roiRequest)
        {
            var marginLookup = new FactLookup(roiRequest.Margins, products, geographies, causals, periods);
            var spendPeriods = roiRequest.Spend.Select(s => s.TimeId).ToList();

            if (!spendPeriods.Any())
                return new List<ConcreteFact>();

            // we can genericize this when more adjustments come online            
            List<NullableFact> adjustment = null;
               
            var adjustmentLookup = new FactLookup(adjustment, products, geographies, causals, periods);

            var timeIsNull = spendPeriods.All(s => s == null);
            var dueTos = roiRequest.Sales.Where(d => timeIsNull || spendPeriods.Contains(d.TimeId));

            dueTos.AsParallel().ForAll(item =>            
                {
                    var margin = marginLookup.GetParent(item, false);                    
                    var adjustmentFactor = adjustmentLookup.GetParent(item, false, 1.0f, false);

                    item.Value *= margin;
                    item.Value *= adjustmentFactor;
                });

            return roiRequest.Sales;
        }

        public List<NullableFact> AggregateSales(List<NullableFact> aggregationLevels, List<ConcreteFact> dueTos)
        {
            var result = new List<NullableFact>();
            var duetoLookup = new ConcreteFactLookup(dueTos, this.products, this.geographies, this.causals, this.periods);

            // Return a list of NullableFacts that aggregates the values for all children of the facts in aggregationLevels
            throw new NotImplementedException();
        }
    }
}
