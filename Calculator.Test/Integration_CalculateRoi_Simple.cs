using System;
using System.Collections.Generic;
using System.Linq;
using Calculator.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Calculator.Test
{
    [TestClass]
    public class Integration_CalculateRoi_Simple
    {
        /// <summary>
        /// Simple one-one hierarcy
        /// </summary>
        private List<Relation> hierarchy
        {
            get
            {
                var result = new List<Relation>();
                result.Add(new Relation { Child = 2, Parent = 1 });
                return result;
            }
        }

        private List<ConcreteFact> makeDuetos(short fromProd, short toProd, short fromGeog, short toGeog, short fromCausal, short toCausal, short fromTime, short toTime, float value)
        {
            var result = new List<ConcreteFact>();

            for (short i = fromProd; i <= toProd; i++)
                for (short j = fromGeog; j <= toGeog; j++)
                    for (short k = fromCausal; k <= toCausal; k++)
                        for (short t = fromTime; t <= toTime; t++)
                            result.Add(new ConcreteFact() { ProductId = i, GeographyId = j, SalesComponentId = k, TimeId = t, Value = value });
            return result;
        }

        /// <summary>
        /// one campaign over ten periods
        /// spending 100
        /// generating 1000 units
        /// margin 0.15 => 150 revenue
        /// ROI = 1.5
        /// </summary>
        [TestMethod]
        public void IntegratedCalculateRoi_BasicTest()
        {
            var request = new Request();
            request.ProductHierarchy = new List<Relation>(this.hierarchy);
            request.GeographyHierarchy = new List<Relation>(this.hierarchy);
            request.SalesComponentHierarchy = new List<Relation>();
            request.PeriodHierarchy = new List<Relation>();

            request.Sales = new List<ConcreteFact>();
            request.Sales.AddRange(makeDuetos(2, 2, 2, 2, 1, 1, 1, 10, 100));

            request.Margins = Enumerable.Range(1, 10).Select(r => new NullableFact() { Value = 0.15f, TimeId = (short)r }).ToList();
            request.Spend = new List<NullableFact>() { new NullableFact() { ProductId = 1, GeographyId = 1, Value = 100 } };

            // no adjustments
            request.Adjustments = null;

            var calculator = new CalculatorService();
            var result = calculator.CalculateRoi(request);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1.5f, result.Single().Value, 0.0001);
        }

        /// <summary>
        /// one campaign over ten periods
        /// second campaign over ten periods
        /// third campaign over final five periods only
        /// spending 100
        /// generating 2500 units
        /// margin 0.15 => 375 revenue
        /// ROI = 3.75
        /// </summary>
        [TestMethod]
        public void IntegratedCalculateRoi_IntermediateTest()
        {
            var request = new Request();
            request.ProductHierarchy = new List<Relation>(this.hierarchy);
            request.GeographyHierarchy = new List<Relation>(this.hierarchy);
            request.SalesComponentHierarchy = new List<Relation>();
            request.PeriodHierarchy = new List<Relation>();

            request.Sales = new List<ConcreteFact>();
            request.Sales.AddRange(makeDuetos(2, 2, 2, 2, 1, 1, 1, 10, 100));
            request.Sales.AddRange(makeDuetos(2, 2, 2, 2, 2, 2, 1, 10, 100));
            request.Sales.AddRange(makeDuetos(2, 2, 2, 2, 3, 3, 6, 10, 100));

            request.Margins = Enumerable.Range(1, 10).Select(r => new NullableFact() { TimeId = (short)r, Value = 0.15f }).ToList();
            request.Spend = new List<NullableFact>() { new NullableFact() { ProductId = 1, GeographyId = 1, Value = 100 } };

            // no adjustments
            request.Adjustments = null;

            var calculator = new CalculatorService();
            var result = calculator.CalculateRoi(request);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2500 / 100 * 0.15f, result.Single().Value, 0.01);
        }

        /// <summary>
        /// Different return for campaigns;
        /// 10 * 50 + 10 * 120 + 5 * 95 = 2175
        /// 2175 * 0.15 = 326.25
        /// ROI = 3.2625
        /// </summary>
        [TestMethod]
        public void IntegratedCalculateRoi_ComplexTest()
        {
            var request = new Request();
            request.ProductHierarchy = new List<Relation>(this.hierarchy);
            request.GeographyHierarchy = new List<Relation>(this.hierarchy);
            request.SalesComponentHierarchy = new List<Relation>();
            request.PeriodHierarchy = new List<Relation>();

            request.Sales = new List<ConcreteFact>();
            request.Sales.AddRange(makeDuetos(2, 2, 2, 2, 1, 1, 1, 10, 50));
            request.Sales.AddRange(makeDuetos(2, 2, 2, 2, 2, 2, 1, 10, 120));
            request.Sales.AddRange(makeDuetos(2, 2, 2, 2, 3, 3, 6, 10, 95));

            request.Margins = Enumerable.Range(1, 10).Select(r => new NullableFact() { TimeId = (short)r, Value = 0.15f }).ToList();
            request.Spend = new List<NullableFact>() { new NullableFact() { ProductId = 1, GeographyId = 1, Value = 100 } };

            // no adjustments
            request.Adjustments = null;

            var calculator = new CalculatorService();
            var result = calculator.CalculateRoi(request);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3.2625f, result.Single().Value, 0.01);
        }
    }
}
