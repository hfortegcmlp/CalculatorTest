using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator.Contract;

namespace Calculator.Transformation
{
    public class Aggregator
    {
        private Hierarchy hierarchy;

        internal Aggregator(Hierarchy hierarchy)
        {
            this.hierarchy = hierarchy;
        }

        public List<NullableFact> Aggregate(List<NullableFact> facts, short parentId)
        {
            var results = new List<NullableFact>();
            var salesComponents = this.hierarchy.GetChildren(parentId).ToList();
            salesComponents.RemoveAll(s => s == parentId);


            foreach (var group in facts
                .Where(f => f.SalesComponentId == null || salesComponents.Contains(f.SalesComponentId.Value))
                .GroupBy(f => new { f.ProductId, f.GeographyId, f.TimeId }))
            {
                var fact = new NullableFact()
                {
                    ProductId = group.Key.ProductId,
                    GeographyId = group.Key.GeographyId,
                    TimeId = group.Key.TimeId,
                    SalesComponentId = parentId,
                    Value = group.Sum(g => g.Value)
                };
                results.Add(fact);
            }
            return results;
        }
    }
}
