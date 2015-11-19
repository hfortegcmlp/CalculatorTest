using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator.Contract;
using Calculator.Transformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Calculator.Test
{
    [TestClass]
    public class AggregatorTests
    {
        [TestMethod]
        public void Empty_Test()
        {
            short productId = 7;
            short geographyId = 11;
            short timeId = 13;

            var facts = new List<NullableFact>();
            facts.Add(new NullableFact() { ProductId = productId, GeographyId = geographyId, SalesComponentId = 2, TimeId = timeId, Value = 100 });
            facts.Add(new NullableFact() { ProductId = productId, GeographyId = geographyId, SalesComponentId = 3, TimeId = timeId, Value = 100 });

            var relations = new List<Relation>();            
            var hierarchy = new Hierarchy(relations);
            var aggregator = new Aggregator(hierarchy);
            var result = aggregator.Aggregate(facts, 1);

            Assert.AreEqual(0, result.Count());            
        }

        [TestMethod]
        public void SingleProdGeog_Test()
        {
            short productId = 7;
            short geographyId = 11;
            short timeId = 13;

            var facts = new List<NullableFact>();
            facts.Add(new NullableFact() { ProductId = productId, GeographyId = geographyId, SalesComponentId = 2, TimeId = timeId, Value = 100 });
            facts.Add(new NullableFact() { ProductId = productId, GeographyId = geographyId, SalesComponentId = 3, TimeId = timeId, Value = 100 });

            var relations = new List<Relation>();
            relations.Add(new Relation() { Child = 2, Parent = 1 });
            relations.Add(new Relation() { Child = 3, Parent = 1 });
            
            var hierarchy = new Hierarchy(relations);

            var aggregator = new Aggregator(hierarchy);
            var result = aggregator.Aggregate(facts, 1);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(result[0].ProductId, productId);
            Assert.AreEqual(result[0].GeographyId, geographyId);
            Assert.AreEqual(result[0].TimeId, timeId);
            Assert.AreEqual(result[0].SalesComponentId, (short)1);
            Assert.AreEqual(result[0].Value, 200);
        }

        [TestMethod]
        public void TwiceSingleProdGeog_Test()
        {
            short productIdOne = 7;
            short geographyIdOne = 11;
            short productIdTwo = 19;
            short geographyIdTwo = 23;
            short timeId = 13;

            var facts = new List<NullableFact>();
            facts.Add(new NullableFact() { ProductId = productIdOne, GeographyId = geographyIdOne, SalesComponentId = 2, TimeId = timeId, Value = 100 });
            facts.Add(new NullableFact() { ProductId = productIdOne, GeographyId = geographyIdOne, SalesComponentId = 3, TimeId = timeId, Value = 100 });
            facts.Add(new NullableFact() { ProductId = productIdTwo, GeographyId = geographyIdTwo, SalesComponentId = 2, TimeId = timeId, Value = 1000 });
            facts.Add(new NullableFact() { ProductId = productIdTwo, GeographyId = geographyIdTwo, SalesComponentId = 3, TimeId = timeId, Value = 1000 });

            var relations = new List<Relation>();
            relations.Add(new Relation() { Child = 2, Parent = 1 });
            relations.Add(new Relation() { Child = 3, Parent = 1 });

            var hierarchy = new Hierarchy(relations);

            var aggregator = new Aggregator(hierarchy);
            var result = aggregator.Aggregate(facts, 1);

            Assert.AreEqual(2, result.Count());

            var resultOne = result.Single(r => r.ProductId == productIdOne);
            Assert.AreEqual(resultOne.GeographyId, geographyIdOne);
            Assert.AreEqual(resultOne.TimeId, timeId);
            Assert.AreEqual(resultOne.SalesComponentId, (short)1);
            Assert.AreEqual(resultOne.Value, 200);

            var resultTwo = result.Single(r => r.ProductId == productIdTwo);
            Assert.AreEqual(resultTwo.GeographyId, geographyIdTwo);
            Assert.AreEqual(resultTwo.TimeId, timeId);
            Assert.AreEqual(resultTwo.SalesComponentId, (short)1);
            Assert.AreEqual(resultTwo.Value, 2000);
        }
    }
}
