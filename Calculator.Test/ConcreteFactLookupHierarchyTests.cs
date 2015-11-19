using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Calculator.Contract;
using Calculator.Transformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Calculator.Test
{
    [TestClass]
    public class ConcreteFactLookupHierarchyTests
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
                result.Add(new Relation{ Child = 2, Parent = 1});
                result.Add(new Relation{ Child = 3, Parent = 1});
                result.Add(new Relation{ Child = 4, Parent = 2});
                result.Add(new Relation{ Child = 5, Parent = 2});
                result.Add(new Relation{ Child = 6, Parent = 3});
                result.Add(new Relation{ Child = 7, Parent = 3});
                return result;
            }
        }

        Hierarchy prod;
        Hierarchy geog;
        Hierarchy causal;
        Hierarchy period;
        List<ConcreteFact> facts;
        
        [TestInitialize]
        public void Setup()
        {
            this.prod = new Hierarchy(this.hierarchy);
            this.geog = new Hierarchy(this.hierarchy);
            this.causal = new Hierarchy(null); // no causal hierarchy, this is 'all causal' the same
            this.period = new Hierarchy(this.hierarchy);

            this.facts = new List<ConcreteFact>();
        }
    
        /// <summary>
        /// Single NULL fact returns same value for everything.
        /// </summary>
        [TestMethod]
        public void FactHierarchy_SimpleLeafLevelTest()
        {
            facts.Add(new ConcreteFact() { ProductId = 4, GeographyId = 1, SalesComponentId = 1, TimeId = 1, Value = 100 });
            facts.Add(new ConcreteFact() { ProductId = 5, GeographyId = 1, SalesComponentId = 1, TimeId = 1, Value = 100 });
            facts.Add(new ConcreteFact() { ProductId = 6, GeographyId = 1, SalesComponentId = 1, TimeId = 1, Value = 100 });
            facts.Add(new ConcreteFact() { ProductId = 7, GeographyId = 1, SalesComponentId = 1, TimeId = 1, Value = 100 });
            
            var lookup = new ConcreteFactLookup(this.facts, this.prod, this.geog, this.causal, this.period);

            Assert.AreEqual(400, lookup.SumChildren(1, 1, 1, 1));
            Assert.AreEqual(200, lookup.SumChildren(2, 1, 1, 1));
            Assert.AreEqual(200, lookup.SumChildren(3, 1, 1, 1));

            // we can also provide null values for any 'all' ids.
            Assert.AreEqual(400, lookup.SumChildren(null, null, null, null));
            Assert.AreEqual(400, lookup.SumChildren(1, null, null, null));
            Assert.AreEqual(200, lookup.SumChildren(2, null, null, null));
            Assert.AreEqual(200, lookup.SumChildren(3, null, null, null));
        }

        /// <summary>
        /// Crossing the leaf level combinations
        /// </summary>
        [TestMethod]
        public void FactHierarchy_CrossLeaf_Test()
        {
            for (short i = 4; i <= 7; i++)
                for (short j = 4; j <= 7; j++)
                {
                    facts.Add(new ConcreteFact() { ProductId = i, GeographyId = j, SalesComponentId = 1, TimeId = 1, Value = 1 });
                    facts.Add(new ConcreteFact() { ProductId = i, GeographyId = j, SalesComponentId = 2, TimeId = 1, Value = 1 });
                }

            var lookup = new ConcreteFactLookup(this.facts, this.prod, this.geog, this.causal, this.period);

            // this is a 4x4 graph, so 16.
            Assert.AreEqual(16, lookup.SumChildren(1, 1, 1, 1));

            // including both causals it will be 32
            Assert.AreEqual(32, lookup.SumChildren(1, 1, null, 1));
            Assert.AreEqual(32, lookup.SumChildren(null, null, null, null));

            // each product will have 4 leaf items (the 4 geogs)
            Assert.AreEqual(4, lookup.SumChildren(4, 1, 1, 1));
            // both causals
            Assert.AreEqual(8, lookup.SumChildren(4, 1, null, 1));
            Assert.AreEqual(8, lookup.SumChildren(4, null, null, 1));

            // each geog will have 4 leaf items (the 4 prods)
            Assert.AreEqual(4, lookup.SumChildren(1, 4, 1, 1));
            // both causals
            Assert.AreEqual(8, lookup.SumChildren(1, 4, null, 1));
            Assert.AreEqual(8, lookup.SumChildren(null, 4, null, 1));

            // the real leaf level 
            Assert.AreEqual(1, lookup.SumChildren(6, 6, 1, 1));
            Assert.AreEqual(1, lookup.SumChildren(5, 7, 2, 1));
            Assert.AreEqual(2, lookup.SumChildren(6, 6, null, 1));            
        }

        /// <summary>
        /// Facts covering an entire tier (level 2 of product hierarchy) slice the hierarcy 
        /// </summary>
        [TestMethod]
        public void FactHierarchy_CrossLeaf_UnbalancedTest()
        {
            for (short i = 4; i <= 7; i++)
                for (short j = 4; j <= 7; j++)
                {
                    facts.Add(new ConcreteFact() { ProductId = i, GeographyId = j, SalesComponentId = 1, TimeId = 1, Value = 1 });
                    facts.Add(new ConcreteFact() { ProductId = i, GeographyId = j, SalesComponentId = 2, TimeId = 1, Value = 1 });
                }

            // we remove item 7 from the geo hierarchy
            var geo = new List<Relation>(this.hierarchy);
            geo.RemoveAll(r => r.Child == 7);
            
            var lookup = new ConcreteFactLookup(this.facts, this.prod, new Hierarchy(geo), this.causal, this.period);

            // this is a 4x3 graph, so 12
            Assert.AreEqual(12, lookup.SumChildren(1, 1, 1, 1));
            // including both causals will double
            Assert.AreEqual(24, lookup.SumChildren(1, 1, null, 1));
            // null will still include geog 7, so 4x4x2
            Assert.AreEqual(32, lookup.SumChildren(null, null, null, null));

            // each product will have 3 leaf items (the 3 geogs)
            Assert.AreEqual(3, lookup.SumChildren(4, 1, 1, 1));
            // both causals
            Assert.AreEqual(6, lookup.SumChildren(4, 1, null, 1));
            // null includes geog 7
            Assert.AreEqual(8, lookup.SumChildren(4, null, null, 1));

            // each geog will have 4 leaf items (the 4 prods)
            Assert.AreEqual(4, lookup.SumChildren(1, 4, 1, 1));
            // both causals
            Assert.AreEqual(8, lookup.SumChildren(1, 4, null, 1));
            Assert.AreEqual(8, lookup.SumChildren(null, 4, null, 1));

            // the real leaf level 
            Assert.AreEqual(1, lookup.SumChildren(6, 6, 1, 1));            
            Assert.AreEqual(2, lookup.SumChildren(6, 6, null, 1));

            // we can still query 7 - it exists, just not part of other hierarchy
            Assert.AreEqual(1, lookup.SumChildren(5, 7, 2, 1));
            Assert.AreEqual(4, lookup.SumChildren(null, 7, 2, 1));
            Assert.AreEqual(8, lookup.SumChildren(null, 7, null, 1));
        }

        /// <summary>
        /// Facts covering two entire tiers (level 2 of product, level 2 of geog) slice & dice the hierarcy!
        /// </summary>
        [TestMethod]
        public void FactHierarchy_CrossLeaf_WithGapsTest()
        {
            for (short i = 4; i <= 7; i++)
                for (short j = 4; j <= 7; j++)
                {
                    facts.Add(new ConcreteFact() { ProductId = i, GeographyId = j, SalesComponentId = 1, TimeId = 1, Value = 1 });
                    facts.Add(new ConcreteFact() { ProductId = i, GeographyId = j, SalesComponentId = 2, TimeId = 1, Value = 1 });
                }

            // product 5 is not sold in geography 5
            facts.RemoveAll(f => f.ProductId == 5 && f.GeographyId == 5);

            var lookup = new ConcreteFactLookup(this.facts, this.prod, this.geog, this.causal, this.period);

            // this is a 4x4 graph, so 16 less 1 gap
            Assert.AreEqual(15, lookup.SumChildren(1, 1, 1, 1));

            // including both causals it will be 32
            Assert.AreEqual(30, lookup.SumChildren(1, 1, null, 1));
            Assert.AreEqual(30, lookup.SumChildren(null, null, null, null));

            // each product will have 4 leaf items (the 4 geogs)
            Assert.AreEqual(4, lookup.SumChildren(4, 1, 1, 1));
            // both causals
            Assert.AreEqual(8, lookup.SumChildren(4, 1, null, 1));
            Assert.AreEqual(8, lookup.SumChildren(4, null, null, 1));

            // product 5 has 3 leaf items
            Assert.AreEqual(3, lookup.SumChildren(5, 1, 1, 1));
            // both causals
            Assert.AreEqual(6, lookup.SumChildren(5, 1, null, 1));
            Assert.AreEqual(6, lookup.SumChildren(5, null, null, 1));

            // each geog will have 4 leaf items (the 4 prods)
            Assert.AreEqual(4, lookup.SumChildren(1, 4, 1, 1));
            // both causals
            Assert.AreEqual(8, lookup.SumChildren(1, 4, null, 1));
            Assert.AreEqual(8, lookup.SumChildren(null, 4, null, 1));
            
            // geog 5 has 3 leaf items
            Assert.AreEqual(3, lookup.SumChildren(1, 5, 1, 1));
            // both causals
            Assert.AreEqual(6, lookup.SumChildren(1, 5, null, 1));
            Assert.AreEqual(6, lookup.SumChildren(null, 5, null, 1));

            // the real leaf level 
            Assert.AreEqual(1, lookup.SumChildren(6, 6, 1, 1));
            Assert.AreEqual(1, lookup.SumChildren(5, 7, 2, 1));
            Assert.AreEqual(2, lookup.SumChildren(6, 6, null, 1));

            bool exception = false;
            try
            {
                Assert.AreEqual(0, lookup.SumChildren(5, 5, 2, 1, true));
            }
            catch (InvalidOperationException)
            {
                exception = true;
            }
            Assert.IsTrue(exception);
        }
    }
}
