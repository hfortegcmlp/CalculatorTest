using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Calculator.Contract;
using Calculator.Transformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Calculator.Test
{
    [TestClass]
    public class FactLookup_ParentTests
    {
        /// <summary>
        /// If you don't pass any facts to the lookup it will return 1.0f for any key
        /// We use this instead of zero because we're almost always performing multiplicative calcs.
        /// </summary>
        [TestMethod]
        public void Test_NullFacts()
        {
            var products = new Moq.Mock<IHierarchy>();
            var geographies = new Moq.Mock<IHierarchy>();
            var causals = new Moq.Mock<IHierarchy>();
            var periods = new Moq.Mock<IHierarchy>();

            Expression<Func<IHierarchy, IEnumerable<short>>> anyInt = p => p.GetParents(Moq.It.IsAny<short>());
            var result = new List<short> { 1 };
            products.Setup(anyInt).Returns(result);
            geographies.Setup(anyInt).Returns(result);
            causals.Setup(anyInt).Returns(result);
            periods.Setup(anyInt).Returns(result);

            var factLookup = new FactLookup(null, products.Object, geographies.Object, causals.Object, periods.Object);

            Assert.AreEqual(1, factLookup.GetParent(2, 2, 2, 2, false, 1));
        }

        [TestMethod]
        public void Test_FactVsIds()
        {
            var products = new Moq.Mock<IHierarchy>();
            var geographies = new Moq.Mock<IHierarchy>();
            var causals = new Moq.Mock<IHierarchy>();
            var periods = new Moq.Mock<IHierarchy>();

            Expression<Func<IHierarchy, IEnumerable<short>>> anyInt = p => p.GetParents(Moq.It.IsAny<short>());
            var result = new List<short> { 1 };
            products.Setup(anyInt).Returns(result);
            geographies.Setup(anyInt).Returns(result);
            causals.Setup(anyInt).Returns(result);
            periods.Setup(anyInt).Returns(result);

            var factLookup = new FactLookup(null, products.Object, geographies.Object, causals.Object, periods.Object);

            Assert.AreEqual(1, factLookup.GetParent(2, 2, 2, 2, false, 1));
            Assert.AreEqual(1, factLookup.GetParent(new ConcreteFact() { ProductId = 2, GeographyId = 2, SalesComponentId = 2, TimeId = 2 }, false, 1));
        }

        
        /// <summary>
        /// Testing a single fact lookup, where everything is a child & parent of the concrete fact
        /// </summary>
        [TestMethod]
        public void TestSimple_1()
        {            
            var facts = new List<NullableFact>();
            facts.Add(new NullableFact() { ProductId = 1, GeographyId = 1, SalesComponentId = 1, TimeId = 1, Value = 100 });

            var products = new Moq.Mock<IHierarchy>();
            var geographies = new Moq.Mock<IHierarchy>();
            var causals = new Moq.Mock<IHierarchy>();
            var periods = new Moq.Mock<IHierarchy>();

            Expression<Func<IHierarchy, IEnumerable<short>>> anyInt = p => p.GetParents(Moq.It.IsAny<short>());
            var result = new List<short> { 1 };
            products.Setup(anyInt).Returns(result);
            geographies.Setup(anyInt).Returns(result);
            causals.Setup(anyInt).Returns(result);
            periods.Setup(anyInt).Returns(result);

            Expression<Func<IHierarchy, bool>> allRelationships = p => p.RelationExists(Moq.It.IsAny<short>(), Moq.It.IsAny<short>());
            products.Setup(allRelationships).Returns(true);
            geographies.Setup(allRelationships).Returns(true);
            causals.Setup(allRelationships).Returns(true);
            periods.Setup(allRelationships).Returns(true);

            //, Hierarchy products, Hierarchy geographies, Hierarchy causals, Hierarchy periods

            var lookup = new FactLookup(facts, products.Object, geographies.Object, causals.Object, periods.Object);

            Assert.AreEqual(100, lookup.GetParent(0, 0, 0, 1));
            Assert.AreEqual(100, lookup.GetParent(1, 1, 1, 1));
            Assert.AreEqual(100, lookup.GetParent(2, 2, 2, 1));
        }

        /// <summary>
        /// Testing a single fact lookup, where everything is a child & parent of the global fact
        /// </summary>
        [TestMethod]
        public void TestSimple_2()
        {
            var facts = new List<NullableFact>();
            facts.Add(new NullableFact() { ProductId = null, GeographyId = null, SalesComponentId = null, TimeId = 1, Value = 100 });

            var products = new Moq.Mock<IHierarchy>();
            var geographies = new Moq.Mock<IHierarchy>();
            var causals = new Moq.Mock<IHierarchy>();
            var periods = new Moq.Mock<IHierarchy>();

            Expression<Func<IHierarchy, IEnumerable<short>>> anyInt = p => p.GetParents(Moq.It.IsAny<short>());
            var result = new List<short> { 1 };
            products.Setup(anyInt).Returns(result);
            geographies.Setup(anyInt).Returns(result);
            causals.Setup(anyInt).Returns(result);
            periods.Setup(anyInt).Returns(result);

            Expression<Func<IHierarchy, bool>> allRelationships = p => p.RelationExists(Moq.It.IsAny<short>(), Moq.It.IsAny<short>());
            products.Setup(allRelationships).Returns(true);
            geographies.Setup(allRelationships).Returns(true);
            causals.Setup(allRelationships).Returns(true);
            periods.Setup(allRelationships).Returns(true);

            //, Hierarchy products, Hierarchy geographies, Hierarchy causals, Hierarchy periods

            var lookup = new FactLookup(facts, products.Object, geographies.Object, causals.Object, periods.Object);

            Assert.AreEqual(100, lookup.GetParent(0, 0, 0, 1));
            Assert.AreEqual(100, lookup.GetParent(1, 1, 1, 1));
            Assert.AreEqual(100, lookup.GetParent(2, 2, 2, 1));
        }

        /// <summary>
        /// Testing a single fact lookup, where everything is a child & parent of the global fact
        /// </summary>
        [TestMethod]
        public void TestSimple_3()
        {
            var facts = new List<NullableFact>();
            facts.Add(new NullableFact() { ProductId = 1, GeographyId = 1, SalesComponentId = 1, TimeId = 1, Value = 100 });

            var products = new Moq.Mock<IHierarchy>();
            var geographies = new Moq.Mock<IHierarchy>();
            var causals = new Moq.Mock<IHierarchy>();
            var periods = new Moq.Mock<IHierarchy>();

            Expression<Func<IHierarchy, IEnumerable<short>>> anyInt = p => p.GetParents(2);
            var result = new List<short> { 1 };
            products.Setup(anyInt).Returns(result);
            geographies.Setup(anyInt).Returns(result);
            causals.Setup(anyInt).Returns(result);
            periods.Setup(anyInt).Returns(result);

            Expression<Func<IHierarchy, bool>> allRelationships = p => p.RelationExists(Moq.It.IsAny<short>(), Moq.It.IsAny<short>());
            products.Setup(allRelationships).Returns(true);
            geographies.Setup(allRelationships).Returns(true);
            causals.Setup(allRelationships).Returns(true);
            periods.Setup(allRelationships).Returns(true);

            //, Hierarchy products, Hierarchy geographies, Hierarchy causals, Hierarchy periods

            var lookup = new FactLookup(facts, products.Object, geographies.Object, causals.Object, periods.Object);

            Assert.AreEqual(100, lookup.GetParent(2, 2, 2, 1));
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSimple_Unmatched1()
        {
            var facts = new List<NullableFact>();
            facts.Add(new NullableFact() { ProductId = 1, GeographyId = 1, SalesComponentId = 1, TimeId = 1, Value = 100 });

            var products = new Moq.Mock<IHierarchy>();
            var geographies = new Moq.Mock<IHierarchy>();
            var causals = new Moq.Mock<IHierarchy>();
            var periods = new Moq.Mock<IHierarchy>();

            Expression<Func<IHierarchy, IEnumerable<short>>> anyInt = p => p.GetParents(2);
            var result = new List<short> { 1 };
            products.Setup(anyInt).Returns(result);
            geographies.Setup(anyInt).Returns(result);
            causals.Setup(anyInt).Returns(result);
            periods.Setup(anyInt).Returns(result);

            //, Hierarchy products, Hierarchy geographies, Hierarchy causals, Hierarchy periods
            var lookup = new FactLookup(facts, products.Object, geographies.Object, causals.Object, periods.Object);

            Assert.AreEqual(100, lookup.GetParent(3, 2, 2, 2));
        }
    }
}
