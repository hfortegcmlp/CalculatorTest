using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calculator.Contract;

namespace Calculator.Transformation
{
    internal class ConcreteFactLookup
    {
        private List<ConcreteFact> facts;
        private IHierarchy products;
        private IHierarchy geographies;
        private IHierarchy causals;
        private IHierarchy periods;

        public ConcreteFactLookup(List<ConcreteFact> facts, IHierarchy products, IHierarchy geographies, IHierarchy causals, IHierarchy periods)
        {
            if (facts == null)
                facts = new List<ConcreteFact>();
            
            if (!facts.Any())
                facts.Add(new ConcreteFact() { Value = 0 });

            this.facts = facts;
            this.products = products;
            this.geographies = geographies;
            this.causals = causals;
            this.periods = periods;
        }

        /// <summary>
        /// This is the 'reverse' of the GetParent - it finds any children in the hierarchy
        /// and sums their values. There can be multiple children. 
        /// An element is considered a child of itself 
        /// (i.e. sum of children for a leaf level is the value of that leaf level item)
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="geographyId"></param>
        /// <param name="causalId"></param>
        /// <param name="periodId"></param>
        /// <returns></returns>
        internal float SumChildren(short? productId, short? geographyId, short? causalId, short? periodId, bool fail = false)
        {
            var match = this.facts.SingleOrDefault(f => f.ProductId == productId && f.GeographyId == geographyId && f.SalesComponentId == causalId && f.TimeId == periodId);
            if (match != null && match.Value != 0)
                return match.Value;

#if DEBUG
            var prods = productId == null ? new List<short>() : new List<short>(this.products.GetChildren(productId.Value));
            var geogs = geographyId == null ? new List<short>() : new List<short>(this.geographies.GetChildren(geographyId.Value));
            var causal = causalId == null ? new List<short>() : new List<short>(this.causals.GetChildren(causalId.Value));
            var period = periodId == null ? new List<short>() : new List<short>(this.periods.GetChildren(periodId.Value));
#endif
            // Return matches that have the correct IDs
            var matches = new List<ConcreteFact>();

            if (matches == null)
                throw new InvalidOperationException(String.Format("Invalid facts identified for given dimensions p:{0}, g:{1}, c:{2}, t{3}", productId, geographyId, causalId, periodId));

            if (matches.Count() == 0 && fail)
            {
                throw new InvalidOperationException(String.Format("Zero facts identified for given dimensions p:{0}, g:{1}, c:{2}, t{3}", productId, geographyId, causalId, periodId));
            }

            return matches.Sum(f => f.Value);
        }

        internal float SumChildren(NullableFact fact)
        {
            return this.SumChildren(fact.ProductId, fact.GeographyId, fact.SalesComponentId, fact.TimeId);
        }
    }
}
