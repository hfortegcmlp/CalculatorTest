using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Calculator.Contract;
using Calculator.Transformation;

namespace Calculator
{
    public class CalculatorService
    {
        public List<NullableFact> CalculateRoi(Request request)
        {
            var products = new Hierarchy(request.ProductHierarchy);
            var geographies = new Hierarchy(request.GeographyHierarchy);
            var causals = new Hierarchy(request.SalesComponentHierarchy);
            var periods = new Hierarchy(request.PeriodHierarchy);

            var adjuster = new Adjuster(products, geographies, causals, periods);
            var sales = adjuster.AdjustSales(request);
            var aggregatedDuetos = adjuster.AggregateSales(request.Spend, sales);

            var rois = request.Spend.Join(aggregatedDuetos,
                s => new { s.ProductId, s.GeographyId, CausalId = s.SalesComponentId, s.TimeId },
                adt => new { adt.ProductId, adt.GeographyId, CausalId = adt.SalesComponentId, adt.TimeId },
                (s, adt) =>
                    new NullableFact()
                    {
                        ProductId = s.ProductId,
                        GeographyId = s.GeographyId,
                        SalesComponentId = s.SalesComponentId,
                        TimeId = s.TimeId,
                        Value = adt.Value / s.Value
                    });

            return rois.ToList();          
        }

        public List<NullableFact> AggregateFactsBySalesComponent(List<NullableFact> facts, List<Relation> salesComponentRelations)
        {
            var hierarchy = new Hierarchy(salesComponentRelations);
            var aggregator = new Aggregator(hierarchy);
            var results = new List<NullableFact>();

            foreach (var parent in salesComponentRelations.Select(s => s.Parent).Distinct())
            {
                results.AddRange(aggregator.Aggregate(facts, parent));
            }
            facts.AddRange(results);
            return facts;           
        }
    }
}
