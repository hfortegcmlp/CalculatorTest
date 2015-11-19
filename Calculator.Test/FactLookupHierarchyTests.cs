using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Calculator.Contract;
using Calculator.Transformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Calculator.Test
{
    [TestClass]
    public class FactLookupHierarchyTests
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
        List<NullableFact> facts;
        
        [TestInitialize]
        public void Setup()
        {
            this.prod = new Hierarchy(this.hierarchy);
            this.geog = new Hierarchy(this.hierarchy);
            this.causal = new Hierarchy(null); // no causal hierarchy, this is 'all causal' the same
            this.period = new Hierarchy(this.hierarchy);

            this.facts = new List<NullableFact>();
        }
    
        /// <summary>
        /// Single NULL fact returns same value for everything.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FactHierarchy_NullTest()
        {
            facts.Add(new NullableFact() { ProductId = null, GeographyId = null, SalesComponentId = null, TimeId = null, Value = 100 });
            var lookup = new FactLookup(this.facts, this.prod, this.geog, this.causal, this.period);

            Assert.AreEqual(100, lookup.GetParent(1, 1, 1, 1));
            Assert.AreEqual(100, lookup.GetParent(7, 7, 7, 7));
            Assert.AreEqual(100, lookup.GetParent(3, 3, 3, 3));
            Assert.AreEqual(100, lookup.GetParent(1, 4, 6, 5));
            Assert.AreEqual(100, lookup.GetParent(2, 3, 3, 1));
            Assert.AreEqual(100, lookup.GetParent(6, 2, 4, 3));
        }

        /// <summary>
        /// Single NULL fact returns same value for everything - no null time allowed
        /// </summary>
        [TestMethod]        
        public void FactHierarchy_NullTest_ExactTime()
        {
            facts.Add(new NullableFact() { ProductId = null, GeographyId = null, SalesComponentId = null, TimeId = 1, Value = 100 });
            var lookup = new FactLookup(this.facts, this.prod, this.geog, this.causal, this.period);

            Assert.AreEqual(100, lookup.GetParent(1, 1, 1, 1));
            Assert.AreEqual(100, lookup.GetParent(7, 7, 7, 1));
            Assert.AreEqual(100, lookup.GetParent(3, 3, 3, 1));
            Assert.AreEqual(100, lookup.GetParent(1, 4, 6, 1));
            Assert.AreEqual(100, lookup.GetParent(2, 3, 3, 1));
            Assert.AreEqual(100, lookup.GetParent(6, 2, 4, 1));
        }

        /// <summary>
        /// Single top level fact returns same value for everything.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FactHierarchy_TopParentTest()
        {
            facts.Add(new NullableFact() { ProductId = 1, GeographyId = 1, SalesComponentId = null, TimeId = 1, Value = 100 });
            var lookup = new FactLookup(this.facts, this.prod, this.geog, this.causal, this.period);

            Assert.AreEqual(100, lookup.GetParent(1, 1, 1, 1));
            Assert.AreEqual(100, lookup.GetParent(7, 7, 7, 7));
            Assert.AreEqual(100, lookup.GetParent(3, 3, 3, 3));
            Assert.AreEqual(100, lookup.GetParent(1, 4, 6, 5));
            Assert.AreEqual(100, lookup.GetParent(2, 3, 3, 1));
            Assert.AreEqual(100, lookup.GetParent(6, 2, 4, 3));
        }

        /// <summary>
        /// Single top level fact returns same value for everything.
        /// </summary>
        [TestMethod]
        public void FactHierarchy_TopParentTest_ExactTime()
        {
            facts.Add(new NullableFact() { ProductId = 1, GeographyId = 1, SalesComponentId = null, TimeId = 1, Value = 100 });
            var lookup = new FactLookup(this.facts, this.prod, this.geog, this.causal, this.period);

            Assert.AreEqual(100, lookup.GetParent(1, 1, 1, 1));
            Assert.AreEqual(100, lookup.GetParent(7, 7, 7, 1));
            Assert.AreEqual(100, lookup.GetParent(3, 3, 3, 1));
            Assert.AreEqual(100, lookup.GetParent(1, 4, 6, 1));
            Assert.AreEqual(100, lookup.GetParent(2, 3, 3, 1));
            Assert.AreEqual(100, lookup.GetParent(6, 2, 4, 1));
        }

        /// <summary>
        /// Facts covering an entire tier (level 2 of product hierarchy) slice the hierarcy 
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),"Zero facts identified for given dimensions p:7, g:7, c:7, t:7")]
        public void FactHierarchy_MultipleFactsTest()
        {
            facts.Add(new NullableFact() { ProductId = 3, GeographyId = 1, SalesComponentId = null, TimeId = 1, Value = 100 });
            facts.Add(new NullableFact() { ProductId = 2, GeographyId = 1, SalesComponentId = null, TimeId = 1, Value = 50 });
            var lookup = new FactLookup(this.facts, this.prod, this.geog, this.causal, this.period);

            // items in the '3' branch for products
            Assert.AreEqual(100, lookup.GetParent(3, 1, 1, 1));
            Assert.AreEqual(100, lookup.GetParent(7, 7, 7, 7));
            Assert.AreEqual(100, lookup.GetParent(6, 1, 1, 1));
            Assert.AreEqual(100, lookup.GetParent(7, 3, 2, 4));
            
            // in the '2' branch
            Assert.AreEqual(50, lookup.GetParent(2, 1, 1, 1));
            Assert.AreEqual(50, lookup.GetParent(2, 7, 7, 7));

            Assert.AreEqual(50, lookup.GetParent(4, 1, 1, 1));
            Assert.AreEqual(50, lookup.GetParent(4, 7, 7, 7));
            Assert.AreEqual(50, lookup.GetParent(5, 3, 3, 3));
            Assert.AreEqual(50, lookup.GetParent(5, 6, 3, 1));
        }

        /// <summary>
        /// Facts covering an entire tier (level 2 of product hierarchy) slice the hierarcy 
        /// </summary>
        [TestMethod]        
        public void FactHierarchy_MultipleFactsTest_ExactTime()
        {
            facts.Add(new NullableFact() { ProductId = 3, GeographyId = 1, SalesComponentId = null, TimeId = 1, Value = 100 });
            facts.Add(new NullableFact() { ProductId = 2, GeographyId = 1, SalesComponentId = null, TimeId = 1, Value = 50 });
            var lookup = new FactLookup(this.facts, this.prod, this.geog, this.causal, this.period);

            // items in the '3' branch for products
            Assert.AreEqual(100, lookup.GetParent(3, 1, 1, 1));
            Assert.AreEqual(100, lookup.GetParent(7, 7, 7, 1));
            Assert.AreEqual(100, lookup.GetParent(6, 1, 1, 1));
            Assert.AreEqual(100, lookup.GetParent(7, 3, 2, 1));

            // in the '2' branch
            Assert.AreEqual(50, lookup.GetParent(2, 1, 1, 1));
            Assert.AreEqual(50, lookup.GetParent(2, 7, 7, 1));

            Assert.AreEqual(50, lookup.GetParent(4, 1, 1, 1));
            Assert.AreEqual(50, lookup.GetParent(4, 7, 7, 1));
            Assert.AreEqual(50, lookup.GetParent(5, 3, 3, 1));
            Assert.AreEqual(50, lookup.GetParent(5, 6, 3, 1));
        }

        /// <summary>
        /// Facts covering two entire tiers (level 2 of product, level 2 of geog) slice & dice the hierarcy!
        /// Failing due to 'exact time match' implementation
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Zero facts identified for given dimensions p:7, g:4, c:7, t:7")]
        public void FactHierarchy_CrossMultipleFactsTest()
        {
            var p2g2 = 50;
            var p2g3 = 250;
            var p3g2 = 100;
            var p3g3 = 200;

            // geog 2
            facts.Add(new NullableFact() { ProductId = 3, GeographyId = 2, SalesComponentId = null, TimeId = 1, Value = p3g2 });
            facts.Add(new NullableFact() { ProductId = 2, GeographyId = 2, SalesComponentId = null, TimeId = 1, Value = p2g2 });

            // geog 3
            facts.Add(new NullableFact() { ProductId = 3, GeographyId = 3, SalesComponentId = null, TimeId = 1, Value = p3g3 });
            facts.Add(new NullableFact() { ProductId = 2, GeographyId = 3, SalesComponentId = null, TimeId = 1, Value = p2g3 });
            
            var lookup = new FactLookup(this.facts, this.prod, this.geog, this.causal, this.period);

            // tests from above but constrained to the 'geog 2' branch:
            // i.e. geog 2, or children 4 / 5

                // items in the '3' branch for products
            Assert.AreEqual(p3g2, lookup.GetParent(3, 2, 1, 1));
            Assert.AreEqual(p3g2, lookup.GetParent(7, 4, 7, 7));
            Assert.AreEqual(p3g2, lookup.GetParent(6, 5, 1, 1));
            Assert.AreEqual(p3g2, lookup.GetParent(7, 2, 2, 4));

                // in the '2' branch
            Assert.AreEqual(p2g2, lookup.GetParent(2, 5, 1, 1));
            Assert.AreEqual(p2g2, lookup.GetParent(2, 4, 7, 7));

            Assert.AreEqual(p2g2, lookup.GetParent(4, 2, 1, 1));
            Assert.AreEqual(p2g2, lookup.GetParent(4, 5, 7, 7));
            Assert.AreEqual(p2g2, lookup.GetParent(5, 4, 3, 3));
            Assert.AreEqual(p2g2, lookup.GetParent(5, 2, 3, 1));


            // repeated tests from above but now constrained to the 'geog 3' branch:
            // i.e. geog 3, or children 6 / 7

            // items in the '3' branch for products
            Assert.AreEqual(p3g3, lookup.GetParent(3, 3, 1, 1));
            Assert.AreEqual(p3g3, lookup.GetParent(7, 6, 7, 7));
            Assert.AreEqual(p3g3, lookup.GetParent(6, 7, 1, 1));
            Assert.AreEqual(p3g3, lookup.GetParent(7, 3, 2, 4));

            // in the '2' branch
            Assert.AreEqual(p2g3, lookup.GetParent(2, 6, 1, 1));
            Assert.AreEqual(p2g3, lookup.GetParent(2, 7, 7, 7));

            Assert.AreEqual(p2g3, lookup.GetParent(4, 3, 1, 1));
            Assert.AreEqual(p2g3, lookup.GetParent(4, 6, 7, 7));
            Assert.AreEqual(p2g3, lookup.GetParent(5, 7, 3, 3));
            Assert.AreEqual(p2g3, lookup.GetParent(5, 3, 3, 1));

        }

        /// <summary>
        /// Facts covering two entire tiers (level 2 of product, level 2 of geog) slice & dice the hierarcy!
        /// </summary>
        [TestMethod]
        public void FactHierarchy_CrossMultipleFactsTest_ExactTime()
        {
            var p2g2 = 50;
            var p2g3 = 250;
            var p3g2 = 100;
            var p3g3 = 200;

            // geog 2
            facts.Add(new NullableFact() { ProductId = 3, GeographyId = 2, SalesComponentId = null, TimeId = 1, Value = p3g2 });
            facts.Add(new NullableFact() { ProductId = 2, GeographyId = 2, SalesComponentId = null, TimeId = 1, Value = p2g2 });

            // geog 3
            facts.Add(new NullableFact() { ProductId = 3, GeographyId = 3, SalesComponentId = null, TimeId = 1, Value = p3g3 });
            facts.Add(new NullableFact() { ProductId = 2, GeographyId = 3, SalesComponentId = null, TimeId = 1, Value = p2g3 });

            var lookup = new FactLookup(this.facts, this.prod, this.geog, this.causal, this.period);

            // tests from above but constrained to the 'geog 2' branch:
            // i.e. geog 2, or children 4 / 5

            // items in the '3' branch for products
            Assert.AreEqual(p3g2, lookup.GetParent(3, 2, 1, 1));
            Assert.AreEqual(p3g2, lookup.GetParent(7, 4, 7, 1));
            Assert.AreEqual(p3g2, lookup.GetParent(6, 5, 1, 1));
            Assert.AreEqual(p3g2, lookup.GetParent(7, 2, 2, 1));

            // in the '2' branch
            Assert.AreEqual(p2g2, lookup.GetParent(2, 5, 1, 1));
            Assert.AreEqual(p2g2, lookup.GetParent(2, 4, 7, 1));
            Assert.AreEqual(p2g2, lookup.GetParent(4, 2, 1, 1));
            Assert.AreEqual(p2g2, lookup.GetParent(4, 5, 7, 1));
            Assert.AreEqual(p2g2, lookup.GetParent(5, 4, 3, 1));
            Assert.AreEqual(p2g2, lookup.GetParent(5, 2, 3, 1));

            // repeated tests from above but now constrained to the 'geog 3' branch:
            // i.e. geog 3, or children 6 / 7

            // items in the '3' branch for products
            Assert.AreEqual(p3g3, lookup.GetParent(3, 3, 1, 1));
            Assert.AreEqual(p3g3, lookup.GetParent(7, 6, 7, 1));
            Assert.AreEqual(p3g3, lookup.GetParent(6, 7, 1, 1));
            Assert.AreEqual(p3g3, lookup.GetParent(7, 3, 2, 1));

            // in the '2' branch
            Assert.AreEqual(p2g3, lookup.GetParent(2, 6, 1, 1));
            Assert.AreEqual(p2g3, lookup.GetParent(2, 7, 7, 1));

            Assert.AreEqual(p2g3, lookup.GetParent(4, 3, 1, 1));
            Assert.AreEqual(p2g3, lookup.GetParent(4, 6, 7, 1));
            Assert.AreEqual(p2g3, lookup.GetParent(5, 7, 3, 1));
            Assert.AreEqual(p2g3, lookup.GetParent(5, 3, 3, 1));
        }
    }
}
