using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calculator.Contract;

namespace Calculator.Transformation
{
    internal class FactLookup
    {
        private List<NullableFact> facts;
        private IHierarchy products;
        private IHierarchy geographies;
        private IHierarchy causals;
        private IHierarchy periods;
        

        public FactLookup(List<NullableFact> facts, IHierarchy products, IHierarchy geographies, IHierarchy causals, IHierarchy periods)
        {
            if (facts == null)
                facts = new List<NullableFact>();
            
            if (!facts.Any())
                facts.Add(new NullableFact() { Value = 1.0f });

            this.facts = facts;
            this.products = products;
            this.geographies = geographies;
            this.causals = causals;
            this.periods = periods;
        }

        /// <summary>
        /// Get the value of the only parent for this fact in the hierarchy
        /// </summary>        
        /// <remarks>
        /// There can be only one parent value for each fact (including 'self').
        /// e.g. if a margin is specified at product level (all geog, all periods) then this function will return that value 
        ///     for any key combination using that product (or children of that product). Note that in this case margin cannot 
        ///     be specified at any level 'under' the product level due to single-value-per-hierarchical-branch rule.
        /// 
        /// alternatively if post-period adjustment is specified that p/g/c/p level this will return that value. In this
        ///     case post-period adjustment cannot be present in the hierarchy at any higher level (e.g. 'all causals'). We
        ///     enforce this at the database level.
        /// </remarks>
        internal float GetParent(short productId, short geographyId, short causalId, short periodId, bool failing = true, float defaultValue = 0.0f, bool exactTime = true)
        {
//#if DEBUG
//            var prods = new List<int>(this.products.GetParents(productId));
//            var geogs = new List<int>(this.geographies.GetParents(geographyId));
//            var causal = new List<int>(this.causals.GetParents(causalId));
//            var period = new List<int>(this.periods.GetParents(periodId));
//#endif       

            //var matches = this.facts
            //    .Where(f => f.ProductId == null || this.products.GetParents(productId).Contains(f.ProductId.Value))
            //    .Where(f => f.GeographyId == null || this.geographies.GetParents(geographyId).Contains(f.GeographyId.Value))
            //    .Where(f => f.SalesComponentId == null || this.causals.GetParents(causalId).Contains(f.SalesComponentId.Value))
            //    .Where(f => f.TimeId == null || this.periods.GetParents(periodId).Contains(f.TimeId.Value));

            Func<NullableFact, bool> timeDelegate = f => exactTime
                ? f.TimeId == periodId
                : f.TimeId == null || this.periods.RelationExists(periodId, f.TimeId.Value);

            var matches = this.facts
                .Where(timeDelegate)
                .Where(f => f.ProductId == null || this.products.RelationExists(productId, f.ProductId.Value))
                .Where(f => f.GeographyId == null || this.geographies.RelationExists(geographyId, f.GeographyId.Value))
                .Where(f => f.SalesComponentId == null || this.causals.RelationExists(causalId, f.SalesComponentId.Value));

            if (matches.Count() == 0)
            {
                if (failing)
                    throw new InvalidOperationException(String.Format("Zero facts identified for given dimensions p:{0}, g:{1}, c:{2}, t:{3}", productId, geographyId, causalId, periodId));
                else
                    return defaultValue;
            }

            if (matches.Count() > 1)
            {
                if (failing)
                    throw new InvalidOperationException(String.Format("Multiple facts identified for given dimensions p:{0}, g:{1}, c:{2}, t:{3}", productId, geographyId, causalId, periodId));
                else
                {
                    return GetBestValue(matches, defaultValue, productId, geographyId, causalId, periodId);
                }
            }

            return matches.Single().Value;
        }

        /// <summary>
        /// If user gives us a margin value at multiple points in a hierarchy, we can't just return default value.
        /// </summary>
        /// <param name="matches"></param>
        /// <param name="defaultValue"></param>
        /// <param name="productId"></param>
        /// <param name="geographyId"></param>
        /// <param name="causalId"></param>
        /// <param name="periodId"></param>
        /// <returns></returns>
        internal float GetBestValue(IEnumerable<NullableFact> matches, float defaultValue, short productId, short geographyId, short causalId, short periodId)
        {
            if (matches == null || !matches.Any())
                return defaultValue;

            // Return the fact with the best match, i.e. the one with the most Ids that match (ProductId, GeographyId, SalesComponentId and TimeId)

            return defaultValue;
        }

        internal float GetParent(ConcreteFact fact, bool failing = true, float defaultValue = 0.0f, bool exactTime = true)
        {
            return this.GetParent(fact.ProductId, fact.GeographyId, fact.SalesComponentId, fact.TimeId, failing, defaultValue, exactTime);
        }
    }
}
