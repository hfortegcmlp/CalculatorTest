using System;
using System.Collections.Generic;
using System.Linq;
using Calculator.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Calculator.Test
{
    [TestClass]
    public class Integration_CalculateRoi_WithAdjustment
    {
        /// <summary>
        /// Simple three level hierarcy
        /// 1
        /// |\
        /// 2  3
        /// |\  |\
        /// 4 5 6 7
        /// </summary>
        private List<Relation> hierarchy
        {
            get
            {
                var result = new List<Relation>();
                result.Add(new Relation { Child = 2, Parent = 1 });
                result.Add(new Relation { Child = 3, Parent = 1 });
                result.Add(new Relation { Child = 4, Parent = 2 });
                result.Add(new Relation { Child = 5, Parent = 2 });
                result.Add(new Relation { Child = 6, Parent = 3 });
                result.Add(new Relation { Child = 7, Parent = 3 });
                return result;
            }
        }

        private List<ConcreteFact> makeDuetos(short prod1, short prod2, short geog1, short geog2, short causal1, short causal2, short time1, short time2, float value)
        {
            var result = new List<ConcreteFact>();

            for (short i = prod1; i <= prod2; i++)
                for (short j = geog1; j <= geog2; j++)
                    for (short k = causal1; k <= causal2; k++)
                        for (short t = time1; t <= time2; t++)
                            result.Add(new ConcreteFact() { ProductId = i, GeographyId = j, SalesComponentId = k, TimeId = t, Value = value });
            return result;
        }

        /// <summary>
        /// 4 x products; 4 x geogs; 4 x sales components
        ///     => 64 combos, over 10 weeks.
        /// one margin; one spend value
        ///     => 64000 * 0.15 / 100 = 96
        /// </summary>
        [TestMethod]
        public void IntegratedTest()
        {
            var request = new Request();
            request.ProductHierarchy = new List<Relation>(this.hierarchy);
            request.GeographyHierarchy = new List<Relation>(this.hierarchy);
            request.SalesComponentHierarchy = new List<Relation>();
            request.PeriodHierarchy = new List<Relation>();

            request.Sales = new List<ConcreteFact>();
            request.Sales.AddRange(makeDuetos(4, 7, 4, 7, 4, 7, 1, 10, 100));

            request.Margins = new List<NullableFact>() { new NullableFact() { Value = 0.15f } };
            request.Spend = new List<NullableFact>() { new NullableFact() { ProductId = 1, GeographyId = 1, Value = 100 } };

            var calculator = new CalculatorService();

            var adjustments = new List<NullableFact>();

            for (short g = 4; g <= 7; g++)
            for (short s = 4; s <= 7; s++)
            {
                adjustments.Add(new NullableFact() { GeographyId = g, SalesComponentId = s, Value = 1.5f });
            }
                
            request.Adjustments = new List<Adjustment>() { new Adjustment() { AdjustmentType = AdjustmentType.Adjustment, Facts = adjustments } };
            var result = calculator.CalculateRoi(request);

            // Missing assert
            Assert.Fail();
        }

        /// <summary>
        /// 4 x products; 4 x geogs; 4 x sales components
        ///     => 64 combos, over 10 weeks.
        ///     32 @ 0.15 and 32 @0.25
        /// one margin; one spend value
        ///     => ( 32000 * 0.15 + 32000 * 0.25 ) / 100 = 128
        /// </summary>
        [TestMethod]
        public void DifferentMarginTest()
        {
            var request = new Request();
            request.ProductHierarchy = new List<Relation>(this.hierarchy);
            request.GeographyHierarchy = new List<Relation>(this.hierarchy);
            request.SalesComponentHierarchy = new List<Relation>(this.hierarchy);
            request.PeriodHierarchy = new List<Relation>();

            request.Sales = new List<ConcreteFact>();
            request.Sales.AddRange(makeDuetos(4, 7, 4, 7, 4, 7, 1, 10, 100));

            request.Margins = Enumerable.Range(1, 10).SelectMany(r =>
            {
                var facts = new List<NullableFact>();
                facts.Add(new NullableFact() { TimeId = (short)r, ProductId = 2, Value = 0.15f });
                facts.Add(new NullableFact() { TimeId = (short)r, ProductId = 3, Value = 0.25f });
                return facts;
            }).ToList();             
            request.Spend = new List<NullableFact>() { new NullableFact() { ProductId = 1, GeographyId = 1, Value = 100 } };

            // no adjustments
            request.Adjustments = null;

            var calculator = new CalculatorService();
            var result = calculator.CalculateRoi(request);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(128f, result.Single().Value, 0.0001);
        }

        /// <summary>
        /// 4 x products; 4 x geogs; 4 x sales components
        /// except:
        /// no product 7 in geog 7
        /// causal 6 not run in geogs 4,5
        /// 
        ///     => 64 combos, over 10 weeks.
        ///     with 120 gaps => 640-120 = 520;
        ///
        /// for prod2:
        ///     2 prods * 3 sc * 4 geogs * 10 weeks, and (240)
        ///     2 prods * 1 sc * 2 geogs * 10 weeks       (40) -- causal 6 doesn't run for geogs 4,5
        ///     
        /// for prod3;
        ///     --prod 6
        ///     1 prod * 2 geogs * 4 sc * 10 weeks        (80)
        ///     1 prod * 2 geogs * 3 sc * 10 weeks        (60) -- causal 6 doesn't run for geogs 4,5
        ///     --prod 7
        ///     1 prod * 1 geogs * 4 sc * 10 weeks, and   (40)
        ///     1 prod * 2 geogs * 3 sc * 10 weeks, and   (60) -- causal 6 doesn't run for geogs 4,5
        ///     
        /// 
        /// except @ 0.15 and 32 @0.25
        /// one margin; one spend value
        ///     => ( 280 * 0.15 + 240 * 0.25 ) / 100 = 128
        /// </summary>
        [TestMethod]
        public void DifferentMarginTest_WithDiscreteGaps()
        {
            var request = new Request();
            request.ProductHierarchy = new List<Relation>(this.hierarchy);
            request.GeographyHierarchy = new List<Relation>(this.hierarchy);
            request.SalesComponentHierarchy = new List<Relation>(this.hierarchy);
            request.PeriodHierarchy = new List<Relation>();

            request.Sales = new List<ConcreteFact>();
            request.Sales.AddRange(makeDuetos(4, 7, 4, 7, 4, 7, 1, 10, 1));

            // no product 7 in geog 7
            // will exclude for all 4 sales components x 10 weeks (40 leaf)
            request.Sales.RemoveAll(d => d.ProductId == 7 && d.GeographyId == 7);

            // causal 6 not run in geogs 4,5
            // will exclude for all 4 products x 10 weeks in both geogs (80 leaf)
            request.Sales.RemoveAll(d => d.SalesComponentId == 6 && (d.GeographyId == 4 || d.GeographyId == 5));

            request.Margins = Enumerable.Range(1, 10).SelectMany(r =>
            {
                var facts = new List<NullableFact>();
                facts.Add(new NullableFact() { TimeId = (short)r, ProductId = 2, Value = 0.15f });
                facts.Add(new NullableFact() { TimeId = (short)r, ProductId = 3, Value = 0.25f });
                return facts;
            }).ToList();     

            request.Spend = new List<NullableFact>() { new NullableFact() { ProductId = 1, GeographyId = 1, Value = 100 } };

            // no adjustments
            request.Adjustments = null;

            var calculator = new CalculatorService();
            var result = calculator.CalculateRoi(request);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1.02f, result.Single().Value, 0.0001);
        }
    }
}
