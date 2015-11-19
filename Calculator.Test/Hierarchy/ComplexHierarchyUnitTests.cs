using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Calculator.Transformation;
using Calculator.Contract;

namespace Calculator.Test
{
    [TestClass]
    public class ComplexHierarchyUnitTests
    {
        short i1 = 1;
        short i2 = 2;
        short i3 = 3;
        short i4 = 4;
        short i5 = 5;
        short i6 = 6;
        short i7 = 7;

        private void AssertHelper(List<short> expected, IEnumerable<short> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count());
            expected = expected.OrderBy(e => e).ToList();
            var actualList = actual.OrderBy(e => e).ToList();

            for (short i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], actualList[i]);
            }
        }

        [TestMethod]
        public void AllItemsPoshortDown_GetChildren()
        {
            var items = new List<Relation>();
            items.Add(new Relation{ Child = i1, Parent = i3});
            items.Add(new Relation{ Child = i1, Parent = i4});
            items.Add(new Relation{ Child = i2, Parent = i3});
            items.Add(new Relation{ Child = i2, Parent = i4});
            items.Add(new Relation{ Child = i1, Parent = i5});
            items.Add(new Relation{ Child = i2, Parent = i5});
            items.Add(new Relation{ Child = i3, Parent = i5});
            items.Add(new Relation{ Child = i4, Parent = i5});

            var result = new Hierarchy(items);

            this.AssertHelper(new List<short> { i1 }, result.GetChildren(i1));
            this.AssertHelper(new List<short> { i2 }, result.GetChildren(i2));
            this.AssertHelper(new List<short> { i1, i2, i3 }, result.GetChildren(i3));
            this.AssertHelper(new List<short> { i1, i2, i4 }, result.GetChildren(i4));
            this.AssertHelper(new List<short> { i1, i2, i3, i4, i5 }, result.GetChildren(i5));
        }

        [TestMethod]
        public void AllItemsPoshortDown_GetParents()
        {
            var items = new List<Relation>();
            items.Add(new Relation { Child = i1, Parent = i3 });
            items.Add(new Relation { Child = i1, Parent = i4 });
            items.Add(new Relation { Child = i2, Parent = i3 });
            items.Add(new Relation { Child = i2, Parent = i4 });
            items.Add(new Relation { Child = i1, Parent = i5 });
            items.Add(new Relation { Child = i2, Parent = i5 });
            items.Add(new Relation { Child = i3, Parent = i5 });
            items.Add(new Relation { Child = i4, Parent = i5 });

            var result = new Hierarchy(items);

            this.AssertHelper(new List<short> { i1, i3, i4, i5 }, result.GetParents(i1));
            this.AssertHelper(new List<short> { i2, i3, i4, i5 }, result.GetParents(i2));
            this.AssertHelper(new List<short> { i3, i5 }, result.GetParents(i3));
            this.AssertHelper(new List<short> { i4, i5 }, result.GetParents(i4));
            this.AssertHelper(new List<short> { i5 }, result.GetParents(i5));            
        }

        [TestMethod]
        public void ComplexHierarchy_TwoLevelTree()
        {
            var items = new List<Relation>();
            items.Add(new Relation{ Child = i2, Parent = i1});
            items.Add(new Relation{ Child = i3, Parent = i2});
            items.Add(new Relation{ Child = i4, Parent = i2});
            items.Add(new Relation{ Child = i5, Parent = i1});
            items.Add(new Relation{ Child = i6, Parent = i5});
            items.Add(new Relation{ Child = i7, Parent = i5});
            var result = new Hierarchy(items);

            this.AssertHelper(new List<short> { i1 }, result.GetParents(i1));
            this.AssertHelper(new List<short> { i1, i2, i3, i4, i5, i6, i7 }, result.GetChildren(i1));

            this.AssertHelper(new List<short> { i1, i2 }, result.GetParents(i2));
            this.AssertHelper(new List<short> { i2, i3, i4 }, result.GetChildren(i2));

            this.AssertHelper(new List<short> { i1, i2, i3 }, result.GetParents(i3));
            this.AssertHelper(new List<short> { i3 }, result.GetChildren(i3));
        }
    }
}
